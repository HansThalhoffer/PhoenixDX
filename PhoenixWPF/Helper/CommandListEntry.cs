using PhoenixModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Helper {
    /// <summary>
    /// Listenelement für die Auflistung im Datagrid. Damit unabhängig von der Collection in SharedData
    /// </summary>
    internal class CommandListEntry : INotifyPropertyChanged {
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
}
