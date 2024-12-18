using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Data.Common;

namespace PhoenixModel.dbZugdaten
{
    public class Krieger : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Krieger";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int Nummer { get; set; }
        public int staerke_alt { get; set; }
        public int staerke { get; set; }
        public int hf_alt { get; set; }
        public int hf { get; set; }
        public int LKP_alt { get; set; }
        public int LKP { get; set; }
        public int SKP_alt { get; set; }
        public int SKP { get; set; }
        public int pferde_alt { get; set; }
        public int Pferde { get; set; }
        public int gf_von { get; set; }
        public int kf_von { get; set; }
        public int gf_nach { get; set; }
        public int kf_nach { get; set; }
        public int rp { get; set; }
        public int bp { get; set; }
        public string? ph_xy { get; set; }
        public bool Garde { get; set; }
        public string? Befehl_bew { get; set; }
        public string? Befehl_ang { get; set; }
        public string? Befehl_erobert { get; set; }
        public int GS { get; set; }
        public int GS_alt { get; set; }
        public int Kampfeinnahmen { get; set; }
        public int Kampfeinnahmen_alt { get; set; }
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
        public string? auf_Flotte { get; set; }
        public string? Sonstiges { get; set; }
        public string? spaltetab { get; set; }
        public string? fusmit { get; set; }
        public string? Chars { get; set; }
        public int bp_max { get; set; }
        public int isbanned { get; set; }
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

