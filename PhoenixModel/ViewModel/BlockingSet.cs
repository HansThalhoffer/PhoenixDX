using PhoenixModel.Commands.Parser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Ein BlockingSet kann zu einem key mehrere Einträge haben
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class BlockingSet<TKey, TValue> : INotifyCollectionChanged where TKey : notnull {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly ConcurrentDictionary<TKey, List<TValue>> _dict = [];
        // private readonly Dictionary<TKey, List<TValue>> _dict = new();

        public void Add([NotNull] TKey key, [NotNull]TValue value) {
            if (value == null) throw new ArgumentNullException(nameof(key), "value cannot be null.");
            if (!_dict.ContainsKey(key))
                _dict[key] = [];

            _dict[key].Add(value);
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
        }

        public IEnumerable<TValue> GetValues([NotNull] TKey key) {
            return _dict.TryGetValue(key, out var values) ? values : [];
        }

        public bool Remove([NotNull] TKey key, [NotNull] TValue value) {
            if (value == null) throw new ArgumentNullException(nameof(key), "value cannot be null.");

            if (_dict.TryGetValue(key, out var values)) {
                bool removed = values.Remove(value);
                if (values.Count == 0) 
                    _ = _dict.TryRemove(key, out _);
                if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
                return removed;
            }
            return false;
        }

        public IEnumerable<TValue> GetValues() {
            List<TValue> list = [];
            foreach(var kv in _dict.Values) {
                list.AddRange(kv);
            }
            return list;
        }

        public void RemoveAll(TKey key) => _dict.TryRemove(key, out _);

        public bool Contains(TKey key, TValue value) => _dict.ContainsKey(key) && _dict[key].Contains(value);

        public IEnumerable<TKey> Keys => _dict.Keys;
    }
}
