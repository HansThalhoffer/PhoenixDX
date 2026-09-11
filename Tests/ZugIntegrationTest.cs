using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Tests der Zugverwaltung gegen die echten Karten- und Zugdaten.
    /// </summary>
    public class ZugIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
            TeleportRules.ResetCache();
            BewegungsRules.ResetCache();
        }

        /// <summary>
        /// Die Tabelle Zugreihenfolge liegt in der Kartendatenbank und wurde bisher nie geladen
        /// </summary>
        [StaFact]
        public void ZugreihenfolgeWirdGeladen() {
            LadeAlles();
            Assert.NotNull(SharedData.Zugreihenfolge);
            Assert.NotEmpty(SharedData.Zugreihenfolge);
        }

        /// <summary>
        /// Innerhalb eines Monats muss jedes Reich genau einmal am Zug sein, und die Reihenfolge
        /// muss lückenlos bei 1 beginnen.
        /// </summary>
        [StaFact]
        public void ZugreihenfolgeIstInSichStimmig() {
            LadeAlles();
            Assert.NotNull(SharedData.Zugreihenfolge);

            // ein Monat, für den Daten vorliegen
            var monate = SharedData.Zugreihenfolge
                .Select(z => PhoenixModel.Commands.SimpleParser.ParseInt(z.Monat ?? string.Empty))
                .Where(m => m > 0)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
            Assert.NotEmpty(monate);

            foreach (int monat in monate) {
                var reihenfolge = ZugView.GetZugreihenfolge(monat);
                Assert.NotEmpty(reihenfolge);

                // keine Nation doppelt
                var reiche = reihenfolge.Select(z => z.Reich).ToList();
                Assert.Equal(reiche.Count, reiche.Distinct().Count());

                // aufsteigend sortiert
                for (int i = 1; i < reihenfolge.Count; i++)
                    Assert.True(reihenfolge[i].Reihenfolge >= reihenfolge[i - 1].Reihenfolge);
            }
        }

        /// <summary>
        /// Der Auftauchpunkt der Spielleitung steht in der Zugreihenfolge und muss ein Teleportfeld
        /// der Karte sein (Regelwerk 6.6.3).
        /// </summary>
        [StaFact]
        public void AuftauchpunkteSindTeleportfelder() {
            LadeAlles();
            Assert.NotNull(SharedData.Zugreihenfolge);

            var monate = SharedData.Zugreihenfolge
                .Select(z => PhoenixModel.Commands.SimpleParser.ParseInt(z.Monat ?? string.Empty))
                .Where(m => m > 0)
                .Distinct()
                .ToList();

            int geprüft = 0;
            foreach (int monat in monate) {
                var punkt = ZugView.GetAuftauchpunkt(monat);
                if (punkt == null)
                    continue;
                geprüft++;
                var art = TeleportRules.GetArt(punkt);
                Assert.True(art == TeleportArt.Erkenfara || art == TeleportArt.Tiefsee,
                    $"Der Auftauchpunkt {punkt.CreateBezeichner()} aus Monat {monat} ist kein Teleportfeld von Erkenfara, sondern {art}");
            }
            Assert.True(geprüft > 0, "Für keinen Monat war ein Auftauchpunkt eingetragen");
        }

        /// <summary>
        /// Die Phase kommt aus der settings-Tabelle der Zugdatenbank
        /// </summary>
        [StaFact]
        public void PhaseWirdGelesen() {
            LadeAlles();
            Assert.NotNull(ZugView.Settings);
            Assert.True(ZugView.Phase == Zugphase.Rüstphase || ZugView.Phase == Zugphase.Bewegungsphase);
            // Bauen ist nur in der Rüstphase erlaubt
            Assert.Equal(ZugView.Phase == Zugphase.Rüstphase, ProgramView.CanConstruct());
        }

        /// <summary>
        /// Der Phasenwechsel geht nur vorwärts, und nur die Spielleitung kann ihn erzwingen.
        /// Geschrieben wird dabei nichts - die StoreQueue wird im Testlauf nicht abgearbeitet.
        /// </summary>
        [StaFact]
        public void PhaseWechseltNurVorwaerts() {
            LadeAlles();
            var ausgangslage = ZugView.Phase;

            // in die Rüstphase zurück, damit der Test immer gleich startet
            Assert.False(ZugView.SetzePhase(Zugphase.Rüstphase, erzwingen: true).HasErrors);
            Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);
            Assert.True(ZugView.KannRüsten);
            Assert.False(ZugView.KannBewegen);
            Assert.True(ProgramView.CanConstruct());

            // Rüstphase beenden
            Assert.False(ZugView.NächstePhase().HasErrors);
            Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);
            Assert.False(ZugView.KannRüsten);
            Assert.True(ZugView.KannBewegen);
            Assert.False(ProgramView.CanConstruct());

            // weiter geht es nur über die Zugabgabe
            Assert.True(ZugView.NächstePhase().HasErrors);
            // und zurück nur mit Nachdruck
            Assert.True(ZugView.SetzePhase(Zugphase.Rüstphase).HasErrors);
            Assert.Equal(Zugphase.Bewegungsphase, ZugView.Phase);
            Assert.False(ZugView.SetzePhase(Zugphase.Rüstphase, erzwingen: true).HasErrors);

            // Ausgangslage wiederherstellen
            ZugView.SetzePhase(ausgangslage, erzwingen: true);
            Assert.Equal(ausgangslage, ZugView.Phase);
        }

        /// <summary>
        /// In der Rüstphase wird nicht bewegt - so wie in der Altanwendung, wo die Bewegungspfeile
        /// dort schlicht ausgeblendet waren.
        /// </summary>
        [StaFact]
        public void InDerRuestphaseWirdNichtBewegt() {
            LadeAlles();
            var ausgangslage = ZugView.Phase;

            // eine Figur, die sich in der Bewegungsphase auch tatsächlich bewegen kann
            ZugView.SetzePhase(Zugphase.Bewegungsphase, erzwingen: true);
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .FirstOrDefault(f => f.bp > 0 && BewegungsRules.GetMöglicheSchritte(f).Count > 0);
            Assert.True(figur != null, "In den Zugdaten gibt es keine Figur, die sich bewegen kann");
            Assert.NotEmpty(BewegungsRules.GetMöglicheSchritte(figur!));

            ZugView.SetzePhase(Zugphase.Rüstphase, erzwingen: true);
            Assert.Empty(BewegungsRules.GetMöglicheSchritte(figur!));
            Assert.True(BewegungsRules.PrüfeSchritt(figur!, Direction.NW).HasErrors);

            ZugView.SetzePhase(ausgangslage, erzwingen: true);
        }

        /// <summary>
        /// Der Monat steht im Verzeichnisnamen, in der settings-Tabelle und - als Historie - in der
        /// Schatzkammer. Massgeblich ist der Verzeichnisname; die Anwendung muss Widersprüche melden.
        /// </summary>
        [StaFact]
        public void DerZugmonatKommtAusDemVerzeichnis() {
            LadeAlles();
            Assert.True(ProgramView.SelectedMonth > 0);
            Assert.Equal(ProgramView.SelectedMonth, ZugView.AktuellerZug.Zug);

            // die Schatzkammer ist eine lückenlose Historie bis zum aktuellen Monat
            Assert.True(ZugView.LetzterSchatzkammerMonat > 0);
            Assert.Equal(ZugView.LetzterSchatzkammerMonat, ProgramView.SelectedMonth);
        }

        /// <summary>
        /// Ein abweichender Monat in der settings-Tabelle darf den Zugmonat nicht verstellen
        /// </summary>
        [StaFact]
        public void SettingsTabelleVerstelltDenZugmonatNicht() {
            LadeAlles();
            int erwartet = ZugView.LetzterSchatzkammerMonat;
            Assert.True(erwartet > 0);

            ZugView.BestimmeAktuellenZug(erwartet);
            Assert.Equal(erwartet, ProgramView.SelectedMonth);

            // ein falsches Verzeichnis fällt auf, weil die Schatzkammer nicht dazu passt
            Assert.False(ZugView.BestimmeAktuellenZug(erwartet + 7));
            Assert.Equal(erwartet + 7, ProgramView.SelectedMonth);

            // wieder geradeziehen, damit nachfolgende Tests sauber starten
            ZugView.BestimmeAktuellenZug(erwartet);
        }
    }
}
