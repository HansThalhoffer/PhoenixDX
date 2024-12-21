using PhoenixModel.Program;
using PhoenixWPF.Helper;
using System;
using System.Collections.Generic;
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
