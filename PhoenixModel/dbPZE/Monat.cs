﻿using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE {
    internal class Monat :  IDatabaseTable, IEigenschaftler
    {
        public static string DatabaseName { get; set;  } = string.Empty;
        public string Database { get { return DatabaseName; } set { DatabaseName = value; } }
        public const string TableName = "Monat";
        string IDatabaseTable.TableName => TableName;
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["DatabaseName"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

        public string? zug { get; set; }

        public string Bezeichner => zug ?? string.Empty;

        public enum Felder
        {
            zug
        }
        public void Load(DbDataReader reader)
        {
            zug = DatabaseConverter.ToString(reader[(int)(int)Felder.zug]);          
        }

        public void Save(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public void Delete(DbCommand reader) => throw new NotImplementedException();
    }
}
