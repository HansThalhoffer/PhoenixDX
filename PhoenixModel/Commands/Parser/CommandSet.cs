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

        public bool Undo(BaseCommand command) {
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
