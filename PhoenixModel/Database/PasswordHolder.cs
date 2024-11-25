using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace PhoenixModel.Database
{
    public class PasswordHolder
    {
        public class EncryptedString
        {
            [JsonInclude] 
            protected string _value;
            
            public EncryptedString() { _value = string.Empty; }

            public EncryptedString(string value)
            {
                this._value = value;
            }
            public static implicit operator string(EncryptedString d)
            {
                return d._value;
            }

            public static implicit operator EncryptedString(string d)
            {
                return new EncryptedString(d);
            }
        }
        
        public interface IPasswordProvider
        {
            EncryptedString Password { get; }
        }

        [JsonIgnore] 
        private EncryptedString encryptedPasswordBase64;

        public PasswordHolder()
        { encryptedPasswordBase64 = string.Empty; }

        public PasswordHolder(string plainPassword)
        { encryptedPasswordBase64 = EncryptPassword(plainPassword); }

        // Constructor that accepts a password
        public PasswordHolder(EncryptedString? password, IPasswordProvider provider)
        {
            if (password == null || string.IsNullOrEmpty(password))
            {
                // Show dialog to enter password
                password = provider.Password;
                // Encrypt the password using the computer name
                encryptedPasswordBase64 = EncryptPassword(password);
            }
            else
            {
                encryptedPasswordBase64 = password;
            }
        }

        // Method to encrypt password using computer name
        private string EncryptPassword(string password)
        {
            // Get computer name
            string computerName = Environment.MachineName;

            // Derive key from computer name
            byte[] key = DeriveKeyFromString(computerName);

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

        // Method to decrypt password
        public string? DecryptPassword()
        {
            try
            {
                // Get computer name
                string computerName = Environment.MachineName;

                // Derive key from computer name
                byte[] key = DeriveKeyFromString(computerName);

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
                return null;
            }
        }

        
        // Method to derive key from string
        private byte[] DeriveKeyFromString(string input)
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

        // Property for JSON serialization/deserialization
        [JsonInclude]
        public EncryptedString EncryptedPasswordBase64
        {
            get { return encryptedPasswordBase64; }
            set { encryptedPasswordBase64 = value; }
        }
    }
}