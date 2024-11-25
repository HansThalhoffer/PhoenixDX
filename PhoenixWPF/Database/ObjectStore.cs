using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Reflection;

namespace PhoenixWPF.Database
{
    public class ObjectStore
    {
        private Dictionary<Type, object> store = [];

        /// <summary>
        /// Adds an object to the store. If the type already exists, it will be overwritten.
        /// </summary>
        public void Add<T>(T obj)
        {
            if (obj == null)
                return;
            Type type = typeof(T);
            store[type] = obj;
        }

        /// <summary>
        /// Retrieves an object of the specified type from the store.
        /// </summary>
        public T Get<T>()
        {
            Type type = typeof(T);
            if (store.TryGetValue(type, out var obj))
            {
                return (T)obj;
            }
            throw new KeyNotFoundException($"Object of type {type} not found in the store.");
        }

        /// <summary>
        /// Serializes the store to a JSON string.
        /// </summary>
        public string Serialize()
        {
            var serializedObjects = new List<SerializedObject>();

            foreach (var kvp in store)
            {
                var type = kvp.Key;
                var obj = kvp.Value;
                var typeName = $"{type.FullName}, {type.Assembly.GetName().Name}";
                var jsonData = JsonSerializer.Serialize(obj, type);

                serializedObjects.Add(new SerializedObject
                {
                    TypeName = typeName,
                    JsonData = jsonData
                });
            }

            return JsonSerializer.Serialize(serializedObjects);
        }

        /// <summary>
        /// Deserializes the store from a JSON string.
        /// </summary>
        public void Deserialize(string json)
        {
            var serializedObjects = JsonSerializer.Deserialize<List<SerializedObject>>(json);
            if (serializedObjects != null && serializedObjects.Count > 0)
            {
                store.Clear();
                foreach (var item in serializedObjects)
                {
                    if (item == null || string.IsNullOrEmpty(item.TypeName) || string.IsNullOrEmpty(item.JsonData))
                        continue;
                    var type = ResolveType(item.TypeName);
                    if (type == null)
                    {
                        throw new Exception($"Type '{item.TypeName}' could not be found.");
                    }

                    var obj = JsonSerializer.Deserialize(item.JsonData, type);
                    if (obj != null)
                        store[type] = obj;
                }
            }
        }

        /// <summary>
        /// Resolves a type from its full name and assembly name.
        /// </summary>
        private Type? ResolveType(string typeName)
        {
            return Type.GetType(typeName);
        }

        private class SerializedObject
        {
            public string? TypeName { get; set; }
            public string? JsonData { get; set; }
        }
    }
}