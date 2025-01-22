using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class CommandParser {
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

      

       
    }
}
