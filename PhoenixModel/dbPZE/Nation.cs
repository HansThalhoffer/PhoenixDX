using PhoenixModel.Database;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixModel.dbPZE {
    public class Nation :  IDatabaseTable, IEigenschaftler
    {
        public Nation() { }
        public Nation(string name) { Reich = name; }
        
        #region InterfaceFelder
        // IDatabaseTable
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "DBhandle";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => Reich; }

        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = { "DatabaseName", "Alias", "DBname", "DBpass", "Farbe", "Nummer", "Name" };
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }
        #endregion

        #region DatenbankFelder
        public HashSet<string>? Alias { get; set; }
        public string? Farbname { get; set; }
        public Color? Farbe { get; set; }

        public int? Nummer { get; set; }
        public string Reich { get; set; } = string.Empty;
        public string Name => Reich;
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
            var defData = ReichTabelle.Find(Reich);
            if (defData != null)
            {
                Alias = defData.Alias;
                Farbname = defData.Farbname;
                try
                {
                    Farbe = System.Drawing.ColorTranslator.FromHtml(defData.FarbeHex);
                }
                catch (Exception ex)
                {
                    ProgramView.LogError($"Fehler bei der Farbkonvertierung {Farbe}", ex.Message);
                }
            }
            else
            {
                ProgramView.LogError($"Die Nation {Reich} wurde nicht in der Vorbelegung gefunden", "Eventuell ist es eine fehlerhafte Schreibweise");
            }
            
        }

        public void Save(DbCommand command) {
            command.CommandText = $@"
        UPDATE {TableName} SET
            Reich = '{DatabaseConverter.EscapeString(this.Reich)}',
            DBname = '{DatabaseConverter.EscapeString(this.DBname)}',
            DBpass = '{DatabaseConverter.EscapeString(this.DBpass)}'
        WHERE Nummer = {this.Nummer}";

            // Execute the command
            command.ExecuteNonQuery();
        }


        public void Insert(DbCommand command) {
            command.CommandText = $@"
        INSERT INTO {TableName} (
            Nummer, Reich, DBname, DBpass
        ) VALUES (
            {this.Nummer}, 
            '{DatabaseConverter.EscapeString(this.Reich)}', 
            '{DatabaseConverter.EscapeString(this.DBname)}', 
            '{DatabaseConverter.EscapeString(this.DBpass)}'
        )";

            // Execute the command
            command.ExecuteNonQuery();
        }

        #endregion

    }

}
