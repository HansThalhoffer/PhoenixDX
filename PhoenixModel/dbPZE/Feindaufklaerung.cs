using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    internal class Feindaufklaerung : IDatabaseTable
    {
        public const string TableName = "Feindaufklaerung";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner => id.ToString();

        public int id { get; set; } = 0;
        public int? Nummer { get; set; }
        public string? Reich { get; set; }
        public string? Art { get; set; }
        public int? GF { get; set; }
        public int? KF { get; set; }
        public string? Notiz { get; set; }


        public enum Felder
        {
            id, Nummer, Reich, Art, GF, KF, Notiz
        }
        public void Load(DbDataReader reader)
        {
            id = reader.GetInt32((int)Felder.id);
            Nummer = reader.GetInt32((int)Felder.Nummer);
            Reich = reader.GetString((int)Felder.Reich);
            Art = reader.GetString((int)Felder.Art);
            GF = reader.GetInt32((int)Felder.GF);
            KF = reader.GetInt32((int)Felder.KF);
            Notiz = reader.GetString((int)Felder.Notiz);
        }
    }
}
