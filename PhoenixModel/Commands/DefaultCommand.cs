using PhoenixModel.Commands.Parser;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {
    /// <summary>
    /// DefaultCommand wird zurückgegeben, wenn kein CommandParser den Befehlsstring versteht.
    /// </summary>
    public class DefaultCommand : ICommand {
        protected string CommandString;

        /// <summary>
        /// Erstellt eine Instanz eines Standardbefehls.
        /// </summary>
        /// <param name="commandString">Der nicht erkannte Befehl.</param>
        public DefaultCommand(string commandString) { CommandString = commandString; }

        /// <summary>
        /// Führt den Standardbefehl aus und gibt eine Fehlermeldung zurück.
        /// </summary>
        public virtual CommandResult ExecuteCommand() {
            if (this.GetType() == typeof(DefaultCommand)) {
                return new CommandResultError("Das übergebene Kommando wurde nicht verstanden",
                    $"'{CommandString}' \r\n konnte von keinem registrierten CommandParser interpretiert werden");
            }
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch ExecuteCommand überschreiben");
        }

        protected static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }
    }
}
