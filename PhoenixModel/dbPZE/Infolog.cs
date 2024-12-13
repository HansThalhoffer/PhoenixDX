using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    internal class Infolog : IDatabaseTable
    {
        public const string TableName = "Infolog";
        string IDatabaseTable.TableName => TableName;

        public int ID { get; set; } = 0;
        public string? InfoType { get; set; }
        public string? Infotext { get; set; }
        public int? ReichID { get; set; }
        public string? Monat { get; set; }

        public string Bezeichner => ID.ToString();

        public enum Felder
        {
            ID, InfoType, Infotext, ReichID, Monat   
        }
        public void Load(DbDataReader reader)
        {
            ID = reader.GetInt32((int)Felder.ID);
            InfoType = reader.GetString((int)Felder.InfoType);
            Infotext = reader.GetString((int)Felder.Infotext);
            ReichID = reader.GetInt32((int) Felder.ReichID);
            Monat = reader.GetString((int)Felder.Monat);
        }
    }
}
