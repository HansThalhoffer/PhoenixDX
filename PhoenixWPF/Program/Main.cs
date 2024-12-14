using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PhoenixModel.Database;
using PhoenixModel.Program;
using PhoenixWPF.Database;
using PhoenixWPF.Helper;
using static PhoenixModel.Database.PasswordHolder;
using static PhoenixWPF.Program.ErkenfaraKarte;

namespace PhoenixWPF.Program
{
    public class Main :IDisposable
    {
        private static Main _instance = new Main();
        public AppSettings? Settings { get; private set; }
        public PhoenixDX.MappaMundi? Map { get; set; }
        public PhoenixWPF.Spiel? Spiel { get; set; }

        static public Main Instance { get { return _instance; } }


        public void InitInstance() 
        {
            Settings = new AppSettings("Settings.jpk");
            Settings.InitializeSettings();
            LoadCrossRef(); // die referenzen vor der Karte laden, auch wenn es dann weniger zu sehen gibt - insgesamt geht das schneller
            LoadKarte();            
            LoadPZE();
        }

        public void SetReichOverlay(Visibility visibility)
        {
            if (Map != null)
            {
                if (visibility == Visibility.Visible)
                    Map.ReichOverlay = true;
                else
                    Map.ReichOverlay = false;
            }
        }

      
        
        public delegate ILoadableDatabase LoadableDatabase(string databaseLocation, string encryptedPassword);


        public void Load(ref string databaseLocation, ref string encryptedPassword, LoadableDatabase dbCreator,string databaseName) 
        {
            if (Settings == null)
                return;

            databaseLocation = FileSystem.LocateFile(databaseLocation);
            PasswordHolder pwdHolder = new PasswordHolder(encryptedPassword, new DatabaseLoader.PasswortProvider(databaseName));
            encryptedPassword = pwdHolder.EncryptedPasswordBase64;
            using (ILoadableDatabase db = dbCreator(databaseLocation, encryptedPassword))
            {
                db.Load();
            }
        }

    
        private ILoadableDatabase CreateCrossRef(string databaseLocation, string encryptedPassword)
        {
            return new CrossRef(databaseLocation, encryptedPassword);
        }

        public void LoadCrossRef()
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationCrossRef;
            string encryptedPassword = Settings.UserSettings.PasswordCrossRef;
            Load(ref databaseLocation,ref encryptedPassword, CreateCrossRef, "CrossRef");
            Settings.UserSettings.DatabaseLocationCrossRef = databaseLocation;
            Settings.UserSettings.PasswordCrossRef =encryptedPassword;
        }

        private ILoadableDatabase CreateKarte(string databaseLocation, string encryptedPassword)
        {
            return new ErkenfaraKarte(databaseLocation, encryptedPassword);
        }

        public void LoadKarte()
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationKarte;
            string encryptedPassword = Settings.UserSettings.PasswordKarte;
            Load(ref databaseLocation, ref encryptedPassword, CreateKarte,"ErkenfaraKarte");
            Settings.UserSettings.DatabaseLocationKarte = databaseLocation;
            Settings.UserSettings.PasswordKarte = encryptedPassword;
        }


        private ILoadableDatabase CreatePZE(string databaseLocation, string encryptedPassword)
        {
            return new PZE(databaseLocation, encryptedPassword);
        }

        public void LoadPZE()
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationPZE;
            string encryptedPassword = Settings.UserSettings.PasswordPZE;
            Load(ref databaseLocation, ref encryptedPassword, CreatePZE,"PZ");
            Settings.UserSettings.DatabaseLocationPZE = databaseLocation;
            Settings.UserSettings.PasswordPZE = encryptedPassword;
        }
        
        public void Dispose()
        {
            if (Settings != null) 
                Settings.Dispose(); 
        }
    }
}
