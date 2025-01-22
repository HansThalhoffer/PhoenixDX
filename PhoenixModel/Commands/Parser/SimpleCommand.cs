using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhoenixModel.Commands.Parser {
    public abstract class SimpleCommand : ICommand {
        protected string CommandString;

        /// <param name="commandString">Der nicht erkannte Befehl.</param>
        public SimpleCommand(string commandString) { CommandString = commandString; }

        public abstract CommandResult CheckPreconditions();

        public abstract CommandResult ExecuteCommand();

        public abstract CommandResult UndoCommand();
    }

    public abstract class SimpleParser : ICommandParser {
        protected static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }
        public abstract bool ParseCommand(string commandString, out ICommand? command);
        
        private static Regex locationRegex = new Regex(@"(\d+)/(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Analysiert eine Eingabe und extrahiert eine Kleinfeld-Position.
        /// </summary>
        /// <param name="input">Der zu analysierende Eingabestring.</param>
        /// <returns>Die extrahierte Kleinfeld-Position oder null, falls ungültig.</returns>
        public static KleinfeldPosition? ParseLocation(string input) {
            var match = locationRegex.Match(input);
            if (match.Success && match.Groups.Count == 3) {
                int gf = int.Parse(match.Groups[1].Value);
                int kf = int.Parse(match.Groups[2].Value);
                return new KleinfeldPosition(gf, kf);
            }
            return null;
        }

        /// <summary>
        /// Analysiert eine Eingabe und gibt den entsprechenden Einheitstyp zurück.
        /// </summary>
        /// <param name="input">Der eingegebene Einheitstyp als String.</param>
        /// <returns>Der erkannte Einheitstyp.</returns>
        public static FigurType ParseUnitType(string input) {
            return input.ToLower() switch {
                "reiter" => FigurType.Reiter,
                "krieger" => FigurType.Krieger,
                "schiff" => FigurType.Schiff,
                "zauberer" => FigurType.Zauberer,
                "charakter" => FigurType.Charakter,
                "charakterzauberer" => FigurType.CharakterZauberer,
                "LKP" => FigurType.LeichteArtillerie,
                "SKP" => FigurType.SchwereArtillerie,
                "LKS" => FigurType.LeichtesKriegsschiff,
                "SKS" => FigurType.SchweresKriegsschiff,
                "leichtes katapult" => FigurType.LeichteArtillerie,
                "leichtem katapult" => FigurType.LeichteArtillerie,
                "leichten katapulten" => FigurType.LeichteArtillerie,
                "schweres katapult" => FigurType.SchwereArtillerie,
                "schwerem katapult" => FigurType.SchwereArtillerie,
                "schweren katapulten" => FigurType.SchwereArtillerie,
                "leichtes kriegsschiff" => FigurType.LeichtesKriegsschiff,
                "leichtem kriegsschiff" => FigurType.LeichtesKriegsschiff,
                "leichten kriegsschiffen" => FigurType.LeichtesKriegsschiff,
                "schweres kriegsschiff" => FigurType.SchweresKriegsschiff,
                "schwerem kriegsschiff" => FigurType.SchweresKriegsschiff,
                "schweren kriegsschiffen" => FigurType.SchweresKriegsschiff,
                "piratenschiff" => FigurType.PiratenSchiff,
                "piratenschiffen" => FigurType.PiratenSchiff,
                "schweres piratenkriegsschiff" => FigurType.PiratenSchweresKriegsschiff,
                "schwerem piratenkriegsschiff" => FigurType.PiratenSchweresKriegsschiff,
                "schweren piratenkriegsschiffen" => FigurType.PiratenSchweresKriegsschiff,
                "leichtes piratenkriegsschiff" => FigurType.PiratenLeichtesKriegsschiff,
                "leichtem piratenkriegsschiff" => FigurType.PiratenLeichtesKriegsschiff,
                "leichten piratenkriegsschiffen" => FigurType.PiratenLeichtesKriegsschiff,
                "berittene schwere artillerie" => FigurType.BeritteneSchwereArtillerie,
                "berittener schwerer artillerie" => FigurType.BeritteneSchwereArtillerie,
                "berittene leichte artillerie" => FigurType.BeritteneLeichteArtillerie,
                "berittener leichter artillerie" => FigurType.BeritteneLeichteArtillerie,
                _ => FigurType.NaN,
            };
        }

        /// <summary>
        /// die Umkehrfunktion zum Erstellen von Befehlen
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string UnitTypeToString(FigurType type) {
            return type switch {
                FigurType.Reiter => "Reiter",
                FigurType.Krieger => "Krieger",
                FigurType.Schiff => "Schiff",
                FigurType.Zauberer => "Zauberer",
                FigurType.Charakter => "Charakter",
                FigurType.CharakterZauberer => "Charakterzauberer",
                FigurType.LeichteArtillerie => "Leichtes Katapult",
                FigurType.SchwereArtillerie => "Schweres Katapult",
                FigurType.LeichtesKriegsschiff => "Leichtes Kriegsschiff",
                FigurType.SchweresKriegsschiff => "Schweres Kriegsschiff",
                FigurType.PiratenSchiff => "Piratenschiff",
                FigurType.PiratenSchweresKriegsschiff => "Schweres Piratenkriegsschiff",
                FigurType.PiratenLeichtesKriegsschiff => "Leichtes Piratenkriegsschiff",
                FigurType.BeritteneSchwereArtillerie => "Berittene schwere Artillerie",
                FigurType.BeritteneLeichteArtillerie => "Berittene leichte Artillerie",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Analysiert eine Eingabe und gibt den entsprechenden Einheitstyp zurück.
        /// </summary>
        /// <param name="input">Der eingegebene Einheitstyp als String.</param>
        /// <returns>Der erkannte Einheitstyp.</returns>
        public static Direction? ParseDirection(string input) {
            return input.ToLower() switch {
                "no" => Direction.NO,
                "nordost" => Direction.NO,
                "nordosten" => Direction.NO,
                "o" => Direction.O,
                "ost" => Direction.O,
                "osten" => Direction.O,
                "so" => Direction.SO,
                "südost" => Direction.SO,
                "südosten" => Direction.SO,
                "sw" => Direction.SW,
                "südwest" => Direction.SW,
                "südwesten" => Direction.SW,
                "w" => Direction.W,
                "west" => Direction.W,
                "westen" => Direction.W,
                "nw" => Direction.NW,
                "nordwest" => Direction.NW,
                "nordwesten" => Direction.NW,
                _ => null,
            };
        }

        /// <summary>
        /// liest eine Zahl und ignoriert Exceptions
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int ParseInt(string input) {
            try { return int.Parse(input); } catch { };
            return 0;
        }
    }
}
