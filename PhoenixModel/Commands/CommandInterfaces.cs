using PhoenixModel.Commands.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Commands {


    /// <summary>
    /// die Namen entsprechen 1:1 der Kostentabelle in crossref.mdb
    /// </summary>
    public enum ConstructionElementType {
        None,
        K, // Krieger
        S, // Schiffe
        R, // Reiter
        P, // PFerde
        LKP,// Leichte Katapulte
        SKP,// Schwere Katapulte
        LKS,// Leichte Kriegsschiffe 
        SKS,// Schwere Kriegsschiffe
        HF, // HF 
        ZA, // Zauberer Klasse A
        ZB, // Zauberer Klasse B
        Strasse,
        Bruecke,
        Wall,
        Burg,
        ausbau
    }



    /// <summary>
    /// Definiert eine Schnittstelle für ausführbare Befehle.
    /// </summary>
    public interface ICommand {
        /// <summary>
        /// Führt den Befehl aus und gibt das Ergebnis zurück.
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        CommandResult ExecuteCommand();

        /// <summary>
        /// überprüft, ob die Vorbedingungen gegeben sind, das Kommando auszuführen - das Kommando wird aber noch nicht ausgeführt
        /// </summary>
        /// <returns></returns>
        CommandResult CheckPreconditions();

        /// <summary>
        /// Versucht den Befehl rückgängig zu machen.
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        CommandResult UndoCommand();

        /// <summary>
        /// Versucht den Befehl rückgängig zu machen.
        /// </summary>
        /// <returns>Ein Objekt vom Typ CommandResult.</returns>
        string ToString();
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
