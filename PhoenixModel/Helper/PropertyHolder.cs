using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Helper.PropertyProcessor;

namespace PhoenixModel.Helper
{
    public interface IEigenschaftler
    {
        public abstract List<Eigenschaft> Eigenschaften
        {
            get;
        }

        public abstract string Bezeichner
        {
            get;
        }
    }
}
