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

namespace PhoenixModel.Commands.Parser {
    /// <summary>
    /// Spezialisierung des BlockingSet
    /// </summary>
    public class CommandSet : ObservableCollection<BaseCommand>, INotifyCollectionChanged, IEnumerable<BaseCommand> {

        public IEnumerable<BaseCommand> GetCommands(ISelectable selectable) {
            return this.Where(item => item.HasEffectOn(selectable));
        }

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
