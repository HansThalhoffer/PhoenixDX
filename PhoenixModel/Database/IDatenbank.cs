using System.Data;
using System.Data.Common;

namespace PhoenixModel.Database {

    /// <summary>
    /// Eine offene Verbindung zu <em>einer</em> Datenbankdatei.
    ///
    /// <b>Warum es diese Schnittstelle gibt:</b> bis hierher war die Anwendung an
    /// <c>AccessDatabase</c> namentlich gebunden - an rund zwanzig Stellen, und der Lader nahm sie
    /// als Parametertyp. Ein anderer Unterbau (SQLite) hätte jede dieser Stellen angefasst. Die
    /// Zeilenklassen selbst waren nie das Problem: sie arbeiten mit <see cref="DbDataReader"/> und
    /// <see cref="DbCommand"/>, den anbieterfreien Basisklassen von ADO.NET, und sind damit schon
    /// immer austauschbar gewesen. Nur die Verbindung fehlte.
    ///
    /// <b>Die Namen der Methoden sind englisch geblieben</b>, obwohl der neuere Quelltext deutsch
    /// benennt. Sie sind die gewachsene Schnittstelle von <c>AccessDatabase</c>; sie umzubenennen
    /// hätte jede Aufrufstelle verändert, ohne etwas zu verbessern. Was hier neu hinzukommt, heisst
    /// deutsch.
    ///
    /// <b>Lebensdauer:</b> eine Instanz ist kurzlebig und gehört in ein <c>using</c>. Sie ist
    /// <em>nicht</em> die Verbindung selbst - die gehört dem
    /// <see cref="Datenbankwaechter"/> und bleibt offen. <see cref="Open"/> leiht sie aus und
    /// sperrt sie dabei für andere, <see cref="Close"/> gibt sie zurück. Wer das Schliessen
    /// vergisst, blockiert die Datei für alle anderen bis zum Ende des Prozesses.
    /// </summary>
    public interface IDatenbank : IDisposable {

        /// <summary>
        /// Die Datei, zu der diese Verbindung gehört - der vollständige Pfad.
        /// Die Zeilenklassen tragen ihn mit, damit der Speicherlauf weiss, wohin ein Datensatz gehört.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Leiht die Verbindung aus und öffnet sie, falls sie noch nicht offen ist.
        /// </summary>
        /// <returns>
        /// false, wenn die Verbindung nicht zu bekommen war - weil sie ein anderer Ablauf zu lange
        /// belegt hält oder weil sich die Datei nicht öffnen lässt. Der Fehler ist dann bereits
        /// protokolliert. <b>Der Aufrufer muss das prüfen</b> und darf anschliessend nichts
        /// schreiben: ein nicht geschriebener Datensatz, der wie ein geschriebener aussieht, ist
        /// der schlimmste Fehler, den diese Schicht machen kann.
        /// </returns>
        bool Open();

        /// <summary>
        /// Gibt die Verbindung zurück. Offen bleibt sie trotzdem - geschlossen wird erst von
        /// <see cref="Datenbankwaechter.SchliesseAlle"/>, wenn jemand die Datei selbst braucht.
        /// </summary>
        void Close();

        /// <summary>
        /// Öffnet einen Leser über die Abfrage. Der Leser lebt bis zum <see cref="Close"/>.
        /// </summary>
        DbDataReader OpenReader(string query);

        /// <summary>
        /// Ein Befehlsobjekt für Schreibvorgänge, das die Zeilenklassen füllen.
        /// Es muss vor dem <see cref="Close"/> freigegeben werden.
        /// </summary>
        DbCommand OpenDBCommand();

        /// <summary>
        /// Führt eine Anweisung aus, die keine Zeilen liefert.
        /// </summary>
        /// <returns>die Anzahl der betroffenen Zeilen</returns>
        int ExecuteNonQuery(string commandText);

        /// <summary>
        /// Führt eine Abfrage aus und liefert den ersten Wert der ersten Zeile.
        /// </summary>
        object? ExecuteScalar(string query);

        /// <summary>
        /// Führt eine Abfrage aus und liefert das vollständige Ergebnis.
        /// Gebraucht für den Vergleich zweier Datenbanken, wo die Spalten nicht vorab bekannt sind.
        /// </summary>
        DataTable ExecuteQuery(string query);

        /// <summary>
        /// Die Namen aller Tabellen der Datenbank, ohne die Systemtabellen.
        ///
        /// Welche Tabellen es gibt, steht nirgends im Quelltext vollständig - die Anwendung lädt
        /// nur die, die sie kennt. Für den Vergleich zweier Zugdatenbanken braucht es alle.
        /// </summary>
        List<string> GetTabellennamen();

        /// <summary>
        /// Die Spalten des Primärschlüssels einer Tabelle, in der Reihenfolge des Schlüssels.
        /// Eine leere Liste heisst: die Tabelle hat keinen.
        /// </summary>
        List<string> GetPrimärschluessel(string tabelle);

        /// <summary>
        /// Beginnt eine Transaktion über die ausgeliehene Verbindung, oder liefert null, wenn der
        /// Unterbau keine kann.
        ///
        /// <b>Wofür:</b> ein Speicherdurchgang schreibt mehrere Datensätze. Reisst der Treiber
        /// mitten darin ab - und der ACE-Treiber tut das, ohne eine Ausnahme zu werfen -, dann
        /// stand bisher die halbe Hälfte in der Datei: ein Zustand, den niemand gewollt hat und
        /// den niemand mehr auseinandersortiert. Innerhalb einer Transaktion bleibt die Datei in
        /// diesem Fall unverändert.
        ///
        /// Der Aufrufer ist für <c>Commit</c> oder <c>Rollback</c> verantwortlich, und zwar in
        /// einem <c>finally</c>: eine offen gelassene Transaktion auf der gemeinsamen Verbindung
        /// trifft als nächstes einen fremden Ablauf.
        /// </summary>
        DbTransaction? BeginneTransaktion();
    }
}
