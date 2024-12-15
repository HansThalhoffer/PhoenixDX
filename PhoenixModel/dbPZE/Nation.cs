using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    public class Nation : IDatabaseTable, IEigenschaftler
    {
        // IDatabaseTable
        public const string TableName = "DBhandle";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => Reich ?? "Null"; }

        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = { "Alias" };
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

       

        public string[]? Alias { get; set; }
        public string? Farbname { get; set; }
        public Color? Farbe { get; set; }

        public int? Nummer { get; set; }
        public string? Reich { get; set; }
        public string? DBname { get; set; }
        public string? DBpass { get; set; }

        // Datenbankfelder
        public enum Felder
        { Nummer, Reich, DBname, DBpass };
        public void Load(DbDataReader reader)
        {
            Nummer = DatabaseConverter.ToInt32(reader[(int)Nation.Felder.Nummer]);
            Reich = DatabaseConverter.ToString(reader[(int)Nation.Felder.Reich]);
            DBname = DatabaseConverter.ToString(reader[(int)Nation.Felder.DBname]);
            DBpass = DatabaseConverter.ToString(reader[(int)Nation.Felder.DBpass]);
            foreach (var defData in PhoenixModel.dbPZE.Defaults.ReichDefaultData.Vorbelegung)
            {
                foreach (var name in defData.Alias)
                {
                    if (name.ToUpper() == Reich.ToUpper())
                    {
                        Alias = defData.Alias;
                        Farbname = defData.Farbname;
                        Farbe = System.Drawing.ColorTranslator.FromHtml(defData.FarbeHex);
                        break;
                    }
                }
                if (Farbname != null)
                    break;
            }

        }

    }

}
