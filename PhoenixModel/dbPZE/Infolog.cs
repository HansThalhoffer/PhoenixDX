﻿using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE
{
    public class Infolog : IDatabaseTable, IEigenschaftler
    {
        public const string TableName = "Infolog";
        string IDatabaseTable.TableName => TableName;
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = [];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

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
            ID = DatabaseConverter.ToInt32(reader[(int)Felder.ID]);
            InfoType = DatabaseConverter.ToString(reader[(int)Felder.InfoType]);
            Infotext = DatabaseConverter.ToString(reader[(int)Felder.Infotext]);
            ReichID = DatabaseConverter.ToInt32(reader[(int)Felder.ReichID]);
            Monat = DatabaseConverter.ToString(reader[(int)Felder.Monat]);
        }
    }
}
