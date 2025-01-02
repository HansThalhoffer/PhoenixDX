using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View
{
    public static class NationenView
    {
        public static Nation? GetNationFromString(string name)
        {
            if (SharedData.Nationen == null)
                return null;
            foreach (var nation in SharedData.Nationen)
            {
                if (nation.Alias == null)
                    continue;
                foreach (var alias  in nation.Alias)
                {
                    if (name.ToUpper() == alias.ToUpper())
                    {
                        return nation;
                    }
                }

            }
            return null;
        }
    }
}
