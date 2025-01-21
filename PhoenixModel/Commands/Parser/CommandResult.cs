using PhoenixModel.Commands.Parser;

namespace PhoenixModel.Commands.Parser {

    /// <summary>
    /// Basisklasse für das Ergebnis eines Befehls.
    /// </summary>
    public abstract class CommandResult {
        /// <summary>
        /// Erstellt eine neue Instanz eines Befehls-Ergebnisses.
        /// </summary>
        /// <param name="title">Der Titel der Meldung.</param>
        /// <param name="message">Die Nachricht des Ergebnisses.</param>
        public CommandResult(string title, string message) {
            Title = title;
            Message = message;
        }

        /// <summary>
        /// Titel der Nachricht.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Inhalt der Nachricht.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gibt an, ob das Ergebnis Fehler enthält.
        /// </summary>
        public abstract bool HasErrors { get; }

        /// <summary>
        /// Implizite Konvertierung zu bool, um eine einfache Abfrage zu ermöglichen.
        /// if (result == true) oder einfach nur if (result)
        /// </summary>
        public static implicit operator bool(CommandResult? result) {
            return result == null ? true : result.HasErrors;
        }
    }
}

/// <summary>
/// Repräsentiert ein Befehls-Ergebnis mit Fehler.
/// </summary>
public class CommandResultError : CommandResult {
    public CommandResultError(string title, string message) : base(title, message) { }
    public override bool HasErrors => true;
}

/// <summary>
/// Repräsentiert ein erfolgreiches Befehls-Ergebnis.
/// </summary>
public class CommandResultSuccess : CommandResult {
    public CommandResultSuccess(string title, string message) : base(title, message) { }
    public override bool HasErrors => false;
}