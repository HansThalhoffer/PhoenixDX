using PhoenixDX;
using PhoenixModel.Database;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
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
                }
                catch { } // wenn das mit den Passwötern im USB Stick nicht klappt, dann sind wir schön schweigsam
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
                LoadFeinderkennung(true);
            }
            else {
                Application.Current.Shutdown();
            }

            DatabaseLog.Cache.Enqueue($"Progammstart: {DateTime.Now.ToString()} \n\r");

            if (Map != null)
                Map.SetTerrainOpacity(Main.Instance.Settings.UserSettings.Opacity);

            _backgroundSave = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(1)
            };
            _backgroundSave.Tick += PerformSave;
        }

        /// <summary>
        /// den Zug wechseln, ohne die Anwendung zu beenden
        /// </summary>
        public void Zugwechsel() {

            /// gibt  es noch was zum Speichern?
            _backgroundSave?.Stop();
            PerformSave(null, new EventArgs());

            // truppen müssen auch in der Karte entfernt werden
            var armee = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
            List<KleinFeld> updateList = [];
            if (SharedData.Map != null) {
                foreach (var figur in armee) {
                    updateList.Add(SharedData.Map[figur.CreateBezeichner()]);
                }
            }
            SharedData.BilanzEinnahmen_Zugdaten?.Dispose();
            SharedData.Character?.Dispose();
            SharedData.Diplomatiechange?.Dispose();
            SharedData.Kreaturen?.Dispose();
            SharedData.Krieger?.Dispose();
            SharedData.Lehensvergabe?.Dispose();
            //  SharedData.Personal ?.Dispose();
            SharedData.Reiter?.Dispose();
            SharedData.RuestungBauwerke?.Dispose();
            SharedData.RuestungRuestorte?.Dispose();
            SharedData.Schatzkammer?.Dispose();
            SharedData.Schenkungen?.Dispose();
            SharedData.Schiffe?.Dispose();
            SharedData.Units_Zugdaten?.Dispose();
            SharedData.Zauberer?.Dispose();

            SharedData.BilanzEinnahmen_Zugdaten = null;
            SharedData.Character=null;
            SharedData.Diplomatiechange=null;
            SharedData.Kreaturen=null;
            SharedData.Krieger=null;
            SharedData.Lehensvergabe=null;
            //  SharedData.Personal =null;
            SharedData.Reiter=null;
            SharedData.RuestungBauwerke=null;
            SharedData.RuestungRuestorte=null;
            SharedData.Schatzkammer=null;
            SharedData.Schenkungen=null;
            SharedData.Schiffe=null;
            SharedData.Units_Zugdaten=null;
            SharedData.Zauberer=null;
            // die Truppen in der Darstellung löschen
            foreach (var f in updateList)
                UpdateKleinfeld(f);

            if (SelectReich()) {
                LoadZugdaten(true);
            }
            else {
                Application.Current.Shutdown();
            }
            
            // und wieder im Hintergrund speichern
            _backgroundSave?.Start();
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
                        if (data.Table.Database == Settings.UserSettings.DatabaseLocationCrossRef) {
                            db = CreateCrossRef(data.Table.Database, Settings.UserSettings.PasswordCrossRef);
                        }
                        else if (data.Table.Database == Settings.UserSettings.DatabaseLocationKarte) {
                            db = CreateKarte(data.Table.Database, Settings.UserSettings.PasswordKarte);
                        }
                        else if (data.Table.Database == Settings.UserSettings.DatabaseLocationZugdaten) {
                            db = CreateZugdaten(data.Table.Database, Settings.UserSettings.PasswordReich);
                        }
                        else if (data.Table.Database == Settings.UserSettings.DatabaseLocationPZE) {
                            db = CreatePZE(data.Table.Database, Settings.UserSettings.PasswordPZE);
                        }
                        else {
                            SpielWPF.LogError($"Die Datenbank {data.Table.Database} ist unbenkannt", $"Die daten können nicht in der Tabelle {data.Table.TableName} gespeichert werden, wenn die Datenbank nicht bekannt ist");
                        }
                        if (db != null) {
                            switch(data.Command) {
                                case DatabaseQueue.DatabaseQueueCommand.Save:
                                    db.Save(data.Table);
                                    break;
                                case DatabaseQueue.DatabaseQueueCommand.Insert:
                                    db.Insert(data.Table);
                                    break;
                                case DatabaseQueue.DatabaseQueueCommand.Delete:
                                    db.Delete(data.Table);
                                    break;
                            }
                        }
                    }
                }

                while (DatabaseLog.Cache.Count > 0) {
                    string? line;
                    if (DatabaseLog.Cache.TryDequeue(out line)) {
                        List<string> strings = [line];
                        strings.Append(Environment.NewLine);
                        File.AppendAllLinesAsync("SQL.log", strings);
                     }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error during save: {ex.Message}");
            }
        }

        /// <summary>
        /// die Anmeldemaske mit dem Passwort und der Zugauswahl wird hier gezeigt, um die spezifischen Daten des Reiches zu laden
        /// </summary>
        /// <returns></returns>
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

                        }
                        else {
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
        #region Options
        /// <summary>
        /// Ein Overlay wird gezeigt, welche die Karte einfärbt
        /// </summary>
        /// <param name="visibility"></param>
        public void SetReichOverlay(Visibility visibility) {
            if (Map != null) {
                if (visibility == Visibility.Visible)
                    Map.ReichOverlay = true;
                else
                    Map.ReichOverlay = false;
            }
        }

        /// <summary>
        /// Das Küstenrecht wird als Gelände "Küste" in der Karte eingeblendet und bekommt eine eigene Textur
        /// </summary>
        /// <param name="visibility"></param>
        public void ShowKüstenRecht(Visibility visibility) {
            Settings.UserSettings.ShowKüstenregel = visibility == Visibility.Visible;
            if (Map != null)
                Map.Küsten = Settings.UserSettings.ShowKüstenregel;
            if (SharedData.Map != null) {
                var küsten = SharedData.Map.Values.Where(f => f.HasKüstenRecht == true);
                foreach (var k in küsten) {
                    UpdateKleinfeld(k);
                }
            }
        }

        /// <summary>
        /// Ein Kleinfeld soll neu gezeichnet werden
        /// </summary>
        /// <param name="gem"></param>
        public void UpdateKleinfeld(KleinFeld gem) {
            SharedData.UpdateQueue.Enqueue(gem);
        }

        /// <summary>
        /// Ein Kleinfeld soll neu gezeichnet werden
        /// </summary>
        /// <param name="gem"></param>
        public void UpdateKleinfeld(KleinfeldPosition pos) {
            KleinFeld? kleinFeld = null;
            if ( SharedData.Map != null && SharedData.Map.TryGetValue(pos.CreateBezeichner(), out kleinFeld)) {
                SharedData.UpdateQueue.Enqueue(kleinFeld);
            }
        }
        #endregion

        /// <summary>
        /// wenn alles datenbanken geladen sind, und das Programm soweit aktiv sein darf, wird die Funktion ausgeführt
        /// </summary>
        private void EverythingLoaded() {
            ProgramView.DataLoadingCompleted();
            _backgroundSave?.Start();
            if (Settings.UserSettings.ShowKüstenregel == true)
                ShowKüstenRecht(Visibility.Visible);
        }

                   
        /// <summary>
        /// wird gefeuert wenn eine Datenbank geladen wurde
        /// </summary>
        /// <param name="database"></param>
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

     

        #region Daten Laden
        /// <summary>
        /// Das delegate wird verwendet, da Templates in C# nicht wirklich existieren
        /// </summary>
        /// <param name="databaseLocation"></param>
        /// <param name="encryptedPassword"></param>
        /// <returns></returns>
        public delegate ILoadableDatabase LoadableDatabase(string databaseLocation, string encryptedPassword);
        public void Load(ref string databaseLocation, ref string encryptedPassword, LoadableDatabase dbCreator, string databaseName, LoadCompleted? loadCompletedDelegate = null) {
            if (Settings == null)
                return;

            try {
                databaseLocation = StorageSystem.LocateFile(databaseLocation, $"Datenbank {databaseName}");
                PasswordHolder pwdHolder = new PasswordHolder(encryptedPassword, new PasswortProvider(databaseName));
                encryptedPassword = pwdHolder.EncryptedPasswordBase64;
                using (ILoadableDatabase db = dbCreator(databaseLocation, encryptedPassword)) {
                    if (loadCompletedDelegate != null)
                        db.BackgroundLoad(loadCompletedDelegate);
                    else
                        db.Load();
                }
            }
            catch (Exception ex) {
                SpielWPF.LogError($"Die Datenbank {databaseName} konnte nicht geladen werden", ex.Message);
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
            if (string.IsNullOrEmpty(Settings.DataRootPath))
                Settings.DataRootPath = StorageSystem.ExtractBasePath(databaseLocation, "_Data");
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

        /// <summary>
        /// lädt aus der Feindaufklaerung.dat die Daten und legt diese an
        /// </summary>
        /// <param name="inBackground"></param>
        public void LoadFeinderkennung(bool inBackground = false) {
            if (Settings == null || SharedData.Nationen == null || Settings.UserSettings.SelectedReich < 0)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationFeindaufklärung;
            if (string.IsNullOrEmpty(databaseLocation) && string.IsNullOrEmpty(Settings.DataRootPath) == false)
                databaseLocation = Path.Combine(Settings.DataRootPath, AppSettings.DatabaseLocationFeindaufklärung);
            var reich = ProgramView.SelectedNation;
            if (reich == null) {
                SpielWPF.LogError("Zugdaten: Es wurde kein Reich ausgewählt", "Beim Laden ist ein Fehler aufgetregten, da kein Reich ausgewählt wurde. Daher können die Fremd nicht erkannt werden.");
                return;
            }
            Feinde.LoadFeinderkennung(databaseLocation);
            if (SharedData.Feinde != null) {
                foreach (var k in SharedData.Feinde) {
                    UpdateKleinfeld(k);
                }
            }
        }
        #endregion

        public void Dispose() {
            if (Settings != null)
                Settings.Dispose();
        }
    }
}
