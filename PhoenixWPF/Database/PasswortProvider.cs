using PhoenixModel.Database;
using PhoenixWPF.Dialogs;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixWPF.Database {
    /// <summary>
    /// Eine Implementierung von IPasswordProvider zur Bereitstellung von Datenbankpasswörtern über eine Benutzereingabe.
    /// </summary>
    public class PasswortProvider : PasswordHolder.IPasswordProvider {
        private string _key = string.Empty;

        /// <summary>
        /// Erstellt eine neue Instanz des PasswortProviders für eine bestimmte Datenbank.
        /// </summary>
        /// <param name="databaseName">Der Name der Datenbank, für die das Passwort benötigt wird.</param>
        public PasswortProvider(string databaseName) {
            _key = databaseName;
        }

        /// <summary>
        /// Repräsentiert ein gespeichertes Passwort mit einem zugehörigen Schlüssel.
        /// </summary>
        public class Passwort {
            /// <summary>
            /// Der Schlüssel für das gespeicherte Passwort.
            /// </summary>
            public string key;

            /// <summary>
            /// Der verschlüsselte Passwortwert.
            /// </summary>
            public EncryptedString value;

            /// <summary>
            /// Erstellt eine neue Instanz eines gespeicherten Passworts.
            /// </summary>
            /// <param name="key">Der Schlüssel für das Passwort.</param>
            /// <param name="value">Der verschlüsselte Passwortwert.</param>
            public Passwort(string key, EncryptedString value) {
                this.key = key;
                this.value = value;
            }
        }

        /// <summary>
        /// Holt das Passwort für die zugehörige Datenbank, indem ein Passwortdialog angezeigt wird.
        /// </summary>
        public EncryptedString Password {
            get {
                PasswordDialog dialog = new PasswordDialog($"Das Passwort für die Datenbank {_key} bitte eingeben");
                dialog.ShowDialog();
                return dialog.ProvidePassword();
            }
        }
    }
}