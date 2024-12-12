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

            public BlockingDictionary() { }
            public bool IsAddingCompleted { get => _isAddingCompleted; set => _isAddingCompleted = value; }
        }

        public static BlockingDictionary<Gemark>? Map { get; set; }
        public static BlockingCollection<Nation>? Nationen { get; set; }
    }
}
