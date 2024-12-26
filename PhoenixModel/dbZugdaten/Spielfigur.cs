using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbZugdaten
{
 
    public abstract class Spielfigur: GemarkPosition
    {
        public abstract FigurType Typ { get; }

        // wenn die Spielfigur bewegt wurde, dann steht die aktuelle Position in gf/kf_nach
        public override int gf 
        { 
            get
            {
                if (gf_nach > 0 )
                    return gf_nach;
                return gf_von;
            }
            set
            {
                gf_nach = value;
            }
        } 

        public override int kf
        {
            get
            {
                if (kf_nach > 0)
                    return kf_nach;
                return kf_von;
            }
            set
            {
                kf_nach = value;
            }
        }

        public int Nummer { get; set; }
        public string Bezeichner => $"{Typ.ToString()} {Nummer.ToString()}";
        public dbPZE.Nation? Reich { get; set; } = null;

        public int gf_von { get; set; }
        public int kf_von { get; set; }
        public int rp { get; set; }
        public int bp { get; set; }

        private int _gfNach;
        private int _kfNach;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int gf_nach
        {
            get => _gfNach;
            set
            {
                if (_gfNach != value)
                {
                    _gfNach = value;
                    OnPropertyChanged();
                }
            }
        }

        public int kf_nach
        {
            get => _kfNach;
            set
            {
                if (_kfNach != value)
                {
                    _kfNach = value;
                    OnPropertyChanged();
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Load(DbDataReader reader)
        {
            // wenn es aus der Zugdaten Datenbank kommt, dann ist es eine Spielfigur des ausgewählten Reiches
            AssignToSelectedReich();
        }

        public void AssignToSelectedReich()
        {
            if (ViewModel.SelectedNation == null)
                throw new InvalidOperationException("Zuext muss ein Reich ausgewählt sein");
            this.Reich = ViewModel.SelectedNation;
        }
        // wird nicht benötigt
        public string? ph_xy { get; set; }
    }
}
