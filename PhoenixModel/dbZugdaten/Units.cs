using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Units :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Units";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => Nummer.ToString();
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
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

        public enum Felder
        {
            nummer, staerke_alt, staerke, hf_alt, hf, LKP_alt, LKP, SKP_alt, SKP, pferde_alt, Pferde, gf_von, kf_von, gf_nach, kf_nach, rp, bp, ph_xy, Garde, Befehl_bew, Befehl_ang, Befehl_erobert, GS, GS_alt, Kampfeinnahmen, Kampfeinnahmen_alt, x1, y1, x2, y2, x3,
            y3, hoehenstufen, schritt, x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, auf_Flotte, Sonstiges, spaltetab, fusmit, Chars, bp_max, isbanned,
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
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            staerke_alt = {this.staerke_alt},
            staerke = {this.staerke},
            hf_alt = {this.hf_alt},
            hf = {this.hf},
            LKP_alt = {this.LKP_alt},
            LKP = {this.LKP},
            SKP_alt = {this.SKP_alt},
            SKP = {this.SKP},
            pferde_alt = {this.pferde_alt},
            Pferde = {this.Pferde},
            gf_von = {this.gf_von},
            kf_von = {this.kf_von},
            gf_nach = {this.gf_nach},
            kf_nach = {this.kf_nach},
            rp = {this.rp},
            bp = {this.bp},
            ph_xy = '{DatabaseConverter.EscapeString(this.ph_xy)}',
            Garde = {(this.Garde ? 1 : 0)},
            Befehl_bew = '{DatabaseConverter.EscapeString(this.Befehl_bew)}',
            Befehl_ang = '{DatabaseConverter.EscapeString(this.Befehl_ang)}',
            Befehl_erobert = '{DatabaseConverter.EscapeString(this.Befehl_erobert)}',
            GS = {this.GS},
            GS_alt = {this.GS_alt},
            Kampfeinnahmen = {this.Kampfeinnahmen},
            Kampfeinnahmen_alt = {this.Kampfeinnahmen_alt},
            x1 = {this.x1}, y1 = {this.y1},
            x2 = {this.x2}, y2 = {this.y2},
            x3 = {this.x3}, y3 = {this.y3},
            hoehenstufen = {this.hoehenstufen},
            schritt = {this.schritt},
            x4 = {this.x4}, y4 = {this.y4},
            x5 = {this.x5}, y5 = {this.y5},
            x6 = {this.x6}, y6 = {this.y6},
            x7 = {this.x7}, y7 = {this.y7},
            x8 = {this.x8}, y8 = {this.y8},
            x9 = {this.x9}, y9 = {this.y9},
            auf_Flotte = '{DatabaseConverter.EscapeString(this.auf_Flotte)}',
            Sonstiges = '{DatabaseConverter.EscapeString(this.Sonstiges)}',
            spaltetab = '{DatabaseConverter.EscapeString(this.spaltetab)}',
            fusmit = '{DatabaseConverter.EscapeString(this.fusmit)}',
            Chars = '{DatabaseConverter.EscapeString(this.Chars)}',
            bp_max = {this.bp_max},
            isbanned = {this.isbanned}
        WHERE Nummer = {this.Nummer}";

            // Execute the command
            if (command.ExecuteNonQuery() == 0)
                Insert(command);
        }

        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Nummer, staerke_alt, staerke, hf_alt, hf, LKP_alt, LKP, SKP_alt, SKP,
            pferde_alt, Pferde, gf_von, kf_von, gf_nach, kf_nach, rp, bp, ph_xy,
            Garde, Befehl_bew, Befehl_ang, Befehl_erobert, GS, GS_alt, Kampfeinnahmen,
            Kampfeinnahmen_alt, x1, y1, x2, y2, x3, y3, hoehenstufen, schritt,
            x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, auf_Flotte, Sonstiges,
            spaltetab, fusmit, Chars, bp_max, isbanned
        ) VALUES (
            {this.Nummer}, {this.staerke_alt}, {this.staerke}, {this.hf_alt}, {this.hf},
            {this.LKP_alt}, {this.LKP}, {this.SKP_alt}, {this.SKP},
            {this.pferde_alt}, {this.Pferde}, {this.gf_von}, {this.kf_von},
            {this.gf_nach}, {this.kf_nach}, {this.rp}, {this.bp}, 
            '{DatabaseConverter.EscapeString(this.ph_xy)}', {(this.Garde ? 1 : 0)},
            '{DatabaseConverter.EscapeString(this.Befehl_bew)}',
            '{DatabaseConverter.EscapeString(this.Befehl_ang)}',
            '{DatabaseConverter.EscapeString(this.Befehl_erobert)}',
            {this.GS}, {this.GS_alt}, {this.Kampfeinnahmen}, {this.Kampfeinnahmen_alt},
            {this.x1}, {this.y1}, {this.x2}, {this.y2}, {this.x3}, {this.y3},
            {this.hoehenstufen}, {this.schritt}, {this.x4}, {this.y4},
            {this.x5}, {this.y5}, {this.x6}, {this.y6}, {this.x7}, {this.y7},
            {this.x8}, {this.y8}, {this.x9}, {this.y9},
            '{DatabaseConverter.EscapeString(this.auf_Flotte)}',
            '{DatabaseConverter.EscapeString(this.Sonstiges)}',
            '{DatabaseConverter.EscapeString(this.spaltetab)}',
            '{DatabaseConverter.EscapeString(this.fusmit)}',
            '{DatabaseConverter.EscapeString(this.Chars)}',
            {this.bp_max}, {this.isbanned}
        )";

            // Execute the command
            command.ExecuteNonQuery();
        }
        public void Delete(DbCommand reader) => throw new NotImplementedException();

    }
}
