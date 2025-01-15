using PhoenixModel.ExternalTables;
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
    public abstract class TruppenSpielfigur : Spielfigur {
        public override string Stärke { get { return staerke.ToString("n0"); } }

        public int staerke_alt { get; set; } = 0;
        public int staerke { get; set; } = 0;
        public int hf_alt { get; set; } = 0;
        public int hf { get; set; } = 0;

        /// <summary>
        /// da die Datenbank für lkp_alt zwei Schreibweisen kennt und die Abbildung 1:1 sein soll zur AccessDB, wird hier gedoppelt
        /// </summary>
        public int lkp_alt { get; set; }
        public int LKP_alt { get => lkp_alt; set { lkp_alt = value; } }
        public int LKP { get; set; }

        /// <summary>
        /// da die Datenbank für skp_alt zwei Schreibweisen kennt und die Abbildung 1:1 sein soll zur AccessDB, wird hier gedoppelt
        /// </summary>
        public int skp_alt { get; set; }
        public int SKP_alt { get => skp_alt; set { skp_alt = value; } }
        public int SKP { get; set; }
        public int pferde_alt { get; set; }
        public int Pferde { get; set; }
        public bool Garde { get; set; }
        public int isbanned { get; set; }


        public string auf_Flotte { get; set; } = string.Empty;
        public string Sonstiges { get; set; } = string.Empty;
        public string spaltetab { get; set; } = string.Empty;
        public string fusmit { get; set; } = string.Empty;
        public string chars { get; set; } = string.Empty;
        public string Chars { get => chars; set { chars = value; } }

        public string Befehl_bew { get; set; } = string.Empty;
        public string Befehl_ang { get; set; } = string.Empty;
        public string Befehl_erobert { get; set; } = string.Empty;

        public int x10 { get; set; }
        public int y10 { get; set; }
        public int x11 { get; set; }
        public int y11 { get; set; }
        public int x12 { get; set; }
        public int y12 { get; set; }
        public int x13 { get; set; }
        public int y13 { get; set; }
        public int x14 { get; set; }
        public int y14 { get; set; }
        public int x15 { get; set; }
        public int y15 { get; set; }
        public int x16 { get; set; }
        public int y16 { get; set; }
        public int x17 { get; set; }
        public int y17 { get; set; }
        public int x18 { get; set; }
        public int y18 { get; set; }
        public int x19 { get; set; }
        public int y19 { get; set; }

        public override List<Eigenschaft> Eigenschaften {
            get {
                List<Eigenschaft> list = [];

                list.Add(new Eigenschaft("ID", Nummer.ToString(), false, this));
                string typ = BaseTyp.ToString();
                string str = Stärke;
                string katapult = string.Empty;
                int lineCount = 1;
                if (LKP > 0) {
                    typ += BaseTyp == FigurType.Schiff ? $"\r+ Leichte Kriegsschiffe" : $"\r+ Leichte Katapulte";
                    katapult += $"\r+ {LKP} LK";
                    lineCount++;
                }
                if (SKP > 0) {
                    typ += BaseTyp == FigurType.Schiff ? $"\r+ Schwere Kriegsschiffe" : $"\r+ Schwere Katapulte";
                    katapult += $"\r+ {SKP} SK";
                    lineCount++;
                }
                list.Add(new Eigenschaft("Typ", typ, false, this));
                list.Add(new Eigenschaft("Stärke", str, false, this));
                list.Add(new Eigenschaft("Katapulte", katapult, false, this));
                list.Add(new Eigenschaft("Koordinaten", CreateBezeichner(), false, this));
                list.Add(new Eigenschaft(NamensSpielfigur.HeaderBeschriftung, Titel, true, this));
                list.Add(new Eigenschaft(NamensSpielfigur.HeaderCharakterName, CharakterName, true, this));
                list.Add(new Eigenschaft(NamensSpielfigur.HeaderSpielerName, SpielerName, true, this));
                return list;
            }
        }
    }

}
