using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.Extensions {
    public static class BlockingCollectionExtension {
        public static void Reopen<T>(this BlockingCollection<T> collection) {
            collection = new BlockingCollection<T>();
        }
    }
}
