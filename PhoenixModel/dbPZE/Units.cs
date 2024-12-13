using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    public class Units : IDatabaseTable
    {
        public const string TableName = "Units";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => ID.ToString();

        public int ID { get; set; } = 0;
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

        public enum Felder
        {
            ID,
            nummer,
            staerke_alt,
            staerke,
            hf_alt,
            hf,
            LKP_alt,
            LKP,
            SKP_alt,
            SKP,
            pferde_alt,
            Pferde,
            gf_von,
            kf_von,
            gf_nach,
            kf_nach,
            rp,
            bp,
            ph_xy,
            Garde,
            Befehl_bew,
            Befehl_ang,
            Befehl_erobert,
            GS,
            Kampfeinnahmen,
            x1,
            y1,
            x2,
            y2,
            x3,
            y3,
            hoehenstufen,
            schritt,
            x4,
            y4,
            x5,
            y5,
            x6,
            y6,
            x7,
            y7,
            x8,
            y8,
            x9,
            y9,
            auf_Flotte,
            Sonstiges,
            spaltetab,
            fusmit,
            Chars,
            bp_max
        }

        public void Load(DbDataReader reader)
        {
            ID = reader.GetInt32((int)Felder.ID);
            nummer = reader.GetInt32((int)Felder.nummer);
            staerke_alt = reader.GetInt32((int)Felder.staerke_alt);
            staerke = reader.GetInt32((int)Felder.staerke);
            hf_alt = reader.GetInt32((int)Felder.hf_alt);
            hf = reader.GetInt32((int)Felder.hf);
            LKP_alt = reader.GetInt32((int)Felder.LKP_alt);
            LKP = reader.GetInt32((int)Felder.LKP);
            SKP_alt = reader.GetInt32((int)Felder.SKP_alt);
            SKP = reader.GetInt32((int)Felder.SKP);
            pferde_alt = reader.GetInt32((int)Felder.pferde_alt);
            Pferde = reader.GetInt32((int)Felder.Pferde);
            gf_von = reader.GetInt32((int)Felder.gf_von);
            kf_von = reader.GetInt32((int)Felder.kf_von);
            gf_nach = reader.GetInt32((int)Felder.gf_nach);
            kf_nach = reader.GetInt32((int)Felder.kf_nach);
            rp = reader.GetInt32((int)Felder.rp);
            bp = reader.GetInt32((int)Felder.bp);
            ph_xy = reader.GetString((int)Felder.ph_xy);
            Garde = reader.GetBoolean((int)Felder.Garde);
            Befehl_bew = reader.GetString((int)Felder.Befehl_bew);
            Befehl_ang = reader.GetString((int)Felder.Befehl_ang);
            Befehl_erobert = reader.GetString((int)Felder.Befehl_erobert);
            GS = reader.GetInt32((int)Felder.GS);
            Kampfeinnahmen = reader.GetInt32((int)Felder.Kampfeinnahmen);
            x1 = reader.GetInt32((int)Felder.x1);
            y1 = reader.GetInt32((int)Felder.y1);
            x2 = reader.GetInt32((int)Felder.x2);
            y2 = reader.GetInt32((int)Felder.y2);
            x3 = reader.GetInt32((int)Felder.x3);
            y3 = reader.GetInt32((int)Felder.y3);
            hoehenstufen = reader.GetInt32((int)Felder.hoehenstufen);
            schritt = reader.GetInt32((int)Felder.schritt);
            x4 = reader.GetInt32((int)Felder.x4);
            y4 = reader.GetInt32((int)Felder.y4);
            x5 = reader.GetInt32((int)Felder.x5);
            y5 = reader.GetInt32((int)Felder.y5);
            x6 = reader.GetInt32((int)Felder.x6);
            y6 = reader.GetInt32((int)Felder.y6);
            x7 = reader.GetInt32((int)Felder.x7);
            y7 = reader.GetInt32((int)Felder.y7);
            x8 = reader.GetInt32((int)Felder.x8);
            y8 = reader.GetInt32((int)Felder.y8);
            x9 = reader.GetInt32((int)Felder.x9);
            y9 = reader.GetInt32((int)Felder.y9);
            auf_Flotte = reader.GetString((int)Felder.auf_Flotte);
            Sonstiges = reader.GetString((int)Felder.Sonstiges);
            spaltetab = reader.GetString((int)Felder.spaltetab);
            fusmit = reader.GetString((int)Felder.fusmit);
            Chars = reader.GetString((int)Felder.Chars);
            bp_max = reader.GetInt32((int)Felder.bp_max);
        }
    }




}
