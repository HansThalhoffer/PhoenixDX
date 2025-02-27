using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbZugdaten {
    public class Ruestung : KleinfeldPosition, IDatabaseTable, IEigenschaftler, IEquatable<Ruestung> {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "ruestung";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(); }
        private static readonly string[] PropertiestoIgnore = ["DatabaseName", "Database", "Bezeichner", "ID", "Key"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public int ZugMonat { get; set; }
        public int Nummer { get; set; }
        public int HF { get; set; }
        public int Z { get; set; }
        public int K { get; set; }
        public int R { get; set; }
        public int P { get; set; }
        public int LKS { get; set; }
        public int SKS { get; set; }
        public int LKP { get; set; }
        public int SKP { get; set; }
        public int GP_akt { get; set; }
        public int GP_ges { get; set; }
        public int ZB { get; set; }
        public int S { get; set; }
        public int Neuruestung { get; set; }
        public int KF_Flotte { get; set; }
        public int GF_Flotte { get; set; }
        public int Garde { get; set; }
        public string? Name_x { get; set; }
        public string? Beschriftung { get; set; }
        public int id { get; set; }
        public int besRuestung { get; set; }

        public enum Felder
        {
            GF, KF, Nummer, HF, Z, K, R, P, LKS, SKS, LKP, SKP, GP_akt, GP_ges, ZB, S, Neuruestung, KF_Flotte, GF_Flotte, Garde, Name_x, Beschriftung, id, besRuestung,
        }

        public void Load(DbDataReader reader)
        {
            this.gf = DatabaseConverter.ToInt32(reader[(int)Felder.GF]);
            this.kf = DatabaseConverter.ToInt32(reader[(int)Felder.KF]);
            this.Nummer = DatabaseConverter.ToInt32(reader[(int)Felder.Nummer]);
            this.HF = DatabaseConverter.ToInt32(reader[(int)Felder.HF]);
            this.Z = DatabaseConverter.ToInt32(reader[(int)Felder.Z]);
            this.K = DatabaseConverter.ToInt32(reader[(int)Felder.K]);
            this.R = DatabaseConverter.ToInt32(reader[(int)Felder.R]);
            this.P = DatabaseConverter.ToInt32(reader[(int)Felder.P]);
            this.LKS = DatabaseConverter.ToInt32(reader[(int)Felder.LKS]);
            this.SKS = DatabaseConverter.ToInt32(reader[(int)Felder.SKS]);
            this.LKP = DatabaseConverter.ToInt32(reader[(int)Felder.LKP]);
            this.SKP = DatabaseConverter.ToInt32(reader[(int)Felder.SKP]);
            this.GP_akt = DatabaseConverter.ToInt32(reader[(int)Felder.GP_akt]);
            this.GP_ges = DatabaseConverter.ToInt32(reader[(int)Felder.GP_ges]);
            this.ZB = DatabaseConverter.ToInt32(reader[(int)Felder.ZB]);
            this.S = DatabaseConverter.ToInt32(reader[(int)Felder.S]);
            this.Neuruestung = DatabaseConverter.ToInt32(reader[(int)Felder.Neuruestung]);
            this.KF_Flotte = DatabaseConverter.ToInt32(reader[(int)Felder.KF_Flotte]);
            this.GF_Flotte = DatabaseConverter.ToInt32(reader[(int)Felder.GF_Flotte]);
            this.Garde = DatabaseConverter.ToInt32(reader[(int)Felder.Garde]);
            this.Name_x = DatabaseConverter.ToString(reader[(int)Felder.Name_x]);
            this.Beschriftung = DatabaseConverter.ToString(reader[(int)Felder.Beschriftung]);
            this.id = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
            this.besRuestung = DatabaseConverter.ToInt32(reader[(int)Felder.besRuestung]);
            this.ZugMonat = ProgramView.SelectedMonth;
        }

        public void Save(DbCommand command)
        {
            command.CommandText = $@" UPDATE {DatabaseConverter.EscapeString(TableName)} SET
            gf = {this.gf}, kf = {this.kf},
            HF = {this.HF}, Z = {this.Z},
            K = {this.K}, R = {this.R},
            P = {this.P}, LKS = {this.LKS},
            SKS = {this.SKS}, LKP = {this.LKP},
            SKP = {this.SKP}, GP_akt = {this.GP_akt},
            GP_ges = {this.GP_ges}, ZB = {this.ZB},
            S = {this.S}, Neuruestung = {this.Neuruestung},
            KF_Flotte = {this.KF_Flotte},
            GF_Flotte = {this.GF_Flotte},
            Garde = {this.Garde},
            Name_x = '{DatabaseConverter.EscapeString(this.Name_x)}',
            Beschriftung = '{DatabaseConverter.EscapeString(this.Beschriftung)}',
            id = {this.id},
            besRuestung = {this.besRuestung}
        WHERE Nummer = {this.Nummer}";

            // Execute the command
            if (command.ExecuteNonQuery() == 0)
                Insert(command);
        }

        public void Insert(DbCommand command)
        {
            command.CommandText = $@"INSERT INTO {DatabaseConverter.EscapeString(TableName)} 
            (gf,kf,Nummer,HF,Z,K,R,P,LKS,SKS,LKP,SKP,GP_akt,GP_ges,ZB,S,Neuruestung,KF_Flotte,GF_Flotte,Garde,Name_x,Beschriftung, besRuestung) 
            VALUES ({this.gf},{this.kf},{this.Nummer},{this.HF},{this.Z},{this.K},{this.R},{this.P},{this.LKS},{this.SKS},{this.LKP},{this.SKP},{this.GP_akt},{this.GP_ges},{this.ZB},{this.S},
                    {this.Neuruestung},{this.KF_Flotte},{this.GF_Flotte},{this.Garde},'{DatabaseConverter.EscapeString(this.Name_x)}','{DatabaseConverter.EscapeString(this.Beschriftung)}',
                    {this.besRuestung})";
            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Delete(DbCommand command) {
            command.CommandText = $@"
        DELETE FROM {DatabaseConverter.EscapeString(TableName)}
        WHERE 
            gf = {this.gf} AND
            kf = {this.kf} AND
            HF = {this.HF} AND
            Z = {this.Z} AND
            K = {this.K} AND
            R = {this.R} AND
            P = {this.P} AND
            LKS = {this.LKS} AND
            SKS = {this.SKS} AND
            LKP = {this.LKP} AND
            SKP = {this.SKP} AND
            GP_akt = {this.GP_akt} AND
            GP_ges = {this.GP_ges} AND
            ZB = {this.ZB} AND
            S = {this.S} AND
            Neuruestung = {this.Neuruestung} AND
            KF_Flotte = {this.KF_Flotte} AND
            GF_Flotte = {this.GF_Flotte} AND
            Garde = {this.Garde} AND
            Name_x = '{DatabaseConverter.EscapeString(this.Name_x)}' AND
            Beschriftung = '{DatabaseConverter.EscapeString(this.Beschriftung)}' AND
            besRuestung = {this.besRuestung} AND
            Nummer = {this.Nummer}";

            // Execute the delete command
            command.ExecuteNonQuery();
        }

        public bool Equals(Ruestung? other) {
            if (other == null) return false;

            return HF == other.HF &&
                   Z == other.Z &&
                   K == other.K &&
                   R == other.R &&
                   P == other.P &&
                   LKS == other.LKS &&
                   SKS == other.SKS &&
                   LKP == other.LKP &&
                   SKP == other.SKP &&
                   GP_akt == other.GP_akt &&
                   GP_ges == other.GP_ges &&
                   Garde == other.Garde &&
                   ZB == other.ZB &&
                   S == other.S &&
                   Neuruestung == other.Neuruestung &&
                   KF_Flotte == other.KF_Flotte &&
                   GF_Flotte == other.GF_Flotte &&
                   Name_x == other.Name_x &&
                   Beschriftung == other.Beschriftung &&
                   besRuestung == other.besRuestung;
        }

        public override bool Equals(object? obj) {
            return Equals(obj as Ruestung);
        }

        public override int GetHashCode() {
            return HashCode.Combine(
                HashCode.Combine(HF, Z, K, R, P),
                HashCode.Combine(LKS, SKS, LKP, SKP),
                HashCode.Combine(GP_akt, GP_ges, Garde, ZB, S, Neuruestung, KF_Flotte, GF_Flotte),
                HashCode.Combine(Name_x, Beschriftung, besRuestung)
            );
        }

    }
}
