using PhoenixModel.Commands;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PhoenixWPF.Pages
{
    /// <summary>
    /// Interaktionslogik für CommandPage.xaml
    /// </summary>
    public partial class CommandPage : Page
    {
        /// <summary>
        /// Liste zum Bind an das DataGrid
        /// </summary>
        private ObservableCollection<CommandListEntry> _commandList = [];


        public CommandPage()
        {
            InitializeComponent();
            SharedData.Commands.CollectionChanged += Commands_CollectionChanged;
            foreach (BaseCommand item in SharedData.Commands) {
                _commandList.Add(new CommandListEntry(item));
            }
            CommandDataGrid.ItemsSource = _commandList;
            Main.Instance.SelectionHistory.PropertyChanged += SelectionHistory_PropertyChanged;
        }

        private void SelectionHistory_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            RefreshList();
        }

        private void RefreshList() {
            _commandList.Clear();
             foreach (BaseCommand item in SharedData.Commands) {
                if (AffectsSelected(item))
                    _commandList.Add(new CommandListEntry(item));
            }
        }

        private bool AffectsSelected(BaseCommand command) {
            if (command == null) 
                return false;

            var selected = Main.Instance.SelectionHistory.Current;
            if (selected == null)
                return false;
           return command.HasEffectOn(selected);
        }

        /// <summary>
        /// Behandelt die Sammlung von Befehlen, wenn Elemente hinzugefügt oder entfernt werden.
        /// Wird aufgerufen, wenn sich die Sammlung ändert (z. B. bei Hinzufügen oder Entfernen von Befehlen).
        /// </summary>
        /// <param name="sender">Der Sender des Ereignisses (normalerweise die Sammlung selbst).</param>
        /// <param name="e">Die Ereignisdaten, die Informationen über die Änderung der Sammlung enthalten.</param>
        private void Commands_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // Überprüft, ob Elemente aus der Sammlung entfernt wurden
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove) {
                if (e.OldItems != null) {
                    // Entfernt die zugehörigen Einträge aus der _commandList
                    foreach (BaseCommand item in e.OldItems) {
                        var entry = _commandList.Where(e => e.Command == item).First();
                        if (entry != null) {
                            _commandList.Remove(entry);
                        }
                    }
                }
            }
            // Überprüft, ob Elemente zur Sammlung hinzugefügt wurden
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
                if (e.NewItems != null) {
                    // Fügt die neuen Befehle als neue CommandListEntry-Objekte hinzu
                    foreach (BaseCommand item in e.NewItems) {
                        _commandList.Add(new CommandListEntry(item));
                    }
                }
            }
            // massive Änderungen, neu laden
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset) {
                RefreshList();
            }

            // Benachrichtigt jedes Element in _commandList, dass sich die Eigenschaft "CanUndo" geändert hat
            foreach (var item in _commandList) {
                item.OnPropertyChanged("CanUndo");
            }
        }
    }
}
