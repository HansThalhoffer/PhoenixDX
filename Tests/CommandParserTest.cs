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
using System.Windows;
using static PhoenixModel.Commands.DiplomacyCommand;
using static PhoenixModel.Database.PasswordHolder;

namespace Tests {


    public class CommandParserTest {
        
        private ICommand? ParseCommand(string commandString) {
            ICommand? command = null;
            CommandParser.ParseCommand(commandString, out command);
            return command;
        }

        [Fact]
        public void ParsingMoveCommandTest() {
            MoveCommand[] testCommands = [
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

            foreach (var testCommand in testCommands) {
                ICommand? command = ParseCommand(testCommand.CommandString);
                Assert.NotNull(command);
                Assert.True(command is MoveCommand);
                Assert.True(testCommand.Equals(command));
            }
        }

        [StaFact]
        public void ParsingConstructCommandTest() {
           
            ConstructCommand[] testCommands = [
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
            TestSetup.LoadCrossRef();
        
            foreach (var testCommand in testCommands) {
                ICommand? command = ParseCommand(testCommand.CommandString);
                Assert.NotNull(command);
                Assert.True(command.GetType() == testCommand.GetType());
                Assert.True(testCommand.Equals(command));
            }
        }

        [StaFact]
        public void ParsingDiplomacyCommanddTest() {

            DiplomacyCommand[] testCommands = [
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
                    Recht = BewegungsRecht.Küstenrecht,
                    RemoveRecht = false,
                    Nation = NationenView.GetNationFromString("Helborn"),
                    ReferenzNation = ProgramView.SelectedNation
                },
                new DiplomacyCommand("Entziehe Helborn Wegerecht"){
                    Recht = BewegungsRecht.Küstenrecht,
                    RemoveRecht = true,
                    Nation = NationenView.GetNationFromString("Helborn"),
                    ReferenzNation = ProgramView.SelectedNation
                },
            ];
            TestSetup.Setup();
            TestSetup.LoadCrossRef();

            foreach (var testCommand in testCommands) {
                ICommand? command = ParseCommand(testCommand.CommandString);
                Assert.NotNull(command);
                Assert.True(command.GetType() == testCommand.GetType());
                Assert.True(testCommand.Equals(command));
            }
        }
    }
}
