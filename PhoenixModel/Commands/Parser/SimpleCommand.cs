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
        protected readonly string _CommandString;
        public bool IsExecuted { get; protected set; }

        /// <param name="commandString">Der nicht erkannte Befehl.</param>
        public SimpleCommand(string commandString) { _CommandString = commandString; }

        public string CommandString => _CommandString;

        public abstract CommandResult CheckPreconditions();

        public abstract CommandResult ExecuteCommand();

        public abstract CommandResult UndoCommand();

        /// <summary>
        /// Versucht den Befehl rückgängig zu machen.
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        public abstract override string ToString();
    }

    public abstract class SimpleParser : ICommandParser {
        protected static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }
        public abstract bool ParseCommand(string commandString, out ICommand? command);


        /// <summary>
        /// Analysiert eine Eingabe und extrahiert eine Kleinfeld-Position.
        /// </summary>
        /// <param name="input">Der zu analysierende Eingabestring.</param>
        /// <returns>Die extrahierte Kleinfeld-Position oder null, falls ungültig.</returns>
        public static KleinfeldPosition? ParseLocation(string input) {
            var numbers = input.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (numbers.Length == 2) {
                return new KleinfeldPosition(ParseInt(numbers[0]), ParseInt(numbers[1]));
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
                "kreatur" => FigurType.Kreatur,
                "schiff" => FigurType.Schiff,
                "zauberer" => FigurType.Zauberer,
                "charakter" => FigurType.Charakter,
                "charakterzauberer" => FigurType.CharakterZauberer,
                "lkp" => FigurType.LeichteArtillerie,
                "skp" => FigurType.SchwereArtillerie,
                "lks" => FigurType.LeichtesKriegsschiff,
                "sks" => FigurType.SchweresKriegsschiff,
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
                _ => FigurType.None,
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
            try {
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                    return 0;
                if (input.All(char.IsDigit) == false)
                    return 0;
                return int.Parse(input);
            }
            catch { };
            return 0;
        }

        /// <summary>
        /// die Namen entsprechen 1:1 der Kostentabelle in crossref.mdb
        /// </summary>
        public static string ConstructionElementTypeToString(ConstructionElementType type) {
            return type
            switch {
                ConstructionElementType.None => "",
                ConstructionElementType.K => "Krieger", // Krieger
                ConstructionElementType.S => "Schiffe", // 
                ConstructionElementType.R => "Reiter", // Reiter
                ConstructionElementType.P => "PFerde", // PFerde
                ConstructionElementType.LKP => "Leichte Katapulte",// 
                ConstructionElementType.SKP => "Schwere Katapulte",// 
                ConstructionElementType.LKS => "Leichte Kriegsschiffe ",// 
                ConstructionElementType.SKS => "Schwere Kriegsschiffe",// 
                ConstructionElementType.HF => "Heerführer", // HF 
                ConstructionElementType.ZA => "Zauberer Klasse A", // Zauberer Klasse A
                ConstructionElementType.ZB => "Zauberer Klasse B", // Zauberer Klasse B
                ConstructionElementType.Strasse => "Straße",
                ConstructionElementType.Bruecke => "Brücke",
                ConstructionElementType.Wall => "Wall",
                ConstructionElementType.Burg => "Burg",
                ConstructionElementType.ausbau => "ausbau",
                _ => string.Empty,
            };
        }



        public ConstructionElementType parseConstructionElement(string input) {
            return input.ToLower()
            switch {
                "k" => ConstructionElementType.K,
                "krieger" => ConstructionElementType.K,
                "kriegern" => ConstructionElementType.K,
                "r" => ConstructionElementType.R,
                "reiter" => ConstructionElementType.R,
                "reitern" => ConstructionElementType.R,
                "s" => ConstructionElementType.S,
                "schiff" => ConstructionElementType.S,
                "schiffe" => ConstructionElementType.S,
                "schiffen" => ConstructionElementType.S,
                "pferde" => ConstructionElementType.P,
                "lkp" => ConstructionElementType.LKP,
                "leichte katapulte" => ConstructionElementType.LKP,
                "leichte kp" => ConstructionElementType.LKP,
                "skp" => ConstructionElementType.SKP,
                "schwere katapulte" => ConstructionElementType.SKP,
                "schwere kp" => ConstructionElementType.SKP,
                "lks" => ConstructionElementType.LKS,
                "leichte kriegsschiffe" => ConstructionElementType.LKS,
                "leichte ks" => ConstructionElementType.LKS,
                "sks" => ConstructionElementType.SKS,
                "schwere kriegsschiffe" => ConstructionElementType.SKS,
                "schwere ks" => ConstructionElementType.SKS,
                "heerführer" => ConstructionElementType.HF,
                "hf" => ConstructionElementType.HF,
                "za" => ConstructionElementType.ZA,
                "zauberer klasse a" => ConstructionElementType.ZA,
                "zb" => ConstructionElementType.ZB,
                "zauberer klasse b" => ConstructionElementType.ZB,
                "wall" => ConstructionElementType.Wall,
                "strasse" => ConstructionElementType.Strasse,
                "straße" => ConstructionElementType.Strasse,
                "brücke" => ConstructionElementType.Bruecke,
                "bruecke" => ConstructionElementType.Bruecke,
                "burg" => ConstructionElementType.Burg,
                _ => ConstructionElementType.None
            };
        }
    }
}
