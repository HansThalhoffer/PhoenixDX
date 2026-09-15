using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Ein Bauauftrag muss in den Zugdaten landen - und er tut es, auf einem Weg, der zweimal
    /// hinsehen verlangt.
    ///
    /// In der Speicherschlange steht er als "Update" und nicht als "Insert". Das sieht nach einem
    /// vergessenen Einfuegen aus, ist aber richtig: RuestungBauwerke.Save setzt ein UPDATE ueber
    /// die ID ab und legt die Zeile an, wenn keine getroffen wurde - und eine neue Zeile hat die
    /// ID 0. Diese Tests halten das fest, damit niemand ein zweites Einfuegen danebensetzt.
    ///
    /// Geschrieben wird hier nichts: die Speicherschlange wird im Testlauf nicht geleert. Geprueft
    /// wird, dass der Auftrag darin steht.
    /// </summary>
    public class BauauftragTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Rüstphase);
        }

        /// <summary>
        /// Sucht eine eigene Gemark, auf der sich eine Strasse bauen laesst, und die Richtung dazu.
        /// </summary>
        private static (KleinFeld Feld, Direction Richtung) FindeStrassenbau() {
            foreach (var feld in SharedData.Map!.Values) {
                if (feld.Nation == null || feld.Nation.Equals(ProgramView.SelectedNation) == false)
                    continue;
                var option = BauoptionenView.Bestimme(feld)
                    .FirstOrDefault(o => o.Art == ConstructionElementType.Strasse && o.Möglich);
                if (option?.Richtung != null)
                    return (feld, option.Richtung.Value);
            }
            Assert.Fail("Auf keiner eigenen Gemark laesst sich eine Strasse bauen");
            return default;
        }

        /// <summary>
        /// Die Auftraege, die seit dem Merkpunkt in der Speicherschlange gelandet sind.
        /// </summary>
        private static List<DatabaseQueue.DatabaseQueueItem> Neu(int abIndex)
            => [.. SharedData.StoreQueue.Skip(abIndex)];

        /// <summary>
        /// Der Bauauftrag wird angelegt - und genau einmal zum Schreiben vorgemerkt.
        /// </summary>
        [StaFact]
        public void DerBauauftragWirdZumSchreibenVorgemerkt() {
            LadeAlles();
            var (feld, richtung) = FindeStrassenbau();
            int vorher = SharedData.StoreQueue.Count;
            int auftraegeVorher = SharedData.RuestungBauwerke!.Count;

            Assert.True(CommandParser.ParseCommand(
                $"Errichte Straße im {richtung} von {feld.CreateBezeichner()}", out var befehl));
            var bau = Assert.IsType<ConstructCommand>(befehl);
            try {
                Assert.False(bau.ExecuteCommand().HasErrors);

                // im Speicher steht er
                Assert.Equal(auftraegeVorher + 1, SharedData.RuestungBauwerke.Count);

                // und er ist genau einmal zum Schreiben vorgemerkt - als Update, das die Zeile
                // anlegt, weil es ueber die ID 0 keine trifft (siehe RuestungBauwerke.Save)
                var vorgemerkt = Neu(vorher)
                    .Where(eintrag => eintrag.Table is RuestungBauwerke)
                    .ToList();
                Assert.Single(vorgemerkt);
                Assert.Equal(DatabaseQueue.DatabaseQueueCommand.Update, vorgemerkt[0].Command);

                var auftrag = Assert.IsType<RuestungBauwerke>(vorgemerkt[0].Table);
                Assert.Equal(0, auftrag.ID);
                Assert.Equal(feld.gf, auftrag.gf);
                Assert.Equal(feld.kf, auftrag.kf);
                Assert.Equal($"Strasse_{richtung}", auftrag.Art);
                Assert.True(auftrag.Kosten > 0);
            }
            finally {
                bau.UndoCommand();
            }
        }

        /// <summary>
        /// Das Zuruecknehmen merkt das Loeschen vor und raeumt den Auftrag aus dem Speicher.
        /// </summary>
        [StaFact]
        public void DasZuruecknehmenMerktDasLoeschenVor() {
            LadeAlles();
            var (feld, richtung) = FindeStrassenbau();
            int auftraegeVorher = SharedData.RuestungBauwerke!.Count;

            Assert.True(CommandParser.ParseCommand(
                $"Errichte Straße im {richtung} von {feld.CreateBezeichner()}", out var befehl));
            var bau = (ConstructCommand)befehl!;
            Assert.False(bau.ExecuteCommand().HasErrors);

            int vorher = SharedData.StoreQueue.Count;
            Assert.False(bau.UndoCommand().HasErrors);

            Assert.Equal(auftraegeVorher, SharedData.RuestungBauwerke.Count);

            var vorgemerkt = Neu(vorher)
                .Where(eintrag => eintrag.Table is RuestungBauwerke)
                .ToList();
            Assert.Single(vorgemerkt);
            Assert.Equal(DatabaseQueue.DatabaseQueueCommand.Delete, vorgemerkt[0].Command);
        }

        /// <summary>
        /// Ein zweites Zuruecknehmen findet nichts mehr - und antwortet, statt zu werfen.
        ///
        /// Vorher stand hier First() statt FirstOrDefault(): die Pruefung darunter war unerreichbar,
        /// und eine InvalidOperationException kam mitten im Zuruecknehmen hoch.
        /// </summary>
        [StaFact]
        public void EinZweitesZuruecknehmenWirftNicht() {
            LadeAlles();
            var (feld, richtung) = FindeStrassenbau();

            Assert.True(CommandParser.ParseCommand(
                $"Errichte Straße im {richtung} von {feld.CreateBezeichner()}", out var befehl));
            var bau = (ConstructCommand)befehl!;
            Assert.False(bau.ExecuteCommand().HasErrors);
            Assert.False(bau.UndoCommand().HasErrors);

            var nochmal = bau.UndoCommand();
            Assert.True(nochmal.HasErrors);
            Assert.Contains("existiert nicht", nochmal.Title);
        }
    }
}
