using PhoenixModel.Commands.Parser;
using PhoenixModel.ViewModel;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für CommanndHistoryPage.xaml
    /// </summary>
    public partial class CommanndHistoryPage : Page {


        class CommandListEntry : INotifyPropertyChanged {
            // Private fields
            private BaseCommand _command;

            // Public properties
            public string CommandString {
                get { return _command.CommandString; }
            }

            public bool IsExecuted {
                get { return _command.IsExecuted; }
            }

            public bool CanUndo {
                get { return _command.CanUndo; }
            }

            public BaseCommand Command {
                get { return _command; }                
            }

            // Constructor
            public CommandListEntry(BaseCommand command) {
                _command = command;
            }

            // Implement INotifyPropertyChanged
            public event PropertyChangedEventHandler? PropertyChanged;

            // Helper method to raise PropertyChanged event
            public void OnPropertyChanged(string propertyName) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<CommandListEntry> _commandList = [];

        public CommanndHistoryPage() {
            InitializeComponent();
            SharedData.Commands.CollectionChanged += Commands_CollectionChanged;
            foreach (BaseCommand item in SharedData.Commands) {
                _commandList.Add(new CommandListEntry(item));
            }
            CommandDataGrid.ItemsSource = _commandList;
        }

        private void Commands_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove) {
                if (e.OldItems != null) {
                    foreach (BaseCommand item in e.OldItems) {
                        var entry = _commandList.Where( e => e.Command ==  item).First();
                        if (entry != null) {
                            _commandList.Remove(entry);
                        }
                    }
                }
             }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
                if (e.NewItems != null) {
                    foreach (BaseCommand item in e.NewItems) {
                        _commandList.Add(new CommandListEntry(item));
                    }
                }
            }
            foreach (var item in _commandList) {
                item.OnPropertyChanged("CanUndo");
            }

            
        }

        private void Refresh() {
            CommandDataGrid.Items.Refresh();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e) {
            if (sender is Button button && button.DataContext is CommandListEntry entry) {
                if (SharedData.Commands.Undo(entry.Command) == true) {
                    Refresh();
                }
            }
        }

    }
}
