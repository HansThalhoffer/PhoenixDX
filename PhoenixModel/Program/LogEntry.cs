using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    public class LogEntry
    {
        public enum LogType
        {
            Info,
            Warning,
            Error
        }
        public LogType Type { get; set; } = LogType.Info;
        public string Titel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public LogEntry(LogType type, string titel, string message)
        {
            Type = type;
            Titel = titel;
            Message = message;
        }

        public LogEntry(string titel, string message)
        {
            Titel = titel;
            Message = message;
        }

        /// <summary>
        /// Der Eintrag als Text, wie er in der Zwischenablage landen soll: die Kopfzeile, darunter
        /// die Erklaerung. Hat der Eintrag keine Erklaerung, bleibt es bei der Kopfzeile - eine
        /// angehaengte Leerzeile waere beim Einfuegen nur im Weg.
        /// </summary>
        public string AlsText()
        {
            return string.IsNullOrWhiteSpace(Message) ? Titel : $"{Titel}{Environment.NewLine}{Message}";
        }

        /// <summary>
        /// Mehrere Eintraege als Text, durch eine Leerzeile voneinander getrennt. Eintraege ohne
        /// Titel fallen weg: die zeigt die Liste auch nicht an, und in der Zwischenablage waeren
        /// sie nur Luecken.
        /// </summary>
        public static string AlsText(IEnumerable<LogEntry>? eintraege)
        {
            if (eintraege == null)
                return string.Empty;
            return string.Join(Environment.NewLine + Environment.NewLine,
                eintraege.Where(eintrag => eintrag != null && string.IsNullOrWhiteSpace(eintrag.Titel) == false)
                         .Select(eintrag => eintrag.AlsText()));
        }
    }
}
