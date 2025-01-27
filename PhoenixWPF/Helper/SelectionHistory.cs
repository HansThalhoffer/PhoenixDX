using PhoenixModel.Program;
using PhoenixWPF.Program;
using System.ComponentModel;

namespace PhoenixWPF.Helper {

    /// <summary>
    /// Verwaltet eine Auswahlhistorie für ISelectable-Objekte und ermöglicht die Navigation zurück und vorwärts.
    /// </summary>
    public class SelectionHistory : List<ISelectable> {
        private int _index = -1;

        /// <summary>
        /// Navigiert in der Historie einen Schritt zurück.
        /// </summary>
        /// <returns>Das vorherige ausgewählte Objekt oder null, falls nicht möglich.</returns>
        public ISelectable? NavigateBack() {
            if (_index < 0)
                return null;
            if (_index > 0)
                _index--;
            return _OnSelectionChange(Current);
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich die Auswahl ändert.
        /// </summary>
        /// <param name="selected">Das neue ausgewählte Objekt.</param>
        /// <returns>Das ausgewählte Objekt.</returns>
        protected ISelectable? _OnSelectionChange(ISelectable? selected) {
            if (selected != null) {
                OnPropertyChanged(nameof(Current));
                OnPropertyChanged(nameof(CurrentDisplay));
                Main.Instance.PropertyDisplay?.Display(selected);
            }
            return selected;
        }

        /// <summary>
        /// Navigiert in der Historie einen Schritt vorwärts.
        /// </summary>
        /// <returns>Das nächste ausgewählte Objekt oder null, falls nicht möglich.</returns>
        public ISelectable? NavigateForward() {
            if (_index < this.Count - 1)
                _index++;
            return _OnSelectionChange(Current);
        }

        /// <summary>
        /// Gibt eine formatierte Zeichenkette der aktuellen Auswahl zurück.
        /// </summary>
        public string CurrentDisplay {
            get {
                var current = Current;
                if (current != null) {
                    return $"{current.GetType().Name}: {current.Bezeichner}";
                }
                return "No selection";
            }
        }

        /// <summary>
        /// Überprüft, ob eine Navigation zurück möglich ist.
        /// </summary>
        /// <returns>True, wenn eine Rücknavigation möglich ist, sonst false.</returns>
        public bool CanNavigateBack() {
            return Current != null && _index > 0;
        }

        /// <summary>
        /// Überprüft, ob eine Navigation vorwärts möglich ist.
        /// </summary>
        /// <returns>True, wenn eine Vorwärtsnavigation möglich ist, sonst false.</returns>
        public bool CanNavigateForward() {
            return Current != null && _index < this.Count - 1;
        }

        /// <summary>
        /// Ereignis zur Benachrichtigung über Eigenschaftsänderungen.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Benachrichtigt über eine Eigenschaftsänderung.
        /// </summary>
        /// <param name="propertyName">Der Name der geänderten Eigenschaft.</param>
        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gibt das aktuell ausgewählte Objekt zurück.
        /// Beim Setzen wird die Historie bei Bedarf abgeschnitten, falls zurück navigiert wurde.
        /// </summary>
        public ISelectable? Current {
            get {
                if (_index < 0)
                    return null;
                return this[_index];
            }
            set {
                if (value != null && value.Select()) {
                    if (Current != value) {
                        if (_index >= 0) {
                            // Entfernt alle späteren Einträge, falls bereits zurück navigiert wurde.
                            while (this.Count > _index + 1)
                                this.RemoveAt(this.Count - 1);
                        }
                        this.Add(value);
                        NavigateForward();
                    }
                    _OnSelectionChange(Current);
                }
            }
        }
    }
}
