using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {

    [DebuggerDisplay("{CreateBezeichner()}")]
    public class KleinfeldPosition {
        /// <summary>
        ///  Provinz / Großfeld
        /// </summary>
        public virtual int gf { get; set; } = 0;
        /// <summary>
        ///  Gemark / Kleinfeld
        /// </summary>
        public virtual int kf { get; set; } = 0;

        public KleinfeldPosition() { }

        public KleinfeldPosition(int gf, int kf) {
            this.gf = gf;
            this.kf = kf;
        }
        
        public string CreateBezeichner() {
            return $"{gf}/{kf}";
        }

        public static string CreateBezeichner(int gf, int kf) {
            return $"{gf}/{kf}";
        }

        public static string CreateBezeichner(KleinfeldPosition pos) {
            return $"{pos.gf}/{pos.kf}";
        }

        public int Key {
            get { return gf * 100 + kf; }
        }
    }
}

