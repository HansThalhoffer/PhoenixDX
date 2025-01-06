using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PhoenixDX;
using PhoenixDX.Structures;
using PhoenixModel.Database;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixModel.View;
using PhoenixWPF.Database;
using PhoenixWPF.Dialogs;
using PhoenixWPF.Helper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static PhoenixModel.Database.PasswordHolder;
using static PhoenixModel.Helper.MapEventArgs;
using static PhoenixWPF.Program.ErkenfaraKarte;

namespace PhoenixWPF.Program
{
    public class Main : IDisposable
    {
        private static Main _instance = new Main();
        public AppSettings Settings { get; } = new AppSettings("Settings.jpk");
        public PhoenixDX.MappaMundi? Map { get; set; }
        public SpielWPF? Spiel { get; set; }
        public IPropertyDisplay? PropertyDisplay { get; set; } = null;
        public IOptions? Options { get; set; } = null;
        public SelectionHistory SelectionHistory = [];
        private DispatcherTimer? _backgroundSave = null;

        static public Main Instance { get { return _instance; } }


        public void InitInstance()
        {
            Settings.InitializeSettings();
            LoadCrossRef(); // die referenzen vor der Karte laden, auch wenn es dann weniger zu sehen gibt - insgesamt geht das schneller
            LoadKarte();
            LoadPZE();
            // die Anteile laden, die im Hintergrund geladen werden können
            LoadCrossRef(true);
            LoadKarte(true);
            if (SelectReich())
            {
                LoadZugdaten(true);
            }
            else
            {
                Application.Current.Shutdown();
            }

            _backgroundSave = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _backgroundSave.Tick += PerformSave;
        }

        /// <summary>
        /// Hier wird geschaut, ob in der Queue zum Speichern etwas liegt. Falls ja, wird es gespeichert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PerformSave(object? sender, EventArgs e)
        {
            try
            {
                while (SharedData.StoreQueue.Count > 0)
                {
                    SharedData.StoreQueue.TryDequeue(out var data);
                    if (data != null)
                    {
                        ILoadableDatabase? db = null;
                        if (data.DatabaseName == Settings.UserSettings.DatabaseLocationCrossRef)
                        {
                            db = CreateCrossRef(data.DatabaseName, Settings.UserSettings.PasswordCrossRef);
                        }
                        else if (data.DatabaseName == Settings.UserSettings.DatabaseLocationKarte)
                        {
                            db = CreateKarte(data.DatabaseName, Settings.UserSettings.PasswordKarte);
                        }
                        else if (data.DatabaseName == Settings.UserSettings.DatabaseLocationZugdaten)
                        {
                            db = CreateZugdaten(data.DatabaseName, Settings.UserSettings.PasswordReich);
                        }
                        else if (data.DatabaseName == Settings.UserSettings.DatabaseLocationPZE)
                        {
                            db = CreatePZE(data.DatabaseName, Settings.UserSettings.PasswordPZE);
                        }
                        else
                        {
                            SpielWPF.LogError($"Die Datenbank {data.DatabaseName} ist unbenkannt", $"Die daten können nicht in der Tabelle {data.TableName} gespeichert werden, wenn die Datenbank nicht bekannt ist");
                        }
                        if (db != null)
                        {
                            db.Save(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during save: {ex.Message}");
            }
        }

        public bool SelectReich()
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
                        return true;
                    }
                }
            }
            return false;
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
        /// <summary>
        /// wenn alles datenbanken geladen sind, und das Programm soweit aktiv sein darf, wird die Funktion ausgeführt
        /// </summary>
        private void EverythingLoaded()
        {
            ViewModel.DataLoadingCompleted();
            _backgroundSave?.Start();
        }


        public delegate ILoadableDatabase LoadableDatabase(string databaseLocation, string encryptedPassword);
        private void OnLoadCompleted(ILoadableDatabase database)
        {
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
            if (database is Zugdaten && SharedData.Map != null)
            {
                foreach (var gem in SharedData.Map.Values)
                {
                    var figuren = SpielfigurenView.GetSpielfiguren(gem);
                    if (figuren != null && figuren.Count > 0)
                    {
                        this.Map?.OnUpdateEvent(new MapEventArgs(gem, MapEventType.UpdateGemark));
                    }
                }
                EverythingLoaded();
            }

        }

        public void Load(ref string databaseLocation, ref string encryptedPassword, LoadableDatabase dbCreator, string databaseName, LoadCompleted? loadCompletedDelegate = null)
        {
            if (Settings == null)
                return;

            databaseLocation = StorageSystem.LocateFile(databaseLocation);
            PasswordHolder pwdHolder = new PasswordHolder(encryptedPassword, new DatabaseLoader.PasswortProvider(databaseName));
            encryptedPassword = pwdHolder.EncryptedPasswordBase64;
            using (ILoadableDatabase db = dbCreator(databaseLocation, encryptedPassword))
            {
                if (loadCompletedDelegate != null)
                    db.BackgroundLoad(loadCompletedDelegate);
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
            Load(ref databaseLocation, ref encryptedPassword, CreateCrossRef, "dbCrossRef", inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationCrossRef = databaseLocation;
            Settings.UserSettings.PasswordCrossRef = encryptedPassword;
        }



        private ILoadableDatabase CreateKarte(string databaseLocation, string encryptedPassword)
        {
            return new ErkenfaraKarte(databaseLocation, encryptedPassword);
        }

        public void LoadKarte(bool inBackground = false)
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationKarte;
            string encryptedPassword = Settings.UserSettings.PasswordKarte;
            Load(ref databaseLocation, ref encryptedPassword, CreateKarte, "ErkenfaraKarte", inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationKarte = databaseLocation;
            Settings.UserSettings.PasswordKarte = encryptedPassword;
        }


        private ILoadableDatabase CreatePZE(string databaseLocation, string encryptedPassword)
        {
            return new PZE(databaseLocation, encryptedPassword);
        }

        public void LoadPZE(bool inBackground = false)
        {
            if (Settings == null)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationPZE;
            string encryptedPassword = Settings.UserSettings.PasswordPZE;
            Load(ref databaseLocation, ref encryptedPassword, CreatePZE, "PZ", inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationPZE = databaseLocation;
            Settings.UserSettings.PasswordPZE = encryptedPassword;
        }

        private ILoadableDatabase CreateZugdaten(string databaseLocation, string encryptedPassword)
        {
            return new Zugdaten(databaseLocation, encryptedPassword);
        }

        public void LoadZugdaten(bool inBackground = false)
        {
            if (Settings == null || SharedData.Nationen == null || Settings.UserSettings.SelectedReich < 0)
                return;
            string databaseLocation = Settings.UserSettings.DatabaseLocationZugdaten;
            string encryptedPassword = Settings.UserSettings.PasswordReich;
            var reich = SharedData.Nationen.ElementAt(Settings.UserSettings.SelectedReich);
            Load(ref databaseLocation, ref encryptedPassword, CreateZugdaten, $"{reich.DBname}", inBackground ? OnLoadCompleted : null);
            Settings.UserSettings.DatabaseLocationZugdaten = databaseLocation;
            Settings.UserSettings.PasswordReich = encryptedPassword;
        }
        #endregion

        public void Dispose()
        {
            if (Settings != null)
                Settings.Dispose();
            _backgroundSave?.Stop();
            PerformSave(null, new EventArgs());
        }
    }
}
