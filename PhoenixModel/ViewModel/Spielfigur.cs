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

    
   
}
