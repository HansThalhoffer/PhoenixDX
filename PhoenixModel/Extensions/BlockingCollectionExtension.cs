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
        /// Entfernt genau dieses eine Objekt aus der Sammlung - verglichen wird die Referenz und
        /// nicht der Inhalt.
        ///
        /// Für Spielfiguren ist das der einzig richtige Weg: sie erben ihr Equals von
        /// <see cref="KleinfeldPosition"/> und gelten damit als gleich, sobald sie auf derselben
        /// Gemark stehen. <see cref="Remove"/> würde deshalb beim Entfernen einer Figur alle
        /// anderen Figuren desselben Feldes gleich mitnehmen.
        /// </summary>
        /// <returns>die Anzahl der entfernten Elemente, also 0 oder 1</returns>
        public static int RemoveInstance<T>(this BlockingCollection<T> collection, T item) where T : class {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            collection = ReopenSharedData(collection);

            var übrige = new ConcurrentQueue<T>();
            int entfernt = 0;

            while (collection.TryTake(out T? aktuell)) {
                if (entfernt == 0 && ReferenceEquals(aktuell, item))
                    entfernt++;
                else
                    übrige.Enqueue(aktuell);
            }

            foreach (var rest in übrige)
                collection.Add(rest);

            return entfernt;
        }

        /// <summary>
        /// Entfernt alle Werte aus der BlockingCollection, die dem übergebenen inhaltlich gleichen.
        /// </summary>
        /// <remarks>
        /// Achtung: verglichen wird über Equals. Bei Spielfiguren bedeutet das "steht auf derselben
        /// Gemark", weshalb dort <see cref="RemoveInstance"/> zu verwenden ist.
        /// </remarks>
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
