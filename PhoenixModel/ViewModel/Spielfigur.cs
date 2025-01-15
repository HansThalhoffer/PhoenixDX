using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.ViewModel {

    public abstract class Spielfigur : KleinfeldPosition, ISelectable {
        public abstract FigurType Typ { get; }
        public abstract FigurType BaseTyp { get; }
        public abstract string Stärke { get; }

        public int GS { get; set; } = 0;
        public int GS_alt { get; set; } = 0;
        public int Kampfeinnahmen { get; set; } = 0;
        public int Kampfeinnahmen_alt { get; set; } = 0;
        public int Nummer { get; set; } = 0;
        public string Bezeichner => $"{Typ.ToString()} {Nummer.ToString()}";
        public dbPZE.Nation? Nation { get; set; } = null;

        public int gf_von { get; set; } = 0;
        public int kf_von { get; set; } = 0;
        public int rp { get; set; } = 0;
        public int bp { get; set; } = 0;
        public int bp_max { get; set; }

        private int _gfNach;
        private int _kfNach;
        public int Kosten { get; set; } = 0;

        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }
        public int x3 { get; set; }
        public int y3 { get; set; }
        public int hoehenstufen { get; set; }
        public int schritt { get; set; }
        public int x4 { get; set; }
        public int y4 { get; set; }
        public int x5 { get; set; }
        public int y5 { get; set; }
        public int x6 { get; set; }
        public int y6 { get; set; }
        public int x7 { get; set; }
        public int y7 { get; set; }
        public int x8 { get; set; }
        public int y8 { get; set; }
        public int x9 { get; set; }
        public int y9 { get; set; }

        // wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        public override int gf {
            get {
                if (gf_nach > 0)
                    return gf_nach;
                return gf_von;
            }
            set {
                gf_nach = value;
            }
        }

        public override int kf {
            get {
                if (kf_nach > 0)
                    return kf_nach;
                return kf_von;
            }
            set {
                kf_nach = value;
            }
        }
        public int Heerführer {
            get {
                if (this is TruppenSpielfigur truppe)
                    return truppe.hf;
                return 0;
            }
        }

        public int LeichteKP {
            get {
                if (this is TruppenSpielfigur truppe)
                    return truppe.LKP;
                return 0;
            }
        }
        public int SchwereKP {
            get {
                if (this is TruppenSpielfigur truppe)
                    return truppe.SKP;
                return 0;
            }
        }

        [View.Editable]
        public string CharakterName {
            get {
                if (this is NamensSpielfigur figur)
                    return figur.Charname;
                return string.Empty;
            }
            set {
                if (this is NamensSpielfigur figur && value != null)
                    figur.Charname = value;
            }
        }

        [View.Editable]
        public string SpielerName {
            get {
                if (this is NamensSpielfigur figur)
                    return figur.Spielername;
                return string.Empty;
            }
            set {
                if (this is NamensSpielfigur figur && value != null)
                    figur.Spielername = value;
            }
        }

        [View.Editable]
        public string Titel {
            get {
                if (this is NamensSpielfigur figur)
                    return figur.Beschriftung;
                return string.Empty;
            }
            set {
                if (this is NamensSpielfigur figur && value != null)
                    figur.Beschriftung = value;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int gf_nach {
            get => _gfNach;
            set {
                if (_gfNach != value) {
                    _gfNach = value;
                    OnPropertyChanged();
                }
            }
        }

        public int kf_nach {
            get => _kfNach;
            set {
                if (_kfNach != value) {
                    _kfNach = value;
                    OnPropertyChanged();
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Load(DbDataReader reader) {
            // wenn es aus der Zugdaten Datenbank kommt, dann ist es eine Spielfigur des ausgewählten Reiches
            AssignToSelectedReich();
        }

        public abstract void Save(DbCommand command);


        public void AssignToSelectedReich() {
            if (ProgramView.SelectedNation == null)
                throw new InvalidOperationException("Zuext muss ein Nation ausgewählt sein");
            Nation = ProgramView.SelectedNation;
        }

        public bool Select() {
            if (SharedData.Map == null)
                return false;
            if (ProgramView.SelectedNation == null || Nation == null)
                return false;
            // die Spielfigur gehört evtl. zum Nation des aktuellen Nutzers und kann daher ausgeählt werden
            return ProgramView.SelectedNation == Nation;
        }

        public bool Edit() {
            return Select();
        }

        // wird nicht benötigt
        public string? ph_xy { get; set; }

        private static readonly string[] PropertiestoIgnore = ["DatabaseName", "Bezeichner", "Nation", "GS_alt", "Kampfeinnahmen_alt", "Nummer", "gf_von", "kf_von", "gf", "kf", "gf_nach", "kf_nach", "ph_xy", "Key"];
        public virtual List<Eigenschaft> Eigenschaften {
            get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore);
        }
    }

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
