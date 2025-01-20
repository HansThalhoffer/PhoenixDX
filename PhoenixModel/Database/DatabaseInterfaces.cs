using PhoenixModel.EventsAndArgs;
using System.Data.Common;
using static PhoenixModel.Database.PasswordHolder;

namespace PhoenixModel.Database {

    /// <summary>
    /// Definiert eine Schnittstelle für eine ladbare Datenbank, die über eine verschlüsselte Passwortverwaltung verfügt.
    /// </summary>
    public interface ILoadableDatabase : IDisposable {
        /// <summary>
        /// Verschlüsseltes Passwort für den Zugriff auf die Datenbank.
        /// </summary>
        public EncryptedString Encryptedpassword { get; set; }

        /// <summary>
        /// Name der Datenbankdatei.
        /// </summary>
        public string DatabaseFileName { get; set; }

        /// <summary>
        /// Lädt die Datenbank.
        /// </summary>
        public void Load();

        /// <summary>
        /// Speichert eine Tabelle in der Datenbank.
        /// </summary>
        /// <param name="table">Die zu speichernde Datenbanktabelle.</param>
        public void Save(IDatabaseTable table);

        /// <summary>
        /// Lädt die Datenbank im Hintergrund und ruft nach Abschluss den angegebenen Delegate auf.
        /// </summary>
        /// <param name="loadCompletedDelegate">Der Delegate, der nach Abschluss des Ladevorgangs aufgerufen wird.</param>
        public void BackgroundLoad(LoadCompleted loadCompletedDelegate);
    }

    /// <summary>
    /// Definiert eine Schnittstelle für eine Datenbanktabelle mit grundlegenden Lade- und Speichermethoden.
    /// </summary>
    public interface IDatabaseTable {
        /// <summary>
        /// Name der Tabelle in der Datenbank.
        /// </summary>
        public abstract string TableName { get; }

        /// <summary>
        /// Name der zugehörigen Datenbank.
        /// </summary>
        public abstract string Database { get; set; }

        /// <summary>
        /// Eindeutige Kennung oder Bezeichnung der Tabelle.
        /// </summary>
        public string Bezeichner { get; }

        /// <summary>
        /// Lädt die Tabellendaten aus einem Datenbankleser.
        /// </summary>
        /// <param name="reader">Datenbankleser, der die Tabellendaten enthält.</param>
        public abstract void Load(DbDataReader reader);

        /// <summary>
        /// Speichert die Tabellendaten in die Datenbank.
        /// </summary>
        /// <param name="reader">Datenbankbefehl zur Speicherung der Daten.</param>
        public abstract void Save(DbCommand reader);

        /// <summary>
        /// Fügt neue Daten in die Tabelle ein.
        /// </summary>
        /// <param name="reader">Datenbankbefehl für das Einfügen neuer Datensätze.</param>
        public abstract void Insert(DbCommand reader);
    }
}