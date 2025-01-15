using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class Editable: Attribute
    {
        // Optionally, you can add properties or parameters for additional metadata
        public string? Description { get; }

        public Editable(string? description = null)
        {
            Description = description;
        }
    }
}
