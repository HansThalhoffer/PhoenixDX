using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// DefaultCommand wird zurückgegeben, wenn kein CommandParser den Befehlsstring versteht.
    /// </summary>
    public class DefaultCommand : SimpleCommand {
        
        /// <summary>
        /// Erstellt eine Instanz eines Standardbefehls.
        /// </summary>
        /// <param name="commandString">Der nicht erkannte Befehl.</param>
        public DefaultCommand(string commandString) :base(commandString) { }

        /// <summary>
        /// Führt den Standardbefehl aus und gibt eine Fehlermeldung zurück.
        /// </summary>
        public override CommandResult ExecuteCommand() {
            if (GetType() == typeof(DefaultCommand)) {
                return new CommandResultError("Das übergebene Kommando wurde nicht verstanden",
                    $"'{CommandString}' \r\n konnte von keinem registrierten CommandParser interpretiert werden");
            }
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch ExecuteCommand überschreiben");
        }

        protected static bool Fail(out ICommand? command) {
            command = null;
            return false;
        }

        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch CheckPreconditions überschreiben");
        }

        public override CommandResult UndoCommand() {
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch UndoCommand überschreiben");
        }
    }
}
