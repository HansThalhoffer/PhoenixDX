using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.Karte;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Program
{
    public class Karte: IDisposable
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;
        Dictionary<string, Gemark> map = new Dictionary<string, Gemark>();

        public Dictionary<string, Gemark> Map { get => map; set => map = value; }

        public Karte(string databaseFileName, EncryptedString encryptedpassword)
        {
           _databaseFileName = databaseFileName;
            _encryptedpassword = encryptedpassword;
        }

        public class PasswortProvider : PasswordHolder.IPasswordProvider
        {
            public EncryptedString Password
            {
                get 
                {
                    PasswordDialog dialog = new PasswordDialog("Das Passwort für die PZE.mdb Datenbank bitte eingeben");
                    dialog.ShowDialog();
                    return dialog.ProvidePassword();
                }
            }
        }

        public int Load()
        {
            PasswordHolder holder = new PasswordHolder(_encryptedpassword, new PasswortProvider());
            using (AccessDatabase connector = new AccessDatabase(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return 0;
                using (var reader = connector?.OpenReader("SELECT * FROM " + Gemark.TableName))
                {
                    while (reader != null && reader.Read())
                    {
                        var gemark = LoadGemark(reader);
                        if (gemark.db_xy != null)
                        {
                            if (map.ContainsKey(gemark.Bezeichner) == false)
                                map.Add(gemark.Bezeichner, gemark);
                            else
                                map[gemark.Bezeichner] = gemark;
                        }

                    }
                }
                int total = map.Count();
                connector?.Close();
                SharedData.Map = new BlockingCollection<Dictionary<string, Gemark>>(1);
                SharedData.Map.Add(map);
                SharedData.Map.CompleteAdding();
                return total;
            }
        }       

        Gemark LoadGemark(DbDataReader reader)
        {
            return new Gemark
            {
                gf = AccessDatabase.ToInt(reader["gf"]),
                kf = AccessDatabase.ToInt(reader["kf"]),
                ph_xy = AccessDatabase.ToString(reader["ph_xy"]),
                x = AccessDatabase.ToInt(reader["x"]),
                y = AccessDatabase.ToInt(reader["y"]),
                db_xy = AccessDatabase.ToString(reader["db_xy"]),
                Rand = AccessDatabase.ToInt(reader["Rand"]),
                Index = AccessDatabase.ToInt(reader["Index"]),
                Gelaendetyp = AccessDatabase.ToInt(reader["Gelaendetyp"]),
                Ruestort = AccessDatabase.ToInt(reader["Ruestort"]),
                Fluss_NW = AccessDatabase.ToInt(reader["Fluss_NW"]),
                Fluss_NO = AccessDatabase.ToInt(reader["Fluss_NO"]),
                Fluss_O = AccessDatabase.ToInt(reader["Fluss_O"]),
                Fluss_SO = AccessDatabase.ToInt(reader["Fluss_SO"]),
                Fluss_SW = AccessDatabase.ToInt(reader["Fluss_SW"]),
                Fluss_W = AccessDatabase.ToInt(reader["Fluss_W"]),
                Wall_NW = AccessDatabase.ToInt(reader["Wall_NW"]),
                Wall_NO = AccessDatabase.ToInt(reader["Wall_NO"]),
                Wall_O = AccessDatabase.ToInt(reader["Wall_O"]),
                Wall_SO = AccessDatabase.ToInt(reader["Wall_SO"]),
                Wall_SW = AccessDatabase.ToInt(reader["Wall_SW"]),
                Wall_W = AccessDatabase.ToInt(reader["Wall_W"]),
                Kai_NW = AccessDatabase.ToInt(reader["Kai_NW"]),
                Kai_NO = AccessDatabase.ToInt(reader["Kai_NO"]),
                Kai_O = AccessDatabase.ToInt(reader["Kai_O"]),
                Kai_SO = AccessDatabase.ToInt(reader["Kai_SO"]),
                Kai_SW = AccessDatabase.ToInt(reader["Kai_SW"]),
                Kai_W = AccessDatabase.ToInt(reader["Kai_W"]),
                Strasse_NW = AccessDatabase.ToInt(reader["Strasse_NW"]),
                Strasse_NO = AccessDatabase.ToInt(reader["Strasse_NO"]),
                Strasse_O = AccessDatabase.ToInt(reader["Strasse_O"]),
                Strasse_SO = AccessDatabase.ToInt(reader["Strasse_SO"]),
                Strasse_SW = AccessDatabase.ToInt(reader["Strasse_SW"]),
                Strasse_W = AccessDatabase.ToInt(reader["Strasse_W"]),
                Bruecke_NW = AccessDatabase.ToInt(reader["Bruecke_NW"]),
                Bruecke_NO = AccessDatabase.ToInt(reader["Bruecke_NO"]),
                Bruecke_O = AccessDatabase.ToInt(reader["Bruecke_O"]),
                Bruecke_SO = AccessDatabase.ToInt(reader["Bruecke_SO"]),
                Bruecke_SW = AccessDatabase.ToInt(reader["Bruecke_SW"]),
                Bruecke_W = AccessDatabase.ToInt(reader["Bruecke_W"]),
                Reich = AccessDatabase.ToInt(reader["Reich"]),
                Krieger_eigen = AccessDatabase.ToInt(reader["Krieger_eigen"]),
                Krieger_feind = AccessDatabase.ToInt(reader["Krieger_feind"]),
                Krieger_freund = AccessDatabase.ToInt(reader["Krieger_freund"]),
                Reiter_eigene = AccessDatabase.ToInt(reader["Reiter_eigene"]),
                Reiter_feind = AccessDatabase.ToInt(reader["Reiter_feind"]),
                Reiter_freund = AccessDatabase.ToInt(reader["Reiter_freund"]),
                Schiffe_eigene = AccessDatabase.ToInt(reader["Schiffe_eigene"]),
                schiffe_feind = AccessDatabase.ToInt(reader["schiffe_feind"]),
                Schiffe_freund = AccessDatabase.ToInt(reader["Schiffe_freund"]),
                Zauberer_eigene = AccessDatabase.ToInt(reader["Zauberer_eigene"]),
                Zauberer_feind = AccessDatabase.ToInt(reader["Zauberer_feind"]),
                Zauberer_freund = AccessDatabase.ToInt(reader["Zauberer_freund"]),
                Char_eigene = AccessDatabase.ToInt(reader["Char_eigene"]),
                Char_feind = AccessDatabase.ToInt(reader["Char_feind"]),
                Char_freund = AccessDatabase.ToInt(reader["Char_freund"]),
                krieger_text = AccessDatabase.ToString(reader["krieger_text"]),
                kreatur_eigen = AccessDatabase.ToInt(reader["kreatur_eigen"]),
                kreatur_feind = AccessDatabase.ToInt(reader["kreatur_feind"]),
                kreatur_freund = AccessDatabase.ToInt(reader["kreatur_freund"]),
                Baupunkte = AccessDatabase.ToInt(reader["Baupunkte"]),
                Bauwerknamen = AccessDatabase.ToString(reader["Bauwerknamen"]),
                lehensid = AccessDatabase.ToInt(reader["lehensid"])
            };
        }

        public void Dispose()
        {
        }
    }
}
