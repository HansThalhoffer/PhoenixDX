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
                    // Im Testlauf darf kein modaler Dialog aufgehen - fehlt das Passwort, soll der
                    // Test mit einer klaren Meldung scheitern.
                    if (StorageSystem.BenutzerdialogeErlaubt == false)
                        throw new InvalidOperationException(
                            $"Für '{ForWhat}' ist in Tests.jpk kein Passwort hinterlegt. " +
                            "Die Datei liegt im Roaming-Verzeichnis der Anwendung und lässt sich aus Settings.jpk übernehmen.");
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
            // Kein Testlauf darf Datei- oder Passwortdialoge aufpoppen lassen. Fehlt eine Angabe,
            // scheitert der Test sofort mit einer Meldung, statt den Lauf modal zu blockieren.
            StorageSystem.BenutzerdialogeErlaubt = false;
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

            // Massgeblich für den Spielmonat ist der Verzeichnisname, nicht die settings-Tabelle der
            // Zugdatenbank - deren Monat läuft in den echten Daten vor. BestimmeAktuellenZug warnt,
            // wenn beide auseinanderlaufen.
            ProgramView.SelectedMonth = monat;
            ZugView.BestimmeAktuellenZug(monat);

        }

        /// <summary>
        /// Lädt die Zugdaten eines anderen, bereits abgeschlossenen Zuges. Die Zugverzeichnisse
        /// liegen nebeneinander und heissen nach ihrer Zugnummer, deshalb genügt es, im
        /// konfigurierten Pfad das Verzeichnis auszutauschen.
        ///
        /// Damit lassen sich Berechnungen gegen einen Monat prüfen, dessen Ergebnis schon feststeht.
        /// Der Aufrufer muss hinterher <see cref="LoadZugdaten"/> aufrufen, sonst arbeiten die
        /// folgenden Tests auf dem falschen Zug weiter.
        /// </summary>
        /// <returns>true, wenn es das Verzeichnis gibt und geladen werden konnte</returns>
        public static bool LoadZugdatenAusZug(int zug) {
            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();
            settings.UserSettings.DatabaseLocationZugdaten = StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationZugdaten, "Zugdaten.mdb");

            string? dir = Path.GetDirectoryName(settings.UserSettings.DatabaseLocationZugdaten);
            string? wurzel = dir == null ? null : Path.GetDirectoryName(dir);
            if (wurzel == null)
                return false;

            string pfad = Path.Combine(wurzel, zug.ToString(), Path.GetFileName(settings.UserSettings.DatabaseLocationZugdaten));
            return LadeZugdatenAusDatei(pfad, zug);
        }

        /// <summary>
        /// Lädt eine beliebige Zugdatenbank, auch ausserhalb des konfigurierten Datenverzeichnisses.
        /// Damit lassen sich schreibende Tests gegen eine Kopie fahren, ohne die echten Spieldaten
        /// anzufassen.
        ///
        /// Der Aufrufer muss hinterher <see cref="LoadZugdaten"/> aufrufen, sonst arbeiten die
        /// folgenden Tests auf der falschen Datenbank weiter.
        /// </summary>
        /// <returns>true, wenn es die Datei gibt und sie geladen werden konnte</returns>
        public static bool LadeZugdatenAusDatei(string pfad, int zug) {
            if (File.Exists(pfad) == false)
                return false;

            AppSettings settings = new AppSettings("Tests.jpk");
            settings.InitializeSettings();

            if (Application.Current == null)
                new Application();

            using (var db = new Zugdaten(pfad, settings.UserSettings.PasswordReich)) {
                db.Load();
                db.LoadBackgroundSynchronous();
            }

            ProgramView.SelectedMonth = zug;
            ZugView.BestimmeAktuellenZug(zug);
            return true;
        }

        /// <summary>
        /// Das Passwort der Zugdatenbanken, wie es in den Testeinstellungen hinterlegt ist
        /// </summary>
        public static PasswordHolder.EncryptedString ZugdatenPasswort {
            get {
                AppSettings settings = new AppSettings("Tests.jpk");
                settings.InitializeSettings();
                return settings.UserSettings.PasswordReich;
            }
        }

        /// <summary>
        /// Der konfigurierte Pfad zur Zugdatenbank des laufenden Zuges
        /// </summary>
        public static string ZugdatenPfad {
            get {
                AppSettings settings = new AppSettings("Tests.jpk");
                settings.InitializeSettings();
                return StorageSystem.LocateFile(settings.UserSettings.DatabaseLocationZugdaten, "Zugdaten.mdb");
            }
        }

        /// <summary>
        /// Räumt eine Spielwiese im Temp-Verzeichnis wieder ab.
        ///
        /// Mehrere Anläufe, weil eine gerade erst geschlossene Datenbankdatei kurz belegt sein
        /// kann und ein einzelner Versuch dann Reste liegen lässt. Klappt es am Ende trotzdem
        /// nicht, ist das kein Grund, einen Test scheitern zu lassen - die Spielwiese liegt im
        /// Temp-Verzeichnis.
        ///
        /// Hinweis: eine frühere Fassung dieses Kommentars hat dem Aufräumen die sporadischen
        /// Abstürze des Testhosts angelastet. Das war falsch. Die Ursache lag im Speicherweg der
        /// Anwendung, siehe SpeichernIntegrationTest.
        /// </summary>
        public static void RäumeAuf(string verzeichnis) {
            if (Directory.Exists(verzeichnis) == false)
                return;

            for (int versuch = 0; versuch < 5; versuch++) {
                try {
                    Directory.Delete(verzeichnis, true);
                    return;
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                Thread.Sleep(200);
            }
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
