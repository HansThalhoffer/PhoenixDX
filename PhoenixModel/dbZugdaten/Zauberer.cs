using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public enum Zaubererklasse { ZA, ZB, ZC, ZD, ZE, ZF, none }
    public class Zauberer : NamensSpielfigur, IDatabaseTable, IEigenschaftler
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "Zauberer";
        string IDatabaseTable.TableName => TableName;
        public override FigurType BaseTyp => FigurType.Zauberer;

        public override FigurType Typ
        {
            get
            {
                // wenn der Spielername gesetzt ist oder die Beschriftung mit CZ beginnt, ist es ein Charakter
                if (string.IsNullOrEmpty(this.Spielername) == false || (string.IsNullOrEmpty(this.Beschriftung) == false && Beschriftung.StartsWith("CZ")))
                    return FigurType.CharakterZauberer;

                return FigurType.Zauberer;
            }
        }
 
        public enum Felder
        {
            nummer, Beschriftung, GP_ges_alt, GP_ges, GP_akt_alt, GP_akt, charname, Spielername, gf_von, kf_von, gf_nach, kf_nach, rp, bp, tp_alt, tp, ph_xy, Teleport_gf_von, Teleport_kf_von, Teleport_gf_nach, Teleport_kf_nach, Befehl_magie, Befehl_Teleport, Befehl_bannt,
            x1, y1, x2, y2, x3, y3, hoehenstufen, schritt, x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, sonstiges, einheit, bp_max,
        }

        public override void Load(DbDataReader reader)
        {
            base.Load(reader);
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.Beschriftung = DatabaseConverter.ToString(reader[(int)Felder.Beschriftung]);
            this.GP_ges_alt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_ges_alt]);
            this.GP_ges = DatabaseConverter.ToInt32(reader[(int)Felder.GP_ges]);
            this.GP_akt_alt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_akt_alt]);
            this.GP_akt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_akt]);
            this.charname = DatabaseConverter.ToString(reader[(int)Felder.charname]);
            this.Spielername = DatabaseConverter.ToString(reader[(int)Felder.Spielername]).Trim();
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
            this.einheit = DatabaseConverter.ToString(reader[(int)Felder.einheit]);
            this.bp_max = DatabaseConverter.ToInt32(reader[(int)Felder.bp_max]);
        }

        public override void Save(DbCommand command)
        {
            command.CommandText = $@"
         UPDATE {TableName} SET
             Beschriftung = '{DatabaseConverter.EscapeString(this.Beschriftung)}',
            GP_ges_alt = {this.GP_ges_alt},
            GP_ges = {this.GP_ges},
            GP_akt_alt = {this.GP_akt_alt},
            GP_akt = {this.GP_akt},
            charname = '{DatabaseConverter.EscapeString(this.charname)}',
            Spielername = '{DatabaseConverter.EscapeString(this.Spielername)}',
            gf_von = {this.gf_von},
            kf_von = {this.kf_von},
            gf_nach = {this.gf_nach},
            kf_nach = {this.kf_nach},
            rp = {this.rp},
            bp = {this.bp},
            tp_alt = {this.tp_alt},
            tp = {this.tp},
            ph_xy = '{DatabaseConverter.EscapeString(this.ph_xy)}',
            Teleport_gf_von = {this.Teleport_gf_von},
            Teleport_kf_von = {this.Teleport_kf_von},
            Teleport_gf_nach = {this.Teleport_gf_nach},
            Teleport_kf_nach = {this.Teleport_kf_nach},
            Befehl_magie = '{DatabaseConverter.EscapeString(this.Befehl_magie)}',
            Befehl_Teleport = '{DatabaseConverter.EscapeString(this.Befehl_Teleport)}',
            Befehl_bannt = '{DatabaseConverter.EscapeString(this.Befehl_bannt)}',
            x1 = {this.x1},
            y1 = {this.y1},
            x2 = {this.x2},
            y2 = {this.y2},
            x3 = {this.x3},
            y3 = {this.y3},
            hoehenstufen = {this.hoehenstufen},
            schritt = {this.schritt},
            x4 = {this.x4},
            y4 = {this.y4},
            x5 = {this.x5},
            y5 = {this.y5},
            x6 = {this.x6},
            y6 = {this.y6},
            x7 = {this.x7},
            y7 = {this.y7},
            x8 = {this.x8},
            y8 = {this.y8},
            x9 = {this.x9},
            y9 = {this.y9},
            sonstiges = '{DatabaseConverter.EscapeString(this.sonstiges)}',
            einheit = '{DatabaseConverter.EscapeString(this.einheit)}',
            bp_max = {this.bp_max}
        WHERE nummer = {this.Nummer}";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public override void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Beschriftung, GP_ges_alt, GP_ges, GP_akt_alt, GP_akt, charname, Spielername, 
            gf_von, kf_von, gf_nach, kf_nach, rp, bp, tp_alt, tp, ph_xy, 
            Teleport_gf_von, Teleport_kf_von, Teleport_gf_nach, Teleport_kf_nach, 
            Befehl_magie, Befehl_Teleport, Befehl_bannt, x1, y1, x2, y2, x3, y3, 
            hoehenstufen, schritt, x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, 
            sonstiges, einheit, bp_max, nummer
        ) VALUES (
            '{DatabaseConverter.EscapeString(this.Beschriftung)}', {this.GP_ges_alt}, {this.GP_ges}, 
            {this.GP_akt_alt}, {this.GP_akt}, '{DatabaseConverter.EscapeString(this.charname)}', 
            '{DatabaseConverter.EscapeString(this.Spielername)}', {this.gf_von}, {this.kf_von}, 
            {this.gf_nach}, {this.kf_nach}, {this.rp}, {this.bp}, {this.tp_alt}, {this.tp}, 
            '{DatabaseConverter.EscapeString(this.ph_xy)}', {this.Teleport_gf_von}, {this.Teleport_kf_von}, 
            {this.Teleport_gf_nach}, {this.Teleport_kf_nach}, '{DatabaseConverter.EscapeString(this.Befehl_magie)}', 
            '{DatabaseConverter.EscapeString(this.Befehl_Teleport)}', '{DatabaseConverter.EscapeString(this.Befehl_bannt)}', 
            {this.x1}, {this.y1}, {this.x2}, {this.y2}, {this.x3}, {this.y3}, {this.hoehenstufen}, {this.schritt}, 
            {this.x4}, {this.y4}, {this.x5}, {this.y5}, {this.x6}, {this.y6}, {this.x7}, {this.y7}, 
            {this.x8}, {this.y8}, {this.x9}, {this.y9}, '{DatabaseConverter.EscapeString(this.sonstiges)}', 
            '{DatabaseConverter.EscapeString(this.einheit)}', {this.bp_max}, {this.Nummer}
        )";

            // Execute the command
            command.ExecuteNonQuery();
        }
    }
}
