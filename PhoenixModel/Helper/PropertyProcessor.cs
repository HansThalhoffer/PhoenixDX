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
    /// <summary>
    /// Provides utility methods for processing properties and creating lists of <see cref="Eigenschaft"/> objects.
    /// </summary>
    public static class PropertyProcessor
    {
        /// <summary>
        /// List of direction identifiers used in property grouping.
        /// </summary>
        private static readonly string[] Directions = { "NW", "NO", "O", "SO", "SW", "W", "eigen", "eigene", "feind", "freund" };

        /// <summary>
        /// Retrieves the value of a specified property from an object.
        /// </summary>
        /// <param name="obj">The object containing the property.</param>
        /// <param name="property">The name of the property to retrieve.</param>
        /// <returns>The value of the property as a string, or an empty string if the property does not exist or its value is null.</returns>
        public static string GetPropertyValue(object? obj, string property)
        {
            var propertyInfo = obj?.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return propertyInfo?.GetValue(obj)?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the value of a constant field by its name from a specified type.
        /// </summary>
        /// <typeparam name="T">The type containing the constant field.</typeparam>
        /// <param name="name">The name of the constant field.</param>
        /// <returns>The value of the constant field as a string, or an empty string if the field does not exist.</returns>
        public static string GetConstValue<T>(string name)
        {
            var info = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return info?.GetValue(null)?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Creates a list of <see cref="Eigenschaft"/> objects based on the properties of a given data object.
        /// </summary>
        /// <typeparam name="T">The type of the data object.</typeparam>
        /// <param name="data">The data object to process.</param>
        /// <param name="toIgnore">An array of property names to ignore during processing.</param>
        /// <returns>A list of <see cref="Eigenschaft"/> objects representing the properties of the data object.</returns>
        public static List<Eigenschaft> CreateProperties<T>(T data, string[] toIgnore)
        {
            List<Eigenschaft> eigenschaften = new List<Eigenschaft>();
            AppendProperties(data, ref eigenschaften, toIgnore);
            return eigenschaften;
        }

        /// <summary>
        /// Appends a property to the result list as an <see cref="Eigenschaft"/>.
        /// </summary>
        /// <param name="result">The list of <see cref="Eigenschaft"/> objects to append to.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="isEditable">Indicates whether the property is editable.</param>
        static void AppendProperty(ref List<Eigenschaft> result, string name, object? value, bool isEditable)
        {
            if (value == null)
                return;
            if (value is IEigenschaftler)
            {
                foreach (var eigenschaft in ((IEigenschaftler)value).Eigenschaften)
                {
                    eigenschaft.Name = $"{name}.{eigenschaft.Name}";
                    result.Add(eigenschaft);
                }
            }
            else
                result.Add(new Eigenschaft(name, value.ToString(), isEditable));
        }

        /// <summary>
        /// Appends properties of a data object to the result list as <see cref="Eigenschaft"/> objects.
        /// </summary>
        /// <typeparam name="T">The type of the data object.</typeparam>
        /// <param name="data">The data object to process.</param>
        /// <param name="result">The list of <see cref="Eigenschaft"/> objects to append to.</param>
        /// <param name="toIgnore">An array of property names to ignore during processing.</param>
        static void AppendProperties<T>(T data, ref List<Eigenschaft> result, string[] toIgnore)
        {
            // Get all properties of the Data class
            var ar = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var safeproperties = ar.Where(p => p.Name != "Bezeichner" && p.Name != "Eigenschaften").ToArray();
            var properties = safeproperties.Where(prop => !toIgnore.Contains(prop.Name)).ToList();

            // Collect editable properties
            var editableProperties = typeof(T).GetProperties()
               .Where(prop => Attribute.IsDefined(prop, typeof(View.Editable)))
               .ToList();
            foreach (var property in editableProperties)
            {
                properties.Remove(property);
                AppendProperty(ref result, property.Name, property.GetValue(data), true);
            }

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
                    result.Add(new Eigenschaft(key, string.Join(" ", directionList), false));
                }
            }

            // Append remaining properties
            foreach (var property in properties)
            {
                AppendProperty(ref result, property.Name, property.GetValue(data), false);
            }
        }
    }

}
