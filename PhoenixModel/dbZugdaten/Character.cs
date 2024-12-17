using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
namespace PhoenixModel.dbZugdaten
{
    public class Character : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "chars";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }


        public int Nummer { get; set; }
        public string? Beschriftung { get; set; }
        public int GP_ges { get; set; }
        public int GP_akt { get; set; }
        public int GP_ges_alt { get; set; }
        public int GP_akt_alt { get; set; }
        public string? Charname { get; set; }
        public string? Spielername { get; set; }
        public int gf_von { get; set; }
        public int kf_von { get; set; }
        public int gf_nach { get; set; }
        public int kf_nach { get; set; }
        public int rp { get; set; }
        public int bp { get; set; }
        public int tp_alt { get; set; }
        public int tp { get; set; }
        public string? ph_xy { get; set; }
        public int Teleport_gf_von { get; set; }
        public int Teleport_kf_von { get; set; }
        public int Teleport_gf_nach { get; set; }
        public int Teleport_kf_nach { get; set; }
        public string? Befehl_magie { get; set; }
        public string? Befehl_Teleport { get; set; }
        public string? Befehl_bannt { get; set; }
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
        public string? sonstiges { get; set; }
        public string? Einheit { get; set; }
        public int bp_max { get; set; }

        public enum Felder
        {
            nummer, Beschriftung, GP_ges, GP_akt, GP_ges_alt, GP_akt_alt, Charname, Spielername, gf_von, kf_von, gf_nach, kf_nach, rp, bp, tp_alt, tp, ph_xy, Teleport_gf_von, Teleport_kf_von, Teleport_gf_nach, Teleport_kf_nach, Befehl_magie, Befehl_Teleport, Befehl_bannt,
            x1, y1, x2, y2, x3, y3, hoehenstufen, schritt, x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, sonstiges, Einheit, bp_max,
        }

        public void Load(DbDataReader reader)
        {
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.Beschriftung = DatabaseConverter.ToString(reader[(int)Felder.Beschriftung]);
            this.GP_ges = DatabaseConverter.ToInt32(reader[(int)Felder.GP_ges]);
            this.GP_akt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_akt]);
            this.GP_ges_alt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_ges_alt]);
            this.GP_akt_alt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_akt_alt]);
            this.Charname = DatabaseConverter.ToString(reader[(int)Felder.Charname]);
            this.Spielername = DatabaseConverter.ToString(reader[(int)Felder.Spielername]);
            this.gf_von = DatabaseConverter.ToInt32(reader[(int)Felder.gf_von]);
            this.kf_von = DatabaseConverter.ToInt32(reader[(int)Felder.kf_von]);
            this.gf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.gf_nach]);
            this.kf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.kf_nach]);
            this.rp = DatabaseConverter.ToInt32(reader[(int)Felder.rp]);
            this.bp = DatabaseConverter.ToInt32(reader[(int)Felder.bp]);
            this.tp_alt = DatabaseConverter.ToInt32(reader[(int)Felder.tp_alt]);
            this.tp = DatabaseConverter.ToInt32(reader[(int)Felder.tp]);
            this.ph_xy = DatabaseConverter.ToString(reader[(int)Felder.ph_xy]);
            this.Teleport_gf_von = DatabaseConverter.ToInt32(reader[(int)Felder.Teleport_gf_von]);
            this.Teleport_kf_von = DatabaseConverter.ToInt32(reader[(int)Felder.Teleport_kf_von]);
            this.Teleport_gf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.Teleport_gf_nach]);
            this.Teleport_kf_nach = DatabaseConverter.ToInt32(reader[(int)Felder.Teleport_kf_nach]);
            this.Befehl_magie = DatabaseConverter.ToString(reader[(int)Felder.Befehl_magie]);
            this.Befehl_Teleport = DatabaseConverter.ToString(reader[(int)Felder.Befehl_Teleport]);
            this.Befehl_bannt = DatabaseConverter.ToString(reader[(int)Felder.Befehl_bannt]);
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
            this.sonstiges = DatabaseConverter.ToString(reader[(int)Felder.sonstiges]);
            this.Einheit = DatabaseConverter.ToString(reader[(int)Felder.Einheit]);
            this.bp_max = DatabaseConverter.ToInt32(reader[(int)Felder.bp_max]);
        }
    }
}
