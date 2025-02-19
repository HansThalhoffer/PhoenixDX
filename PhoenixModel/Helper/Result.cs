using PhoenixModel.Commands;
using PhoenixModel.Commands.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Helper {
    public class Result {

        /// <summary>
        /// Erstellt eine neue Instanz eines Befehls-Ergebnisses.
        /// </summary>
        /// <param name="title">Der Titel der Meldung.</param>
        /// <param name="message">Die Nachricht des Ergebnisses.</param>
        public Result(string title, string message, bool hasErrors) {
            Title = title;
            Message = message;
            HasErrors = hasErrors;
        }

        /// <summary>
        /// copy constructor für abgeleitete Klassen
        /// </summary>
        /// <param name="result"></param>
        public Result(Result result) {
            this.Title = result.Title;
            this.Message = result.Message;
            this.HasErrors = result.HasErrors;
        }
        /// <summary>
        /// Titel der Nachricht.
        /// </summary>
        public string Title { get; protected set; } = string.Empty;

        /// <summary>
        /// Inhalt der Nachricht.
        /// </summary>
        public string Message { get; protected set; } = string.Empty;

        /// <summary>
        /// Gibt an, ob das Ergebnis Fehler enthält.
        /// </summary>
        public virtual bool HasErrors { get; protected set; } = false;


        /// <summary>
        /// Implizite Konvertierung zu bool, um eine einfache Abfrage zu ermöglichen.
        /// if (result == true) oder einfach nur if (result)
        /// </summary>
        public static implicit operator bool(Result result) {
            return result.HasErrors == false;
        }

        private static readonly Result _success = new Result("Super!", "Das war erfolgreich", false);

        public static Result Fail(string title, string message) {  return new Result(title, message, true); }
        public static Result Success(string title, string message) { return new Result(title, message, false); }
        public static Result Success() { return _success; }
    }
}
