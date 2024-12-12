using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PhoenixModel.Helper
{
    public static class SharedData
    {
        public class BlockingDictionary<Tvalue> : ConcurrentDictionary<string, Tvalue>
        {

            public BlockingDictionary(int access, int capacity): base(access, capacity) { }

            bool _isAddingCompleted = false;
            bool _isBlocked = false;
            bool _isUpdated = false;

            public BlockingDictionary() { }
            // dann kommwen keine weiteren Elemente dazu
            public bool IsAddingCompleted { get => _isAddingCompleted; set => _isAddingCompleted = value; }
            public bool IsBlocked { get => _isBlocked; set => _isBlocked = value; }
            // Elemente haben sich geändert
            public bool IsUpdated { get => _isUpdated; set => _isUpdated = value; }
        }

        public static BlockingDictionary<Gemark>? Map { get; set; }
        public static BlockingCollection<Nation>? Nationen { get; set; }
    }
}
