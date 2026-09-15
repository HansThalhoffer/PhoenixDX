using PhoenixModel.Commands;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Umladen zwischen Heeren und Buchungen gegen den Reichsschatz (Regelwerk 1.8, 3.2.3 und 6.6).
    ///
    /// Der Weg in den Reichsschatz ist ueberall moeglich - Kampfeinnahmen und Pluendergut "koennen
    /// sofort delokalisiert werden". Der Weg heraus nur ueber einen Ruestort.
    /// </summary>
    public class VerschiebenIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        private static List<TruppenSpielfigur> Armee()
            => SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>().ToList();

        /// <summary>Zwei eigene Heere auf demselben Feld</summary>
        private static (TruppenSpielfigur quelle, TruppenSpielfigur ziel)? FindePaarAufEinemFeld() {
            var armee = Armee();
            foreach (var eine in armee) {
                var standort = HeeresRules.GetStandort(eine);
                var andere = armee.FirstOrDefault(t => t.Nummer != eine.Nummer
                                                    && HeeresRules.GetStandort(t).Equals(standort));
                if (andere != null)
                    return (eine, andere);
            }
            return null;
        }

        [StaFact]
        public void UmgeladenWirdNurAufDerselbenGemark() {
            LadeAlles();
            var armee = Armee();
            var eine = armee.FirstOrDefault();
            Assert.True(eine != null, "In den Zugdaten steht keine Truppe");
            var standort = HeeresRules.GetStandort(eine!);
            var entfernt = armee.FirstOrDefault(t => HeeresRules.GetStandort(t).Equals(standort) == false);
            Assert.True(entfernt != null, "Alle Truppen stehen auf demselben Feld");

            var ergebnis = VerschiebeRules.PrüfeZwischenHeeren(eine, entfernt, Verschiebbar.Heerführer, 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("derselben Gemark", ergebnis.Title);
        }

        [StaFact]
        public void EinHeerLaedtNichtBeiSichSelbstUm() {
            LadeAlles();
            var eine = Armee().FirstOrDefault();
            Assert.True(eine != null, "In den Zugdaten steht keine Truppe");

            var ergebnis = VerschiebeRules.PrüfeZwischenHeeren(eine, eine, Verschiebbar.Gold, 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("sich selbst", ergebnis.Title);
        }

        [StaFact]
        public void MehrAlsVorhandenWirdAbgelehnt() {
            LadeAlles();
            var paar = FindePaarAufEinemFeld();
            if (paar == null)
                return; // in diesem Zug steht kein Paar auf einem Feld

            var (quelle, ziel) = paar.Value;
            int bestand = VerschiebeRules.GetBestand(quelle, Verschiebbar.Pferde);
            var ergebnis = VerschiebeRules.PrüfeZwischenHeeren(quelle, ziel, Verschiebbar.Pferde, bestand + 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("nicht", ergebnis.Title);
        }

        /// <summary>
        /// "Heerfuehrer duerfen ... verschoben werden, wenn das abgebende Heer sich in diesem Monat
        /// noch nicht bewegt hat" (Errata 17 zu Regelwerk 1.8)
        /// </summary>
        [StaFact]
        public void EinBewegtesHeerGibtKeineHeerfuehrerAb() {
            LadeAlles();
            var paar = FindePaarAufEinemFeld();
            if (paar == null)
                return;

            var (quelle, ziel) = paar.Value;
            var standort = HeeresRules.GetStandort(quelle);
            int gfNachVorher = quelle.gf_nach;
            int kfNachVorher = quelle.kf_nach;
            int kfVonVorher = quelle.kf_von;
            try {
                // Die Truppe soll als bewegt gelten, aber am selben Ort stehen bleiben: der
                // Standort ist gf_nach, also wird nur die Herkunft verschoben.
                quelle.gf_nach = standort.gf;
                quelle.kf_nach = standort.kf;
                quelle.kf_von = standort.kf + 1;
                Assert.True(VerschiebeRules.HatSichBewegt(quelle));
                Assert.Equal(standort, HeeresRules.GetStandort(quelle));

                var ergebnis = VerschiebeRules.PrüfeZwischenHeeren(quelle, ziel, Verschiebbar.Heerführer, 1);
                Assert.True(ergebnis.HasErrors);
                Assert.Contains("bewegt", ergebnis.Title);
            }
            finally {
                quelle.gf_nach = gfNachVorher;
                quelle.kf_nach = kfNachVorher;
                quelle.kf_von = kfVonVorher;
            }
        }

        /// <summary>
        /// "Soll Gold aus dem Reichsschatz wieder einer Einheit zugeordnet werden so kann dies nur
        /// ueber einen Ruestort oder Audvacar geschehen." (Regelwerk 6.6)
        /// </summary>
        [StaFact]
        public void AusDemReichsschatzNurUeberEinenRuestort() {
            LadeAlles();
            var ohneRuestort = Armee().FirstOrDefault(t => VerschiebeRules.GetEigenenRüstort(t) == null);
            Assert.True(ohneRuestort != null, "Alle Truppen stehen auf einem eigenen Ruestort");

            var ergebnis = VerschiebeRules.PrüfeVomReichsschatz(ohneRuestort, 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("Rüstort", ergebnis.Title);
        }

        /// <summary>
        /// In den Reichsschatz geht es dagegen ueberall - anders als in der Altanwendung, die auch
        /// dafuer einen Ruestort verlangte.
        /// </summary>
        [StaFact]
        public void InDenReichsschatzGehtEsAuchOhneRuestort() {
            LadeAlles();
            var mitGeld = Armee().FirstOrDefault(t => t.GS > 0 && VerschiebeRules.GetEigenenRüstort(t) == null);
            if (mitGeld == null)
                return; // in diesem Zug traegt keine Truppe abseits eines Ruestortes Gold

            var ergebnis = VerschiebeRules.PrüfeZumReichsschatz(mitGeld, Verschiebbar.Gold, 1);
            Assert.False(ergebnis.HasErrors, $"{ergebnis.Title}: {ergebnis.Message}");
        }

        [StaFact]
        public void NurGeldWandertInDenReichsschatz() {
            LadeAlles();
            var truppe = Armee().FirstOrDefault(t => t.Pferde > 0);
            if (truppe == null)
                return;

            var ergebnis = VerschiebeRules.PrüfeZumReichsschatz(truppe, Verschiebbar.Pferde, 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("Reichsschatz", ergebnis.Title);
        }

        /// <summary>
        /// Der Durchlauf: umladen, nachsehen, zuruecknehmen.
        /// </summary>
        [StaFact]
        public void UmladenWirdSauberZurueckgenommen() {
            LadeAlles();
            var paar = FindePaarAufEinemFeld();
            if (paar == null)
                return;

            var (quelle, ziel) = paar.Value;
            // eine Sorte, von der die Quelle wirklich etwas hat
            var sorten = new[] { Verschiebbar.Pferde, Verschiebbar.Gold, Verschiebbar.Kampfeinnahmen,
                                 Verschiebbar.LeichteKatapulte, Verschiebbar.SchwereKatapulte };
            var was = sorten.FirstOrDefault(s => VerschiebeRules.GetBestand(quelle, s) > 0);
            if (VerschiebeRules.GetBestand(quelle, was) == 0)
                return; // die Quelle hat nichts zu verschieben

            int quelleVorher = VerschiebeRules.GetBestand(quelle, was);
            int zielVorher = VerschiebeRules.GetBestand(ziel, was);

            var befehl = new ShareCommand("Testverschiebung") {
                Mode = ShareCommand.Modus.zwischenHeeren,
                Quelle = quelle,
                Ziel = ziel,
                Was = was,
                Menge = 1,
            };

            var ausgeführt = befehl.ExecuteCommand();
            try {
                Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title}: {ausgeführt.Message}");
                Assert.Equal(quelleVorher - 1, VerschiebeRules.GetBestand(quelle, was));
                Assert.Equal(zielVorher + 1, VerschiebeRules.GetBestand(ziel, was));
            }
            finally {
                var zurück = befehl.UndoCommand();
                Assert.False(zurück.HasErrors, $"{zurück.Title} {zurück.Message}");
            }

            Assert.Equal(quelleVorher, VerschiebeRules.GetBestand(quelle, was));
            Assert.Equal(zielVorher, VerschiebeRules.GetBestand(ziel, was));
        }

        [StaFact]
        public void ParserLiestDieVerschiebebefehle() {
            var parser = new ShareCommandParser();

            Assert.True(parser.ParseCommand("Verschiebe 100 Pferde von Krieger 101 zu Krieger 103", out var pferde));
            var p = Assert.IsType<ShareCommand>(pferde);
            Assert.Equal(ShareCommand.Modus.zwischenHeeren, p.Mode);
            Assert.Equal(Verschiebbar.Pferde, p.Was);
            Assert.Equal(100, p.Menge);
            Assert.Equal(101, p.QuelleUnitId);
            Assert.Equal(103, p.ZielUnitId);

            Assert.True(parser.ParseCommand("Verschiebe 2 Heerführer von Krieger 101 zu Krieger 103", out var hf));
            Assert.Equal(Verschiebbar.Heerführer, Assert.IsType<ShareCommand>(hf).Was);

            Assert.True(parser.ParseCommand("Verschiebe 4 leichte Katapulte von Krieger 101 zu Krieger 103", out var lkp));
            Assert.Equal(Verschiebbar.LeichteKatapulte, Assert.IsType<ShareCommand>(lkp).Was);

            Assert.True(parser.ParseCommand("Verschiebe 2000 Gold von Krieger 101 in den Reichsschatz", out var rein));
            Assert.Equal(ShareCommand.Modus.inDenReichsschatz, Assert.IsType<ShareCommand>(rein).Mode);

            Assert.True(parser.ParseCommand("Verschiebe 2000 Gold aus dem Reichsschatz zu Krieger 101", out var raus));
            var r = Assert.IsType<ShareCommand>(raus);
            Assert.Equal(ShareCommand.Modus.ausDemReichsschatz, r.Mode);
            Assert.Equal(101, r.ZielUnitId);

            Assert.False(parser.ParseCommand("Verschiebe viele Pferde von Krieger 101 zu Krieger 103", out _));
        }

        /// <summary>
        /// Pluendern statt Erobern war bisher nicht erreichbar: der Modus stand fest auf Erobern.
        /// </summary>
        [StaFact]
        public void DerBewegungsbefehlKannAuchPluendern() {
            var parser = new MoveCommandParser();

            Assert.True(parser.ParseCommand("Bewege Reiter 220 nach 504/07", out var normal));
            Assert.Equal(EroberungsModus.Erobern, Assert.IsType<MoveCommand>(normal).Eroberung);

            Assert.True(parser.ParseCommand("Bewege Reiter 220 nach 504/07 und plündere", out var pluendern));
            Assert.Equal(EroberungsModus.Plündern, Assert.IsType<MoveCommand>(pluendern).Eroberung);

            Assert.True(parser.ParseCommand("Bewege Reiter 220 nach 504/07 via 503/21, 503/16 und plündere", out var mitVia));
            var via = Assert.IsType<MoveCommand>(mitVia);
            Assert.Equal(EroberungsModus.Plündern, via.Eroberung);
            Assert.NotNull(via.ViaLocations);
            Assert.Equal(2, via.ViaLocations!.Count);
        }
    }
}
