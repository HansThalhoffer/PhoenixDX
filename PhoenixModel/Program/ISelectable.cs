using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Program
{
    public interface ISelectable:IEigenschaftler
    {
        public void Select();
    }
}
