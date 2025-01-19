using PhoenixModel.Helper;
using PhoenixModel.Program;
using PhoenixWPF.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Database {
    /// <summary>
    /// Die Einstellungen werden automatisch gespeichert, wenn Änderungen stattfinden.
    /// Wenn alles neu gemacht werden soll, dann müssen die Einstellungen gelöscht werden im
    /// Roaming App Data des Benutzers.
    /// </summary>
    public class AppSettings : IDisposable {
        /// <summary>
        /// Benutzerdefinierte Einstellungen, die im Programm verwendet werden.
        /// </summary>
        public UserSettings UserSettings { get; private set; }

        /// <summary>
        /// Das sollte auf den absoluten Pfad _Data zeigen, der als Speicherort für Daten dient.
        /// </summary>
        public string DataRootPath { get; set; } = string.Empty;

        /// <summary>
        /// Hier stehen die relativen Pfade zu den Datenbanken ab dem Verzeichnis _Data.
        /// Die absoluten Pfade werden dann im UserSettings gespeichert.
        /// </summary>
        public const string DatabaseLocationKarte = "Kartendaten\\Erkenfarakarte.mdb";

        /// <summary>
        /// Der Pfad zur PZE-Datenbank.
        /// </summary>
        public const string DatabaseLocationPZE = "Database\\PZE.mdb";

        /// <summary>
        /// Standardwerte für Reicheinstellungen, gespeichert in einer Textdatei.
        /// </summary>
        public const string DefaultValuesReiche = "EinstellungenReiche.txt";

        /// <summary>
        /// Der Pfad zur Cross-Referenzdatenbank.
        /// </summary>
        public const string DatabaseLocationCrossRef = "Crossreferenzen\\crossref.mdb";

        /// <summary>
        /// Der Pfad zur Feindaufklärungsdatenbank.
        /// </summary>
        public const string DatabaseLocationFeindaufklärung = "Feindaufklaerung\\Feindaufklaerung.dat";

        string UserFilename;

        /// <summary>
        /// Konstruktor für die AppSettings-Klasse.
        /// </summary>
        /// <param name="fileName">Der Name der Einstellungsdatei.</param>
        public AppSettings(string fileName) {
            UserSettings = new UserSettings();
            UserFilename = StorageSystem.AppSettingsFile(fileName);
        }

        /// <summary>
        /// Initialisiert die Einstellungen, indem sie die gespeicherten Werte lädt.
        /// </summary>
        public void InitializeSettings() {
            if (string.IsNullOrEmpty(UserFilename) == false) {
                string? jsonString = StorageSystem.LoadJsonFile(UserFilename);
                if (string.IsNullOrEmpty(jsonString) == false) {
                    ObjectStore store = new ObjectStore();
                    store.Deserialize(jsonString);
                    UserSettings = store.Get<UserSettings>();
                }
            }

            // Falls keine Benutzereinstellungen vorhanden sind, wird ein neues Objekt erstellt.
            if (UserSettings == null) {
                UserSettings = new UserSettings();
                UpdateSetting();
            }

            // Ereignis-Handler für Änderungen an den Benutzereinstellungen registrieren.
            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich eine Eigenschaft in den Benutzereinstellungen ändert.
        /// </summary>
        /// <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        /// <param name="e">Informationen zur geänderten Eigenschaft.</param>
        private void UserSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            // Wenn der Name der geänderten Eigenschaft mit "DatabaseLocation" beginnt und der Pfad noch nicht gesetzt ist,
            // wird der Pfad basierend auf den Benutzereinstellungen extrahiert.
            if (e.PropertyName != null && e.PropertyName.StartsWith("DatabaseLocation") && string.IsNullOrEmpty(DataRootPath)) {
                string path = PropertyProcessor.GetPropertyValue(UserSettings, e.PropertyName);
                DataRootPath = StorageSystem.ExtractBasePath(path, "_Data");
            }
            UpdateSetting();
        }

        /// <summary>
        /// Speichert die aktuellen Benutzereinstellungen in einer Datei.
        /// </summary>
        public void UpdateSetting() {
            ObjectStore store = new ObjectStore();
            if (UserSettings != null)
                store.Add<UserSettings>(UserSettings);

            // Die Einstellungen werden in eine JSON-Zeichenkette serialisiert und gespeichert.
            string jsonString = store.Serialize();
            StorageSystem.StoreJsonFile(UserFilename, jsonString);
        }

        /// <summary>
        /// Gibt die verwendeten Ressourcen frei und speichert die Einstellungen.
        /// </summary>
        public void Dispose() {
            UpdateSetting();

            // Entfernt den Event-Handler für die Benutzereinstellungen.
            if (UserSettings != null)
                UserSettings.PropertyChanged -= UserSettings_PropertyChanged;
        }
    }
}
