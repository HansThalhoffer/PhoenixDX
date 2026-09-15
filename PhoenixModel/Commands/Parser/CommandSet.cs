using PhoenixModel.Program;
using PhoenixModel.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// Spezialisierung des BlockingSet
    /// </summary>
    public class CommandSet : ObservableCollection<BaseCommand>, INotifyCollectionChanged, IEnumerable<BaseCommand> {

        public IEnumerable<BaseCommand> GetCommands(ISelectable selectable) {
            return this.Where(item => item.HasEffectOn(selectable));
        }
        public new void Add(BaseCommand command) {
            
            Dispatch(() => {
                base.Add(command); // Safely modify collection
                Console.WriteLine("Item added safely.");
            });
        }
        static void Dispatch(Action action) {
            if (_syncContext != null)
                _syncContext.Post(_ => action(), null);
            else
                action(); // If no context, run directly
        }
        static SynchronizationContext? _syncContext;

        /// <summary>
        /// Nimmt einen Befehl zurück.
        ///
        /// Zurücknehmen lässt sich nur, was im laufenden Zug gemacht wurde
        /// (<see cref="BaseCommand.GehörtZumAktuellenZug"/>). Ein Befehl aus einem vergangenen
        /// Monat ist ausgewertet; ihn zurückzunehmen würde die eigene Karte gegen die Auswertung
        /// der Spielleitung verschieben.
        /// </summary>
        /// <returns>true, wenn der Befehl zurückgenommen wurde</returns>
        public bool Undo(BaseCommand command) {
            if (command.GehörtZumAktuellenZug == false) {
                ProgramView.LogWarning($"Der Befehl stammt aus Zug {command.Zug} und lässt sich nicht zurücknehmen",
                    $"Gespielt wird Zug {ProgramView.SelectedMonth}. Zurücknehmen lässt sich nur, was im "
                    + "laufenden Zug gemacht wurde - alles Ältere hat die Spielleitung längst ausgewertet.");
                return false;
            }
            if (command.CanUndo == true) {
                var result = command.UndoCommand();
                if (result.HasErrors == false) {
                    Remove(command);
                    return true;
                }
                ProgramView.LogError(result.Title, result.Message);
            }
            return false;
        }
    }
}
