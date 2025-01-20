using System.Collections.Concurrent;
using System.Data.Common;

namespace PhoenixModel.Database {
    /// <summary>
    /// Statische Klasse zur Protokollierung von Datenbankbefehlen.
    /// </summary>
    public static class DatabaseLog {
        /// <summary>
        /// Warteschlange zum Zwischenspeichern der protokollierten SQL-Befehle.
        /// </summary>
        public static ConcurrentQueue<string> Cache { get; } = [];

        /// <summary>
        /// Protokolliert einen ausgeführten <see cref="DbCommand"/> und speichert ihn in der Warteschlange.
        /// </summary>
        /// <param name="command">Das zu protokollierende Datenbankkommando.</param>
        public static void Log(DbCommand command) {
            // Entfernt unnötige Leerzeichen und Zeilenumbrüche aus dem SQL-Befehl
            string logText = command.CommandText.Replace("\r\n", "");
            logText = logText.Replace("     ", " ");
            logText = logText.Replace("    ", " ");
            logText = logText.Replace("  ", " ");
            logText = logText.Replace("\t", " ");

            // Fügt den bereinigten SQL-Befehl der Warteschlange hinzu
            Cache.Enqueue(logText);
        }
    }
}
