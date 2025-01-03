using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Program;

namespace PhoenixModel.dbZugdaten
{
    public class Schiffe : Spielfigur, IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Schiffe";
        string IDatabaseTable.TableName => TableName;
        // IEigenschaftler
        public override FigurType Typ
        {
            get
            {
                // es sind piraten
                if (this.Nation!= null && Nation.Nummer == ReichTabelle.Piraten)
                {
                    if (SKP > 0)
                        return FigurType.PiratenSchweresKriegsschiff;
                    if (LKP > 0)
                        return FigurType.PiratenLeichtesKriegsschiff;
                    return FigurType.PiratenSchiff;
                }
                if (SKP > 0)
                    return FigurType.SchweresKriegsschiff;
                if (LKP > 0)
                    return FigurType.LeichtesKriegsschiff;
                return FigurType.Schiff;
            }
        }
        public override string St�rke { get { return staerke.ToString("n0"); } }


        public int staerke_alt { get; set; }
        public int staerke { get; set; }
        public int hf_alt { get; set; }
        public int hf { get; set; }
        public int lkp_alt { get; set; }
        public int LKP { get; set; }
        public int skp_alt { get; set; }
        public int SKP { get; set; }
        public int pferde_alt { get; set; }
        public int Pferde { get; set; }
        public bool Garde { get; set; }
        public string? Befehl_bew { get; set; }
        public string? Befehl_ang { get; set; }
        public string? Befehl_erobert { get; set; }
        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }
        public int x3 { get; set; }
        public int y3 { get; set; }
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
        public int hoehenstufen { get; set; }
        public int schritt { get; set; }
        public string? auf_Flotte { get; set; }
        public string? Sonstiges { get; set; }
        public string? spaltetab { get; set; }
        public string? fusmit { get; set; }
        public string? chars { get; set; }
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
            nummer, staerke_alt, staerke, hf_alt, hf, lkp_alt, LKP, skp_alt, SKP, pferde_alt, Pferde, gf_von, kf_von, gf_nach, kf_nach, rp, bp, ph_xy, Garde, Befehl_bew, Befehl_ang, Befehl_erobert, GS, GS_alt, Kampfeinnahmen, Kampfeinnahmen_alt, x1, y1, x2, y2, x3,
            y3, x4, y4, x5, y5, x6, y6, x7, y7, x8, y8, x9, y9, hoehenstufen, schritt, auf_Flotte, Sonstiges, spaltetab, fusmit, chars, bp_max, isbanned, x10, y10, x11, y11, x12, y12, x13, y13, x14, y14, x15, y15, x16, y16, x17, y17, x18, y18, x19, y19,
        }

        public override void Load(DbDataReader reader)
        {
            base.Load(reader);
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.nummer]);
            this.staerke_alt = DatabaseConverter.ToInt32(reader[(int)Felder.staerke_alt]);
            this.staerke = DatabaseConverter.ToInt32(reader[(int)Felder.staerke]);
            this.hf_alt = DatabaseConverter.ToInt32(reader[(int)Felder.hf_alt]);
            this.hf = DatabaseConverter.ToInt32(reader[(int)Felder.hf]);
            this.lkp_alt = DatabaseConverter.ToInt32(reader[(int)Felder.lkp_alt]);
            this.LKP = DatabaseConverter.ToInt32(reader[(int)Felder.LKP]);
            this.skp_alt = DatabaseConverter.ToInt32(reader[(int)Felder.skp_alt]);
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
            this.hoehenstufen = DatabaseConverter.ToInt32(reader[(int)Felder.hoehenstufen]);
            this.schritt = DatabaseConverter.ToInt32(reader[(int)Felder.schritt]);
            this.auf_Flotte = DatabaseConverter.ToString(reader[(int)Felder.auf_Flotte]);
            this.Sonstiges = DatabaseConverter.ToString(reader[(int)Felder.Sonstiges]);
            this.spaltetab = DatabaseConverter.ToString(reader[(int)Felder.spaltetab]);
            this.fusmit = DatabaseConverter.ToString(reader[(int)Felder.fusmit]);
            this.chars = DatabaseConverter.ToString(reader[(int)Felder.chars]);
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

        public override void Save(DbCommand command)
        {
            base.Save(command);

            command.CommandText = $@"
        UPDATE {TableName} SET
            staerke_alt = {this.staerke_alt},
            staerke = {this.staerke},
            hf_alt = {this.hf_alt},
            hf = {this.hf},
            lkp_alt = {this.lkp_alt},
            LKP = {this.LKP},
            skp_alt = {this.skp_alt},
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
            x1 = {this.x1},
            y1 = {this.y1},
            x2 = {this.x2},
            y2 = {this.y2},
            x3 = {this.x3},
            y3 = {this.y3},
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
            hoehenstufen = {this.hoehenstufen},
            schritt = {this.schritt},
            auf_Flotte = '{DatabaseConverter.EscapeString(this.auf_Flotte)}',
            Sonstiges = '{DatabaseConverter.EscapeString(this.Sonstiges)}',
            spaltetab = '{DatabaseConverter.EscapeString(this.spaltetab)}',
            fusmit = '{DatabaseConverter.EscapeString(this.fusmit)}',
            chars = '{DatabaseConverter.EscapeString(this.chars)}',
            bp_max = {this.bp_max},
            isbanned = {this.isbanned},
            x10 = {this.x10},
            y10 = {this.y10},
            x11 = {this.x11},
            y11 = {this.y11},
            x12 = {this.x12},
            y12 = {this.y12},
            x13 = {this.x13},
            y13 = {this.y13},
            x14 = {this.x14},
            y14 = {this.y14},
            x15 = {this.x15},
            y15 = {this.y15},
            x16 = {this.x16},
            y16 = {this.y16},
            x17 = {this.x17},
            y17 = {this.y17},
            x18 = {this.x18},
            y18 = {this.y18},
            x19 = {this.x19},
            y19 = {this.y19}
        WHERE Nummer = {this.Nummer}";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }
    }
}
