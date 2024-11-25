using System;
using System.IO;
using Microsoft.Win32;

namespace PhoenixWPF.Helper
{
    public class FileSystem
    {

        public static string AppSettingsFile(string fileName)
        {
            // Get the path to the user's application data folder
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Optionally, create a subfolder for your application
            string appFolder = Path.Combine(appDataFolder, "YourAppName");
            Directory.CreateDirectory(appFolder); // Ensures the folder exists

            // Combine the folder path with the file name
            string filePath = Path.Combine(appFolder, fileName);

            // Create the file if it doesn't exist, or open it for writing if it does
            if (!File.Exists(filePath))
            {
                // Create the file
                using (FileStream fs = File.Create(filePath))
                {
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
        public static string LocateFile(string relativePath)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = Path.GetFileName(relativePath);
            string fullPath = Path.Combine(appDirectory, relativePath);

            if (File.Exists(fullPath))
                return fullPath;
            else
            {
                string filter = "Any File (*.*)|*.*";
                if (fileName.EndsWith(".mdb"))
                    filter = "Access Database Files (*.mdb)|*.mdb";
                // File not found, open file picker
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = filter,
                    Title = "Locate " + fileName
                };

                bool? result = openFileDialog.ShowDialog();

                if (result == true && !string.IsNullOrEmpty(openFileDialog.FileName))
                    return openFileDialog.FileName;
                else
                    return "";
            }
        }

        /// <summary>
        /// Loads the contents of a JSON file into a string.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>A string containing the contents of the JSON file.</returns>
        public static string LoadJsonFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The file path must not be null or empty.", nameof(filePath));

            try
            {
                // Ensure the file exists
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("The specified JSON file was not found.", filePath);

                // Read all text from the file
                string jsonContent = File.ReadAllText(filePath);

                // Optionally, you can validate that the content is valid JSON
                // using System.Text.Json for example (uncomment below if needed)
                // JsonDocument.Parse(jsonContent);

                return jsonContent;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                throw new IOException($"An error occurred while reading the JSON file: {filePath}", ex);
            }
        }
        /// <summary>
        /// Loads the contents of a JSON file into a string.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <returns>A string containing the contents of the JSON file.</returns>
        public static void StoreJsonFile(string filePath, string jsonString)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The file path must not be null or empty.", nameof(filePath));

            try
            {
              
                // Read all text from the file
                File.WriteAllText( filePath, jsonString);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                throw new IOException($"An error occurred while writing the JSON file: {filePath}", ex);
            }
        }
    }
}
