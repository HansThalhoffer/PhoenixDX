using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbErkenfara
{
    public class Figur : IDatabaseTable
    {
        public const string TableName = "Units";
        string IDatabaseTable.TableName => TableName;

        public int? ID { get; set; }
        public int? nummer { get; set; }
        public int? staerke_alt { get; set; }
        public int? staerke { get; set; }
        public int? hf_alt { get; set; }
        public int? hf { get; set; }
        public int? LKP_alt { get; set; }
        public int? LKP { get; set; }
        public int? SKP_alt { get; set; }
        public int? SKP { get; set; }
        public int? pferde_alt { get; set; }
        public int? Pferde { get; set; }
        public int? gf_von { get; set; }
        public int? kf_von { get; set; }
        public int? gf_nach { get; set; }
        public int? kf_nach { get; set; }
        public int? rp { get; set; }
        public int? bp { get; set; }
        public string? ph_xy { get; set; }
        public bool? Garde { get; set; }
        public string? Befehl_bew { get; set; }
        public string? Befehl_ang { get; set; }
        public string? Befehl_erobert { get; set; }
        public int? GS { get; set; }
        public int? Kampfeinnahmen { get; set; }
        public int? x1 { get; set; }
        public int? y1 { get; set; }
        public int? x2 { get; set; }
        public int? y2 { get; set; }
        public int? x3 { get; set; }
        public int? y3 { get; set; }
        public int? hoehenstufen { get; set; }
        public int? schritt { get; set; }
        public int? x4 { get; set; }
        public int? y4 { get; set; }
        public int? x5 { get; set; }
        public int? y5 { get; set; }
        public int? x6 { get; set; }
        public int? y6 { get; set; }
        public int? x7 { get; set; }
        public int? y7 { get; set; }
        public int? x8 { get; set; }
        public int? y8 { get; set; }
        public int? x9 { get; set; }
        public int? y9 { get; set; }
        public string? auf_Flotte { get; set; }
        public string? Sonstiges { get; set; }
        public string? spaltetab { get; set; }
        public string? fusmit { get; set; }
        public string? Chars { get; set; }
        public int? bp_max { get; set; }

     
    }




}
