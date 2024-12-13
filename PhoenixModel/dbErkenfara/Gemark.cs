// Gemark.cs

using PhoenixModel.Database;
using PhoenixModel.Helper;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;

namespace PhoenixModel.dbErkenfara
{
    public class GemarkPosition
    {
        public int gf { get; set; } = 0;
        public int kf { get; set; } = 0;

        public static string CreateBezeichner(int gf, int kf)
        {
            int i = gf * 100 + kf;
            return i.ToString();
        }
    }
    public class Gemark : GemarkPosition, IPropertyHolder, IDatabaseTable
    {
        public const string TableName = "Karte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(gf, kf); }

        public string? ph_xy { get; set; }
        public int x { get; set; } = 0;
        public int? y { get; set; }
        public string? db_xy { get; set; }
        public int? Rand { get; set; }
        public int? Index { get; set; }
        public int? Gelaendetyp { get; set; }
        public int? Ruestort { get; set; }
        public int? Fluss_NW { get; set; }
        public int? Fluss_NO { get; set; }
        public int? Fluss_O { get; set; }
        public int? Fluss_SO { get; set; }
        public int? Fluss_SW { get; set; }
        public int? Fluss_W { get; set; }
        public int? Wall_NW { get; set; }
        public int? Wall_NO { get; set; }
        public int? Wall_O { get; set; }
        public int? Wall_SO { get; set; }
        public int? Wall_SW { get; set; }
        public int? Wall_W { get; set; }
        public int? Kai_NW { get; set; }
        public int? Kai_NO { get; set; }
        public int? Kai_O { get; set; }
        public int? Kai_SO { get; set; }
        public int? Kai_SW { get; set; }
        public int? Kai_W { get; set; }
        public int? Strasse_NW { get; set; }
        public int? Strasse_NO { get; set; }
        public int? Strasse_O { get; set; }
        public int? Strasse_SO { get; set; }
        public int? Strasse_SW { get; set; }
        public int? Strasse_W { get; set; }
        public int? Bruecke_NW { get; set; }
        public int? Bruecke_NO { get; set; }
        public int? Bruecke_O { get; set; }
        public int? Bruecke_SO { get; set; }
        public int? Bruecke_SW { get; set; }
        public int? Bruecke_W { get; set; }
        public int? Reich { get; set; }
        public int? Krieger_eigen { get; set; }
        public int? Krieger_feind { get; set; }
        public int? Krieger_freund { get; set; }
        public int? Reiter_eigene { get; set; }
        public int? Reiter_feind { get; set; }
        public int? Reiter_freund { get; set; }
        public int? Schiffe_eigene { get; set; }
        public int? schiffe_feind { get; set; }
        public int? Schiffe_freund { get; set; }
        public int? Zauberer_eigene { get; set; }
        public int? Zauberer_feind { get; set; }
        public int? Zauberer_freund { get; set; }
        public int? Char_eigene { get; set; }
        public int? Char_feind { get; set; }
        public int? Char_freund { get; set; }
        public string? krieger_text { get; set; }
        public int? kreatur_eigen { get; set; }
        public int? kreatur_feind { get; set; }
        public int? kreatur_freund { get; set; }
        public int? Baupunkte { get; set; }
        public string? Bauwerknamen { get; set; }
        public int? lehensid { get; set; }


        public enum Felder
        {
            gf, kf,
            ph_xy ,
            x , 
            y ,
            db_xy ,
            Rand ,
            Index ,
            Gelaendetyp ,
            Ruestort ,
            Fluss_NW ,
            Fluss_NO ,
            Fluss_O ,
            Fluss_SO ,
            Fluss_SW ,
            Fluss_W ,
            Wall_NW ,
            Wall_NO ,
            Wall_O ,
            Wall_SO ,
            Wall_SW ,
            Wall_W ,
            Kai_NW ,
            Kai_NO ,
            Kai_O ,
            Kai_SO ,
            Kai_SW ,
            Kai_W ,
            Strasse_NW ,
            Strasse_NO ,
            Strasse_O ,
            Strasse_SO ,
            Strasse_SW ,
            Strasse_W ,
            Bruecke_NW ,
            Bruecke_NO ,
            Bruecke_O ,
            Bruecke_SO ,
            Bruecke_SW ,
            Bruecke_W ,
            Reich ,
            Krieger_eigen ,
            Krieger_feind ,
            Krieger_freund ,
            Reiter_eigene ,
            Reiter_feind ,
            Reiter_freund ,
            Schiffe_eigene ,
            schiffe_feind ,
            Schiffe_freund ,
            Zauberer_eigene ,
            Zauberer_feind ,
            Zauberer_freund ,
            Char_eigene ,
            Char_feind ,
            Char_freund ,
            krieger_text ,
            kreatur_eigen ,
            kreatur_feind ,
            kreatur_freund ,
            Baupunkte ,
            Bauwerknamen ,
            lehensid ,
        }

         public string ReichZugehörigkeit {
            get
            {
                if (SharedData.Nationen == null || Reich == null)
                    return string.Empty;
                return SharedData.Nationen.ElementAt(Reich.Value).Reich ?? string.Empty; 
            }
        }
  

