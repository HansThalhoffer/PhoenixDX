using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using PhoenixModel.Helper;

namespace PhoenixModel.Commands.Parser {

    /// <summary>
    /// Basisklasse für das Ergebnis eines Befehls.
    /// </summary>
    public abstract class CommandResult:Result {
        /// <summary>
        /// Erstellt eine neue Instanz eines Befehls-Ergebnisses.
        /// </summary>
        /// <param name="title">Der Titel der Meldung.</param>
        /// <param name="message">Die Nachricht des Ergebnisses.</param>
        public CommandResult(string title, string message, bool hasErrors, IPhoenixCommand? command) : base (title,message, hasErrors) {
            Title = title;
            Message = message;
            Command = command;
        }

        public IPhoenixCommand?  Command { get; } = null;
    }
}

/// <summary>
/// Repräsentiert ein Befehls-Ergebnis mit Fehler.
/// </summary>
public class CommandResultError : CommandResult {
    public CommandResultError(string title, string message, IPhoenixCommand? command) : base(title, message, true, command) { }
}

/// <summary>
/// Repräsentiert ein erfolgreiches Befehls-Ergebnis.
/// </summary>
public class CommandResultSuccess : CommandResult {
    public CommandResultSuccess(string title, string message, IPhoenixCommand? command) : base(title, message, false, command) { }
}