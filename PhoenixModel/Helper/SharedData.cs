using PhoenixModel.Karte;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PhoenixModel.Helper
{
    public static class SharedData
    {
        public static BlockingCollection<Dictionary<string, Gemark>>? Map { get; set; }
    }
}
