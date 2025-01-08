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

        private EncryptedString? TryGetPasswordFromUSB()
        {
            if (string.IsNullOrEmpty(_key))
                SpielWPF.LogError("Ohne Datenbank gibt es auch kein Passwort", "Bitte den Code checken in der Klasse PasswortProvider");
            var lines = StorageSystem.CheckForPassKeyFile();
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line) == false)
                    {
                        var keyval = line.Split('=');
                        if (keyval.Length > 1)
                        {
                            if (keyval[0] == _key)
                                return new EncryptedString(keyval[1]);
                        }
                    }
                }
            }
            return null;
        }

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

        public static bool WritePasswordsToUSB(List<Passwort> list)
        {
            List<string> result = [];
            foreach (var pw in list)
            {
                if (string.IsNullOrEmpty(pw.key) == false && string.IsNullOrEmpty(pw.value) == false)
                {
                    PasswordHolder ph = new PasswordHolder(pw.value);
                    result.Add($"{pw.key}={ph.DecryptedPassword}");
                }
                else
                    SpielWPF.LogError("Key und Value müssen gesetzt sein", "Beim erstellen des Passwort Files auf dem USB Stick kam es zu einem Fehler");
            }
            return StorageSystem.WritePassKeyFile(result);
        }

        public EncryptedString Password
        {
            get
            {
                /*var pw = TryGetPasswordFromUSB();
                if (string.IsNullOrEmpty(pw) == false)
                    return pw;
                */
                PasswordDialog dialog = new PasswordDialog($"Das Passwort für die Datenbank {_key} bitte eingeben");
                dialog.ShowDialog();
                return dialog.ProvidePassword();
            }
        }
    }

}