        public enum Felder
        {
            nummer, staerke_alt, staerke, hf_alt, hf, LKP_alt, LKP, SKP_alt, SKP, pferde_alt, Pferde, gf_von, kf_von, gf_nach, kf_nach, rp, bp, ph_xy, Garde, Befehl_bew, Befehl_ang, Befehl_erobert, GS, GS_alt, Kampfeinnahmen, Kampfeinnahmen_alt, x1, y1, x2, y2, x3,
            y3, hoehenstufen, schritt, x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, auf_Flotte, Sonstiges, spaltetab, fusmit, Chars, bp_max, isbanned, x10, y10, x11, y11, x12, y12, x13, y13, x14, y14, x15, y15, x16, y16, x17, y17, x18, y18, x19, y19,
        }


        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.staerke_alt = DatabaseConverter.ToInt32(reader[(int)Felder.staerke_alt]);
            this.staerke = DatabaseConverter.ToInt32(reader[(int)Felder.staerke]);
            this.hf_alt = DatabaseConverter.ToInt32(reader[(int)Felder.hf_alt]);
            this.hf = DatabaseConverter.ToInt32(reader[(int)Felder.hf]);
            this.LKP_alt = DatabaseConverter.ToInt32(reader[(int)Felder.LKP_alt]);
            this.LKP = DatabaseConverter.ToInt32(reader[(int)Felder.LKP]);
            this.SKP_alt = DatabaseConverter.ToInt32(reader[(int)Felder.SKP_alt]);
            this.SKP = DatabaseConverter.ToInt32(reader[(int)Felder.SKP]);
            this.pferde_alt = DatabaseConverter.ToInt32(reader[(int)Felder.pferde_alt]);
            this.Pferde = DatabaseConverter.ToInt32(reader[(int)Felder.Pferde]);
            this.gf_von = DatabaseConverter.ToInt32(reader[(int)Felder.gf_von]);
            this.kf_von = DatabaseConverter.ToInt32(reader[(int)Felder.kf_von]);
            this.gf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.gf_nach]);
            this.kf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.kf_nach]);
            this.rp = DatabaseConverter.ToInt32(reader[(int)Felder.rp]);
            this.bp = DatabaseConverter.ToInt32(reader[(int)Felder.bp]);
            this.ph_xy = DatabaseConverter.ToString(reader[(int)Felder.ph_xy]);
            this.Garde = DatabaseConverter.ToBool(reader[(int)Felder.Garde]);
            this.Befehl_bew = DatabaseConverter.ToString(reader[(int)Felder.Befehl_bew]);
            this.Befehl_ang = DatabaseConverter.ToString(reader[(int)Felder.Befehl_ang]);
            this.Befehl_erobert = DatabaseConverter.ToString(reader[(int)Felder.Befehl_erobert]);
            this.GS = DatabaseConverter.ToInt32(reader[(int)Felder.GS]);
            this.GS_alt = DatabaseConverter.ToInt32(reader[(int)Felder.GS_alt]);
            this.Kampfeinnahmen = DatabaseConverter.ToInt32(reader[(int)Felder.Kampfeinnahmen]);
            this.Kampfeinnahmen_alt = DatabaseConverter.ToInt32(reader[(int)Felder.Kampfeinnahmen_alt]);
            this.x1 = DatabaseConverter.ToInt32(reader[(int)Felder.x1]);
            this.y1 = DatabaseConverter.ToInt32(reader[(int)Felder.y1]);
            this.x2 = DatabaseConverter.ToInt32(reader[(int)Felder.x2]);
            this.y2 = DatabaseConverter.ToInt32(reader[(int)Felder.y2]);
            this.x3 = DatabaseConverter.ToInt32(reader[(int)Felder.x3]);
            this.y3 = DatabaseConverter.ToInt32(reader[(int)Felder.y3]);
            this.hoehenstufen = DatabaseConverter.ToInt32(reader[(int)Felder.hoehenstufen]);
            this.schritt = DatabaseConverter.ToInt32(reader[(int)Felder.schritt]);
            this.x4 = DatabaseConverter.ToInt32(reader[(int)Felder.x4]);
            this.y4 = DatabaseConverter.ToInt32(reader[(int)Felder.y4]);
            this.x5 = DatabaseConverter.ToInt32(reader[(int)Felder.x5]);
            this.y5 = DatabaseConverter.ToInt32(reader[(int)Felder.y5]);
            this.x6 = DatabaseConverter.ToInt32(reader[(int)Felder.x6]);
            this.y6 = DatabaseConverter.ToInt32(reader[(int)Felder.y6]);
            this.x7 = DatabaseConverter.ToInt32(reader[(int)Felder.x7]);
            this.y7 = DatabaseConverter.ToInt32(reader[(int)Felder.y7]);
            this.x8 = DatabaseConverter.ToInt32(reader[(int)Felder.x8]);
            this.y8 = DatabaseConverter.ToInt32(reader[(int)Felder.y8]);
            this.x9 = DatabaseConverter.ToInt32(reader[(int)Felder.x9]);
            this.y9 = DatabaseConverter.ToInt32(reader[(int)Felder.y9]);
            this.auf_Flotte = DatabaseConverter.ToString(reader[(int)Felder.auf_Flotte]);
            this.Sonstiges = DatabaseConverter.ToString(reader[(int)Felder.Sonstiges]);
            this.spaltetab = DatabaseConverter.ToString(reader[(int)Felder.spaltetab]);
            this.fusmit = DatabaseConverter.ToString(reader[(int)Felder.fusmit]);
            this.Chars = DatabaseConverter.ToString(reader[(int)Felder.Chars]);
            this.bp_max = DatabaseConverter.ToInt32(reader[(int)Felder.bp_max]);
            this.isbanned = DatabaseConverter.ToInt32(reader[(int)Felder.isbanned]);
            this.x10 = DatabaseConverter.ToInt32(reader[(int)Felder.x10]);
            this.y10 = DatabaseConverter.ToInt32(reader[(int)Felder.y10]);
            this.x11 = DatabaseConverter.ToInt32(reader[(int)Felder.x11]);
            this.y11 = DatabaseConverter.ToInt32(reader[(int)Felder.y11]);
            this.x12 = DatabaseConverter.ToInt32(reader[(int)Felder.x12]);
            this.y12 = DatabaseConverter.ToInt32(reader[(int)Felder.y12]);
            this.x13 = DatabaseConverter.ToInt32(reader[(int)Felder.x13]);
            this.y13 = DatabaseConverter.ToInt32(reader[(int)Felder.y13]);
            this.x14 = DatabaseConverter.ToInt32(reader[(int)Felder.x14]);
            this.y14 = DatabaseConverter.ToInt32(reader[(int)Felder.y14]);
            this.x15 = DatabaseConverter.ToInt32(reader[(int)Felder.x15]);
            this.y15 = DatabaseConverter.ToInt32(reader[(int)Felder.y15]);
            this.x16 = DatabaseConverter.ToInt32(reader[(int)Felder.x16]);
            this.y16 = DatabaseConverter.ToInt32(reader[(int)Felder.y16]);
            this.x17 = DatabaseConverter.ToInt32(reader[(int)Felder.x17]);
            this.y17 = DatabaseConverter.ToInt32(reader[(int)Felder.y17]);
            this.x18 = DatabaseConverter.ToInt32(reader[(int)Felder.x18]);
            this.y18 = DatabaseConverter.ToInt32(reader[(int)Felder.y18]);
            this.x19 = DatabaseConverter.ToInt32(reader[(int)Felder.x19]);
            this.y19 = DatabaseConverter.ToInt32(reader[(int)Felder.y19]);
        }
    }
}
