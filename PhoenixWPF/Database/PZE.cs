using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixWPF.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;
using static PhoenixWPF.Program.ErkenfaraKarte;
using System.IO;

namespace PhoenixWPF.Database
{
    public class PZE : ILoadableDatabase
    {
        EncryptedString _encryptedpassword;
        string _databaseFileName;
        public EncryptedString Encryptedpassword { get => _encryptedpassword; set => _encryptedpassword = value; }
        public string DatabaseFileName { get => _databaseFileName; set => _databaseFileName = value; }


        public PZE(string databaseFileName, EncryptedString encryptedpassword)
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

 

        public void UpdateKarte()
        {
            Task.Run(() => { _UpdateKarte(); });
        }

        public void _UpdateKarte()
        {
            while (SharedData.Map == null || SharedData.Map.IsBlocked == true || SharedData.Map.IsAddingCompleted == false)
            {
                Thread.Sleep(100);
            }
            using (SharedData.BlockGuard guard = new(SharedData.Map))
            {
                

                foreach (var gem in SharedData.Map)
                {
                    
                }


            }
        }

        public void Load()
        {
            PasswordHolder holder = new PasswordHolder(_encryptedpassword, new PasswortProvider());
            using (AccessDatabase connector = new AccessDatabase(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                int total = 0;
                SharedData.Nationen = new BlockingCollection<Nation>();
                using (var reader = connector?.OpenReader("SELECT * FROM " + Nation.TableName + " ORDER BY Nummer"))
                {
                    while (reader != null && reader.Read())
                    {
                        var reich = LoadNation(reader);
                        SharedData.Nationen.Add(reich);
                    }
                }
                SharedData.Nationen.CompleteAdding();
                total = SharedData.Nationen.Count();
                Spiel.Log(new PhoenixModel.Program.LogEntry($"{total} Reiche geladen"));
                connector?.Close();
            }
        }

        Nation LoadNation(DbDataReader reader)
        {
            var reich = new Nation
            {
                Nummer = AccessDatabase.ToInt(reader["Nummer"]),
                Reich = AccessDatabase.ToString(reader["Reich"]),
                DBname = AccessDatabase.ToString(reader["DBname"]),
                DBpass = AccessDatabase.ToString(reader["DBpass"])
            };
            foreach (var defData in PhoenixModel.dbPZE.Defaults.ReichDefaultData.Vorbelegung)
            {
                foreach (var name in defData.Alias)
                {
                    if (name.ToUpper() == reich.Reich.ToUpper())
                    {
                        reich.Alias = defData.Alias;
                        reich.Farbname = defData.Farbname;
                        reich.Farbe = System.Drawing.ColorTranslator.FromHtml(defData.FarbeHex);
                        break;
                    }
                }
                if (reich.Farbname != null)
                    break;
            }

            return reich;
        }

        public void Dispose()
        {
        }
    }
}
