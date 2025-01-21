using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// Definiert eine Schnittstelle für ausführbare Befehle.
    /// </summary>
    public interface ICommand {
        /// <summary>
        /// Führt den Befehl aus und gibt das Ergebnis zurück.
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        CommandResult ExecuteCommand();
    }

    /// <summary>
    /// Definiert eine Schnittstelle für Befehls-CommandParser.
    /// </summary>
    public interface ICommandParser {
        /// <summary>
        /// Analysiert einen Befehlsstring und erstellt ein entsprechendes Command-Objekt.
        /// </summary>
        /// <param name="commandString">Der zu analysierende Befehlsstring.</param>
        /// <param name="command">Das resultierende Command-Objekt.</param>
        /// <returns>True, wenn der Befehl erfolgreich analysiert wurde, sonst false.</returns>
        bool ParseCommand(string commandString, out ICommand? command);
    }

}
