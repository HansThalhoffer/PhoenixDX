using System.Text.Json;

namespace PhoenixWPF.Database {
    /// <summary>
    /// Eine generische Speicherklasse zur Verwaltung von Objekten verschiedener Typen.
    /// </summary>
    public class ObjectStore {
        private Dictionary<Type, object> store = [];

        /// <summary>
        /// Fügt ein Objekt in den Speicher ein. Falls der Typ bereits existiert, wird das Objekt überschrieben.
        /// </summary>
        /// <typeparam name="T">Der Typ des Objekts.</typeparam>
        /// <param name="obj">Das einzufügende Objekt.</param>
        public void Add<T>(T obj) {
            if (obj == null)
                return;
            Type type = typeof(T);
            store[type] = obj;
        }

        /// <summary>
        /// Ruft ein Objekt des angegebenen Typs aus dem Speicher ab.
        /// </summary>
        /// <typeparam name="T">Der Typ des abzurufenden Objekts.</typeparam>
        /// <returns>Das gespeicherte Objekt des angegebenen Typs.</returns>
        /// <exception cref="KeyNotFoundException">Wird ausgelöst, wenn kein Objekt des Typs gefunden wurde.</exception>
        public T Get<T>() {
            Type type = typeof(T);
            if (store.TryGetValue(type, out var obj)) {
                return (T)obj;
            }
            throw new KeyNotFoundException($"Object of type {type} not found in the store.");
        }

        /// <summary>
        /// Serialisiert den gesamten Speicherinhalt in eine JSON-Zeichenkette.
        /// </summary>
        /// <returns>Eine JSON-Zeichenkette, die den Zustand des Speichers enthält.</returns>
        public string Serialize() {
            var serializedObjects = new List<SerializedObject>();

            foreach (var kvp in store) {
                var type = kvp.Key;
                var obj = kvp.Value;
                var typeName = $"{type.FullName}, {type.Assembly.GetName().Name}";
                var jsonData = JsonSerializer.Serialize(obj, type);

                serializedObjects.Add(new SerializedObject {
                    TypeName = typeName,
                    JsonData = jsonData
                });
            }

            return JsonSerializer.Serialize(serializedObjects);
        }

        /// <summary>
        /// Deserialisiert eine JSON-Zeichenkette und stellt den Speicherinhalt wieder her.
        /// </summary>
        /// <param name="json">Die JSON-Zeichenkette mit gespeicherten Objekten.</param>
        /// <exception cref="Exception">Wird ausgelöst, wenn ein Typ nicht gefunden werden kann.</exception>
        public void Deserialize(string json) {
            var serializedObjects = JsonSerializer.Deserialize<List<SerializedObject>>(json);
            if (serializedObjects != null && serializedObjects.Count > 0) {
                store.Clear();
                foreach (var item in serializedObjects) {
                    if (item == null || string.IsNullOrEmpty(item.TypeName) || string.IsNullOrEmpty(item.JsonData))
                        continue;
                    var type = ResolveType(item.TypeName);
                    if (type == null) {
                        throw new Exception($"Type '{item.TypeName}' could not be found.");
                    }

                    var obj = JsonSerializer.Deserialize(item.JsonData, type);
                    if (obj != null)
                        store[type] = obj;
                }
            }
        }

        /// <summary>
        /// Löst einen Typ anhand seines vollständigen Namens und des Assembly-Namens auf.
        /// </summary>
        /// <param name="typeName">Der vollständige Name des Typs.</param>
        /// <returns>Der aufgelöste Typ oder null, falls er nicht gefunden wurde.</returns>
        private Type? ResolveType(string typeName) {
            return Type.GetType(typeName);
        }

        /// <summary>
        /// Interne Klasse zur Speicherung serialisierter Objekte mit Typinformationen.
        /// </summary>
        private class SerializedObject {
            /// <summary>
            /// Der vollständige Name des Typs.
            /// </summary>
            public string? TypeName { get; set; }

            /// <summary>
            /// Die JSON-Darstellung des Objekts.
            /// </summary>
            public string? JsonData { get; set; }
        }
    }
}