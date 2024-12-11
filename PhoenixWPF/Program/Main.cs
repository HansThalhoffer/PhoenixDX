using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using PhoenixModel.Database;
using PhoenixModel.Program;
using PhoenixWPF.Database;
using PhoenixWPF.Helper;
using static PhoenixWPF.Program.Karte;

namespace PhoenixWPF.Program
{
    public class Main :IDisposable
    {
        private static Main _instance = new Main();
        public AppSettings? Settings { get; private set; }
        
        static public Main Instance { get { return _instance; } }

        public void InitInstance() 
        {
            Settings = new AppSettings("Settings.jpk");
            Settings.InitializeSettings();
            LoadKarte();
        }
        public bool LoadKarte()
        {
            if (Settings ==null)
                return false;

            Settings.UserSettings.DatabaseLocationKarte = FileSystem.LocateFile(Settings.UserSettings.DatabaseLocationKarte);
            PasswordHolder pwdHolder = new PasswordHolder(Settings.UserSettings.PassworPZE, new PasswortProvider());
            Settings.UserSettings.PassworPZE = pwdHolder.EncryptedPasswordBase64;
            using (Karte karte = new Karte(Settings.UserSettings.DatabaseLocationKarte, Settings.UserSettings.PassworPZE))
            {
                return karte.Load() >0;
            }
        }

        public void Dispose()
        {
            if (Settings != null) 
                Settings.Dispose(); 
        }
    }
}
