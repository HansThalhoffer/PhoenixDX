using PhoenixModel.Database;
using PhoenixModel.dbPZE;
using PhoenixModel.ViewModel;
using PhoenixWPF.Program;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database
{
    public class PZE : DatabaseLoader, ILoadableDatabase
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

        public void Load()
        {
            PasswordHolder holder = new (_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                    Load<Nation>(connector, ref SharedData.Nationen, Enum.GetNames(typeof(Nation.Felder)));              
                }
                catch (Exception ex)
                {
                    SpielWPF.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Fehler beim Öffnen der Datenbank {_databaseFileName} " , ex.Message));
                }
                connector?.Close();
            }
        }

        public void Save(IDatabaseTable table)
        {
            PasswordHolder holder = new(_encryptedpassword);
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword))
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
            using (AccessDatabase connector = new(_databaseFileName, holder.DecryptedPassword))
            {
                if (connector?.Open() == false)
                    return;
                try
                {
                }
                catch (Exception ex)
                {
                    SpielWPF.LogError("Fehler beim Öffnen der PZE Datenbank: " , ex.Message);
                }
                connector?.Close();
            }
        }
    }
}
