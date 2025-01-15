using PhoenixDX;
using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.EventsAndArgs;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using PhoenixWPF.Program;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using static PhoenixModel.Database.PasswordHolder;


namespace PhoenixWPF.Program {
    public class Main : IDisposable {
        private static Main _instance = new Main();
        public AppSettings Settings { get; private set; }
        internal PhoenixDX.MappaMundi? SpielDXBridge { get; set; }
        public SpielWPF? Spiel { get; set; }
        public IPropertyDisplay? PropertyDisplay { get; set; } = null;
        public IOptions? Options { get; set; } = null;
        public SelectionHistory SelectionHistory = [];
        private DispatcherTimer? _backgroundSave = null;

        static public Main Instance { get { return _instance; } }

        public void CreateInstallUSBStick() {
            var store = new ObjectStore();
            var userSettings = new UserSettings {
                PasswordCrossRef = new PasswordHolder((EncryptedString)Settings.UserSettings.PasswordCrossRef).DecryptedPassword,
                PasswordKarte = new PasswordHolder((EncryptedString)Settings.UserSettings.PasswordKarte).DecryptedPassword,
                PasswordPZE = new PasswordHolder((EncryptedString)Settings.UserSettings.PasswordPZE).DecryptedPassword,
            };

            store.Add(userSettings);

            // Serialize the store to a JSON string
            string jsonString = store.Serialize();
            StorageSystem.WritePassKeyFile(jsonString);
        }

        public static MappaMundi? Map {
            get => Main.Instance.SpielDXBridge;
        }

        public void TryLoadFromUSBStick() {
            if (string.IsNullOrEmpty(Settings.UserSettings.PasswordCrossRef) == false) {
                try {
                    var jsonString = StorageSystem.CheckForPassKeyFile();
                    if (string.IsNullOrEmpty(jsonString) == false) {
                        // Serialize the store to a JSON string
                        // Deserialize the store from the JSON string
                        var newStore = new ObjectStore();
                        newStore.Deserialize(jsonString);

                        // Retrieve the UserSettings from the new store
                        var loadedSettings = newStore.Get<UserSettings>();
                        Settings.UserSettings.PasswordCrossRef = new PasswordHolder((string)loadedSettings.PasswordCrossRef).EncryptedPasswordBase64;
                        Settings.UserSettings.PasswordKarte = new PasswordHolder((string)loadedSettings.PasswordKarte).EncryptedPasswordBase64;
                        Settings.UserSettings.PasswordPZE = new PasswordHolder((string)loadedSettings.PasswordPZE).EncryptedPasswordBase64;
                    }
                } catch { } // wenn das mit den Passwötern im USB Stick nicht klappt, dann sind wir schön schweigsam
            }
        }

        private Main() {
            Settings = new AppSettings("Settings.jpk");
            Settings.InitializeSettings();
        }

        /// <summary>
        /// StartInstance wird im MainWindow.Loaded Event aufgerufen. D.h. die Elemente sollten schon alle konstruiert sein
        /// </summary>
        public void StartInstance() {
            TryLoadFromUSBStick();
            LoadCrossRef(); // die referenzen vor der Karte laden, auch wenn es dann weniger zu sehen gibt - insgesamt geht das schneller
            LoadKarte();
            LoadPZE();
            // die Anteile laden, die im Hintergrund geladen werden können
            LoadCrossRef(true);
            LoadKarte(true);
            if (SelectReich()) {
                LoadZugdaten(true);
            } else {
                Application.Current.Shutdown();
            }

            _backgroundSave = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(1)
            };
            _backgroundSave.Tick += PerformSave;
        }

        /// <summary>
        /// StopInstancewird im MainWindow.OnClosing Event aufgerufen. 
        /// Hier sollte Speicherung abgeschlossen werden
        /// </summary>
        public void StopInstance() {
            _backgroundSave?.Stop();
            PerformSave(null, new EventArgs());
            if (Instance.SpielDXBridge != null) 
                Settings.UserSettings.Zoom = Instance.SpielDXBridge.GetZoom();
        }

