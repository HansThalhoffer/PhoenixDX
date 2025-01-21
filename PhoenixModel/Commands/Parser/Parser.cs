using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixModel.Commands.Parser {

    /// <summary>
    /// CommandParser-Klasse zur Registrierung und Verarbeitung von Befehlen.
    /// </summary>
    public partial class CommandParser {
        private static List<CommandParser> parsers = [];
        private static CommandParser _parser = new CommandParser();

        /// <summary>
        /// Privater Konstruktor, der alle verfügbaren Befehls-CommandParser registriert.
        /// </summary>
        private CommandParser() {
            RegisterAllCommandParsers();
        }

        /// <summary>
        /// Registriert alle Klassen, die das Interface CommandParser implementieren.
        /// </summary>
        private static void RegisterAllCommandParsers() {
            var parserTypes = Assembly.GetExecutingAssembly()
               .GetTypes()
               .Where(t => t.IsClass && !t.IsAbstract && typeof(CommandParser).IsAssignableFrom(t))
               .ToList();

            foreach (var type in parserTypes) {
                if (Activator.CreateInstance(type) is CommandParser parser) {
                    parsers.Add(parser);
                }
            }
        }

        /// <summary>
        /// Analysiert den eingegebenen Befehl und gibt das entsprechende Command-Objekt zurück.
        /// </summary>
        /// <param name="commandString">Der zu analysierende Befehl.</param>
        /// <param name="command">Das erzeugte Command-Objekt.</param>
        /// <returns>True, wenn der Befehl erfolgreich analysiert wurde, sonst false.</returns>
        public static bool ParseCommand(string commandString, out ICommand? command) {
            foreach (ICommandParser parser in parsers) {
                if (parser.ParseCommand(commandString, out command)) {
                    return true;
                }
            }
            command = new DefaultCommand(commandString);
            return false;
        }

        private const string locationPattern = @"(\d+)/(\d+)";
        [GeneratedRegex(locationPattern)]
        public static partial Regex LocationRegex();

        /// <summary>
        /// Analysiert eine Eingabe und extrahiert eine Kleinfeld-Position.
        /// </summary>
        /// <param name="input">Der zu analysierende Eingabestring.</param>
        /// <returns>Die extrahierte Kleinfeld-Position oder null, falls ungültig.</returns>
        public static KleinfeldPosition? ParseLocation(string input) {
            var match = LocationRegex().Match(input);
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
                _ => FigurType.NaN,
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
    }
}
