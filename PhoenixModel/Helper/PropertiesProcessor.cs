using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Helper
{
    public class PropertiesProcessor
    {
        private static readonly string[] Directions = { "NW", "NO", "O", "SO", "SW", "W","eigen","eigene","feind","freund" };


        public static Dictionary<string, string> CreateProperties<T>(T data, string[] toIgnore)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            AppendProperties(data, ref result, toIgnore);
            
            return result;
        }

        public static void AppendProperties<T>(T data, ref Dictionary<string, string> result, string[] toIgnore)
        {
            // Get all properties of the Data class
            var ar = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var safeproperties = ar.Where(p => p.Name != "Bezeichner" && p.Name != "Properties").ToArray();
            var properties = safeproperties.Where(prop => !toIgnore.Contains(prop.Name)).ToList();

            // Group properties by their prefix (e.g., Fluss, Wall, etc.)
            var groupedProperties = properties
                .Where(prop => prop.PropertyType == typeof(int?))
                .GroupBy(prop => prop.Name.Split('_')[0]);

            // Iterate through each group (Fluss, Wall, etc.)
            foreach (var group in groupedProperties)
            {
                var key = group.Key; // e.g., Fluss, Wall, etc.
                var directionList = new List<string>();

                foreach (var direction in Directions)
                {
                    var propertyName = $"{key}_{direction}";
                    var property = group.FirstOrDefault(p => p.Name == propertyName);
                    if (property != null)
                    {
                        var value = property.GetValue(data) as int?;
                        properties.Remove(property);
                        if (value.HasValue && value.Value > 0)
                        {
                            directionList.Add(direction);
                        }
                    }
                }

                if (directionList.Any())
                {
                    result[key] = string.Join(" ", directionList);
                } 
            }
            foreach (var property in properties)
            {
                if (!result.ContainsKey(property.Name))
                {
                    var value = property.GetValue(data)?.ToString() ?? string.Empty;
                    if (value != string.Empty)
                        result[property.Name] = value;
                }
            }
        }

        public static void AppendMissingProperties<T>(T obj, ref Dictionary<string, string> dictionary)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            properties = properties.Where(p => p.Name != "Bezeichner" && p.Name != "Properties").ToArray();

            foreach (var property in properties)
            {
                if (!dictionary.ContainsKey(property.Name))
                {
                    var value = property.GetValue(obj)?.ToString() ?? "null";
                    dictionary[property.Name] = value;
                }
            }
        }
    }
}
