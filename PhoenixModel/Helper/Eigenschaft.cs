﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Helper
{
    /// <summary>
    /// Represents a property with a name, value, and editable state.
    /// </summary>
    public class Eigenschaft
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Eigenschaft"/> class with a name, value, and editable state.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="wert">The value of the property. Can be null.</param>
        /// <param name="editable">Indicates whether the property is editable.</param>
        public Eigenschaft(string name, string? wert, bool editable)
        {
            Name = name;
            _wert = wert;
            IsEditable = editable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Eigenschaft"/> class with a name and a list of sub-properties.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="liste">The list of sub-properties.</param>
        public Eigenschaft(string name, List<Eigenschaft> liste)
        {
            Name = name;
            Eigenschaften = liste;
        }

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
        public string? Wert
        {
            get { return _wert; }
            set
            {
                if (_wert != value)
                {
                    _wert = value;
                    IsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the property is editable.
        /// </summary>
        public bool IsEditable { get; set; } = false;

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
