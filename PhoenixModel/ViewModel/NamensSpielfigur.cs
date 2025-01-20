using PhoenixModel.dbZugdaten;
using PhoenixModel.ExternalTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Die Spielfiguren unterscheiden sich in zwei Typen: Truppen- und NamensSpielfigur.
    /// Diese wurden in der Altanwendung noch in weitere Tabellen aufgeteilt, obwohl viele Daten identisch sind.
    /// Hier werden diese wieder weitgehend zusammengefasst.
    /// </summary>
    public abstract class NamensSpielfigur : Spielfigur {
        /// <summary>
        /// Gibt die aktuelle Stärke der Spielfigur zurück.
        /// </summary>
        public override string Stärke { get { return $"{GP_akt.ToString("n0")} (max {GP_ges.ToString("n0")})"; } }
        
        /// <summary>
        /// Maximale Anzahl an Gutpunkten (GP) aktuell
        /// </summary>
        public int GP_ges { get; set; } = 0;

        /// <summary>
        /// Aktuelle Anzahl an Gutpunkten (GP) während des laufenden Zuges.
        /// </summary>
        public int GP_akt { get; set; } = 0;

        /// <summary>
        /// Maximale Anzahl an Gutpunkten (GP) vor Beginn des aktuellen Zuges.
        /// </summary>
        public int GP_ges_alt { get; set; } = 0;

        /// <summary>
        /// Anzahl an Gutpunkten (GP) vor Beginn des aktuellen Zuges.
        /// </summary>
        public int GP_akt_alt { get; set; } = 0;
        
        /// <summary>
        /// Die Beschriftung der Spielfigur.
        /// </summary>
        public string Beschriftung { get; set; } = string.Empty;
        
        /// <summary>
        /// Da die Datenbank für Charname zwei Schreibweisen kennt und die Abbildung 1:1 zur AccessDB sein soll, wird dieser Wert gedoppelt.
        /// </summary>
        private string _charname = string.Empty;
        public string Charname { get => _charname; set { if (value != null) _charname = value; } }
        internal string charname { get => _charname; set { if (value != null) _charname = value; } }
        
        /// <summary>
        /// Name des Spielers, dem diese Spielfigur gehört.
        /// </summary>
        internal string Spielername { get; set; } = string.Empty;

        public static string HeaderBeschriftung = "Beschriftung";
        public static string HeaderCharakterName = "Charaktername";
        public static string HeaderSpielerName = "Spielername";

        /// <summary>
        /// Teleportpunkte vor dem aktuellen Zug.
        /// </summary>
        public int tp_alt { get; set; }
        
        /// <summary>
        /// Aktuelle Teleportpunkte.
        /// </summary>
        public int tp { get; set; }
        
        /// <summary>
        /// Großfeld-Koordinaten vor dem Teleport.
        /// </summary>
        public int Teleport_gf_von { get; set; }
        
        /// <summary>
        /// Kleinfeld-Koordinaten vor dem Teleport.
        /// </summary>
        public int Teleport_kf_von { get; set; }
        
        /// <summary>
        /// Großfeld-Koordinaten nach dem Teleport.
        /// </summary>
        public int Teleport_gf_nach { get; set; }
        
        /// <summary>
        /// Kleinfeld-Koordinaten nach dem Teleport.
        /// </summary>
        public int Teleport_kf_nach { get; set; }
        
        /// <summary>
        /// Magischer Befehl der Spielfigur.
        /// </summary>
        public string Befehl_magie { get; set; } = string.Empty;
        
        /// <summary>
        /// Teleport-Befehl der Spielfigur.
        /// </summary>
        public string Befehl_Teleport { get; set; } = string.Empty;
        
        /// <summary>
        /// Bann-Befehl der Spielfigur.
        /// </summary>
        public string Befehl_bannt { get; set; } = string.Empty;
        
        /// <summary>
        /// Weitere Informationen zur Spielfigur.
        /// </summary>
        public string sonstiges { get; set; } = string.Empty;
        
        /// <summary>
        /// Zugehörige Einheit der Spielfigur.
        /// </summary>
        public string einheit { get; set; } = string.Empty;

        public bool IsSpielerFigur {
            get {
                if (this is Zauberer)
                    return Typ == FigurType.CharakterZauberer;
                /* if (Beschriftung.StartsWith("HF"))
                    return false;*/
                if (string.IsNullOrEmpty(Spielername) == false) 
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gibt eine Liste mit Eigenschaften der Spielfigur zurück.
        /// </summary>
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
