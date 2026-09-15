using PhoenixModel.Commands;
using PhoenixModel.ViewModel;
using PhoenixWPF.Helper;
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
        /// Ein Klick auf einen Eintrag führt die Karte zu der Gemark, auf die der Befehl zeigt -
        /// bei einer Bewegung zum Ziel, denn dort steht die Figur.
        ///
        /// Zeigt der Befehl auf keine Gemark, passiert nichts: die Karte springt lieber gar nicht
        /// als irgendwohin.
        /// </summary>
        private void CommandDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                if (CommandDataGrid.SelectedItem is not CommandListEntry eintrag)
                    return;
                var ziel = PhoenixModel.View.BefehlszielView.GetZielfeld(eintrag.Command);
                if (ziel == null)
                    return;
                Program.Main.Instance.Spiel?.SelectGemark(ziel);
            }
            catch (Exception ex) {
                Program.Absturzbericht.Berichte("Beim Springen zur Gemark ging etwas schief", ex, tödlich: false);
            }
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
                    // Entfernt die zugehörigen Einträge aus der _commandList.
                    //
                    // FirstOrDefault, nicht First: die Anzeige führt nicht zwingend jeden Befehl -
                    // sie wird bei einem Wechsel der Auswahl neu aufgebaut und zeigt dann nur, was
                    // zur Auswahl gehört. Ein Befehl, der dabei herausgefallen ist, liess First()
                    // werfen, und zwar in einem Ereignis der Sammlung: die Ausnahme kam mitten im
                    // Zurücknehmen hoch und beendete die Anwendung.
                    foreach (BaseCommand befehl in e.OldItems) {
                        var entry = _commandList.FirstOrDefault(eintrag => eintrag.Command == befehl);
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
