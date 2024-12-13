using PhoenixModel.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    internal class DBTablesExtern : IDatabaseTable
    {
        public const string TableName = "DBTablesextern";
        string IDatabaseTable.TableName => TableName;

        public int id { get; set; } = 0;
        public string? tablename { get; set; }
        public string? usedbflag { get; set; }

        public string Bezeichner => id.ToString();

        public enum Felder
        {
            id, tablename, usedbflag
        }
        public void Load(DbDataReader reader)
        {
            id = DatabaseConverter.ToInt32(reader[(int)Felder.id]);
            tablename = DatabaseConverter.ToString(reader[(int)Felder.tablename]);
            usedbflag = DatabaseConverter.ToString(reader[(int)Felder.usedbflag]);
        }
    }
}
