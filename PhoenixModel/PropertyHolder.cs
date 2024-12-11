using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel
{
    public interface IPropertyHolder
    {
        public abstract Dictionary<string, string> Properties
        {
            get;
        }

        public abstract string Bezeichner
        {
            get;
        }
    }
}
