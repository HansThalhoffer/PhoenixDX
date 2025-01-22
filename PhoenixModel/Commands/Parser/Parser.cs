namespace PhoenixModel.Commands.Parser {

    /// <summary>
    /// CommandParser-Klasse zur Registrierung und Verarbeitung von Befehlen.
    /// </summary>
    public class CommandParser {
        private static List<ICommandParser> parsers = [];
        private static CommandParser _parser = new CommandParser();

        /// <summary>
        /// Privater Konstruktor, der alle verfügbaren Befehls-CommandParser registriert.
        /// </summary>
        public CommandParser() {
            RegisterAllCommandParsers();
        }

        /// <summary>
        /// Registriert alle Klassen, die das Interface CommandParser implementieren.
        /// </summary>
        private static void RegisterAllCommandParsers() {
            var interfaceType = typeof(ICommandParser);

            var parserTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => interfaceType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            foreach (var type in parserTypes) {
                if (Activator.CreateInstance(type) is ICommandParser parser) {
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
