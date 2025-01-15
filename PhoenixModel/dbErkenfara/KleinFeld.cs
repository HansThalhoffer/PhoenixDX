// KleinFeld.cs

using PhoenixModel.dbCrossRef;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System.Data.Common;
using static PhoenixModel.ExternalTables.GeländeTabelle;
using PhoenixModel.ExternalTables;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.dbPZE;
using PhoenixModel.ViewModel;
using PhoenixModel.EventsAndArgs;

namespace PhoenixModel.dbErkenfara {

    public class KleinFeld : KleinfeldPosition, ISelectable, IDatabaseTable
    {
        #region Schnittstellen
        private static string _datebaseName = string.Empty;
        public string DatabaseName { get { return _datebaseName; } set { _datebaseName = value; } }
        public const string TableName = "Karte";
        string IDatabaseTable.TableName => TableName;
        public string Bezeichner { get => CreateBezeichner(); }
        private static readonly string[] PropertiestoIgnore = { "Gebäude", "Index", "DatabaseName", "x", "y", "Rand", "db_xy", "ph_xy", "krieger_text", "TerrainType", "Key", "Reich", "Gelaendetyp" };
        public List<Eigenschaft> Eigenschaften => PropertyProcessor.CreateProperties(this, PropertiestoIgnore);
        #endregion

        #region Datenbankfelder
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
        public int Baupunkte { get; set; } = 0;
        [View.Editable]
        public string? Bauwerknamen { get; set; }
        public int? lehensid { get; set; }

        private bool? _isWasser = null;
        public bool IsWasser
        {
            get
            {
                if (_isWasser == null)
                    _isWasser = this.Terrain.IsWasser;
                return (bool)_isWasser;
            }
        }

        private bool? _isKüste = null;
        public bool IsKüste
        {
            get
            {
                if (IsWasser == false)
                    return false;
                if (_isKüste == null)
                    _isKüste = KleinfeldView.IsKleinfeldKüstenGewässer(this);
                return (bool)_isKüste;
            }
        }

        public MarkerType Mark { get; set; }

        public enum Felder
        {
            gf, kf, ph_xy, x, y, db_xy, Rand, Index, Gelaendetyp, Ruestort,
            Fluss_NW, Fluss_NO, Fluss_O, Fluss_SO, Fluss_SW, Fluss_W, Wall_NW, Wall_NO, Wall_O, Wall_SO,
            Wall_SW, Wall_W, Kai_NW, Kai_NO, Kai_O, Kai_SO, Kai_SW, Kai_W, Strasse_NW, Strasse_NO,
            Strasse_O, Strasse_SO, Strasse_SW, Strasse_W, Bruecke_NW, Bruecke_NO, Bruecke_O, Bruecke_SO, Bruecke_SW, Bruecke_W,
            Reich, Krieger_eigen, Krieger_feind, Krieger_freund, Reiter_eigene, Reiter_feind, Reiter_freund, Schiffe_eigene, schiffe_feind, Schiffe_freund,
            Zauberer_eigene, Zauberer_feind, Zauberer_freund, Char_eigene, Char_feind, Char_freund, krieger_text, kreatur_eigen, kreatur_feind, kreatur_freund,
            Baupunkte, Bauwerknamen, lehensid
        }

