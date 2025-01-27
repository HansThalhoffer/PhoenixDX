using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Ein BlockingSet kann mehrere Einträge für einen Schlüssel speichern.
    /// </summary>
    /// <typeparam name="TKey">Der Typ des Schlüssels.</typeparam>
    /// <typeparam name="TValue">Der Typ der Werte.</typeparam>
    public class BlockingSet<TKey, TValue> : ObservableCollection<TValue> where TKey : notnull {
        /// <summary>
        /// Internes Dictionary zur Speicherung der Werte.
        /// </summary>
        private readonly ConcurrentDictionary<TKey, List<TValue>> _dict = [];

        /// <summary>
        /// Standardmäßige Add-Methode ist nicht erlaubt.
        /// </summary>
        /// <param name="value">Der hinzuzufügende Wert.</param>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
        public new void Add(TValue value) => throw new ArgumentNullException("value cannot be null.");

        /// <summary>
        /// Löscht alle Einträge aus der Sammlung und dem internen Dictionary.
        /// </summary>
        protected override void ClearItems() {
            base.ClearItems();
            _dict.Clear();
        }

        /// <summary>
        /// Fügt einen Wert mit einem zugehörigen Schlüssel hinzu.
        /// </summary>
        /// <param name="key">Der Schlüssel des Eintrags.</param>
        /// <param name="value">Der hinzuzufügende Wert.</param>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
        public void Add([NotNull] TKey key, [NotNull] TValue value) {
            if (value == null) throw new ArgumentNullException(nameof(key), "value cannot be null.");
            if (!_dict.ContainsKey(key))
                _dict[key] = [];
            base.Add(value);
            _dict[key].Add(value);
        }

        /// <summary>
        /// Entfernt einen Wert aus der Sammlung und dem zugehörigen Schlüssel.
        /// </summary>
        /// <param name="value">Der zu entfernende Wert.</param>
        /// <returns>Gibt true zurück, wenn der Wert entfernt wurde, andernfalls false.</returns>
        /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
        public new bool Remove([NotNull] TValue value) {
            if (value == null) throw new ArgumentNullException("value cannot be null.");
            base.Remove(value);
            foreach (var kv in _dict) {
                var list = kv.Value;
                if (list.Remove(value)) {
                    if (list.Count == 0) {
                        _ = _dict.TryRemove(kv.Key, out _);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ruft alle Werte ab, die einem bestimmten Schlüssel zugeordnet sind.
        /// </summary>
        /// <param name="key">Der Schlüssel, für den Werte abgerufen werden sollen.</param>
        /// <returns>Eine Sammlung der Werte oder null, falls keine vorhanden sind.</returns>
        public IEnumerable<TValue>? Get(TKey key) {
            _dict.TryGetValue(key, out var values);
            return values;
        }
    }
}
