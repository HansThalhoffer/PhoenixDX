using PhoenixModel.Program;
using PhoenixWPF.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Database
{
    public class AppSettings :IDisposable
    {
        public UserSettings UserSettings { get; private set; }

        string? UserFilename;
        public AppSettings() {
            UserSettings = new UserSettings();
        }
        
        public void InitializeSettings(string userFileName)
        {
            UserFilename = userFileName;
            if (string.IsNullOrEmpty(UserFilename) == false)
            {
                string jsonString = FileSystem.LoadJsonFile(UserFilename);
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

        }

        public void Dispose()
        {
            UpdateSetting();
            if (UserSettings != null)
                UserSettings.PropertyChanged -= UserSettings_PropertyChanged;
        }
    }
}
