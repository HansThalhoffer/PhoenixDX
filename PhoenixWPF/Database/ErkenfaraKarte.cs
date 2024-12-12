﻿using PhoenixDX.Structures;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.dbErkenfara;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;
using PhoenixModel.dbPZE;
using static PhoenixModel.Helper.SharedData;

namespace PhoenixWPF.Program
{
    public class ErkenfaraKarte: IDisposable
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;     
        public ErkenfaraKarte(string databaseFileName, EncryptedString encryptedpassword)
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
                    PasswordDialog dialog = new PasswordDialog("Das Passwort für die Erkenfarakarte.mdb Datenbank bitte eingeben");
                    dialog.ShowDialog();
                    return dialog.ProvidePassword();
                }
            }
        }
        public void Load()
        {
           _Load();
        }

        public void _Load()
        {
            PasswordHolder holder = new PasswordHolder(_encryptedpassword, new PasswortProvider());
            using (AccessDatabase connector = new AccessDatabase(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                SharedData.Map = new SharedData.BlockingDictionary<Gemark>(2, 6530);
                SharedData.Map.IsBlocked = true;
                using (var reader = connector?.OpenReader("SELECT * FROM " + Gemark.TableName))
                {
                    while (reader != null && reader.Read())
                    {
                        var gemark = LoadGemark(reader);
                        if (gemark.db_xy != null)
                        {
                            if (SharedData.Map.ContainsKey(gemark.Bezeichner) == false)
                                SharedData.Map.TryAdd(gemark.Bezeichner, gemark);
                            else
                                SharedData.Map[gemark.Bezeichner] = gemark;
                        }
                    }
                }
                SharedData.Map.IsBlocked = false;
                SharedData.Map.IsAddingCompleted = true;
                int total = SharedData.Map.Count();
                Spiel.Log(Spiel.LogType.Info, $"{total} Gemarken geladen");

                SharedData.Gebäude = new BlockingDictionary<Gebäude>();
                using (var reader = connector?.OpenReader("SELECT gf,kf,Reich,Bauwerknamen FROM " + PhoenixModel.dbErkenfara.Gebäude.TableName ))
                {
                    while (reader != null && reader.Read())
                    {
                        var obj = LoadGebäude(reader);
                        SharedData.Gebäude.TryAdd(obj.Bezeichner,obj);
                    }
                }
                SharedData.Gebäude.IsAddingCompleted = true;
                total = SharedData.Gebäude.Count();
                Spiel.Log(Spiel.LogType.Info, $"{total} Gebäude geladen");

                SharedData.Map.IsAddingCompleted = true;
                connector?.Close();
                return;
            }
        }

        Gebäude LoadGebäude(DbDataReader reader)
        {
            return new Gebäude
            {
                gf = AccessDatabase.ToInt(reader[0]),
                kf = AccessDatabase.ToInt(reader[1]),
                Reich = AccessDatabase.ToString(reader[2]),
                Bauwerknamem = AccessDatabase.ToString(reader[3])
            };
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
                Ruestort = AccessDatabase.ToInt(reader["RuestortSymbol"]),
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
