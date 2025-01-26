using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Ein BlockingSet kann zu einem key mehrere Einträge haben
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class BlockingSet<TKey, TValue> : ObservableCollection<TValue>  where TKey : notnull {
        private readonly ConcurrentDictionary<TKey, List<TValue>> _dict = [];

        public new void Add(TValue value) => throw new ArgumentNullException("value cannot be null.");
        
        protected override void ClearItems() {
            base.ClearItems();
            _dict.Clear();
        }

        public void Add([NotNull] TKey key, [NotNull] TValue value) {
            if (value == null) throw new ArgumentNullException(nameof(key), "value cannot be null.");
            if (!_dict.ContainsKey(key))
                _dict[key] = [];
            base.Add(value);
            _dict[key].Add(value);
        }

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

        public IEnumerable<TValue>? Get(TKey key) {
            _dict.TryGetValue(key, out var values);
            return values;
        }
    }
}
