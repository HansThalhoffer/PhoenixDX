using PhoenixModel.dbCrossRef;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhoenixModel.Program;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;

namespace PhoenixModel.dbErkenfara
{
    public class Gebäude : KleinfeldPosition, IEigenschaftler, IDatabaseTable, ISelectable
    {
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }

        // IDatabaseTable
        public const string TableName = "bauwerksliste";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(); }
        // IEigenschaftler
        private static readonly string[] PropertiestoIgnore = ["Key","gf","kf"];
        public List<Eigenschaft> Eigenschaften { get => PropertyProcessor.CreateProperties(this, PropertiestoIgnore); }

      
        // Felder der Tabellen
        public string? Reich { get; set; }
        private Nation? _nation = null;
        public Nation? Nation {  get
            {
                if (_nation == null && Reich != null)
                {
                    _nation = NationenView.GetNationFromString(Reich);
                }
                return _nation;
            } 
        }
        public int BaupunkteNachKarte
        {
            get { 
                return BauwerkeView.GetBaupunkteNachKarte(this) ?? 0;
            }
        }
        public int RuestortNachKarte
        {
            get
            {
                return BauwerkeView.GetRüstortNachKarte(this) ?? 0;
            }
        }

        [View.Editable]
        public string? Bauwerknamen { get; set; }
        public Rüstort? Rüstort { get; set; } = null;
        // falls kaputt oder noch nicht fertig aufgebaut
        public bool InBau { get; set; } = false;
        public bool Zerstört { get; set; } = false;

        public enum Felder
        {
            gf, kf, Reich, Bauwerknamen
        }

        public void Load(DbDataReader reader)
        {
            gf = DatabaseConverter.ToInt32(reader[(int)Felder.gf]);
            kf = DatabaseConverter.ToInt32(reader[(int)Felder.kf]);
            Reich = DatabaseConverter.ToString(reader[(int)Felder.Reich]);
            Bauwerknamen = DatabaseConverter.ToString(reader[(int)Felder.Bauwerknamen]);
        }

        public void Save(DbCommand command)
        {
            command.CommandText = $@"
                UPDATE {TableName} SET
                    Reich = '{DatabaseConverter.EscapeString(this.Reich)}',
                    Bauwerknamen = '{DatabaseConverter.EscapeString(this.Bauwerknamen)}'
                WHERE gf = {this.gf} AND kf = {this.gf} ";

            // Execute the command
            command.ExecuteNonQuery();
        }

        public void Insert(DbCommand command)
        {
            command.CommandText = $@"
                 INSERT INTO {TableName} (gf, kf, Reich, Bauwerknamen)
                VALUES ({this.gf}, {this.kf}, '{DatabaseConverter.EscapeString(this.Reich)}', '{DatabaseConverter.EscapeString(this.Bauwerknamen)}')";
        }

        public bool Select()
        {
            return ViewModel.BelongsToUser(this);
        }
        public bool Edit()
        {
            return Select();
        }
    }
}
