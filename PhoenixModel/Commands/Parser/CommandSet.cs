using PhoenixModel.Program;
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
    public class CommandSet : PhoenixModel.ViewModel.BlockingSet<ISelectable, SimpleCommand>, INotifyCollectionChanged, IEnumerable<SimpleCommand> {
       
    }
}
