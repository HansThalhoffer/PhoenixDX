using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Die Spielfiguren unterscheiden sich in zwei Typen: Truppen- und NamensSpielfigur
    /// Diese wurden in der Altanwendung noch in weitere Tabellen aufgeteilt, obwohl viele Daten identisch sind
    /// Hier werden diese wieder weitgehend zusammengefasst
    /// </summary>
    public abstract class NamensSpielfigur : Spielfigur {
        public override string Stärke { get { return $"{GP_akt.ToString("n0")} (max {GP_ges.ToString("n0")})"; } }
        public int GP_ges { get; set; } = 0;
        public int GP_akt { get; set; } = 0;
        public int GP_ges_alt { get; set; } = 0;
        public int GP_akt_alt { get; set; } = 0;
        internal string Beschriftung { get; set; } = string.Empty;
        /// <summary>
        /// da die Datenbank für Charname zwei Schreibweisen kennt und die Abbildung 1:1 sein soll zur AccessDB, wird hier gedoppelt
        /// </summary>
        private string _charname = string.Empty;
        internal string Charname { get => _charname; set { if (value != null) _charname = value; } }
        internal string charname { get => _charname; set { if (value != null) _charname = value; } }
        internal string Spielername { get; set; } = string.Empty;

        public static string HeaderBeschriftung = "Beschriftung";
        public static string HeaderCharakterName = "Charaktername";
        public static string HeaderSpielerName = "Spielername";

        public int tp_alt { get; set; }
        public int tp { get; set; }
        public int Teleport_gf_von { get; set; }
        public int Teleport_kf_von { get; set; }
        public int Teleport_gf_nach { get; set; }
        public int Teleport_kf_nach { get; set; }
        public string Befehl_magie { get; set; } = string.Empty;
        public string Befehl_Teleport { get; set; } = string.Empty;
        public string Befehl_bannt { get; set; } = string.Empty;
        public string sonstiges { get; set; } = string.Empty;
        public string einheit { get; set; } = string.Empty;

        public override List<Eigenschaft> Eigenschaften {
            get {
                List<Eigenschaft> list = [];
                list.Add(new Eigenschaft("ID", Nummer.ToString(), false, this));
                string typ = Typ.ToString();
                string str = Stärke;
                list.Add(new Eigenschaft("Typ", typ, false, this));
                list.Add(new Eigenschaft("Stärke", str, false, this));
                list.Add(new Eigenschaft("Katapulte", string.Empty, false, this));
                list.Add(new Eigenschaft("Koordinaten", CreateBezeichner(), false, this));
                list.Add(new Eigenschaft(HeaderBeschriftung, Titel, true, this));
                list.Add(new Eigenschaft(HeaderCharakterName, CharakterName, true, this));
                list.Add(new Eigenschaft(HeaderSpielerName, SpielerName, true, this));
                return list;
            }
        }
    }
}
