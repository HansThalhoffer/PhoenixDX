using PhoenixModel.Commands;
using PhoenixModel.ExternalTables;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Auf- und Absitzen gegen die echten Zugdaten (Regelwerk 1.1.2 und 1.1.3).
    ///
    /// Ein Reittier traegt genau einen Krieger, Auf- und Absitzen gehen also eins zu eins.
    /// Eingeschiffte Truppen koennen weder auf- noch absitzen.
    /// </summary>
    public class ReitenIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        private static List<TruppenSpielfigur> Armee()
            => SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).OfType<TruppenSpielfigur>().ToList();

        /// <summary>
        /// Ein Kriegerheer mit Reittieren am Zuegel gibt es in den echten Zugdaten nicht - Pferde
        /// sind dort immer schon unter Reitern. Ein solches Heer entsteht erst durch Absitzen,
        /// weshalb die Tests zum Aufsitzen es sich auf diesem Weg beschaffen.
        /// </summary>
        private static TruppenSpielfigur? FindeAufsitzer()
            => Armee().FirstOrDefault(t => t.BaseTyp == FigurType.Krieger && t.Pferde > 0 && t.hf >= 2
                                        && SchifffahrtsRules.IstEingeschifft(t) == false);

        /// <summary>Ein Reiterheer, von dem ein Teil absitzen kann</summary>
        private static TruppenSpielfigur? FindeAbsitzer()
            => Armee().FirstOrDefault(t => t.BaseTyp == FigurType.Reiter && t.staerke >= 2 && t.hf >= 2
                                        && SchifffahrtsRules.IstEingeschifft(t) == false);

        private static int ZaehleGattung(FigurType typ)
            => Armee().Count(t => t.BaseTyp == typ);

        [StaFact]
        public void EinKriegerheerOhneReittiereSitztNichtAuf() {
            LadeAlles();
            var ohnePferde = Armee().FirstOrDefault(t => t.BaseTyp == FigurType.Krieger && t.Pferde == 0 && t.hf >= 1
                                                      && SchifffahrtsRules.IstEingeschifft(t) == false);
            Assert.True(ohnePferde != null, "In den Zugdaten steht kein Kriegerheer ohne Reittiere");

            var ergebnis = ReitRules.PrüfeAufsitzen(ohnePferde, 1, 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("Reittiere", ergebnis.Title);
        }

        [StaFact]
        public void EinReiterheerSitztNichtAuf() {
            LadeAlles();
            var reiter = Armee().FirstOrDefault(t => t.BaseTyp == FigurType.Reiter);
            Assert.True(reiter != null, "In den Zugdaten steht kein Reiterheer");

            var ergebnis = ReitRules.PrüfeAufsitzen(reiter, 1, 1);
            Assert.True(ergebnis.HasErrors);
            Assert.Contains("kein Kriegerheer", ergebnis.Title);
        }

        /// <summary>
        /// "Eingeschiffte Truppen koennen nicht auf und / oder absitzen." (Regelwerk 1.1.3)
        /// </summary>
        [StaFact]
        public void EingeschiffteTruppenSitzenNichtAb() {
            LadeAlles();
            var heer = FindeAbsitzer();
            Assert.True(heer != null, "In den Zugdaten steht kein absitzfaehiges Reiterheer");

            string? vorher = heer!.auf_Flotte;
            try {
                heer.auf_Flotte = "305";
                var ergebnis = ReitRules.PrüfeAbsitzen(heer, 1, 1);
                Assert.True(ergebnis.HasErrors);
                Assert.Contains("eingeschifft", ergebnis.Title);
                Assert.Contains("Regelwerk 1.1.3", ergebnis.Message);
            }
            finally {
                heer.auf_Flotte = vorher ?? string.Empty;
            }
        }

        [StaFact]
        public void DasNeueHeerBrauchtEinenHeerfuehrer() {
            LadeAlles();
            var heer = FindeAbsitzer();
            Assert.True(heer != null, "In den Zugdaten steht kein absitzfaehiges Reiterheer");

            var ohne = ReitRules.PrüfeAbsitzen(heer, 1, 0);
            Assert.True(ohne.HasErrors);
            Assert.Contains("Heerführer", ohne.Title);
        }

        /// <summary>
        /// Sitzt nur ein Teil auf, braucht auch der Rest einen Heerfuehrer.
        /// </summary>
        [StaFact]
        public void DerRestBrauchtEbenfallsEinenHeerfuehrer() {
            LadeAlles();
            var heer = FindeAbsitzer();
            Assert.True(heer != null, "In den Zugdaten steht kein absitzfaehiges Reiterheer");

            var alleHf = ReitRules.PrüfeAbsitzen(heer, 1, heer!.hf);
            Assert.True(alleHf.HasErrors);
            Assert.Contains("ohne Heerführer", alleHf.Title);
        }

        /// <summary>
        /// Absitzen und wieder aufsitzen muss den Ausgangszustand ergeben.
        ///
        /// In den echten Zugdaten fuehrt kein Kriegerheer Reittiere am Zuegel - ein aufsitzfaehiges
        /// Heer entsteht erst durch Absitzen. Der Test beschafft es sich so und prueft damit beide
        /// Richtungen gegeneinander.
        /// </summary>
        [StaFact]
        public void AbsitzenUndWiederAufsitzenErgibtDenAusgangszustand() {
            LadeAlles();
            var reiterheer = FindeAbsitzer();
            Assert.True(reiterheer != null, "In den Zugdaten steht kein absitzfaehiges Reiterheer");

            int stärkeVorher = reiterheer!.staerke;
            int hfVorher = reiterheer.hf;
            int reiterVorher = ZaehleGattung(FigurType.Reiter);
            int kriegerVorher = ZaehleGattung(FigurType.Krieger);

            var absitzen = new MountCommand("Testabsitzen") {
                Mode = MountCommand.Modus.absitzen,
                Heer = reiterheer,
                Anzahl = 2,
                Heerführer = 1,
            };
            var abgesessen = absitzen.ExecuteCommand();
            Assert.False(abgesessen.HasErrors, $"{abgesessen.Title}: {abgesessen.Message}");

            var kriegerheer = absitzen.Entstanden!;
            Assert.Equal(2, kriegerheer.Pferde);
            Assert.Equal(kriegerVorher + 1, ZaehleGattung(FigurType.Krieger));

            // dieses Kriegerheer fuehrt jetzt Reittiere und kann damit wieder aufsitzen
            var aufsitzen = new MountCommand("Testaufsitzen") {
                Mode = MountCommand.Modus.aufsitzen,
                Heer = kriegerheer,
                Anzahl = 2,
                Heerführer = 1,
            };
            var aufgesessen = aufsitzen.ExecuteCommand();
            try {
                Assert.False(aufgesessen.HasErrors, $"{aufgesessen.Title}: {aufgesessen.Message}");
                var neueReiter = aufsitzen.Entstanden!;

                Assert.Equal(FigurType.Reiter, neueReiter.BaseTyp);
                Assert.Equal(2, neueReiter.staerke);
                // die Reittiere sind jetzt unter den Reitern, nicht mehr am Zuegel
                Assert.Equal(0, neueReiter.Pferde);
                Assert.Equal(0, kriegerheer.staerke);
                Assert.Equal(0, kriegerheer.Pferde);

                // ein Reiter ist schneller als ein Krieger
                Assert.Equal(BewegungsRules.BerechneBewegungspunkte(FigurType.Reiter), neueReiter.bp_max);
                Assert.True(neueReiter.bp_max > BewegungsRules.BerechneBewegungspunkte(FigurType.Krieger));

                // die beiden Heere verweisen aufeinander
                Assert.Contains($"#AGV:{kriegerheer.Nummer}", neueReiter.Befehl_bew);
                Assert.Contains($"#AGZ:{neueReiter.Nummer}", kriegerheer.Befehl_bew);
            }
            finally {
                aufsitzen.UndoCommand();
                absitzen.UndoCommand();
            }

            Assert.Equal(stärkeVorher, reiterheer.staerke);
            Assert.Equal(hfVorher, reiterheer.hf);
            Assert.Equal(reiterVorher, ZaehleGattung(FigurType.Reiter));
            Assert.Equal(kriegerVorher, ZaehleGattung(FigurType.Krieger));
        }

        /// <summary>
        /// "Ein Reiter, der absitzt, wird zu einem Krieger mit einem Reittier am Zuegel"
        /// - aus einem Reiter werden also ein Krieger und ein Reittier.
        /// </summary>
        [StaFact]
        public void BeimAbsitzenKommtDasReittierAnDenZuegel() {
            LadeAlles();
            var heer = FindeAbsitzer();
            Assert.True(heer != null, "In den Zugdaten steht kein absitzfaehiges Reiterheer");

            int stärkeVorher = heer!.staerke;
            int kriegerVorher = ZaehleGattung(FigurType.Krieger);

            var befehl = new MountCommand("Testabsitzen") {
                Mode = MountCommand.Modus.absitzen,
                Heer = heer,
                Anzahl = 1,
                Heerführer = 1,
            };

            var ausgeführt = befehl.ExecuteCommand();
            try {
                Assert.False(ausgeführt.HasErrors, $"{ausgeführt.Title}: {ausgeführt.Message}");
                var neu = befehl.Entstanden!;

                Assert.Equal(FigurType.Krieger, neu.BaseTyp);
                Assert.Equal(1, neu.staerke);
                Assert.Equal(1, neu.Pferde);
                Assert.Equal(stärkeVorher - 1, heer.staerke);
                Assert.Equal(kriegerVorher + 1, ZaehleGattung(FigurType.Krieger));

                // der Vermerk zeigt in beide Richtungen
                Assert.Contains($"#AGV:{neu.Nummer}", heer.Befehl_bew);
                Assert.Contains($"#AGZ:{heer.Nummer}", neu.Befehl_bew);
            }
            finally {
                befehl.UndoCommand();
            }

            Assert.Equal(stärkeVorher, heer.staerke);
            Assert.Equal(kriegerVorher, ZaehleGattung(FigurType.Krieger));
        }

        [StaFact]
        public void ParserLiestAufUndAbsitzen() {
            var parser = new MountCommandParser();

            Assert.True(parser.ParseCommand("Sitze auf mit 300 Kriegern von Krieger 102 und 2 Heerführern", out var auf));
            var aufsitzen = Assert.IsType<MountCommand>(auf);
            Assert.Equal(MountCommand.Modus.aufsitzen, aufsitzen.Mode);
            Assert.Equal(300, aufsitzen.Anzahl);
            Assert.Equal(102, aufsitzen.UnitId);
            Assert.Equal(2, aufsitzen.Heerführer);

            Assert.True(parser.ParseCommand("Sitze ab mit 50 Reitern von Reiter 205 und 1 Heerführer", out var ab));
            var absitzen = Assert.IsType<MountCommand>(ab);
            Assert.Equal(MountCommand.Modus.absitzen, absitzen.Mode);
            Assert.Equal(50, absitzen.Anzahl);

            Assert.False(parser.ParseCommand("Sitze auf mit allen Kriegern", out _));
        }
    }
}
