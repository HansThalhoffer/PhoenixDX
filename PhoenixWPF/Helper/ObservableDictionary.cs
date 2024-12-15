using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Helper
{
    public class ObservableDictionary<TValue> : ObservableCollection<KeyValuePair<string, TValue>>
    {
        private readonly Dictionary<string, TValue> _dictionary = new Dictionary<string, TValue>();

        public ObservableDictionary()
        {
        }

        public void Add(string key, TValue value)
        {
            _dictionary[key] = value;
            Add(new KeyValuePair<string, TValue>(key, value));
        }

        public bool Remove(string key)
        {
            if (_dictionary.Remove(key))
            {
                var item = this.FirstOrDefault(kv => kv.Key.Equals(key));
                if (item.Key != null)
                    Remove(item);
                return true;
            }
            return false;
        }

        public TValue this[string key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value;
                var existingItem = this.FirstOrDefault(kv => kv.Key.Equals(key));
                if (existingItem.Key != null)
                {
                    Remove(existingItem);
                }
                Add(new KeyValuePair<string, TValue>(key, value));
            }
        }

        public ICollection<string> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
    }
}
