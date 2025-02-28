using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Extensions {
    public static class BlockingCollectionExtension {
        /// <summary>
        /// öffnet eine BlockingCollection aus SharedData, wenn sie zuvor durch AddingComplete geschlossen wurde
        /// technisch legt sie eine in SharedData wieder an. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static BlockingCollection<T> ReopenSharedData<T>(this BlockingCollection<T> collection) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            
            if (collection.IsAddingCompleted == false) // already open
                return collection;
            
            Type sharedDataType = typeof(SharedData);
            // Find the static field that holds this instance
            FieldInfo? field = sharedDataType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(f => f.GetValue(null) == collection);

            if (field == null) {
                throw new InvalidOperationException("The BlockingCollection instance was not found in SharedData.");
            }

            // Create a new BlockingCollection and copy existing items
            var newCollection = new BlockingCollection<T>();

            while (collection.TryTake(out T? item)) {
                newCollection.Add(item);
            }

            // Replace the static field with the new instance
            field.SetValue(null, newCollection);
            return newCollection;
        }

        /// <summary>
        /// Entfernt einen Wert aus der BlockingCollection.
        /// </summary>
        /// <typeparam name="T">Der Typ der gespeicherten Werte.</typeparam>
        /// <param name="collection">Die BlockingCollection, aus der der Wert entfernt werden soll.</param>
        /// <param name="item">Das zu entfernende Element.</param>
        /// <returns>Die Anzahl der entfernten Elemente.</returns>
        public static int Remove<T>(this BlockingCollection<T> collection, T item) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            collection = ReopenSharedData(collection);

            var tempQueue = new ConcurrentQueue<T>();
            int removedCount = 0;

            // Entfernt das angegebene Element aus der Sammlung
            while (collection.TryTake(out T? currentItem)) {
                if (EqualityComparer<T>.Default.Equals(currentItem, item)) {
                    removedCount++;
                }
                else {
                    tempQueue.Enqueue(currentItem);
                }
            }

            // Fügt die restlichen Elemente wieder in die BlockingCollection ein
            foreach (var remainingItem in tempQueue) {
                collection.Add(remainingItem);
            }

            return removedCount;
        }
    }
}
