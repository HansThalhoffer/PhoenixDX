using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixWPF.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Database
{
    /// <summary>
    /// die Settings werden automatisch gespeichert, wenn Änderungen stattfinden.
    /// wenn alles neu gemacht werden soll, dann müssen die Settings gelöscht werden im
    /// Roaming App Data des Benutzers
    /// </summary>
    public class AppSettings :IDisposable
    {
        public UserSettings UserSettings { get; private set; }
        /// <summary>
        /// Das sollte zu dem absoluten Pfad _Data zeigen
        /// </summary>
        public string DataRootPath { get; set; } = string.Empty;
        /// <summary>
        /// hier stehen die relativen Pfade zu den Datenbanken ab dem Verzeichnis _Data. Die absoluten Pfade stehen dann im UserSetting
        /// Für Zugdaten gibt es keinen relativen Pfad, da das Reich der Ordnername ist
        /// </summary>
        public const string DatabaseLocationKarte = "Kartendaten\\Erkenfarakarte.mdb";
        public const string DatabaseLocationPZE = "Database\\PZE.mdb";
        public const string DefaultValuesReiche = "EinstellungenReiche.txt";
        public const string DatabaseLocationCrossRef = "Crossreferenzen\\crossref.mdb";
        public const string DatabaseLocationFeinde = "Feindaufklaerung\\Feindaufklaerung.dat";


        string UserFilename;
        public AppSettings( string fileName) {
            UserSettings = new UserSettings();
            UserFilename = StorageSystem.AppSettingsFile(fileName);
        }
        
        public void InitializeSettings()
        {
            if (string.IsNullOrEmpty(UserFilename) == false)
            {
                string? jsonString = StorageSystem.LoadJsonFile(UserFilename);
                if (string.IsNullOrEmpty(jsonString) == false)
                {
                    ObjectStore store = new ObjectStore();
                    store.Deserialize(jsonString);
                    UserSettings = store.Get<UserSettings>();
                }
            }

            if (UserSettings == null)
            {
                UserSettings = new UserSettings();
                UpdateSetting();
            }

            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null && e.PropertyName.StartsWith("DatabaseLocation") && string.IsNullOrEmpty(DataRootPath) )
            {
                string path = PropertyProcessor.GetPropertyValue(UserSettings, e.PropertyName);
                DataRootPath = StorageSystem.ExtractBasePath(path, "_Data");
            }
            UpdateSetting();
        }

        public void UpdateSetting()
        {
            ObjectStore store = new ObjectStore();
            if (UserSettings != null)
                store.Add<UserSettings>(UserSettings);
            string jsonString = store.Serialize();
            StorageSystem.StoreJsonFile(UserFilename, jsonString);
        }

        public void Dispose()
        {
            UpdateSetting();
            if (UserSettings != null)
                UserSettings.PropertyChanged -= UserSettings_PropertyChanged;
        }
    }
}
