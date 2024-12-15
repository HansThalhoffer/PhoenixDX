using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhoenixModel.Helper
{
    public class Eigenschaft
    {
        public Eigenschaft(string name, string? wert)
        {
            Name = name;
            Wert = wert;
        }

        public Eigenschaft(string name, List<Eigenschaft> liste)
        {
            Name = name;
            Eigenschaften = liste;
        }

        public string Name { get; set; } = string.Empty;
        public string? Wert { get; set; } = null;
        public List<Eigenschaft>? Eigenschaften { get; set; } = null;

    }


    public static class PropertyProcessor
    {
        
        private static readonly string[] Directions = { "NW", "NO", "O", "SO", "SW", "W","eigen","eigene","feind","freund" };


        public static string GetPropertyValue(object? obj, string property)
        {
            var propertyInfo = obj?.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return propertyInfo?.GetValue(obj)?.ToString() ?? string.Empty;
        }

        public static string GetConstValue<T>(string name)
        {
            var info = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return info?.GetValue(null)?.ToString() ?? string.Empty;
        }

        public static List<Eigenschaft> CreateProperties<T>(T data, string[] toIgnore)
        {
            List <Eigenschaft> eigenschaften= [];
            AppendProperties(data, ref eigenschaften, toIgnore);

            return eigenschaften;
        }


        static void AppendProperty(ref List<Eigenschaft> result, string name, object? value)
        {
            if (value ==  null)
                return;
            if (value is IEigenschaftler)
            {
               foreach (var eigenschaft in ((IEigenschaftler)value).Eigenschaften)
                {
                    eigenschaft.Name= $"{name}.{eigenschaft.Name}";
                    result.Add(eigenschaft);
                }
            }
            else
                result.Add(new Eigenschaft(name, value.ToString()));
        }

        static void AppendProperties<T>(T data, ref List<Eigenschaft> result, string[] toIgnore)
        {
            // Get all properties of the Data class
            var ar = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var safeproperties = ar.Where(p => p.Name != "Bezeichner" && p.Name != "Eigenschaften").ToArray();
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
                    result.Add(new Eigenschaft(key, string.Join(" ", directionList)));
                }
            }
            foreach (var property in properties)
            {
                AppendProperty(ref result, property.Name, property.GetValue(data));
                         
            }
        }
       
        public static void AppendMissingProperties<T>(T obj, ref List<Eigenschaft> result)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            properties = properties.Where(p => p.Name != "Bezeichner" && p.Name != "Properties").ToArray();

            foreach (var property in properties)
            {
                AppendProperty(ref result, property.Name, property.GetValue(obj));
            }
        }
    }
}
