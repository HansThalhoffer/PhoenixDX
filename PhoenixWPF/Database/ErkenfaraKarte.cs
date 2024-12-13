using PhoenixDX.Structures;
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
    public class ErkenfaraKarte: DatabaseLoader, ILoadableDatabase
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;
        public EncryptedString Encryptedpassword { get => _encryptedpassword; set => _encryptedpassword = value; }
        public string DatabaseFileName { get => _databaseFileName; set => _databaseFileName = value; }

        public ErkenfaraKarte(string databaseFileName, EncryptedString encryptedpassword)
        {
           _databaseFileName = databaseFileName;
            _encryptedpassword = encryptedpassword;
        }
        public void Load()
        {
           _Load();
        }

        public void _Load()
        {
            PasswordHolder holder = new PasswordHolder(_encryptedpassword, new PasswortProvider("ERKENFARA"));
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
                Spiel.Log(new PhoenixModel.Program.LogEntry($"{total} Gemarken geladen"));

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
                Spiel.Log(new PhoenixModel.Program.LogEntry( $"{total} Gebäude geladen"));

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

       

        
        
        public void Dispose()
        {
        }
    }
}
