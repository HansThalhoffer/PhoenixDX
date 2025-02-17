using LiveCharts.Wpf;
using PhoenixModel.Database;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static PhoenixModel.Database.PasswordHolder;

namespace Tests {

    internal static class TestSetup {

        class TestPasswortProvider : PasswordHolder.IPasswordProvider {

            public string ForWhat = string.Empty;

            public TestPasswortProvider(string forwhat) {
                ForWhat = forwhat;
            }

            public EncryptedString Password {
                get {
                    PasswordDialog dialog = new PasswordDialog($"Das Passwort für '{ForWhat} bitte eingeben");
                    dialog.ShowDialog();
                    return dialog.ProvidePassword();
                }
            }
        }

        public static void SelectNation(string nation) {
            if (SharedData.Nationen == null)
                throw new Exception("Daten fehlen. PZE laden");
            var n = NationenView.GetNationFromString(nation);
            if (n == null)
                throw new Exception($"Nation {nation} nicht gefunden");
            ProgramView.SelectedNation = n;
        }

        public static void DispatchUI() {
            // Start a dedicated thread for the dispatcher
            var thread = new Thread(() =>
            {
                // Create a new Dispatcher for this thread
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

                // This call starts the message loop
                Dispatcher.Run();
            });

            // Must be STA for the WPF dispatcher
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // Now we can get that dispatcher object (for instance, by capturing it or storing it in a static).
            // Example: we can store it in a static field or pass it through a synchronization mechanism.
        }

        public static void Setup() {
            if (Application.Current == null) {
                new Application();
            }
        }

        public static void LoadCrossRef(bool resetPasssword, bool resetDB) {
            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();

            if (resetPasssword)
                settings.UserSettings.PasswordCrossRef = string.Empty;
            if (resetDB)
                settings.UserSettings.DatabaseLocationCrossRef = string.Empty;

            settings.UserSettings.DatabaseLocationCrossRef = StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationCrossRef, "CrossRef.mdb");
      
            // Arrange
            PasswordHolder pwdHolder = new PasswordHolder(settings.UserSettings.PasswordCrossRef, new TestPasswortProvider("CrossRef.mdb"));
            settings.UserSettings.PasswordCrossRef = pwdHolder.EncryptedPasswordBase64;
            string? databasePassword = pwdHolder.DecryptedPassword;
            Assert.NotNull(databasePassword);
            Assert.NotEmpty(databasePassword);

            if (Application.Current == null) {
                new Application();
            }

            using (var db = new CrossRef(settings.UserSettings.DatabaseLocationCrossRef, settings.UserSettings.PasswordCrossRef)) {
                db.Load();
                db.LoadBackgroundSynchronous();
            }
        }

        public static void LoadZugdaten(bool resetPasssword, bool resetDB) {
            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();

            if (resetPasssword)
                settings.UserSettings.PasswordReich = string.Empty;
            if (resetDB)
                settings.UserSettings.DatabaseLocationZugdaten = string.Empty;

            settings.UserSettings.DatabaseLocationZugdaten = StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationZugdaten, "Zugdaten.mdb");

            // Arrange
            PasswordHolder pwdHolder = new PasswordHolder(settings.UserSettings.PasswordReich, new TestPasswortProvider("Zugdaten.mdb"));
            settings.UserSettings.PasswordReich = pwdHolder.EncryptedPasswordBase64;
            string? databasePassword = pwdHolder.DecryptedPassword;
            Assert.NotNull(databasePassword);
            Assert.NotEmpty(databasePassword);

            if (Application.Current == null) {
                new Application();
            }

            using (var db = new Zugdaten(settings.UserSettings.DatabaseLocationZugdaten, settings.UserSettings.PasswordReich)) {
                db.Load();
                db.LoadBackgroundSynchronous();
            }
            
            string nat = Path.GetFileName(settings.UserSettings.DatabaseLocationZugdaten);
            nat = nat.Substring(0, nat.Length - 4);
            if (SharedData.Nationen != null) {
                var nation = NationenView.GetNationFromString(nat);
                if (nation != null) {
                    ProgramView.SelectedNation = nation;
                    settings.UserSettings.SelectedReich = nation.Nummer;
                }
            }
            string? dir = System.IO.Path.GetDirectoryName(settings.UserSettings.DatabaseLocationZugdaten);
            if (dir == null)
                return;
            string zugmonat = Path.GetFileName(dir);
            int monat = Convert.ToInt32(zugmonat);
            settings.UserSettings.SelectedZug = monat;
            
            ProgramView.SelectedMonth = monat;

        }

        /// <summary>
        /// mit der PZE kann ProgramView.SelectedNation erst gesetzt werden
        /// </summary>
        public static void LoadPZE(bool resetPasssword, bool resetDB) {
            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();

            if (resetPasssword)
                settings.UserSettings.PasswordPZE = string.Empty;
            if (resetDB)
                settings.UserSettings.DatabaseLocationPZE = string.Empty;

            settings.UserSettings.DatabaseLocationPZE = StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationPZE, "PZE.mdb");

            // Arrange
            PasswordHolder pwdHolder = new PasswordHolder(settings.UserSettings.PasswordPZE, new TestPasswortProvider("PZE.mdb"));
            settings.UserSettings.PasswordPZE = pwdHolder.EncryptedPasswordBase64;
            string? databasePassword = pwdHolder.DecryptedPassword;
            Assert.NotNull(databasePassword);
            Assert.NotEmpty(databasePassword);

            if (Application.Current == null) {
                new Application();
            }

            using (var db = new PZE(settings.UserSettings.DatabaseLocationPZE, settings.UserSettings.PasswordPZE)) {
                db.Load();
                db.LoadBackgroundSynchronous();
            }
            ProgramView.SelectedNation = NationenView.GetNationFromString("Theostelos");
        }

        public static void LoadKarte() {
            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();
            settings.UserSettings.DatabaseLocationKarte = StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationKarte, "Erkenfara.mdb");

            // Arrange
            PasswordHolder pwdHolder = new PasswordHolder(settings.UserSettings.PasswordKarte, new TestPasswortProvider("Erkenfara.mdb"));
            settings.UserSettings.PasswordKarte = pwdHolder.EncryptedPasswordBase64;
            string? databasePassword = pwdHolder.DecryptedPassword;
            Assert.NotNull(databasePassword);
            Assert.NotEmpty(databasePassword);

            if (Application.Current == null) {
                new Application();
            }

            using (var db = new ErkenfaraKarte(settings.UserSettings.DatabaseLocationKarte, settings.UserSettings.PasswordKarte)) { 
                db.Load();
                db.LoadBackgroundSynchronous();
            }
        }
    }
}
