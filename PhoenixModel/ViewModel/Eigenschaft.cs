using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.ExternalTables.EinwohnerUndEinnahmenTabelle;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Represents a property with a name, value, and editable state.
    /// </summary>
    public class Eigenschaft {

        /// <summary>
        /// Initializes a new instance of the <see cref="Eigenschaft"/> class with a name, value, and editable state.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="wert">The value of the property. Can be null.</param>
        /// <param name="editable">Indicates whether the property is editable.</param>
        public Eigenschaft(string name, string? wert, bool editable, IEigenschaftler? source) {
            Name = name;
            _wert = wert;
            var val = SortValue;
            if (val != null && val.ToString() == wert) {
                _wert = Convert.ToInt32(val).ToString("n0");
            }

            IsEditable = editable;
            Source = source;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Eigenschaft"/> class with a name and a list of sub-properties.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="liste">The list of sub-properties.</param>
        public Eigenschaft(string name, List<Eigenschaft> liste, IEigenschaftler? source) {
            Name = name;
            Eigenschaften = liste;
            Source = source;
        }

        public IEigenschaftler? Source { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Backing field for the <see cref="Wert"/> property.
        /// </summary>
        private string? _wert = null;

        /// <summary>
        /// Gets or sets the value of the property. Setting this property marks it as changed.
        /// </summary>
        public string? Wert {
            get { return _wert; }
            set {
                if (_wert != value) {
                    _wert = value;
                    IsChanged = true;
                }
            }
        }

        /// <summary>
        /// erzeugt einen long integer Wert aus der Eigenschaft, um die Daten in einem Datagrid sortieren zu können
        /// column = new DataGridTextColumn
        /// {
        ///     Header = name,
        ///     Binding = new System.Windows.Data.Binding($"Eigenschaften[{index}].Wert"),
        ///     IsReadOnly = !eig.IsEditable,
        ///     SortMemberPath = $"Eigenschaften[{index}].SortValue" 
        /// };
        /// </summary>
        public long? SortValue {
            get {
                if (string.IsNullOrWhiteSpace(_wert))
                    return int.MinValue; // Handle null or empty strings.

                // Regular expression to extract leading numeric part
                var match = System.Text.RegularExpressions.Regex.Match(_wert, @"^[\d.,]+");

                if (match.Success) {
                    string numericPart = match.Value;

                    // Parse the numeric part considering culture-specific formats
                    if (decimal.TryParse(numericPart,
                                          System.Globalization.NumberStyles.Number,
                                          System.Globalization.CultureInfo.CurrentCulture,
                                          out decimal result)) {
                        return (int)Math.Floor(result); // Convert to integer if needed
                    }
                }
                return int.MinValue;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the property is editable.
        /// </summary>
        public bool IsEditable { get; } = false;

        /// <summary>
        /// Gets a value indicating whether the property has been changed.
        /// </summary>
        public bool IsChanged { get; private set; } = false;

        /// <summary>
        /// Gets or sets the list of sub-properties.
        /// </summary>
        public List<Eigenschaft>? Eigenschaften { get; set; } = null;
    }
}
