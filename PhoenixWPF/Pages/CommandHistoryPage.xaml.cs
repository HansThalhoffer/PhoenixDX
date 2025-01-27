using PhoenixModel.Commands.Parser;
using PhoenixModel.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixWPF.Pages {
    /// <summary>
    /// Interaktionslogik für CommanndHistoryPage.xaml
    /// </summary>
    public partial class CommanndHistoryPage : Page {

        /// <summary>
        /// Listenelement für die Auflistung im Datagrid. Damit unabhängig von der Collection in SharedData
        /// </summary>
        class CommandListEntry : INotifyPropertyChanged {
            // Private fields
            private BaseCommand _command;

            /// <summary>
            /// Ruft den Befehl-String des zugeordneten Befehls ab.
            /// </summary>
            public string CommandString {
                get { return _command.CommandString; }
            }

            /// <summary>
            /// Gibt an, ob der zugeordnete Befehl ausgeführt wurde.
            /// </summary>
            public bool IsExecuted {
                get { return _command.IsExecuted; }
            }

            /// <summary>
            /// Gibt an, ob der zugeordnete Befehl rückgängig gemacht werden kann.
            /// </summary>
            public bool CanUndo {
                get { return _command.CanUndo; }
            }

            /// <summary>
            /// Ruft den zugeordneten Befehl ab.
            /// </summary>
            public BaseCommand Command {
                get { return _command; }
            }

            /// <summary>
            /// Konstruktor für die CommandListEntry-Klasse.
            /// Initialisiert die Instanz mit einem gegebenen Befehl.
            /// </summary>
            /// <param name="command">Der Befehl, der zugewiesen wird.</param>
            public CommandListEntry(BaseCommand command) {
                _command = command ?? throw new ArgumentNullException(nameof(command), "Der Befehl darf nicht null sein.");
            }

            /// <summary>
            /// Event, das ausgelöst wird, wenn sich eine Eigenschaft ändert.
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;

            /// <summary>
            /// Hilfsmethode, die das PropertyChanged-Ereignis auslöst.
            /// </summary>
            /// <param name="propertyName">Der Name der geänderten Eigenschaft.</param>
            public void OnPropertyChanged(string propertyName) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Liste zum Bind an das DataGrid
        /// </summary>
        private ObservableCollection<CommandListEntry> _commandList = [];

        /// <summary>
        /// Konstruktor, kopiert Einträge und hängt sich an den change event von SharedData.Commands
        /// </summary>
        public CommanndHistoryPage() {
            InitializeComponent();
            SharedData.Commands.CollectionChanged += Commands_CollectionChanged;
            foreach (BaseCommand item in SharedData.Commands) {
                _commandList.Add(new CommandListEntry(item));
            }
            CommandDataGrid.ItemsSource = _commandList;
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
                _commandList.Clear();
                foreach (BaseCommand item in SharedData.Commands) {
                    _commandList.Add(new CommandListEntry(item));
                }
            }

            // Benachrichtigt jedes Element in _commandList, dass sich die Eigenschaft "CanUndo" geändert hat
            foreach (var item in _commandList) {
                item.OnPropertyChanged("CanUndo");
            }
        }

        /// <summary>
        /// Aktualisiert die Anzeige des DataGrids und erzwingt eine erneute Datenbindung.
        /// Diese Methode wird verwendet, um sicherzustellen, dass alle Änderungen in der UI reflektiert werden.
        /// </summary>
        private void Refresh() {
            CommandDataGrid.Items.Refresh();
        }

        /// <summary>
        /// Behandelt das Klicken des Undo-Buttons und führt den Undo-Vorgang für den entsprechenden Befehl aus.
        /// Falls der Undo-Vorgang erfolgreich war, wird die Anzeige aktualisiert.
        /// </summary>
        /// <param name="sender">Der Sender des Ereignisses (der Button, der geklickt wurde).</param>
        /// <param name="e">Die Ereignisdaten, die Informationen über das Click-Ereignis enthalten.</param>
        private void UndoButton_Click(object sender, RoutedEventArgs e) {
            // Überprüft, ob der Sender ein Button ist und ob der DataContext des Buttons eine CommandListEntry-Instanz ist
            if (sender is Button button && button.DataContext is CommandListEntry entry) {
                // Führt den Undo-Vorgang aus, wenn der Befehl rückgängig gemacht werden kann
                if (SharedData.Commands.Undo(entry.Command) == true) {
                    // Aktualisiert die Anzeige, nachdem der Undo-Vorgang erfolgreich war
                    Refresh();
                }
            }
        }

    }
}
