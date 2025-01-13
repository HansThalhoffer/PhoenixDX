using PhoenixModel.dbPZE;
using PhoenixModel.Program;

namespace PhoenixModel.View
{
    public static class NationenView
    {
        private static Nation _default = new Nation("DEFAULT");

        public static Nation GetNationFromString(string name)
        {
            if (SharedData.Nationen == null || string.IsNullOrEmpty(name))
                return _default;
            name = name.ToLower();
            foreach (var nation in SharedData.Nationen)
            {
                if (nation.Alias == null)
                    continue;
                if (nation.Alias.Contains(name))
                    return nation;
            }
            return _default;
        }
    }
}
