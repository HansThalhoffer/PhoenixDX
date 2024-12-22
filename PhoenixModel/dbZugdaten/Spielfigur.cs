using PhoenixModel.ExternalTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbZugdaten
{
    public abstract class Spielfigur
    {
        public abstract FigurType Type { get; }

        public int Nummer { get; set; }
        public string Bezeichner => $"{Type.ToString()} {Nummer.ToString()}";

        public int gf_von { get; set; }
        public int kf_von { get; set; }
        public int gf_nach { get; set; }
        public int kf_nach { get; set; }
        public int rp { get; set; }
        public int bp { get; set; }
        
        // wird nicht benötigt
        public string? ph_xy { get; set; }
    }
}
