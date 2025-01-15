using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    public class BlockingDictionary<Tvalue> : ConcurrentDictionary<string, Tvalue> {
        public BlockingDictionary(int access, int capacity) : base(access, capacity) { }

        bool _isAddingCompleted = false;
        bool _isBlocked = false;
        bool _isUpdated = false;

        public BlockingDictionary() { }
        // dann kommwen keine weiteren Elemente dazu
        public void CompleteAdding() { IsAddingCompleted = true; }
        public bool Add(string key, Tvalue obj) { return TryAdd(key, obj); }
        public bool IsAddingCompleted { get => _isAddingCompleted; set => _isAddingCompleted = value; }
        public bool IsBlocked { get => _isBlocked; set => _isBlocked = value; }
        // Elemente haben sich geändert
        public bool IsUpdated { get => _isUpdated; set => _isUpdated = value; }
    }
}
