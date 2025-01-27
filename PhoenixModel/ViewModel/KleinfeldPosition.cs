using PhoenixModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Repräsentiert eine Position innerhalb eines Kleinfeldes.
    /// </summary>
    [DebuggerDisplay("{CreateBezeichner()}")]
    public class KleinfeldPosition : IEquatable<KleinfeldPosition> {
        /// <summary>
        /// Die Provinz oder das Großfeld, in dem sich die Position befindet.
        /// </summary>
        public virtual int gf { get; set; } = 0;

        /// <summary>
        /// Das Gemark oder Kleinfeld innerhalb der Provinz.
        /// </summary>
        public virtual int kf { get; set; } = 0;

        /// <summary>
        /// Standardkonstruktor.
        /// </summary>
        public KleinfeldPosition() { }

        /// <summary>
        /// Erstellt eine neue Instanz einer KleinfeldPosition mit bestimmten Werten.
        /// </summary>
        /// <param name="gf">Die Provinz- oder Großfeldnummer.</param>
        /// <param name="kf">Die Gemark- oder Kleinfeldnummer.</param>
        public KleinfeldPosition(int gf, int kf) {
            this.gf = gf;
            this.kf = kf;
        }

        /// <summary>
        /// Erstellt eine Bezeichnung für die Kleinfeldposition im Format "gf/kf".
        /// </summary>
        /// <returns>Eine Zeichenkette, die die Position beschreibt.</returns>
        public string CreateBezeichner() {
            return $"{gf}/{kf}";
        }

        /// <summary>
        /// Erstellt eine Bezeichnung für eine gegebene gf- und kf-Kombination.
        /// </summary>
        /// <param name="gf">Die Provinz- oder Großfeldnummer.</param>
        /// <param name="kf">Die Gemark- oder Kleinfeldnummer.</param>
        /// <returns>Eine Zeichenkette im Format "gf/kf".</returns>
        public static string CreateBezeichner(int gf, int kf) {
            return $"{gf}/{kf}";
        }

        /// <summary>
        /// Erstellt eine Bezeichnung für eine gegebene KleinfeldPosition.
        /// </summary>
        /// <param name="pos">Die Kleinfeldposition.</param>
        /// <returns>Eine Zeichenkette im Format "gf/kf".</returns>
        public static string CreateBezeichner(KleinfeldPosition pos) {
            return $"{pos.gf}/{pos.kf}";
        }

        /// <summary>
        /// Überprüft, ob zwei KleinfeldPosition-Objekte gleich sind.
        /// </summary>
        /// <param name="other">Das zu vergleichende KleinfeldPosition-Objekt.</param>
        /// <returns>True, wenn die Positionen übereinstimmen, sonst false.</returns>
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

        /// <summary>
        /// Gibt einen eindeutigen HashCode für diese Position zurück.
        /// </summary>
        /// <returns>Der HashCode der Position.</returns>
        public override int GetHashCode() {
            return HashCode.Combine(Key);
        }

        /// <summary>
        /// Berechnet einen eindeutigen Schlüssel für die Position.
        /// </summary>
        public int Key {
            get { return gf * 100 + kf; }
        }

        /// <summary>
        /// Gibt die Position als Zeichenkette im Format "gf/kf" zurück.
        /// </summary>
        /// <returns>Eine Zeichenkette, die die Position beschreibt.</returns>
        public override string? ToString() => CreateBezeichner();
    }
}
