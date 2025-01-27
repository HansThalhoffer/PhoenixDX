namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// Standardbefehl, der zurückgegeben wird, wenn kein CommandParser den Befehlsstring versteht.
    /// </summary>
    public class DefaultCommand : BaseCommand {

        /// <summary>
        /// Erstellt eine Instanz eines Standardbefehls.
        /// </summary>
        /// <param name="commandString">Der nicht erkannte Befehl.</param>
        public DefaultCommand(string commandString) : base(commandString) { }

        /// <summary>
        /// Gibt den ursprünglichen Befehl als Zeichenkette zurück.
        /// </summary>
        /// <returns>Der ursprüngliche Befehl als <see cref="string"/>.</returns>
        public override string ToString() {
            return _CommandString;
        }

        /// <summary>
        /// Führt den Standardbefehl aus und gibt eine Fehlermeldung zurück, wenn der Befehl nicht erkannt wurde.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>, das einen Fehler beschreibt.</returns>
        /// <exception cref="NotImplementedException">
        /// Wird geworfen, wenn eine von <see cref="DefaultCommand"/> abgeleitete Klasse die Methode nicht überschreibt.
        /// </exception>
        public override CommandResult ExecuteCommand() {
            if (GetType() == typeof(DefaultCommand)) {
                return new CommandResultError("Das übergebene Kommando wurde nicht verstanden",
                    $"'{CommandString}' \r\n konnte von keinem registrierten CommandParser interpretiert werden", this);
            }
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch ExecuteCommand überschreiben");
        }

        /// <summary>
        /// Setzt den Befehl als fehlgeschlagen und gibt <c>false</c> zurück.
        /// </summary>
        /// <param name="command">Das fehlerhafte Kommando, das auf <c>null</c> gesetzt wird.</param>
        /// <returns>Immer <c>false</c>, um einen Fehlschlag anzuzeigen.</returns>
        protected static bool Fail(out IPhoenixCommand? command) {
            command = null;
            return false;
        }

        /// <summary>
        /// Prüft die Vorbedingungen für den Standardbefehl. Diese Methode muss in abgeleiteten Klassen überschrieben werden.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>, das den Status der Vorbedingungen beschreibt.</returns>
        /// <exception cref="NotImplementedException">
        /// Wird geworfen, wenn eine von <see cref="DefaultCommand"/> abgeleitete Klasse die Methode nicht implementiert.
        /// </exception>
        public override CommandResult CheckPreconditions() {
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch CheckPreconditions überschreiben");
        }

        /// <summary>
        /// Macht den Standardbefehl rückgängig. Diese Methode muss in abgeleiteten Klassen überschrieben werden.
        /// </summary>
        /// <returns>Ein <see cref="CommandResult"/>, das den Status der Rückgängigmachung beschreibt.</returns>
        /// <exception cref="NotImplementedException">
        /// Wird geworfen, wenn eine von <see cref="DefaultCommand"/> abgeleitete Klasse die Methode nicht implementiert.
        /// </exception>
        public override CommandResult UndoCommand() {
            throw new NotImplementedException("Wer die Klasse DefaultCommand ableitet, muss auch UndoCommand überschreiben");
        }
    }
}
