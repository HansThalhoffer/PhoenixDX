using PhoenixModel.CrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static PhoenixModel.Helper.SharedData;

namespace PhoenixModel.Helper
{
    public static class SharedData
    {
        public interface IBlockable
        {
            bool IsBlocked { get; set; }
        }


        public class BlockGuard : IDisposable
        {
            IBlockable? dictionary = null;
            public BlockGuard(IBlockable dic) { 
                dictionary = dic; 
                dictionary.IsBlocked = true;
            }
            public void Dispose() 
            {
                if (dictionary != null) 
                    dictionary.IsBlocked = false;
            }
        }

        public interface IUpdatable
        {
            bool IsUpdated { get; set; }
        }


        public class UpdateGuard : IDisposable
        {
            IUpdatable? dictionary = null;
            public UpdateGuard(IUpdatable dic)
            {
                dictionary = dic;
            }
            public void Dispose()
            {
                if (dictionary != null)
                    dictionary.IsUpdated = true;
            }
        }

        public class BlockingDictionary<Tvalue> : ConcurrentDictionary<string, Tvalue>, IBlockable 
        {
            public BlockingDictionary(int access, int capacity): base(access, capacity) { }

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

        // Karte
        public static BlockingDictionary<Gemark>? Map = null;
        public static BlockingDictionary<Gebäude>? Gebäude = null; // bauwerkliste

        // PZE
        public static BlockingCollection<Nation>? Nationen = null;

        // crossref
        // public static BlockingDictionary<Bauwerk>? BauwerkeRef= null; // kosten für wälle etc - steht aber auch in der Kosten Tabelle
        public static BlockingDictionary<Rüstort>? RüstortReferenz = null;
    }
}
