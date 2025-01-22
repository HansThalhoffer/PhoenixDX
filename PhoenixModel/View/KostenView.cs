using PhoenixModel.dbCrossRef;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.View {
    public static class KostenView {

        public static Kosten? GetKosten(string element) {
            if (string.IsNullOrEmpty(element)) return null; 
            if (SharedData.Kosten == null) return null;
            return SharedData.Kosten.Where(kosten => kosten.Unittyp == element).FirstOrDefault();
        }
    }
}
