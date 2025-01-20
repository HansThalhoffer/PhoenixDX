using System;
using System.Data.Common;
using PhoenixModel.Database;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;

namespace PhoenixModel.dbPZE {
    public class PzeKarte : KleinfeldPosition, IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Karte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(); }
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        
        public string? ph_xy { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public string? db_xy { get; set; }
        public int Rand { get; set; }
        public int Index { get; set; }
        public int Gelaendetyp { get; set; }
        public int Ruestort { get; set; }
        public int Fluss_NW { get; set; }
        public int Fluss_NO { get; set; }
        public int Fluss_O { get; set; }
        public int Fluss_SO { get; set; }
        public int Fluss_SW { get; set; }
        public int Fluss_W { get; set; }
        public int Wall_NW { get; set; }
        public int Wall_NO { get; set; }
        public int Wall_O { get; set; }
        public int Wall_SO { get; set; }
        public int Wall_SW { get; set; }
        public int Wall_W { get; set; }
        public int Kai_NW { get; set; }
        public int Kai_NO { get; set; }
        public int Kai_O { get; set; }
        public int Kai_SO { get; set; }
        public int Kai_SW { get; set; }
        public int Kai_W { get; set; }
        public int Strasse_NW { get; set; }
        public int Strasse_NO { get; set; }
        public int Strasse_O { get; set; }
        public int Strasse_SO { get; set; }
        public int Strasse_SW { get; set; }
        public int Strasse_W { get; set; }
        public int Bruecke_NW { get; set; }
        public int Bruecke_NO { get; set; }
        public int Bruecke_O { get; set; }
        public int Bruecke_SO { get; set; }
        public int Bruecke_SW { get; set; }
        public int Bruecke_W { get; set; }
        public int Reich { get; set; }
        public int Krieger_eigen { get; set; }
        public int Krieger_feind { get; set; }
        public int Krieger_freund { get; set; }
        public int Reiter_eigene { get; set; }
        public int Reiter_feind { get; set; }
        public int Reiter_freund { get; set; }
        public int Schiffe_eigene { get; set; }
        public int schiffe_feind { get; set; }
        public int Schiffe_freund { get; set; }
        public int Zauberer_eigene { get; set; }
        public int Zauberer_feind { get; set; }
        public int Zauberer_freund { get; set; }
        public int Char_eigene { get; set; }
        public int Char_feind { get; set; }
        public int Char_freund { get; set; }
        public string? krieger_text { get; set; }
        public int kreatur_eigen { get; set; }
        public int kreatur_feind { get; set; }
        public int kreatur_freund { get; set; }
        public int Baupunkte { get; set; }
        public string? Bauwerknamen { get; set; }
        public int lehensid { get; set; }

        public enum Felder
        {
            gf, kf, ph_xy, x, y, db_xy, Rand, Index, Gelaendetyp, Ruestort, Fluss_NW, Fluss_NO, Fluss_O, Fluss_SO, Fluss_SW, Fluss_W, Wall_NW, Wall_NO, Wall_O, Wall_SO, Wall_SW, Wall_W, Kai_NW, Kai_NO, Kai_O, Kai_SO, Kai_SW, Kai_W, Strasse_NW, Strasse_NO, Strasse_O,
            Strasse_SO, Strasse_SW, Strasse_W, Bruecke_NW, Bruecke_NO, Bruecke_O, Bruecke_SO, Bruecke_SW, Bruecke_W, Reich, Krieger_eigen, Krieger_feind, Krieger_freund, Reiter_eigene, Reiter_feind, Reiter_freund, Schiffe_eigene, schiffe_feind, Schiffe_freund, Zauberer_eigene,
            Zauberer_feind, Zauberer_freund, Char_eigene, Char_feind, Char_freund, krieger_text, kreatur_eigen, kreatur_feind, kreatur_freund, Baupunkte, Bauwerknamen, lehensid,
        }

        public void Load(DbDataReader reader)
        {
            this.gf = DatabaseConverter.ToInt32(reader[(int)Felder.gf]);
            this.kf = DatabaseConverter.ToInt32(reader[(int)Felder.kf]);
            this.ph_xy = DatabaseConverter.ToString(reader[(int)Felder.ph_xy]);
            this.x = DatabaseConverter.ToInt32(reader[(int)Felder.x]);
            this.y = DatabaseConverter.ToInt32(reader[(int)Felder.y]);
            this.db_xy = DatabaseConverter.ToString(reader[(int)Felder.db_xy]);
            this.Rand = DatabaseConverter.ToInt32(reader[(int)Felder.Rand]);
            this.Index = DatabaseConverter.ToInt32(reader[(int)Felder.Index]);
            this.Gelaendetyp = DatabaseConverter.ToInt32(reader[(int)Felder.Gelaendetyp]);
            this.Ruestort = DatabaseConverter.ToInt32(reader[(int)Felder.Ruestort]);
            this.Fluss_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_NW]);
            this.Fluss_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_NO]);
            this.Fluss_O = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_O]);
            this.Fluss_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_SO]);
            this.Fluss_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_SW]);
            this.Fluss_W = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_W]);
            this.Wall_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_NW]);
            this.Wall_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_NO]);
            this.Wall_O = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_O]);
            this.Wall_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_SO]);
            this.Wall_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_SW]);
            this.Wall_W = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_W]);
            this.Kai_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_NW]);
            this.Kai_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_NO]);
            this.Kai_O = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_O]);
            this.Kai_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_SO]);
            this.Kai_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_SW]);
            this.Kai_W = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_W]);
            this.Strasse_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_NW]);
            this.Strasse_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_NO]);
            this.Strasse_O = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_O]);
            this.Strasse_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_SO]);
            this.Strasse_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_SW]);
            this.Strasse_W = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_W]);
            this.Bruecke_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_NW]);
            this.Bruecke_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_NO]);
            this.Bruecke_O = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_O]);
            this.Bruecke_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_SO]);
            this.Bruecke_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_SW]);
            this.Bruecke_W = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_W]);
            this.Reich = DatabaseConverter.ToInt32(reader[(int)Felder.Reich]);
            this.Krieger_eigen = DatabaseConverter.ToInt32(reader[(int)Felder.Krieger_eigen]);
            this.Krieger_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Krieger_feind]);
            this.Krieger_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Krieger_freund]);
            this.Reiter_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Reiter_eigene]);
            this.Reiter_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Reiter_feind]);
            this.Reiter_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Reiter_freund]);
            this.Schiffe_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Schiffe_eigene]);
            this.schiffe_feind = DatabaseConverter.ToInt32(reader[(int)Felder.schiffe_feind]);
            this.Schiffe_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Schiffe_freund]);
            this.Zauberer_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Zauberer_eigene]);
            this.Zauberer_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Zauberer_feind]);
            this.Zauberer_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Zauberer_freund]);
            this.Char_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Char_eigene]);
            this.Char_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Char_feind]);
            this.Char_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Char_freund]);
            this.krieger_text = DatabaseConverter.ToString(reader[(int)Felder.krieger_text]);
            this.kreatur_eigen = DatabaseConverter.ToInt32(reader[(int)Felder.kreatur_eigen]);
            this.kreatur_feind = DatabaseConverter.ToInt32(reader[(int)Felder.kreatur_feind]);
            this.kreatur_freund = DatabaseConverter.ToInt32(reader[(int)Felder.kreatur_freund]);
            this.Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            this.Bauwerknamen = DatabaseConverter.ToString(reader[(int)Felder.Bauwerknamen]);
            this.lehensid = DatabaseConverter.ToInt32(reader[(int)Felder.lehensid]);
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }
    }
}