        public void Load(DbDataReader reader)
        {
            gf = DatabaseConverter.ToInt32(reader[(int)Felder.gf]);
            kf = DatabaseConverter.ToInt32(reader[(int)Felder.kf]);
            ph_xy = DatabaseConverter.ToString(reader[(int)Felder.ph_xy]);
            x = DatabaseConverter.ToInt32(reader[(int)Felder.x]);
            y = DatabaseConverter.ToInt32(reader[(int)Felder.y]);
            db_xy = DatabaseConverter.ToString(reader[(int)Felder.db_xy]);
            Rand = DatabaseConverter.ToInt32(reader[(int)Felder.Rand]);
            Index = DatabaseConverter.ToInt32(reader[(int)Felder.Index]);
            Gelaendetyp = DatabaseConverter.ToInt32(reader[(int)Felder.Gelaendetyp]);
            Ruestort = DatabaseConverter.ToInt32(reader[(int)Felder.Ruestort]);
            Fluss_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_NW]);
            Fluss_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_NO]);
            Fluss_O = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_O]);
            Fluss_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_SO]);
            Fluss_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_SW]);
            Fluss_W = DatabaseConverter.ToInt32(reader[(int)Felder.Fluss_W]);
            Wall_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_NW]);
            Wall_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_NO]);
            Wall_O = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_O]);
            Wall_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_SO]);
            Wall_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_SW]);
            Wall_W = DatabaseConverter.ToInt32(reader[(int)Felder.Wall_W]);
            Kai_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_NW]);
            Kai_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_NO]);
            Kai_O = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_O]);
            Kai_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_SO]);
            Kai_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_SW]);
            Kai_W = DatabaseConverter.ToInt32(reader[(int)Felder.Kai_W]);
            Strasse_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_NW]);
            Strasse_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_NO]);
            Strasse_O = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_O]);
            Strasse_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_SO]);
            Strasse_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_SW]);
            Strasse_W = DatabaseConverter.ToInt32(reader[(int)Felder.Strasse_W]);
            Bruecke_NW = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_NW]);
            Bruecke_NO = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_NO]);
            Bruecke_O = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_O]);
            Bruecke_SO = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_SO]);
            Bruecke_SW = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_SW]);
            Bruecke_W = DatabaseConverter.ToInt32(reader[(int)Felder.Bruecke_W]);
            Reich = DatabaseConverter.ToInt32(reader[(int)Felder.Reich]);
            Krieger_eigen = DatabaseConverter.ToInt32(reader[(int)Felder.Krieger_eigen]);
            Krieger_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Krieger_feind]);
            Krieger_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Krieger_freund]);
            Reiter_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Reiter_eigene]);
            Reiter_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Reiter_feind]);
            Reiter_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Reiter_freund]);
            Schiffe_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Schiffe_eigene]);
            schiffe_feind = DatabaseConverter.ToInt32(reader[(int)Felder.schiffe_feind]);
            Schiffe_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Schiffe_freund]);
            Zauberer_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Zauberer_eigene]);
            Zauberer_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Zauberer_feind]);
            Zauberer_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Zauberer_freund]);
            Char_eigene = DatabaseConverter.ToInt32(reader[(int)Felder.Char_eigene]);
            Char_feind = DatabaseConverter.ToInt32(reader[(int)Felder.Char_feind]);
            Char_freund = DatabaseConverter.ToInt32(reader[(int)Felder.Char_freund]);
            krieger_text = DatabaseConverter.ToString(reader[(int)Felder.krieger_text]);
            kreatur_eigen = DatabaseConverter.ToInt32(reader[(int)Felder.kreatur_eigen]);
            kreatur_feind = DatabaseConverter.ToInt32(reader[(int)Felder.kreatur_feind]);
            kreatur_freund = DatabaseConverter.ToInt32(reader[(int)Felder.kreatur_freund]);
            Baupunkte = DatabaseConverter.ToInt32(reader[(int)Felder.Baupunkte]);
            Bauwerknamen = reader.GetString((int)Felder.Bauwerknamen);
            lehensid = DatabaseConverter.ToInt32(reader[(int)Felder.lehensid]);
        }
        #endregion

        public TerrainType TerrainType
        {
            get
            {
                if (Gelaendetyp <= (int)TerrainType.AuftauchpunktUnbekannt)
                    return (TerrainType)Gelaendetyp;
                return TerrainType.Default;
            }
        }

        public Armee Truppen
        {
            get { return SpielfigurenView.GetSpielfiguren(this); }
        }

        public void Save(DbCommand command)
        {
            command.CommandText = $@"UPDATE {TableName} SET
            ph_xy = '{DatabaseConverter.EscapeString(this.ph_xy)}',
            x = {this.x},
            y = {this.y},
            db_xy = '{DatabaseConverter.EscapeString(this.db_xy)}',
            Rand = {this.Rand},
            Gelaendetyp = {this.Gelaendetyp},
            Ruestort = {this.Ruestort},
            Fluss_NW = {this.Fluss_NW},
            Fluss_NO = {this.Fluss_NO},
            Fluss_O = {this.Fluss_O},
            Fluss_SO = {this.Fluss_SO},
            Fluss_SW = {this.Fluss_SW},
            Fluss_W = {this.Fluss_W},
            Wall_NW = {this.Wall_NW},
            Wall_NO = {this.Wall_NO},
            Wall_O = {this.Wall_O},
            Wall_SO = {this.Wall_SO},
            Wall_SW = {this.Wall_SW},
            Wall_W = {this.Wall_W},
            Kai_NW = {this.Kai_NW},
            Kai_NO = {this.Kai_NO},
            Kai_O = {this.Kai_O},
            Kai_SO = {this.Kai_SO},
            Kai_SW = {this.Kai_SW},
            Kai_W = {this.Kai_W},
            Strasse_NW = {this.Strasse_NW},
            Strasse_NO = {this.Strasse_NO},
            Strasse_O = {this.Strasse_O},
            Strasse_SO = {this.Strasse_SO},
            Strasse_SW = {this.Strasse_SW},
            Strasse_W = {this.Strasse_W},
            Bruecke_NW = {this.Bruecke_NW},
            Bruecke_NO = {this.Bruecke_NO},
            Bruecke_O = {this.Bruecke_O},
            Bruecke_SO = {this.Bruecke_SO},
            Bruecke_SW = {this.Bruecke_SW},
            Bruecke_W = {this.Bruecke_W},
            Reich = {this.Reich},
            Krieger_eigen = {this.Krieger_eigen},
            Krieger_feind = {this.Krieger_feind},
            Krieger_freund = {this.Krieger_freund},
            Reiter_eigene = {this.Reiter_eigene},
            Reiter_feind = {this.Reiter_feind},
            Reiter_freund = {this.Reiter_freund},
            Schiffe_eigene = {this.Schiffe_eigene},
            schiffe_feind = {this.schiffe_feind},
            Schiffe_freund = {this.Schiffe_freund},
            Zauberer_eigene = {this.Zauberer_eigene},
            Zauberer_feind = {this.Zauberer_feind},
            Zauberer_freund = {this.Zauberer_freund},
            Char_eigene = {this.Char_eigene},
            Char_feind = {this.Char_feind},
            Char_freund = {this.Char_freund},
            krieger_text = '{DatabaseConverter.EscapeString(this.krieger_text)}',
            kreatur_eigen = {this.kreatur_eigen},
            kreatur_feind = {this.kreatur_feind},
            kreatur_freund = {this.kreatur_freund},
            Baupunkte = {this.Baupunkte},
            Bauwerknamen = '{DatabaseConverter.EscapeString(this.Bauwerknamen)}',
            lehensid = {this.lehensid}
             WHERE gf = {this.gf} AND kf = {this.kf} ";

            // Execute the command
            command.ExecuteNonQuery();
            SynchToOtherTables(command);

            ProgramView.Update(ViewEventArgs.ViewEventType.UpdateGebäude);
        }

        /// <summary>
        /// da manche werte doppelt gehalten werden, erfolgt hier die Synchronisation
        /// </summary>
        private void SynchToOtherTables(DbCommand command)
        {
            // synchronisation
            if (this.Gebäude != null)
            {
                this.Gebäude.Bauwerknamen = this.Bauwerknamen;
                // gebäude müssen nicht gespeichert werden, da sie aus den Karten immer wieder aktualisiert werden.
                // this.Gebäude.Save(command);
            }
        }


        public void Insert(DbCommand reader)
        {
            throw new NotImplementedException();
        }

        public bool Select()
        {
            return true;
        }

        public bool Edit()
        {
            return ProgramView.BelongsToUser(this);
        }

        public GeländeTabelle Terrain
        {
            get
            {
                return GeländeTabelle.Terrains[Gelaendetyp != null ? (int)Gelaendetyp : 0];
            }
        }

        /*public string ReichZugehörigkeit
        {
            get
            {
                if (SharedData.Nationen == null || Reich == null)
                    return string.Empty;
                return SharedData.Nationen.ElementAt(Reich.Value).Reich ?? string.Empty;
            }
        }*/

        public Nation? Nation
        {
            get
            {
                if (SharedData.Nationen == null || Reich == null)
                    return null;
                return SharedData.Nationen.ElementAt(Reich.Value);
            }
        }

        // aktuell wird die Update Queue für Gebäude nicht verwendet, da die Gebäude sehr statisch sind
        public Gebäude? Gebäude
        {
            get
            {
                return PhoenixModel.View.BauwerkeView.GetGebäude(this);
            }
        }





    }
}
