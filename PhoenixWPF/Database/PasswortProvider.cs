using PhoenixModel.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database
{
    public class PasswortProvider : PasswordHolder.IPasswordProvider
    {
        string _key = string.Empty;
        public PasswortProvider(string databaseName)
        { _key = databaseName; }

        public class Passwort
        {
            public string key;
            public EncryptedString value;
            public Passwort(string key, EncryptedString value)
            {
                this.key = key;
                this.value = value;
            }

        }

      

        public EncryptedString Password
        {
            get
            {
                PasswordDialog dialog = new PasswordDialog($"Das Passwort für die Datenbank {_key} bitte eingeben");
                dialog.ShowDialog();
                return dialog.ProvidePassword();
            }
        }
    }

}
