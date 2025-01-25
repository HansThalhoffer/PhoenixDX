using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.Database;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.CodeDom;
using System.Collections.Generic;
using System.Windows;
using static PhoenixModel.Commands.DiplomacyCommand;
using static PhoenixModel.Database.PasswordHolder;

namespace Tests {


    public class CommandParserTest {
        
        private ICommand? ParseCommand(string commandString) {
            CommandParser.ParseCommand(commandString, out ICommand? actualCommand);
            return actualCommand;
        }

        private void DoTest(IEnumerable<SimpleCommand> expectedCommands) {
            // Testvergleich mit den expected
            foreach (var expectedCommand in expectedCommands) {
                ICommand? actualCommand = ParseCommand(expectedCommand.CommandString);
                Assert.NotNull(actualCommand);
                Assert.True(actualCommand.GetType() == expectedCommand.GetType());
                Assert.True(expectedCommand.Equals(actualCommand));
            }
            /// Test als Roundtrip
            foreach (var expectedCommand in expectedCommands) {
                string cmd = expectedCommand.ToString();
                ICommand? actualCommand = ParseCommand(cmd);
                Assert.NotNull(actualCommand);
                Assert.True(actualCommand.GetType() == expectedCommand.GetType());
                Assert.True(expectedCommand.Equals(actualCommand));
            }
        }


        [Fact]
        public void ParsingMoveCommandTest() {
            MoveCommand[] expectedCommands = [
                new MoveCommand("Bewege Reiter 220 von 503/22 nach 504/07 via 503/21, 503/16, 503/ 4, 603/27"){
                    Figur = FigurType.Reiter,
                    UnitId = 220,
                    FromLocation = new(503,22),
                    ToLocation = new(504,7),
                    ViaLocations = [new(503,21), new(503,16),new(503,4),new(603,27)],
                },
                new MoveCommand("Bewege Krieger 100 von 503/22 nach 503/23"){
                    Figur = FigurType.Krieger,
                    UnitId = 100,
                    FromLocation = new(503,22),
                    ToLocation = new(503,23),
                    ViaLocations = null,
                },
                 new MoveCommand("Bewege Zauberer 305 von 601/10 nach 601/20 via 601/15"){
                    Figur = FigurType.Zauberer,
                    UnitId = 305,
                    FromLocation = new(601,10),
                    ToLocation = new(601,20),
                    ViaLocations = [new(601,15)],
                },
                new MoveCommand("Bewege Schiff 501 von 700/30 nach 800/40"){
                    Figur = FigurType.Schiff,
                    UnitId = 501,
                    FromLocation = new(700,30),
                    ToLocation = new(800,40),
                    ViaLocations = null,
                }
            ];

            DoTest(expectedCommands);
        }

        [StaFact]
        public void ParsingConstructCommandTest() {
           
            ConstructCommand[] expectedCommands = [
                new ConstructCommand("Errichte Wall im Nordosten von 202/33"){
                    Location = new(202,33),
                    What = ConstructionElementType.Wall,
                    Direction = Direction.NO,
                    Kosten = new PhoenixModel.dbCrossRef.Kosten{ GS = 5000, BauPunkte = 100, RP = 0, Unittyp = "Wall" },
                },
                new ConstructCommand("Errichte Brücke im Südwesten von 1021/03"){
                    Location = new(1021,3),
                    What = ConstructionElementType.Bruecke,
                    Direction = Direction.SW,
                    Kosten = new PhoenixModel.dbCrossRef.Kosten{ GS = 3000, BauPunkte = 60, RP = 0, Unittyp = "Bruecke" },
                },
                 new ConstructCommand("Errichte Straße im Westen von 701/48"){
                    Location = new(701,48),
                    What = ConstructionElementType.Strasse,
                    Direction = Direction.W,
                    Kosten = new PhoenixModel.dbCrossRef.Kosten{ GS = 3000, BauPunkte = 60, RP = 0, Unittyp = "Strasse" },
                },
                new ConstructCommand("Errichte Burg auf 701/48"){
                    Location = new(701,48),
                    What = ConstructionElementType.Burg,
                    Direction = null,
                    Kosten = new PhoenixModel.dbCrossRef.Kosten{ GS = 50000, BauPunkte = 1000, RP = 0, Unittyp = "Burg" },
                },
            ];
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            DoTest(expectedCommands);
        }

        [StaFact]
        public void ParsingDiplomacyCommanddTest() {
            TestSetup.Setup();
            TestSetup.LoadPZE(false, false);

            DiplomacyCommand[] expectedCommands = [
                new DiplomacyCommand("Gebe Yaromo Küstenrecht"){
                    Recht = BewegungsRecht.Küstenrecht,
                    RemoveRecht = false,
                    Nation = NationenView.GetNationFromString("Yaromo"),
                    ReferenzNation = ProgramView.SelectedNation
                },
                new DiplomacyCommand("Entziehe Yaromo Küstenrecht"){
                    Recht = BewegungsRecht.Küstenrecht,
                    RemoveRecht = true,
                    Nation = NationenView.GetNationFromString("Yaromo"),
                    ReferenzNation = ProgramView.SelectedNation
                },
                 new DiplomacyCommand("Gebe Helborn Wegerecht"){
                    Recht = BewegungsRecht.Wegerecht,
                    RemoveRecht = false,
                    Nation = NationenView.GetNationFromString("Helborn"),
                    ReferenzNation = ProgramView.SelectedNation
                },
                new DiplomacyCommand("Entziehe Helborn Wegerecht"){
                    Recht = BewegungsRecht.Wegerecht,
                    RemoveRecht = true,
                    Nation = NationenView.GetNationFromString("Helborn"),
                    ReferenzNation = ProgramView.SelectedNation
                },
            ];
            DoTest(expectedCommands);
        }

        [StaFact]
        public void DoNothingCommanddTest() {
            TestSetup.Setup();
            DoNothingCommand[] expectedCommands = [
                new DoNothingCommand("Kreatur 403 auf 405/22 tut nichts diese Runde"){
                  Figur = FigurType.Kreatur,
                  UnitId = 403,
                  Location = new(405,22),
                },
                new DoNothingCommand("Schiff 303 auf 777/77 tut nichts diese Runde"){
                  Figur = FigurType.Schiff,
                  UnitId = 303,
                  Location = new(777,77),
                },
                new DoNothingCommand("Zauberer 514 auf 123/45 tut nichts diese Runde"){
                  Figur = FigurType.Zauberer,
                  UnitId = 514,
                  Location = new(123,45),
                },
            ];
            DoTest(expectedCommands);
        }
    }
}
