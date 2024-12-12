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
        public void LoadCrossRef()
        {
            if (Settings == null)
                return;

            Settings.UserSettings.DatabaseLocationPZE = FileSystem.LocateFile(Settings.UserSettings.DatabaseLocationCrossRef);
            PasswordHolder pwdHolder = new PasswordHolder(Settings.UserSettings.DatabaseLocationCrossRef, new PasswortProvider());
            Settings.UserSettings.DatabaseLocationCrossRef = pwdHolder.EncryptedPasswordBase64;
            using (CrossRef crossref = new CrossRef(Settings.UserSettings.DatabaseLocationPZE, Settings.UserSettings.DatabaseLocationCrossRef))
            {
                crossref.Load();
            }
        }

        public void LoadKarte()
        {
            if (Settings ==null)
                return;

            Settings.UserSettings.DatabaseLocationKarte = FileSystem.LocateFile(Settings.UserSettings.DatabaseLocationKarte);
            PasswordHolder pwdHolder = new PasswordHolder(Settings.UserSettings.PasswordKarte, new PasswortProvider());
            Settings.UserSettings.PasswordKarte = pwdHolder.EncryptedPasswordBase64;
            using (ErkenfaraKarte karte = new ErkenfaraKarte(Settings.UserSettings.DatabaseLocationKarte, Settings.UserSettings.PasswordKarte))
            {
                karte.Load();
            }
        }

        public void LoadPZE()
        {
            if (Settings == null)
                return ;

            Settings.UserSettings.DatabaseLocationPZE = FileSystem.LocateFile(Settings.UserSettings.DatabaseLocationPZE);
            PasswordHolder pwdHolder = new PasswordHolder(Settings.UserSettings.PasswordPZE, new PasswortProvider());
            Settings.UserSettings.PasswordPZE = pwdHolder.EncryptedPasswordBase64;
            using (PZE pze = new PZE(Settings.UserSettings.DatabaseLocationPZE, Settings.UserSettings.PasswordPZE))
            {
                pze.Load();
            }
        }

        public void Dispose()
        {
            if (Settings != null) 
                Settings.Dispose(); 
        }
    }
}
