using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Commands.Parser {
    internal static class CommonCommandErrors {

        internal static bool MissingUnitID(int id, string commandString, IPhoenixCommand cmd, out CommandResult? result) {
            if (id > 0) {
                result = null;
                return true;
            }
            result = new CommandResultError("Es wurde keine ID der Einheit angegeben", $"Wenn eine Figur geändert werden soll wird, dann muss diese auch eine ID haben.\r\n {commandString}", cmd);
            return false;
        }

        internal static bool NotLoaded(object? test, string tableName, string commandString, IPhoenixCommand cmd, out CommandResult? result) {
            if (test != null) {
                result = null;
                return true;
            }
            result = new CommandResultError($"Die Tabelle {tableName} wurde nicht geladen", $"Der Befehl kann nicht ausgeführt werden, da die betreffende Tabelle aus der Datenbank nicht geladen wurde \r\n {commandString}", cmd);
            return false;
        }

        internal static bool MissingPosition(KleinfeldPosition? test, string commandString, IPhoenixCommand cmd, out CommandResult? result) {
            if (test != null) {
                if (Plausibilität.IsOnMap(test) == false) {
                    result = new CommandResultError($"Die angegebene Kleinfeldposition {test} ist nicht auf der Karte", $"Der Befehl kann nicht ausgeführt werden, da die Kleinfeldposition zwingend auf der Karte sein muss:\r\n {commandString}", cmd);
                    return false;
                }
                result = null;
                return true;
            }
            
            result = new CommandResultError($"Die Kleinfeldposition wurde nicht angegegeben", $"Der Befehl kann nicht ausgeführt werden, da die Kleinfeldposition zwingend ist:\r\n {commandString}", cmd);
            return false;
        }


        internal static bool EmptyString(string? test, string parameterName, string commandString, IPhoenixCommand cmd, out CommandResult? result) {
            if (test != null) {
                result = null;
                return true;
            }
            result = new CommandResultError($"Der {parameterName} darf nicht leer sein", $"Der Befehl kann nicht ausgeführt werden, da der betreffende Parameter nicht gesetzt wurde \r\n {commandString}", cmd);
            return false;
        }
    }
}
