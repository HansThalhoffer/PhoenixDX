using PhoenixModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {

    [DebuggerDisplay("{CreateBezeichner()}")]
    public class KleinfeldPosition: IEquatable<KleinfeldPosition> {
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

        public bool Equals(KleinfeldPosition? other) {
            return (other != null && other.gf == gf && other.kf == kf);
        }

        /// <summary>
        /// Überprüft die Gleichheit mit einem anderen Objekt.
        /// </summary>
        /// <param name="obj">Das zu vergleichende Objekt.</param>
        /// <returns>True, wenn die Objekte gleich sind, sonst false.</returns>
        public override bool Equals(object? obj) {
            return Equals(obj as KleinfeldPosition);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Key);
        }
        public int Key {
            get { return gf * 100 + kf; }
        }
    }
}

