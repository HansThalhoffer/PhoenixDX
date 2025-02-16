using PhoenixModel.Database;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhoenixModel.Helper {
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
        /// sets the value of a static field by its name from a specified type.
        /// </summary>
        /// <typeparam name="T">The type containing the constant field.</typeparam>
        /// <param name="name">The name of the constant field.</param>
        /// <returns>The value of the constant field as a string, or an empty string if the field does not exist.</returns>
        public static void SetStaticValue<T>(string name, string value) {
            var info = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            info?.SetValue(null, value);
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

        private static string GetExpandedValue(string name, object? value)
        {
            if (value == null)
                return string.Empty;

            string? strVal = value.ToString();
            if (value != null)
            {
                if (name.StartsWith("Ruestort") && value.GetType() == typeof(int))
                {
                    int val = (int)value;
                    var rüstort = BauwerkeView.GetRuestortReferenz(val);
                    var einnahmen = (rüstort != null)?EinnahmenView.GetGebäudeEinnahmen(rüstort):0;
                    strVal = $"{strVal} ({rüstort?.Bauwerk}, Einnahmen: {einnahmen})";
                }
                else if (name == "Baupunkte" && value.GetType() == typeof(int))
                {
                    int val = (int)value;
                    var rüstort = BauwerkeView.GetRuestortNachBaupunkten(val);
                    if (rüstort != null)
                        strVal = $"{strVal} ({rüstort.Bauwerk})";
                }

            }
            return string.IsNullOrEmpty(strVal) ? string.Empty : strVal;
        }

        /// <summary>
        /// Appends a property to the result list as an <see cref="Eigenschaft"/>.
        /// </summary>
        /// <param name="result">The list of <see cref="Eigenschaft"/> objects to append to.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="isEditable">Indicates whether the property is editable.</param>
        static void AppendProperty(ref List<Eigenschaft> result, string name, object? value, bool isEditable, IEigenschaftler? source)
        {
            if (value == null)
                return;


            /*if (isEditable == false) // was nicht editierbar ist und empty oder 0 wird nicht angezeigt
            {
                if (value is string && string.IsNullOrEmpty((string)value))
                    return;
                if (value is int && (int)value == 0)
                    return;
            }*/
            if (value is IEigenschaftler)
            {
                foreach (var eigenschaft in ((IEigenschaftler)value).Eigenschaften)
                {
                    eigenschaft.Name = $"{name}.{eigenschaft.Name}";
                    result.Add(eigenschaft);
                }
            }
            else
            {
                result.Add(new Eigenschaft(name, GetExpandedValue(name, value), isEditable, source));
            }
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
            try
            {
                IEigenschaftler? source = data as IEigenschaftler;
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
                    IDatabaseTable? table = data as IDatabaseTable;
                    bool canChange = table != null ? Autorisation.IsAllowedToChange(table, property.Name) : false;
                    AppendProperty(ref result, property.Name, property.GetValue(data), canChange, source);
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
                        result.Add(new Eigenschaft(key, string.Join(" ", directionList), false, source));
                    }
                }

                // Append remaining properties
                foreach (var property in properties)
                {
                    AppendProperty(ref result, property.Name, property.GetValue(data), false, source);
                }
            }
            catch (Exception ex)
            {
                ProgramView.LogError($"Beim Erstellen der Eigenschaften zu {data} ist ein Fehler passiert", ex.Message);
            }
        }

        /// <summary>
        /// holt einen int val aus einem Objekt, wenn er in der übergebenen Liste als Property vorhanden ist
        /// wird in den Spielfiguren benutzt, um die verschiedenen Werte in eine Liste zu bringen
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static int GetIntValueIfExists(object obj, string[] propertyNames)
        {
            if (obj == null || propertyNames == null || !propertyNames.Any())
            {
                return 0;
            }

            foreach (var propertyName in propertyNames)
            {
                var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.PropertyType == typeof(int))
                {
                    var value = property.GetValue(obj);
                    if (value is int intValue)
                    {
                        return intValue;
                    }
                }
            }

            return 0;
        }


        /// <summary>
        /// holt einen int val aus einem Objekt, wenn er in der übergebenen Liste als Property vorhanden ist
        /// wird in den Spielfiguren benutzt, um die verschiedenen Werte in eine Liste zu bringen
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static int GetIntValueIfExists(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return 0;
            }

            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(int))
            {
                var value = property.GetValue(obj);
                if (value is int intValue)
                {
                    return intValue;
                }
            }

            return 0;
        }

        /// <summary>
        /// aktualisert das Objekt mit dem Wert der Eigenschaft, sofern die Eigenschaft das Attribut Editable hat
        /// wird zum Speichern von Daten verwendet, die der Benutzer ändern darf
        /// Die geänderten Objekte werden dann in der Queue zum Speichern abgelegt, wo sie dann on Idle abgeholt werden
        /// </summary>
        /// <param name="changed"></param>
        public static void UpdateSource(Eigenschaft changed)
        {
            if (changed.Source == null)
            {
                ProgramView.LogError($"Eigenschaft {changed.Name} ohne Quelle kann nicht aktualisiert werden", "Es wurde das Aktualisieren einer Eigenschaft aufgerufen, die keine Quelle enthält. Das funktioniert nicht.");
                return;
            }
            if (changed.IsEditable == false)
            {
                ProgramView.LogError($"Eigenschaft {changed.Name} ist nicht aktualisierbar", "Es wurde das Aktualisieren einer Eigenschaft aufgerufen, die nicht bearbeitet werden darf");
                return;
            }
            if (changed.IsChanged == false)
            {
                ProgramView.LogError($"Eigenschaft {changed.Name} ist nicht geändert", "Es wurde das Aktualisieren einer Eigenschaft aufgerufen, die nicht geändert wurde");
                return;
            }

            string name = changed.Name;
            if (name.Contains('.'))
                name = name.Substring(name.IndexOf(".") + 1);

            // Collect editable properties
            var editableProperties = changed.Source.GetType().GetProperties()
               .Where(prop => Attribute.IsDefined(prop, typeof(View.Editable)))
               .ToList();
            foreach (var property in editableProperties)
            {
                if (property.Name == name)
                {
                    try
                    {
                        if (property.PropertyType == typeof(string))
                            property.SetValue(changed.Source, changed.Wert);
                        else if (property.PropertyType == typeof(int))
                        {
                            var i = Convert.ToInt32(changed.Wert);
                            property.SetValue(changed.Source, i);
                        }
                        else
                            ProgramView.LogError($"Eigenschaft mit Typ {property.DeclaringType} nicht aktualisierbar", "Es wurde das Aktualisieren einer Eigenschaft aufgerufen, deren Typ nicht bekannt ist");
                        if (changed.Source is IDatabaseTable table)
                        {
                            SharedData.StoreQueue.Enqueue(table);
                        }
                    }
                    catch (Exception ex)
                    {
                        ProgramView.LogError($"Eigenschaft {changed.Name} konnte wegen einem Fehler nicht aktualisiert werden", ex.Message);
                    }


                    return;
                }
            }
            ProgramView.LogError($"Eigenschaft {changed.Name} wurde nicht als bearbeitbar im Objekt markiert", "Es wurde das Aktualisieren einer Eigenschaft aufgerufen, die das Attribut [[Editable]] nicht besitzt");
        }
    }

}
