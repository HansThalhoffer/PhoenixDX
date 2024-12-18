using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using static PhoenixModel.Database.PasswordHolder;
using static PhoenixWPF.Program.ErkenfaraKarte;

namespace PhoenixWPF.Program
{
    public class Main :IDisposable
    {
        private static Main _instance = new Main();
        public AppSettings Settings { get; } = new AppSettings("Settings.jpk");
        public PhoenixDX.MappaMundi? Map { get; set; }
        public SpielWPF? Spiel { get; set; }
        public IPropertyDisplay? PropertyDisplay { get; set; } = null;
        public IOptions? Options { get; set; } = null;
        

        static public Main Instance { get { return _instance; } }


        public void InitInstance() 
        {
            Settings.InitializeSettings();
            LoadCrossRef(); // die referenzen vor der Karte laden, auch wenn es dann weniger zu sehen gibt - insgesamt geht das schneller
            LoadKarte();            
            LoadPZE();
            BackgroundLoadCrossRef();
            SelectReich();
            
        }

        public void SelectReich()
        {
            var nationen = SharedData.Nationen?.ToArray();
            if (nationen != null)
            {
                if (Settings != null && Settings.UserSettings != null)
                {
                    int r = Settings.UserSettings.SelectedReich;
                    EncryptedString encrypted = Settings.UserSettings.PasswordReich;
                    PasswordHolder holder = new(encrypted);
                    var pw = holder.DecryptPassword();
                    StartDialog dialog = new StartDialog(nationen, r, pw);

                    bool? ok = dialog.ShowDialog();
                    if (ok != null && ok == true)
                    {
                        var pass = dialog.Password;
                        var reich = dialog.SelectedNation;
                        var safe = dialog.IsSaveChecked;
                        if (safe == true)
                        {
                            holder = new(pass);
                            Settings.UserSettings.PasswordReich = holder.EncryptedPasswordBase64;
                            Settings.UserSettings.SelectedReich = reich?.Nummer ?? -1;
                        }
                        else
                        {
                            Settings.UserSettings.SelectedReich = -1;
                            Settings.UserSettings.PasswordReich = string.Empty;
                        }
                    }
                    else
                        Application.Current.Shutdown();
                }
            }
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

        public void BackgroundLoad(ref string databaseLocation, ref string encryptedPassword, LoadableDatabase dbCreator, string databaseName)
        {
            if (Settings == null)
                return;

            databaseLocation = FileSystem.LocateFile(databaseLocation);
            PasswordHolder pwdHolder = new PasswordHolder(encryptedPassword, new DatabaseLoader.PasswortProvider(databaseName));
            encryptedPassword = pwdHolder.EncryptedPasswordBase64;
            using (ILoadableDatabase db = dbCreator(databaseLocation, encryptedPassword))
            {
                db.BackgroundLoad();
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
            Load(ref databaseLocation,ref encryptedPassword, CreateCrossRef, "dbCrossRef");
            Settings.UserSettings.DatabaseLocationCrossRef = databaseLocation;
            Settings.UserSettings.PasswordCrossRef =encryptedPassword;
        }

        public void BackgroundLoadCrossRef()
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationCrossRef;
            string encryptedPassword = Settings.UserSettings.PasswordCrossRef;
            BackgroundLoad(ref databaseLocation, ref encryptedPassword, CreateCrossRef, "dbCrossRef");
            Settings.UserSettings.DatabaseLocationCrossRef = databaseLocation;
            Settings.UserSettings.PasswordCrossRef = encryptedPassword;
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
