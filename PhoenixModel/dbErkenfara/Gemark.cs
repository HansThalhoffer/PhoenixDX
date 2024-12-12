// Gemark.cs

using PhoenixModel.Database;
using PhoenixModel.Helper;
using System.Reflection.Metadata.Ecma335;

namespace PhoenixModel.dbErkenfara
{
    public class Gemark : IPropertyHolder, IDatabaseTable
    {
        public const string TableName = "Karte";
        string IDatabaseTable.TableName => TableName;

        public int gf { get; set; } = 0;
        public int kf { get; set; } = 0;
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


        public static string CreateBezeichner(int gf, int kf)
        {
            int i = gf * 100 + kf;
            return i.ToString();
        }
        public string Bezeichner { get => CreateBezeichner(gf, kf);  }

        private static readonly string[] PropertiestoIgnore = { "x", "y","Rand","db_xy","ph_xy"};
        public Dictionary<string, string> Properties
        {
            get
            {
                return PropertiesProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
    }
}
