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

    public static class BlockingCollectionExtension{
        /// <summary>
        /// Entfernt einen Wert aus der Collection
        /// </summary>
        public static int Remove<T>(this BlockingCollection<T> collection, T item) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var tempQueue = new ConcurrentQueue<T>();
            int removedCount = 0;

            // Remove the specified item from the collection
            while (collection.TryTake(out T? currentItem)) {
                if (EqualityComparer<T>.Default.Equals(currentItem, item)) {
                    removedCount++;
                }
                else {
                    tempQueue.Enqueue(currentItem);
                }
            }

            // Re-add the remaining items back to the BlockingCollection
            foreach (var remainingItem in tempQueue) {
                collection.Add(remainingItem);
            }

            return removedCount;
        }

    }
}
