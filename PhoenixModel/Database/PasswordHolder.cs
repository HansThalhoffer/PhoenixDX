using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace PhoenixModel.Database
{
    /// <summary>
    /// Verwaltet verschlüsselte Passwörter und bietet Methoden zur Verschlüsselung und Entschlüsselung.
    /// </summary>
    public class PasswordHolder
    {
        /// <summary>
        /// Eine Klasse zur Speicherung eines verschlüsselten Strings.
        /// </summary>
        public class EncryptedString
        {
            [JsonInclude] 
            protected string _value = string.Empty;

            /// <summary>
            /// Erstellt eine Instanz von EncryptedString mit einem leeren Wert.
            /// </summary>
            public EncryptedString() { _value = string.Empty; }

            /// <summary>
            /// Erstellt eine Instanz von EncryptedString mit einem gegebenen Wert.
            /// </summary>
            /// <param name="value">Der zu speichernde verschlüsselte Wert.</param>
            public EncryptedString(string? value)
            {
                if (value!= null)
                    this._value = value;
            }
            /// <summary>
            /// Implizite Konvertierung von EncryptedString zu string.
            /// Erlaubt die Benutzung wie ein normaler String, z. B. bei string.IsNullOrEmpty.
            /// </summary>
            public static implicit operator string(EncryptedString? d)
            {
                if (d == null) 
                    return string.Empty;
                return d._value;
            }
            /// <summary>
            /// Implizite Konvertierung von string zu EncryptedString.
            /// </summary>
            public static implicit operator EncryptedString(string? d)
            {
                return new EncryptedString(d);
            }
        }
        /// <summary>
        /// Schnittstelle zur Bereitstellung eines Passworts.
        /// </summary>

        public interface IPasswordProvider
        {
            EncryptedString Password { get; }
        }

        [JsonIgnore] 
        private EncryptedString _encryptedPasswordBase64;

        /// <summary>
        /// Erstellt eine neue Instanz von PasswordHolder mit einem leeren Passwort.
        /// </summary>
        public PasswordHolder()
        { _encryptedPasswordBase64 = string.Empty; }

        /// <summary>
        /// Erstellt eine neue Instanz von PasswordHolder mit einem NICHT verschlüsselten Passwort.
        /// </summary>
        /// <param name="plainPassword">Das unverschlüsselte Passwort.</param>
        public PasswordHolder(string plainPassword)
        { 
            _encryptedPasswordBase64 = EncryptPassword(plainPassword); 
        }
        /// <summary>
        /// Erstellt eine neue Instanz von PasswordHolder mit einem verschlüsselten Passwort.
        /// </summary>
        /// <param name="plainPassword">Das unverschlüsselte Passwort.</param>
        public PasswordHolder(EncryptedString password)
        {
            _encryptedPasswordBase64 = password;
        }

        /// <summary>
        /// Erstellt eine neue Instanz von PasswordHolder mit einer Passwortquelle.
        /// </summary>
        /// <param name="password">Das Passwort, falls bereits vorhanden.</param>
        /// <param name="provider">Die Quelle für das Passwort, falls es nicht angegeben wurde.</param>
        public PasswordHolder(EncryptedString? password, IPasswordProvider provider)
        {
            if (password == null || string.IsNullOrEmpty(password))
            {
                // Show dialog to enter password
                password = provider.Password;
                // Encrypt the password using the computer name
                _encryptedPasswordBase64 = EncryptPassword(password);
            }
            else
            {
                _encryptedPasswordBase64 = password;
            }
        }
        /// <summary>
        /// Verschlüsselt ein Passwort mit einem angegebenen Schlüssel.
        /// </summary>
        /// <param name="password">Das zu verschlüsselnde Passwort.</param>
        /// <param name="passkey">Der Schlüssel für die Verschlüsselung.</param>
        /// <returns>Das verschlüsselte Passwort als Base64-String.</returns>

        public static string Encrypt(string password, string passkey)
        {
            // Derive key from computer name
            byte[] key = DeriveKeyFromString(passkey);

            // Encrypt password
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                // Generate and store IV
                aes.GenerateIV();
                byte[] iv = aes.IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                byte[] encryptedPasswordBytes;
                using (var ms = new System.IO.MemoryStream())
                {
                    // Prepend IV to the encrypted data
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(passwordBytes, 0, passwordBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    encryptedPasswordBytes = ms.ToArray();
                }

                // Convert to base64 string
                string encryptedPasswordBase64 = Convert.ToBase64String(encryptedPasswordBytes);
                return encryptedPasswordBase64;
            }
        }

        /// <summary>
        /// Verschlüsselt ein Passwort unter Verwendung des Rechnernamens als Schlüssel.
        /// </summary>
        private string EncryptPassword(string password)
        {
            // Get computer name
            return Encrypt(password, Environment.MachineName);
        }

        /// <summary>
        /// Gibt das entschlüsselte Passwort zurück.
        /// </summary>
        public string DecryptedPassword
        {
            get
            { // Get computer name
                return Decrypt(_encryptedPasswordBase64, Environment.MachineName);
            }
        }

        /// <summary>
        /// Entschlüsselt ein Passwort.
        /// </summary>
        public static string Decrypt(string encryptedPasswordBase64, string passkey)
        {
            try
            {
              
                // Derive key from computer name
                byte[] key = DeriveKeyFromString(passkey);

                // Convert encrypted password from base64
                byte[] encryptedPasswordBytes = Convert.FromBase64String(encryptedPasswordBase64);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;

                    // Extract IV from the beginning of encrypted data
                    byte[] iv = new byte[16];
                    Array.Copy(encryptedPasswordBytes, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    byte[] passwordBytes;

                    using (var ms = new System.IO.MemoryStream(encryptedPasswordBytes, iv.Length, encryptedPasswordBytes.Length - iv.Length))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var reader = new System.IO.MemoryStream())
                    {
                        cs.CopyTo(reader);
                        passwordBytes = reader.ToArray();
                    }

                    string password = Encoding.UTF8.GetString(passwordBytes);
                    return password;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error decrypting password: " + ex.Message);
                return string.Empty;
            }
        }

        
        /// <summary>
        /// erzeugt einen Schlüssen für die Verschlüsselung
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static byte[] DeriveKeyFromString(string input)
        {
            // Use a fixed salt value (consider using a unique salt in production)
            byte[] salt = Encoding.UTF8.GetBytes("Theostelos");

            // Derive a 256-bit key using PBKDF2
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password: input,
                salt: salt,
                iterations: 1000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32); // Corrected parameter name

            return key;
        }

        /// <summary>
        /// Property for JSON serialization/deserialization
        /// </summary>
        [JsonInclude]
        public EncryptedString EncryptedPasswordBase64
        {
            get { return _encryptedPasswordBase64; }
            set { _encryptedPasswordBase64 = value; }
        }
    }
}