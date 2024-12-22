using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PhoenixDX.Structures;
using PhoenixModel.Database;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
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
            LoadZugdaten();
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
                    string zugdatenPath = Helper.StorageSystem.ExtractBasePath(Settings.UserSettings.DatabaseLocationCrossRef, "_Data");
                    zugdatenPath = System.IO.Path.Combine(zugdatenPath, "Zugdaten");
                    StartDialog dialog = new StartDialog(nationen, r, pw, zugdatenPath);

                    bool? ok = dialog.ShowDialog();
                    if (ok != null && ok == true)
                    {
                        if (dialog.IsSaveChecked == true)
                        {
                            holder = new(dialog.Password);
                            Settings.UserSettings.PasswordReich = holder.EncryptedPasswordBase64;
                            
                        }
                        else
                        {
                            Settings.UserSettings.PasswordReich = string.Empty;
                        }
                        Settings.UserSettings.SelectedReich = dialog.SelectedNation?.Nummer ?? -1;
                        Settings.UserSettings.SelectedZug = dialog.SelectedZug ?? -1;
                        zugdatenPath = System.IO.Path.Combine(zugdatenPath, Settings.UserSettings.SelectedZug.ToString());
                        ViewModel.SelectedNation = SharedData.Nationen?.ElementAt(Settings.UserSettings.SelectedReich);
                        zugdatenPath = System.IO.Path.Combine(zugdatenPath, $"{ViewModel.SelectedNation?.DBname}.mdb");
                        Settings.UserSettings.DatabaseLocationZugdaten = zugdatenPath;
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


        #region Daten Laden
        public delegate ILoadableDatabase LoadableDatabase(string databaseLocation, string encryptedPassword);


        public void Load(ref string databaseLocation, ref string encryptedPassword, LoadableDatabase dbCreator,string databaseName, bool inBackground) 
        {
            if (Settings == null)
                return;

            databaseLocation = StorageSystem.LocateFile(databaseLocation);
            PasswordHolder pwdHolder = new PasswordHolder(encryptedPassword, new DatabaseLoader.PasswortProvider(databaseName));
            encryptedPassword = pwdHolder.EncryptedPasswordBase64;
            using (ILoadableDatabase db = dbCreator(databaseLocation, encryptedPassword))
            {
                if (inBackground)
                    db.BackgroundLoad();
                else
                    db.Load();
            }
        }

        private ILoadableDatabase CreateCrossRef(string databaseLocation, string encryptedPassword)
        {
            return new CrossRef(databaseLocation, encryptedPassword);
        }

        public void LoadCrossRef(bool inBackground = false)
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationCrossRef;
            string encryptedPassword = Settings.UserSettings.PasswordCrossRef;
            Load(ref databaseLocation,ref encryptedPassword, CreateCrossRef, "dbCrossRef", inBackground);
            Settings.UserSettings.DatabaseLocationCrossRef = databaseLocation;
            Settings.UserSettings.PasswordCrossRef =encryptedPassword;
        }

        public void BackgroundLoadCrossRef()
        {
            LoadCrossRef(true);
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
            Load(ref databaseLocation, ref encryptedPassword, CreateKarte,"ErkenfaraKarte", false);
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
            Load(ref databaseLocation, ref encryptedPassword, CreatePZE,"PZ", false);
            Settings.UserSettings.DatabaseLocationPZE = databaseLocation;
            Settings.UserSettings.PasswordPZE = encryptedPassword;
        }

        private ILoadableDatabase CreateZugdaten(string databaseLocation, string encryptedPassword)
        {
            return new Zugdaten(databaseLocation, encryptedPassword);
        }

        public void LoadZugdaten(bool inBackground = false)
        {
            if (Settings == null || SharedData.Nationen == null|| Settings.UserSettings.SelectedReich < 0)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationZugdaten;
            string encryptedPassword = Settings.UserSettings.PasswordReich;
            var reich = SharedData.Nationen.ElementAt(Settings.UserSettings.SelectedReich);
            Load(ref databaseLocation, ref encryptedPassword, CreateZugdaten, $"{reich.DBname}", true);
            Settings.UserSettings.DatabaseLocationZugdaten = databaseLocation;
            Settings.UserSettings.PasswordReich = encryptedPassword;
        }
        #endregion

        public void Dispose()
        {
            if (Settings != null) 
                Settings.Dispose(); 
        }
    }
}
