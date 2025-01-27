using Microsoft.Win32;
using PhoenixModel.Database;
using PhoenixWPF.Database;
using PhoenixWPF.Program;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixWPF.Helper {
    public class StorageSystem {
        /// <summary>
        /// Vorbereitung, um auf einem bestimmten USB Stick die verschlüsselten Datenbank Passwörter für den Install abzulegen
        /// wenn die Passwörter auf dem Stick mit der USB ID verschlüsselt wären, ist eine ausreichende Sicherheit gegeben
        /// </summary>
        /// <returns></returns>
        public static string? CheckForPassKeyFile() {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives) {
                if (drive.DriveType == DriveType.Removable && drive.IsReady) {
                    // Check for a specific file in the root directory
                    string filePath = GetInstallFileName(drive);
                    if (File.Exists(filePath)) {
                        string passkey = CreatePassKey(drive);
                        if (string.IsNullOrEmpty(passkey) == false) {
                            // Read bytes from file
                            string content = File.ReadAllText(filePath);

                            // Convert bytes back to string
                            string dec = Decrypt(content, passkey);
                            return dec;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// die Totalsize ist bei fast jedem USB Stick unterschiedlich genug für die Menge an Sicherheit für diese Passwörter
        /// der Aufwand die Passwörter per Tool aus den AccessDB oder der alten Executable zu holen ist viel geringer
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        private static string CreatePassKey(DriveInfo drive) {
            return $"{drive.VolumeLabel}{drive.DriveFormat}{drive.TotalSize}";
        }

        #region crypt
        public static string Encrypt(string plainText, string passkey) {
            return PasswordHolder.Encrypt(plainText, passkey);
        }

        public static string Decrypt(string cipherText, string passkey) {
            return PasswordHolder.Decrypt(cipherText, passkey);
        }
        #endregion

        private static string GetInstallFileName(DriveInfo drive) {
            string fileNameToSearch = "phoenix_install.jpk"; // Replace with your file name
            return Path.Combine(drive.RootDirectory.FullName, fileNameToSearch);
        }

        public static bool WritePassKeyFile(string content) {
            var drives = DriveInfo.GetDrives();
            int count = 0;
            foreach (var drive in drives) {
                if (drive.DriveType == DriveType.Removable && drive.IsReady) {
                    count++;
                    // Check for a specific file in the root directory
                    string filePath = GetInstallFileName(drive);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    string passkey = CreatePassKey(drive);
                    if (string.IsNullOrEmpty(passkey) == false) {
                        string enc = Encrypt(content, passkey);
                        // Write bytes to file
                        File.WriteAllText(filePath, enc);
                        SpielWPF.LogInfo($"Eine USB Stick wurde beschrieben: {drive.Name}", $"Der USB Stick {drive.Name} kann nun verwendet werden, um auf einem anderen PC die Anwendung zu starten. Dort müssen lediglich die Datenbanken lokalisiert werden beim ersten Start.");
                        return true;
                    }
                }
            }
            if (count > 0)
                SpielWPF.LogWarning($"Das Speichern auf dem USB Stick war nicht erfolgreich", $"Warum das nicht geklappt hat, hat entweder eine Fehlermeldung gesagt, oder wir wissen es auch nicht.");
            else
                SpielWPF.LogError($"Kein USB Stick wurde erkannt", $"Am System wurde kein USB Stick gefunden. Wenn tatsächlich einer drin steckt, dann nimm einen anderen. Der kann leider nicht verwendet werden.");

            return false;
        }


        public static string AppSettingsFile(string fileName) {
            // Get the path to the user's application data folder
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Optionally, create a subfolder for your application
            string appFolder = Path.Combine(appDataFolder, "PhoenixWPF");
            Directory.CreateDirectory(appFolder); // Ensures the folder exists

            // Combine the folder path with the file name
            string filePath = Path.Combine(appFolder, fileName);

            // Create the file if it doesn't exist, or open it for writing if it does
            if (!File.Exists(filePath)) {
                // Create the file
                using (FileStream fs = File.Create(filePath)) {
                    // You can write initial content here if needed
                }
            }
            return filePath;
        }

        /// <summary>
        /// Checks for the existence of "Data\PZE.mdb" in the application directory.
        /// If not found, opens a file picker to let the user locate the file.
        /// </summary>
        /// <returns>True if the file is found or selected; otherwise, false.</returns>
        public static string LocateFile(string relativePath, string windowText) {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = Path.GetFileName(relativePath);
            string fullPath = Path.Combine(appDirectory, relativePath);

            if (File.Exists(fullPath))
                return fullPath;
            //else
            {
                // todo root folder auswahl und zusammenbau der filenamen, wenn sie mit _data beginnen
            }

            if (File.Exists(fullPath) == false) {
                string filter = "Any File (*.*)|*.*";
                if (fileName.EndsWith(".mdb"))
                    filter = "Access Database Files (*.mdb)|*.mdb";
                // File not found, open file picker
                OpenFileDialog openFileDialog = new OpenFileDialog {
                    Filter = filter,
                    Title = windowText
                };

                bool? result = openFileDialog.ShowDialog();

                if (result == true && !string.IsNullOrEmpty(openFileDialog.FileName))
                    return openFileDialog.FileName;

                throw new FileNotFoundException($"Datei {relativePath} konnte nicht lokalisiert werden");
            }
            throw new FileNotFoundException($"Datei {relativePath} konnte nicht lokalisiert werden");
        }

        /// <summary>
        /// Extracts the part of the path ending with the specified folder name.
        /// </summary>
        /// <param name="fullPath">The full path to search in.</param>
        /// <param name="folderName">The folder name to find and end the path.</param>
        /// <returns>The extracted path ending with the specified folder name.</returns>
        public static string ExtractBasePath(string fullPath, string folderName) {
            int index = fullPath.IndexOf(folderName, StringComparison.OrdinalIgnoreCase);
            if (index != -1) {
                return fullPath.Substring(0, index + folderName.Length);
            }
            else {
                throw new ArgumentException($"ExtractBasePath: Das Verzeichnis '{folderName}' wurde im Pfad '{fullPath}' nicht gefunden.");
            }
        }

        /// <summary>
        /// Gets a list of directories with numeric names in the specified path.
        /// </summary>
        /// <param name="path">The path to search for directories.</param>
        /// <returns>A list of directory names that contain only numbers.</returns>
        public static List<string> GetNumericDirectories(string path) {
            if (!Directory.Exists(path)) {
                throw new DirectoryNotFoundException($"The specified path '{path}' does not exist.");
            }

            return Directory.GetDirectories(path)
                            .Select(dir => Path.GetFileName(dir))
                            .Where(name => !string.IsNullOrEmpty(name) && name.All(char.IsDigit))
                            .ToList();
        }

        /// <summary>
        /// Loads the contents of a JSON file into a string.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>A string containing the contents of the JSON file.</returns>
        public static string? LoadJsonFile(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            try {
                // Ensure the file exists
                if (!File.Exists(filePath))
                    return null;

                // Read all text from the file
                string jsonContent = File.ReadAllText(filePath);

                // Optionally, you can validate that the content is valid JSON
                // using System.Text.Json for example (uncomment below if needed)
                // JsonDocument.Parse(jsonContent);

                return jsonContent;
            }
            catch (Exception ex) {
                // Handle exceptions (e.g., log the error)
                throw new IOException($"An error occurred while reading the JSON file: {filePath}", ex);
            }
        }
        /// <summary>
        /// Loads the contents of a JSON file into a string.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>A string containing the contents of the JSON file.</returns>
        public static void StoreJsonFile(string filePath, string jsonString) {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The file path must not be null or empty.", nameof(filePath));

            try {

                // Read all text from the file
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex) {
                // Handle exceptions (e.g., log the error)
                throw new IOException($"An error occurred while writing the JSON file: {filePath}", ex);
            }
        }
    }
}