        private static readonly string[] PropertiestoIgnore = { "x", "y","Rand","db_xy","ph_xy"};
        public Dictionary<string, string> Properties
        {
            get
            {
                return PropertiesProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }

        public void Load(DbDataReader reader)
        {
            gf = reader.GetInt32((int)Felder.gf);
            kf = reader.GetInt32((int)Felder.kf);
            ph_xy = reader.GetString((int)Felder.ph_xy);
            x = reader.GetInt32((int)Felder.x);
            y = reader.GetInt32((int)Felder.y);
            db_xy = reader.GetString((int)Felder.db_xy);
            Rand = reader.GetInt32((int)Felder.Rand);
            Index = reader.GetInt32((int)Felder.Index);
            Gelaendetyp = reader.GetInt32((int)Felder.Gelaendetyp);
            Ruestort = reader.GetInt32((int)Felder.Ruestort);
            Fluss_NW = reader.GetInt32((int)Felder.Fluss_NW);
            Fluss_NO = reader.GetInt32((int)Felder.Fluss_NO);
            Fluss_O = reader.GetInt32((int)Felder.Fluss_O);
            Fluss_SO = reader.GetInt32((int)Felder.Fluss_SO);
            Fluss_SW = reader.GetInt32((int)Felder.Fluss_SW);
            Fluss_W = reader.GetInt32((int)Felder.Fluss_W);
            Wall_NW = reader.GetInt32((int)Felder.Wall_NW);
            Wall_NO = reader.GetInt32((int)Felder.Wall_NO);
            Wall_O = reader.GetInt32((int)Felder.Wall_O);
            Wall_SO = reader.GetInt32((int)Felder.Wall_SO);
            Wall_SW = reader.GetInt32((int)Felder.Wall_SW);
            Wall_W = reader.GetInt32((int)Felder.Wall_W);
            Kai_NW = reader.GetInt32((int)Felder.Kai_NW);
            Kai_NO = reader.GetInt32((int)Felder.Kai_NO);
            Kai_O = reader.GetInt32((int)Felder.Kai_O);
            Kai_SO = reader.GetInt32((int)Felder.Kai_SO);
            Kai_SW = reader.GetInt32((int)Felder.Kai_SW);
            Kai_W = reader.GetInt32((int)Felder.Kai_W);
            Strasse_NW = reader.GetInt32((int)Felder.Strasse_NW);
            Strasse_NO = reader.GetInt32((int)Felder.Strasse_NO);
            Strasse_O = reader.GetInt32((int)Felder.Strasse_O);
            Strasse_SO = reader.GetInt32((int)Felder.Strasse_SO);
            Strasse_SW = reader.GetInt32((int)Felder.Strasse_SW);
            Strasse_W = reader.GetInt32((int)Felder.Strasse_W);
            Bruecke_NW = reader.GetInt32((int)Felder.Bruecke_NW);
            Bruecke_NO = reader.GetInt32((int)Felder.Bruecke_NO);
            Bruecke_O = reader.GetInt32((int)Felder.Bruecke_O);
            Bruecke_SO = reader.GetInt32((int)Felder.Bruecke_SO);
            Bruecke_SW = reader.GetInt32((int)Felder.Bruecke_SW);
            Bruecke_W = reader.GetInt32((int)Felder.Bruecke_W);
            Reich = reader.GetInt32((int)Felder.Reich);
            Krieger_eigen = reader.GetInt32((int)Felder.Krieger_eigen);
            Krieger_feind = reader.GetInt32((int)Felder.Krieger_feind);
            Krieger_freund = reader.GetInt32((int)Felder.Krieger_freund);
            Reiter_eigene = reader.GetInt32((int)Felder.Reiter_eigene);
            Reiter_feind = reader.GetInt32((int)Felder.Reiter_feind);
            Reiter_freund = reader.GetInt32((int)Felder.Reiter_freund);
            Schiffe_eigene = reader.GetInt32((int)Felder.Schiffe_eigene);
            schiffe_feind = reader.GetInt32((int)Felder.schiffe_feind);
            Schiffe_freund = reader.GetInt32((int)Felder.Schiffe_freund);
            Zauberer_eigene = reader.GetInt32((int)Felder.Zauberer_eigene);
            Zauberer_feind = reader.GetInt32((int)Felder.Zauberer_feind);
            Zauberer_freund = reader.GetInt32((int)Felder.Zauberer_freund);
            Char_eigene = reader.GetInt32((int)Felder.Char_eigene);
            Char_feind = reader.GetInt32((int)Felder.Char_feind);
            Char_freund = reader.GetInt32((int)Felder.Char_freund);
            krieger_text = reader.GetString((int)Felder.krieger_text);
            kreatur_eigen = reader.GetInt32((int)Felder.kreatur_eigen);
            kreatur_feind = reader.GetInt32((int)Felder.kreatur_feind);
            kreatur_freund = reader.GetInt32((int)Felder.kreatur_freund);
            Baupunkte = reader.GetInt32((int)Felder.Baupunkte);
            Bauwerknamen = reader.GetString((int)Felder.Bauwerknamen);
            lehensid = reader.GetInt32((int)Felder.lehensid);
        }
    }
}
