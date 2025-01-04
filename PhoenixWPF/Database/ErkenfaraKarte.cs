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
            PasswordHolder holder = new (_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                  Load<KleinFeld>(connector, ref SharedData.Map, Enum.GetNames(typeof(KleinFeld.Felder)));
                Load<Gebäude>(connector, ref SharedData.Gebäude, Enum.GetNames(typeof(Gebäude.Felder)));
                connector?.Close();
                return;
            }
        }

        public void Save(IDatabaseTable table)
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                    if (connector != null)
                    {
                        var command = connector.OpenDBCommand();
                        table.Save(command);
                    }
                }
                catch (Exception ex)
                {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Datenbank {_databaseFileName}", ex.Message));
                }
                connector?.Close();
            }
        }
        public void Dispose()
        {
        }

        protected override void LoadInBackground()
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptPassword()))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Laden der Erkenfare Datenbank: " , ex.Message);
                }
                connector?.Close();
            }
        }
    }
}