        /// <summary>
        /// Hier wird geschaut, ob in der Queue zum Speichern etwas liegt. Falls ja, wird es gespeichert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PerformSave(object? sender, EventArgs e) {
            try {
                while (SharedData.StoreQueue.Count > 0) {
                    SharedData.StoreQueue.TryDequeue(out var data);
                    if (data != null) {
                        ILoadableDatabase? db = null;
                        if (data.DatabaseName == Settings.UserSettings.DatabaseLocationCrossRef) {
                            db = CreateCrossRef(data.DatabaseName, Settings.UserSettings.PasswordCrossRef);
                        } else if (data.DatabaseName == Settings.UserSettings.DatabaseLocationKarte) {
                            db = CreateKarte(data.DatabaseName, Settings.UserSettings.PasswordKarte);
                        } else if (data.DatabaseName == Settings.UserSettings.DatabaseLocationZugdaten) {
                            db = CreateZugdaten(data.DatabaseName, Settings.UserSettings.PasswordReich);
                        } else if (data.DatabaseName == Settings.UserSettings.DatabaseLocationPZE) {
                            db = CreatePZE(data.DatabaseName, Settings.UserSettings.PasswordPZE);
                        } else {
                            SpielWPF.LogError($"Die Datenbank {data.DatabaseName} ist unbenkannt", $"Die daten können nicht in der Tabelle {data.TableName} gespeichert werden, wenn die Datenbank nicht bekannt ist");
                        }
                        if (db != null) {
                            db.Save(data);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error during save: {ex.Message}");
            }
        }

        public bool SelectReich() {
            var nationen = SharedData.Nationen?.ToArray();
            if (nationen != null) {
                if (Settings != null && Settings.UserSettings != null) {
                    int r = Settings.UserSettings.SelectedReich;
                    EncryptedString encrypted = Settings.UserSettings.PasswordReich;
                    PasswordHolder holder = new(encrypted);
                    var pw = holder.DecryptedPassword;
                    string zugdatenPath = Helper.StorageSystem.ExtractBasePath(Settings.UserSettings.DatabaseLocationCrossRef, "_Data");
                    zugdatenPath = System.IO.Path.Combine(zugdatenPath, "Zugdaten");
                    StartDialog dialog = new StartDialog(nationen, r, pw, zugdatenPath);

                    bool? ok = dialog.ShowDialog();
                    if (ok != null && ok == true) {
                        if (dialog.IsSaveChecked == true) {
                            holder = new(dialog.Password);
                            Settings.UserSettings.PasswordReich = holder.EncryptedPasswordBase64;

                        } else {
                            Settings.UserSettings.PasswordReich = string.Empty;
                        }
                        Settings.UserSettings.SelectedReich = dialog.SelectedNation?.Nummer ?? -1;
                        Settings.UserSettings.SelectedZug = dialog.SelectedZug ?? -1;
                        zugdatenPath = System.IO.Path.Combine(zugdatenPath, Settings.UserSettings.SelectedZug.ToString());
                        ProgramView.SelectedNation = SharedData.Nationen?.ElementAt(Settings.UserSettings.SelectedReich);
                        zugdatenPath = System.IO.Path.Combine(zugdatenPath, $"{ProgramView.SelectedNation?.DBname}.mdb");
                        Settings.UserSettings.DatabaseLocationZugdaten = zugdatenPath;
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetReichOverlay(Visibility visibility) {
            if (Map != null) {
                if (visibility == Visibility.Visible)
                    Map.ReichOverlay = true;
                else
                    Map.ReichOverlay = false;
            }
        }


        #region Daten Laden
        /// <summary>
        /// wenn alles datenbanken geladen sind, und das Programm soweit aktiv sein darf, wird die Funktion ausgeführt
        /// </summary>
        private void EverythingLoaded() {
            ProgramView.DataLoadingCompleted();
            _backgroundSave?.Start();
        }


        public delegate ILoadableDatabase LoadableDatabase(string databaseLocation, string encryptedPassword);
        private void OnLoadCompleted(ILoadableDatabase database) {
            // aktuell wird die Update Queue für Gebäude nicht verwendet, da die Gebäude sehr statisch sind
            /*if (database is ErkenfaraKarte && SharedData.Map != null && SharedData.Gebäude != null)
            {
                foreach (var gem in SharedData.Map.Values)
                {
                    var gebäude = BauwerkeView.GetGebäude(gem);
                    if (gebäude != null)
                    {
                        this.Map?.OnUpdateEvent(new MapEventArgs(gem, MapEventType.UpdateGemark));
                    }
                }
                //this.Map?.OnUpdateEvent(new MapEventArgs(MapEventType.UpdateAll));
            }*/

            // fülle die UpdateQueue mit Kleinfeldern, die Truppen erhalten haben
            if (database is Zugdaten && SharedData.Map != null) {
                foreach (var gem in SharedData.Map.Values) {
                    var figuren = SpielfigurenView.GetSpielfiguren(gem);
                    if (figuren != null && figuren.Count > 0) {
                        UpdateKleinfeld(gem);
                    }
                }
                EverythingLoaded();
            }
        }

        public void UpdateKleinfeld(KleinFeld gem) {
            SharedData.UpdateQueue.Enqueue(gem);
        }

        public void Load(ref string databaseLocation, ref string encryptedPassword, LoadableDatabase dbCreator, string databaseName, LoadCompleted? loadCompletedDelegate = null) {
            if (Settings == null)
                return;

            databaseLocation = StorageSystem.LocateFile(databaseLocation);
            PasswordHolder pwdHolder = new PasswordHolder(encryptedPassword, new PasswortProvider(databaseName));
            encryptedPassword = pwdHolder.EncryptedPasswordBase64;
            using (ILoadableDatabase db = dbCreator(databaseLocation, encryptedPassword)) {
                if (loadCompletedDelegate != null)
                    db.BackgroundLoad(loadCompletedDelegate);
                else
                    db.Load();
            }
        }

        private ILoadableDatabase CreateCrossRef(string databaseLocation, string encryptedPassword) {
            return new CrossRef(databaseLocation, encryptedPassword);
        }

        private const string dbCrossRef = "dbCrossRef";
        public void LoadCrossRef(bool inBackground = false) {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationCrossRef;
            if (string.IsNullOrEmpty(databaseLocation) && string.IsNullOrEmpty(Settings.DataRootPath) == false)
                databaseLocation = Path.Combine(Settings.DataRootPath, AppSettings.DatabaseLocationCrossRef);
            string encryptedPassword = Settings.UserSettings.PasswordCrossRef;
            Load(ref databaseLocation, ref encryptedPassword, CreateCrossRef, dbCrossRef, inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationCrossRef = databaseLocation;
            Settings.UserSettings.PasswordCrossRef = encryptedPassword;
        }



        private ILoadableDatabase CreateKarte(string databaseLocation, string encryptedPassword) {
            return new ErkenfaraKarte(databaseLocation, encryptedPassword);
        }
        private const string ErkenfaraKarte = "ErkenfaraKarte";
        public void LoadKarte(bool inBackground = false) {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationKarte;
            if (string.IsNullOrEmpty(databaseLocation) && string.IsNullOrEmpty(Settings.DataRootPath) == false)
                databaseLocation = Path.Combine(Settings.DataRootPath, AppSettings.DatabaseLocationKarte);
            string encryptedPassword = Settings.UserSettings.PasswordKarte;
            Load(ref databaseLocation, ref encryptedPassword, CreateKarte, ErkenfaraKarte, inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationKarte = databaseLocation;
            Settings.UserSettings.PasswordKarte = encryptedPassword;
        }


        private ILoadableDatabase CreatePZE(string databaseLocation, string encryptedPassword) {
            return new PZE(databaseLocation, encryptedPassword);
        }
        private const string PZE = "PZE";
        public void LoadPZE(bool inBackground = false) {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationPZE;
            if (string.IsNullOrEmpty(databaseLocation) && string.IsNullOrEmpty(Settings.DataRootPath) == false)
                databaseLocation = Path.Combine(Settings.DataRootPath, AppSettings.DatabaseLocationPZE);
            string encryptedPassword = Settings.UserSettings.PasswordPZE;
            Load(ref databaseLocation, ref encryptedPassword, CreatePZE, PZE, inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationPZE = databaseLocation;
            Settings.UserSettings.PasswordPZE = encryptedPassword;
        }

        private ILoadableDatabase CreateZugdaten(string databaseLocation, string encryptedPassword) {
            return new Zugdaten(databaseLocation, encryptedPassword);
        }

        public void LoadZugdaten(bool inBackground = false) {
            if (Settings == null || SharedData.Nationen == null || Settings.UserSettings.SelectedReich < 0)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationZugdaten;
            string encryptedPassword = Settings.UserSettings.PasswordReich;
            var reich = ProgramView.SelectedNation;
            if (reich == null) {
                SpielWPF.LogError("Zugdaten: Es wurde kein Reich ausgewählt", "Beim Laden ist ein Fehler aufgetregten, da kein Reich ausgewählt wurde. Daher ist der Ordner für die Zugdaten unbekannt.");
                return;
            }
            Load(ref databaseLocation, ref encryptedPassword, CreateZugdaten, $"{reich.DBname}", inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationZugdaten = databaseLocation;
            Settings.UserSettings.PasswordReich = encryptedPassword;
        }
        #endregion

        public void Dispose() {
            if (Settings != null)
                Settings.Dispose();
        }
    }
}
