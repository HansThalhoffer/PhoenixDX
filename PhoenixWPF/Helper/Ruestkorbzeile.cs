using System.ComponentModel;
using PhoenixModel.Commands;
using PhoenixModel.View;

namespace PhoenixWPF.Helper {

    /// <summary>
    /// Eine Zeile der Rüstoberfläche: ein Rüstgut, sein Preis und wieviel davon gerüstet werden
    /// soll.
    ///
    /// Die Zeile rechnet nur ihre eigene Summe aus. Was der ganze Korb kostet und ob er erlaubt
    /// ist, sagt <see cref="RuestplatzView"/> - zwei Rechnungen für dieselbe Frage laufen sonst
    /// irgendwann auseinander.
    /// </summary>
    internal class Ruestkorbzeile : INotifyPropertyChanged {

        private readonly RuestplatzView.Rüstgut _gut;
        private int _anzahl = 0;

        public Ruestkorbzeile(RuestplatzView.Rüstgut gut) {
            _gut = gut;
        }

        /// <summary>Das Rüstgut, wie Befehl und Rüstungstabelle es führen</summary>
        public ConstructionElementType Art => _gut.Art;

        /// <summary>Die Beschriftung, zugleich das Wort im Befehl</summary>
        public string Name => _gut.Name;

        /// <summary>Die Kosten je Stück</summary>
        public string Preis => $"{_gut.Preis:N0}";

        /// <summary>
        /// Wieviel gerüstet werden soll. Negatives gibt es nicht - dafür gibt es das Abrüsten,
        /// und das ist ein anderer Befehl.
        /// </summary>
        public int Anzahl {
            get => _anzahl;
            set {
                int neu = value < 0 ? 0 : value;
                if (_anzahl == neu)
                    return;
                _anzahl = neu;
                OnPropertyChanged(nameof(Anzahl));
                OnPropertyChanged(nameof(Summe));
                Geändert?.Invoke(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Was diese Zeile kostet</summary>
        public string Summe => _anzahl <= 0 ? string.Empty : $"{(long)_anzahl * _gut.Preis:N0}";

        /// <summary>Wird gemeldet, wenn sich die Stückzahl geändert hat - die Seite rechnet neu</summary>
        public event System.EventHandler? Geändert;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
