using PhoenixModel.Database;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Program
{
    internal class Karte: IDisposable
    {
        Dictionary<string, PhoenixModel.Karte.Gemark> _map = new Dictionary<string, PhoenixModel.Karte.Gemark> ();  

        AccessDatabaseConnector? _connector;
        Karte(string databaseFileName, EncryptedString encryptedpassword)
        {
            PasswordHolder holder = new PasswordHolder(encryptedpassword, new PasswortProvider());
            string? plainPassword = holder.DecryptPassword();
            _connector = new AccessDatabaseConnector(databaseFileName, plainPassword);
        }

        class PasswortProvider : PasswordHolder.IPasswordProvider
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

        public bool Load()
        {
            _connector?.Open();
            return true;
        }

        public void Dispose()
        {
            _connector?.Close();
        }
    }
}
