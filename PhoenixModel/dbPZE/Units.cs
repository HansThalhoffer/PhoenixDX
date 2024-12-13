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
            ID = DatabaseConverter.ToInt32(reader[(int)Felder.ID]);
            nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            staerke_alt = DatabaseConverter.ToInt32(reader[(int)Felder.staerke_alt]);
            staerke = DatabaseConverter.ToInt32(reader[(int)Felder.staerke]);
            hf_alt = DatabaseConverter.ToInt32(reader[(int)Felder.hf_alt]);
            hf = DatabaseConverter.ToInt32(reader[(int)Felder.hf]);
            LKP_alt = DatabaseConverter.ToInt32(reader[(int)Felder.LKP_alt]);
            LKP = DatabaseConverter.ToInt32(reader[(int)Felder.LKP]);
            SKP_alt = DatabaseConverter.ToInt32(reader[(int)Felder.SKP_alt]);
            SKP = DatabaseConverter.ToInt32(reader[(int)Felder.SKP]);
            pferde_alt = DatabaseConverter.ToInt32(reader[(int)Felder.pferde_alt]);
            Pferde = DatabaseConverter.ToInt32(reader[(int)Felder.Pferde]);
            gf_von = DatabaseConverter.ToInt32(reader[(int)Felder.gf_von]);
            kf_von = DatabaseConverter.ToInt32(reader[(int)Felder.kf_von]);
            gf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.gf_nach]);
            kf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.kf_nach]);
            rp = DatabaseConverter.ToInt32(reader[(int)Felder.rp]);
            bp = DatabaseConverter.ToInt32(reader[(int)Felder.bp]);
            ph_xy = DatabaseConverter.ToString(reader[(int)Felder.ph_xy]);
            Garde = reader.GetBoolean((int)Felder.Garde);
            Befehl_bew = DatabaseConverter.ToString(reader[(int)Felder.Befehl_bew]);
            Befehl_ang = DatabaseConverter.ToString(reader[(int)Felder.Befehl_ang]);
            Befehl_erobert = DatabaseConverter.ToString(reader[(int)Felder.Befehl_erobert]);
            GS = DatabaseConverter.ToInt32(reader[(int)Felder.GS]);
            Kampfeinnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.Kampfeinnahmen]);
            x1 = DatabaseConverter.ToInt32(reader[(int)Felder.x1]);
            y1 = DatabaseConverter.ToInt32(reader[(int)Felder.y1]);
            x2 = DatabaseConverter.ToInt32(reader[(int)Felder.x2]);
            y2 = DatabaseConverter.ToInt32(reader[(int)Felder.y2]);
            x3 = DatabaseConverter.ToInt32(reader[(int)Felder.x3]);
            y3 = DatabaseConverter.ToInt32(reader[(int)Felder.y3]);
            hoehenstufen = DatabaseConverter.ToInt32(reader[(int)Felder.hoehenstufen]);
            schritt = DatabaseConverter.ToInt32(reader[(int)Felder.schritt]);
            x4 = DatabaseConverter.ToInt32(reader[(int)Felder.x4]);
            y4 = DatabaseConverter.ToInt32(reader[(int)Felder.y4]);
            x5 = DatabaseConverter.ToInt32(reader[(int)Felder.x5]);
            y5 = DatabaseConverter.ToInt32(reader[(int)Felder.y5]);
            x6 = DatabaseConverter.ToInt32(reader[(int)Felder.x6]);
            y6 = DatabaseConverter.ToInt32(reader[(int)Felder.y6]);
            x7 = DatabaseConverter.ToInt32(reader[(int)Felder.x7]);
            y7 = DatabaseConverter.ToInt32(reader[(int)Felder.y7]);
            x8 = DatabaseConverter.ToInt32(reader[(int)Felder.x8]);
            y8 = DatabaseConverter.ToInt32(reader[(int)Felder.y8]);
            x9 = DatabaseConverter.ToInt32(reader[(int)Felder.x9]);
            y9 = DatabaseConverter.ToInt32(reader[(int)Felder.y9]);
            auf_Flotte = DatabaseConverter.ToString(reader[(int)Felder.auf_Flotte]);
            Sonstiges = DatabaseConverter.ToString(reader[(int)Felder.Sonstiges]);
            spaltetab = DatabaseConverter.ToString(reader[(int)Felder.spaltetab]);
            fusmit = DatabaseConverter.ToString(reader[(int)Felder.fusmit]);
            Chars = DatabaseConverter.ToString(reader[(int)Felder.Chars]);
            bp_max = DatabaseConverter.ToInt32(reader[(int)Felder.bp_max]);
        }
    }




}
