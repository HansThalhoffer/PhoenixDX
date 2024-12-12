using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    public class Nation : IDatabaseTable, IPropertyHolder
    {
        public const string TableName = "DBhandle";
        string IDatabaseTable.TableName => TableName;
        public string[]? Alias { get; set; }
        public string? Farbname { get; set; }
        public Color? Farbe { get; set; }

        public int? Nummer { get; set; }
        public string? Reich { get; set; }
        public string? DBname { get; set; }
        public string? DBpass { get; set; }

        private static readonly string[] PropertiestoIgnore = { "Alias" };
        public Dictionary<string, string> Properties
        {
            get
            {
                return PropertiesProcessor.CreateProperties(this, PropertiestoIgnore);
            }

        }
        public string Bezeichner { get => Reich ?? "Null"; }

    }

}
