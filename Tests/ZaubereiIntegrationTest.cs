using PhoenixModel.Commands;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Zauberei gegen die echten Spieldaten (Regelwerk 1.3 und 1.4).
    ///
    /// Geschrieben wird dabei nichts: die Befehle landen in der StoreQueue, die im Testlauf
    /// niemand leert. Die Zugdatenbank bleibt unangetastet.
    /// </summary>
    public class ZaubereiIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            // gezaubert wird im Spielzug, nicht in der Ruestphase
            if (ZugView.Settings != null)
                ZugView.Settings.Phase = (int)Zugphase.Bewegungsphase;
        }

        /// <summary>Alle Zauberer des eigenen Reiches, die auf der Karte stehen</summary>
        private static List<Zauberer> EigeneZauberer()
            => SharedData.Zauberer!
                .Where(z => Plausibilität.IsValid(z) && z.Nation == ProgramView.SelectedNation)
                .ToList();

        /// <summary>Ein Zauberer, der als einziger auf seiner Gemark steht und noch Zauberkraft hat</summary>
        private static Zauberer? FindeFreienZauberer(int mindestZauberkraft = 1) {
            foreach (var zauberer in EigeneZauberer().OrderByDescending(z => z.GP_akt)) {
                if (zauberer.GP_akt < mindestZauberkraft)
                    continue;
                if (ZaubereiRules.GetZaubererAufGemark(ZaubereiRules.GetStandort(zauberer)).Count == 1)
                    return zauberer;
            }
            return null;
        }

        /// <summary>Eine Gemark, auf der mehr als ein eigener Zauberer steht</summary>
        private static Zauberer? FindeGestapeltenZauberer() {
            foreach (var zauberer in EigeneZauberer()) {
                if (ZaubereiRules.GetZaubererAufGemark(ZaubereiRules.GetStandort(zauberer)).Count > 1)
                    return zauberer;
            }
            return null;
        }

        /// <summary>Ein Nachbarfeld des Zauberers, das auf der Karte liegt</summary>
        private static KleinfeldPosition Nachbarfeld(Zauberer zauberer) {
            var standort = ZaubereiRules.GetStandort(zauberer);
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KartenKoordinaten.GetNachbar(standort, richtung);
                if (nachbar != null && KleinfeldView.GetKleinfeld(nachbar) != null)
                    return nachbar;
            }
            throw new Xunit.Sdk.XunitException($"{standort.CreateBezeichner()} hat keine Nachbarn auf der Karte");
        }

        /// <summary>Eine Richtung, in der der Zauberer einen Nachbarn hat</summary>
        private static Direction RichtungMitNachbar(KleinfeldPosition feld) {
            foreach (Direction richtung in Enum.GetValues<Direction>()) {
                var nachbar = KartenKoordinaten.GetNachbar(feld, richtung);
                if (nachbar != null && KleinfeldView.GetKleinfeld(nachbar) != null)
                    return richtung;
            }
            throw new Xunit.Sdk.XunitException($"{feld.CreateBezeichner()} hat keine Nachbarn auf der Karte");
        }

        /// <summary>
        /// Die Klassengrenzen, die Teleportweiten und die Regeneration stehen in
        /// crossref_zauberer_teleport und muessen zur Tabelle in Regelwerk 1.3 passen.
        /// </summary>
        [StaFact]
        public void DieKlassentabelleDeranwendungPasstZumRegelwerk() {
            LadeAlles();
            var referenz = SharedData.Crossref_zauberer_teleport;
            Assert.NotNull(referenz);
            var klassen = referenz!.OrderBy(e => e.GP).ToList();
            Assert.Equal(6, klassen.Count);

            // die Regeneration verdoppelt sich von Klasse zu Klasse: 1, 2, 4, 8, 16, 32
            Assert.Equal([1, 2, 4, 8, 16, 32], klassen.Select(k => k.Regeneration_GP).ToArray());
            // die Teleportweite steigt von 6 auf 9 Gemarken
            Assert.Equal([6, 6, 6, 7, 8, 9], klassen.Select(k => k.Teleport).ToArray());
            Assert.Equal(["ZA", "ZB", "ZC", "ZD", "ZE", "ZF"], klassen.Select(k => k.ZX).ToArray());

            // und die Klassenstufe der Duellformel zaehlt sie in derselben Reihenfolge durch
            for (int i = 0; i < klassen.Count; i++)
                Assert.Equal(i + 1, ZaubereiRules.GetKlassenstufe(klassen[i].GP));
        }

        /// <summary>
        /// "Stehen mehrere Zauberer auf einer Gemark so koennen sie nicht zaubern."
        /// (Regelwerk 1.4) - in den echten Daten kommt das vor.
        /// </summary>
        [StaFact]
        public void MehrereZaubererAufEinerGemarkKoennenNichtZaubern() {
            LadeAlles();
            var zauberer = FindeGestapeltenZauberer();
            Assert.True(zauberer != null, "In den Zugdaten steht kein Feld mit mehreren Zauberern");

            var standort = ZaubereiRules.GetStandort(zauberer!);
            var geprüft = ZaubereiRules.PrüfeWand(zauberer, Zauberspruch.WandErrichten, standort,
                RichtungMitNachbar(standort), out _);
            Assert.True(geprüft.HasErrors);
            Assert.Contains("mehrere Zauberer", geprüft.Title);
        }

        /// <summary>
        /// "Haben die Zauberer alle die gleiche GP Zahl so sind diese neutralisiert, ansonsten wird
        /// die Differenz des maechtigsten Zauberers zu dem zweitmaechtigsten genommen."
        /// (Regelwerk 1.3)
        /// </summary>
        [StaFact]
        public void AufEinerGemarkNeutralisierenSichZaubererGegenseitig() {
            LadeAlles();
            var einer = FindeGestapeltenZauberer();
            Assert.True(einer != null, "In den Zugdaten steht kein Feld mit mehreren Zauberern");

            var stapel = ZaubereiRules.GetZaubererAufGemark(ZaubereiRules.GetStandort(einer!))
                .OrderByDescending(z => z.GP_ges).ToList();
            Assert.True(stapel.Count > 1);

            // nur dem staerksten bleibt etwas, und zwar hoechstens der Abstand zum zweitstaerksten
            int erwartet = Math.Max(0, Math.Min(stapel[0].GP_akt, stapel[0].GP_ges - stapel[1].GP_ges));
            Assert.Equal(erwartet, ZaubereiRules.GetWirksameZauberkraft(stapel[0]));
            foreach (var schwaecherer in stapel.Skip(1))
                Assert.Equal(0, ZaubereiRules.GetWirksameZauberkraft(schwaecherer));

            // ein allein stehender Zauberer ist nicht neutralisiert
            var allein = FindeFreienZauberer();
            if (allein != null) {
                Assert.False(ZaubereiRules.IstNeutralisiert(allein));
                Assert.Equal(allein.GP_akt, ZaubereiRules.GetWirksameZauberkraft(allein));
            }
        }

        /// <summary>
        /// Eine Wand auf dem eigenen Feld kostet 2 ZKP, und die werden vor dem Sprechen abgezogen
        /// (Regelwerk 1.4 und 1.4.2). Das Zuruecknehmen stellt den Stand davor wieder her.
        /// </summary>
        [StaFact]
        public void EineWandAufDemEigenenFeldKostetZweiZauberkraftpunkte() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");

            var standort = ZaubereiRules.GetStandort(zauberer!);
            var richtung = RichtungMitNachbar(standort);
            int kraftVorher = zauberer!.GP_akt;
            string magieVorher = zauberer.Befehl_magie;

            var befehl = new CastSpellCommando($"Zauberer {zauberer.Nummer} errichtet eine magische Wand") {
                Spell = Zauberspruch.WandErrichten,
                UnitID = zauberer.Nummer,
                LocationTo = standort,
                Richtung = richtung,
            };
            try {
                var geprüft = befehl.CheckPreconditions();
                Assert.False(geprüft.HasErrors, $"{geprüft.Title}: {geprüft.Message}");
                Assert.Equal(2, befehl.Kosten);

                var ausgeführt = befehl.ExecuteCommand();
                Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title}: {ausgeführt.Message}");
                Assert.Equal(kraftVorher - 2, zauberer.GP_akt);

                var gelesen = Zauberbefehl.LiesAlle(zauberer.Befehl_magie);
                Assert.Single(gelesen);
                Assert.Equal(Zauberspruch.WandErrichten, gelesen[0].Spruch);
                Assert.Equal(richtung, gelesen[0].Richtung);
                Assert.Equal(standort.CreateBezeichner(), gelesen[0].Ziel.CreateBezeichner());
            }
            finally {
                if (befehl.IsExecuted)
                    befehl.UndoCommand();
            }

            Assert.Equal(kraftVorher, zauberer!.GP_akt);
            Assert.Equal(magieVorher, zauberer.Befehl_magie);
        }

        /// <summary>
        /// "Zauberer koennen generell nur einen Zauberspruch pro Monat machen" (Regelwerk 1.4) -
        /// wer eine Wand gestellt hat, kann nicht ausserdem noch bannen.
        /// </summary>
        [StaFact]
        public void EinZaubererZaubertNurEinenSpruchProMonat() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(4);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");

            var standort = ZaubereiRules.GetStandort(zauberer!);
            var wand = new CastSpellCommando("Wand") {
                Spell = Zauberspruch.WandErrichten,
                UnitID = zauberer!.Nummer,
                LocationTo = standort,
                Richtung = RichtungMitNachbar(standort),
            };
            try {
                Assert.False(wand.ExecuteCommand().HasErrors);

                var geprüft = ZaubereiRules.PrüfeBann(zauberer, Nachbarfeld(zauberer), 500, out _);
                Assert.True(geprüft.HasErrors);
                Assert.Contains("schon gezaubert", geprüft.Title);
            }
            finally {
                if (wand.IsExecuted)
                    wand.UndoCommand();
            }

            // zurueckgenommen geht es wieder
            Assert.False(ZaubereiRules.PrüfeBann(zauberer, Nachbarfeld(zauberer), 500, out _).HasErrors);
        }

        /// <summary>
        /// "Jedem Zauberer ist es moeglich, in ein oder zwei Gemarken Entfernung gegnerische
        /// Truppen zu bannen." (Regelwerk 1.4.3) - auf das eigene Feld also nicht.
        /// </summary>
        [StaFact]
        public void AufDasEigeneFeldWirdNichtGebannt() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");

            var geprüft = ZaubereiRules.PrüfeBann(zauberer, ZaubereiRules.GetStandort(zauberer!), 500, out _);
            Assert.True(geprüft.HasErrors);
            Assert.Contains("eigene Feld", geprüft.Message);
        }

        /// <summary>
        /// Ein Bann auf das Nachbarfeld kostet 1 ZKP je angefangene 500 Raumpunkte. Reicht die
        /// Zauberkraft nicht, nennt die Meldung die Menge, die noch ginge.
        /// </summary>
        [StaFact]
        public void EinBannAufDasNachbarfeldKostetEinenPunktJeFuenfhundertRaumpunkte() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");
            var ziel = Nachbarfeld(zauberer!);

            var geprüft = ZaubereiRules.PrüfeBann(zauberer, ziel, 1000, out int kosten);
            Assert.False(geprüft.HasErrors, $"{geprüft.Title}: {geprüft.Message}");
            Assert.Equal(2, kosten);

            // mehr als die Zauberkraft hergibt
            int zuviel = (zauberer!.GP_akt + 1) * ZaubereiRules.RaumpunkteProZKPNah;
            var überfordert = ZaubereiRules.PrüfeBann(zauberer, ziel, zuviel, out _);
            Assert.True(überfordert.HasErrors);
            Assert.Contains($"höchstens {zauberer.GP_akt * ZaubereiRules.RaumpunkteProZKPNah} Raumpunkte", überfordert.Message);
        }

        /// <summary>
        /// Der ausgefuehrte Bann steht in der Spalte Befehl_bannt und zieht die Zauberkraft ab.
        /// </summary>
        [StaFact]
        public void EinBannLandetInDerSpalteBefehlBannt() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");
            var ziel = Nachbarfeld(zauberer!);
            int kraftVorher = zauberer!.GP_akt;

            var befehl = new CastSpellCommando($"Zauberer {zauberer.Nummer} bannt") {
                Spell = Zauberspruch.Bannen,
                UnitID = zauberer.Nummer,
                LocationTo = ziel,
                Raumpunkte = 1000,
            };
            try {
                Assert.False(befehl.ExecuteCommand().HasErrors);
                var gelesen = Bannbefehl.Lies(zauberer.Befehl_bannt);
                Assert.NotNull(gelesen);
                Assert.Equal(1000, gelesen!.Raumpunkte);
                Assert.Equal(ziel.CreateBezeichner(), gelesen.Ziel.CreateBezeichner());
                Assert.Equal(kraftVorher - 2, zauberer.GP_akt);
            }
            finally {
                if (befehl.IsExecuted)
                    befehl.UndoCommand();
            }

            Assert.Equal(kraftVorher, zauberer.GP_akt);
            Assert.True(string.IsNullOrEmpty(zauberer.Befehl_bannt));
        }

        /// <summary>
        /// Auf zwei Gemarken Entfernung wirkt erst ein Zauberer ab Klasse ZB (Regelwerk 1.4.2
        /// und 1.4.4). In den Zugdaten stehen ueberwiegend ZA.
        /// </summary>
        [StaFact]
        public void AufZweiGemarkenWirktErstEinZB() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(4);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");

            var standort = ZaubereiRules.GetStandort(zauberer!);
            var zweiWeiter = KleinfeldView.GetNachbarn(standort, ZaubereiRules.MaxEntfernungWand, false)?
                .FirstOrDefault(k => ZaubereiRules.GetEntfernung(zauberer!, k, ZaubereiRules.MaxEntfernungWand) == 2);
            Assert.True(zweiWeiter != null, $"Um {standort.CreateBezeichner()} liegt kein Feld in zwei Gemarken Entfernung");
            var ziel = new KleinfeldPosition(zweiWeiter!.gf, zweiWeiter.kf);
            var richtung = RichtungMitNachbar(ziel);

            // Die Klasse ergibt sich aus dem Gutpunktwert. Um beide Seiten der Grenze zu sehen,
            // wird der Zauberer voruebergehend herabgestuft und danach wiederhergestellt.
            int gesVorher = zauberer!.GP_ges;
            int aktVorher = zauberer.GP_akt;
            try {
                zauberer.GP_ges = 4;
                zauberer.GP_akt = 4;
                Assert.Equal(Zaubererklasse.ZA, zauberer.Klasse);
                var alsZA = ZaubereiRules.PrüfeWand(zauberer, Zauberspruch.WandErrichten, ziel, richtung, out _);
                Assert.True(alsZA.HasErrors);
                Assert.Contains("nicht so weit", alsZA.Title);

                zauberer.GP_ges = 12;
                zauberer.GP_akt = 12;
                Assert.Equal(Zaubererklasse.ZB, zauberer.Klasse);
                var alsZB = ZaubereiRules.PrüfeWand(zauberer, Zauberspruch.WandErrichten, ziel, richtung, out int kosten);
                Assert.False(alsZB.HasErrors, $"{alsZB.Title}: {alsZB.Message}");
                Assert.Equal(4, kosten);
            }
            finally {
                zauberer.GP_ges = gesVorher;
                zauberer.GP_akt = aktVorher;
            }
        }

        /// <summary>
        /// Die Teleportation mit Ruestguetern beherrschen erst Zauberer ab Klasse ZE
        /// (Regelwerk 1.4.1, Errata 33).
        /// </summary>
        [StaFact]
        public void TeleportMitRuestguetternErstAbKlasseZE() {
            LadeAlles();
            var zauberer = FindeFreienZauberer();
            Assert.True(zauberer != null, "Kein allein stehender Zauberer in den Zugdaten");
            var mitfahrer = SpielfigurenView.GetSpielfiguren(ZaubereiRules.GetStandort(zauberer!))
                .OfType<TruppenSpielfigur>().Take(1).Cast<Spielfigur>().ToList();
            var ziel = Nachbarfeld(zauberer!);

            int gesVorher = zauberer!.GP_ges;
            int aktVorher = zauberer.GP_akt;
            try {
                zauberer.GP_ges = 12;   // ZB
                zauberer.GP_akt = 12;
                var alsZB = ZaubereiRules.PrüfeTeleportMitRüstgütern(zauberer, ziel, mitfahrer, out _);
                Assert.True(alsZB.HasErrors);
                Assert.Contains("beherrscht diesen Spruch nicht", alsZB.Title);

                zauberer.GP_ges = 100;  // ZE
                zauberer.GP_akt = 100;
                Assert.Equal(Zaubererklasse.ZE, zauberer.Klasse);
                var alsZE = ZaubereiRules.PrüfeTeleportMitRüstgütern(zauberer, ziel, mitfahrer, out _);
                Assert.DoesNotContain("beherrscht diesen Spruch nicht", alsZE.Title);
            }
            finally {
                zauberer.GP_ges = gesVorher;
                zauberer.GP_akt = aktVorher;
            }
        }

        /// <summary>
        /// In der Ruestphase wird nicht gezaubert - die Spruechephasen liegen im Spielzug.
        /// </summary>
        [StaFact]
        public void InDerRuestphaseWirdNichtGezaubert() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");
            var standort = ZaubereiRules.GetStandort(zauberer!);

            ZugView.Settings!.Phase = (int)Zugphase.Rüstphase;
            try {
                var geprüft = ZaubereiRules.PrüfeWand(zauberer, Zauberspruch.WandErrichten, standort,
                    RichtungMitNachbar(standort), out _);
                Assert.True(geprüft.HasErrors);
                Assert.Contains("wird nicht gezaubert", geprüft.Title);
            }
            finally {
                ZugView.Settings.Phase = (int)Zugphase.Bewegungsphase;
            }
        }

        /// <summary>
        /// Eine Forderung zum Zauberduell kostet keine Zauberkraft und steht neben einem
        /// Zauberspruch - sie ist keiner (Regelwerk 5.3).
        /// </summary>
        [StaFact]
        public void EineForderungZumDuellKostetKeineZauberkraft() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");
            int kraftVorher = zauberer!.GP_akt;

            var befehl = new CastSpellCommando($"Zauberer {zauberer.Nummer} fordert") {
                Spell = Zauberspruch.Zauberduell,
                UnitID = zauberer.Nummer,
                LocationTo = Nachbarfeld(zauberer),
            };
            try {
                Assert.False(befehl.ExecuteCommand().HasErrors);
                Assert.Equal(kraftVorher, zauberer.GP_akt);
                Assert.Equal(Zauberspruch.Keiner, ZaubereiRules.GetLaufendenSpruch(zauberer));

                // und ein Zauberspruch geht danach immer noch
                var standort = ZaubereiRules.GetStandort(zauberer);
                Assert.False(ZaubereiRules.PrüfeWand(zauberer, Zauberspruch.WandErrichten, standort,
                    RichtungMitNachbar(standort), out _).HasErrors);
            }
            finally {
                if (befehl.IsExecuted)
                    befehl.UndoCommand();
            }
            Assert.True(string.IsNullOrEmpty(zauberer.Befehl_magie));
        }

        /// <summary>
        /// Der Teleport eines Zauberers ohne Ladung ist eine Bewegungsfertigkeit: er kostet keine
        /// Zauberkraft, sondern Teleportpunkte, und laesst den Zauberspruch des Monats frei
        /// (Regelwerk 1.3).
        /// </summary>
        [StaFact]
        public void EinTeleportKostetTeleportpunkteUndKeineZauberkraft() {
            LadeAlles();
            var zauberer = FindeFreienZauberer(2);
            Assert.True(zauberer != null, "Kein allein stehender Zauberer mit genug Zauberkraft in den Zugdaten");

            var start = ZaubereiRules.GetStandort(zauberer!);
            var ziel = Nachbarfeld(zauberer!);
            int kraftVorher = zauberer!.GP_akt;
            int punkteVorher = zauberer.tp;

            var befehl = new TeleportCommand($"Zauberer {zauberer.Nummer} teleportiert") {
                UnitID = zauberer.Nummer,
                LocationTo = ziel,
            };
            try {
                var ausgeführt = befehl.ExecuteCommand();
                Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title}: {ausgeführt.Message}");
                Assert.Equal(1, befehl.Entfernung);
                Assert.Equal(kraftVorher, zauberer.GP_akt);
                Assert.Equal(punkteVorher + 1, zauberer.tp);
                Assert.Equal(ziel.CreateBezeichner(), ZaubereiRules.GetStandort(zauberer).CreateBezeichner());

                // und der Zauberspruch des Monats ist noch frei
                Assert.Equal(Zauberspruch.Keiner, ZaubereiRules.GetLaufendenSpruch(zauberer));
            }
            finally {
                if (befehl.IsExecuted)
                    befehl.UndoCommand();
            }

            Assert.Equal(kraftVorher, zauberer.GP_akt);
            Assert.Equal(punkteVorher, zauberer.tp);
            Assert.Equal(start.CreateBezeichner(), ZaubereiRules.GetStandort(zauberer).CreateBezeichner());
            Assert.True(string.IsNullOrEmpty(zauberer.Befehl_Teleport));
        }

        /// <summary>
        /// Die Teleportweite ist ein Monatsbudget: ist sie aufgebraucht, geht nichts mehr
        /// (Regelwerk 1.3, Tabelle in crossref_zauberer_teleport).
        /// </summary>
        [StaFact]
        public void DieTeleportweiteIstEinMonatsbudget() {
            LadeAlles();
            var zauberer = FindeFreienZauberer();
            Assert.True(zauberer != null, "Kein allein stehender Zauberer in den Zugdaten");
            var ziel = Nachbarfeld(zauberer!);
            int punkteVorher = zauberer!.tp;
            try {
                zauberer.tp = zauberer.MaxTeleportPunkte;
                var geprüft = ZaubereiRules.PrüfeTeleport(zauberer, ziel, out _);
                Assert.True(geprüft.HasErrors);
                Assert.Contains("Teleportweite verbraucht", geprüft.Title);

                // mit einem Punkt Rest reicht es genau bis zum Nachbarfeld
                zauberer.tp = zauberer.MaxTeleportPunkte - 1;
                Assert.False(ZaubereiRules.PrüfeTeleport(zauberer, ziel, out int entfernung).HasErrors);
                Assert.Equal(1, entfernung);
            }
            finally {
                zauberer.tp = punkteVorher;
            }
        }

        /// <summary>
        /// Ein Duell kommt nur mit Zauberern in der gleichen oder einer benachbarten Gemark
        /// zustande (Regelwerk 5.3).
        /// </summary>
        [StaFact]
        public void AufZweiGemarkenGibtEsKeinDuell() {
            LadeAlles();
            var zauberer = FindeFreienZauberer();
            Assert.True(zauberer != null, "Kein allein stehender Zauberer in den Zugdaten");
            var standort = ZaubereiRules.GetStandort(zauberer!);

            var zweiWeiter = KleinfeldView.GetNachbarn(standort, 2, false)?
                .FirstOrDefault(k => ZaubereiRules.GetEntfernung(zauberer!, k, 2) == 2);
            Assert.True(zweiWeiter != null, $"Um {standort.CreateBezeichner()} liegt kein Feld in zwei Gemarken Entfernung");

            var geprüft = ZaubereiRules.PrüfeDuell(zauberer, new KleinfeldPosition(zweiWeiter!.gf, zweiWeiter.kf));
            Assert.True(geprüft.HasErrors);
            Assert.Contains("zu weit weg", geprüft.Title);
        }
    }
}
