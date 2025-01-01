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

    }
}
