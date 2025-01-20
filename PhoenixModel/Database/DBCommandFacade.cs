using PhoenixModel.Database;
using System;
using System.Data;
using System.Data.Common;

/// <summary>
/// Eine Fassade für <see cref="DbCommand"/>, die Aufrufe an ein internes <see cref="DbCommand"/>-Objekt weiterleitet.
/// </summary>
public class DbCommandFacade : DbCommand {
    private readonly DbCommand dbCommand;

    /// <summary>
    /// Erstellt eine neue Instanz von <see cref="DbCommandFacade"/>.
    /// </summary>
    /// <param name="command">Das zu verwendende <see cref="DbCommand"/>-Objekt.</param>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn <paramref name="command"/> null ist.</exception>
    public DbCommandFacade(DbCommand command) {
        dbCommand = command ?? throw new ArgumentNullException(nameof(command));
    }

    /// <summary>
    /// Ruft den SQL-Befehlstext ab oder legt ihn fest.
    /// </summary>
    public override string CommandText {
        get => dbCommand.CommandText;
        set => dbCommand.CommandText = value;
    }

    /// <summary>
    /// Ruft das Zeitlimit für den Befehl in Sekunden ab oder legt es fest.
    /// </summary>
    public override int CommandTimeout {
        get => dbCommand.CommandTimeout;
        set => dbCommand.CommandTimeout = value;
    }

    /// <summary>
    /// Ruft den Typ des Befehls ab oder legt ihn fest.
    /// </summary>
    public override CommandType CommandType {
        get => dbCommand.CommandType;
        set => dbCommand.CommandType = value;
    }

    /// <summary>
    /// Ruft die zugehörige Datenbankverbindung ab oder legt sie fest.
    /// </summary>
    protected override DbConnection? DbConnection {
        get => dbCommand.Connection;
        set => dbCommand.Connection = value;
    }

    /// <summary>
    /// Ruft die Sammlung der Parameter für den Befehl ab.
    /// </summary>
    protected override DbParameterCollection DbParameterCollection => dbCommand.Parameters;

    /// <summary>
    /// Ruft die zugehörige Transaktion ab oder legt sie fest.
    /// </summary>
    protected override DbTransaction? DbTransaction {
        get => dbCommand.Transaction;
        set => dbCommand.Transaction = value;
    }

    /// <summary>
    /// Ruft einen Wert ab oder legt ihn fest, der angibt, ob der Befehl in einem Designer sichtbar ist.
    /// </summary>
    public override bool DesignTimeVisible {
        get => dbCommand.DesignTimeVisible;
        set => dbCommand.DesignTimeVisible = value;
    }

    /// <summary>
    /// Ruft die UpdateRowSource-Eigenschaft ab oder legt sie fest.
    /// </summary>
    public override UpdateRowSource UpdatedRowSource {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <summary>
    /// Bricht die Ausführung des Befehls ab.
    /// </summary>
    public override void Cancel() => dbCommand.Cancel();

    /// <summary>
    /// Führt eine SQL-Anweisung aus und gibt die Anzahl der betroffenen Zeilen zurück.
    /// </summary>
    /// <returns>Anzahl der betroffenen Zeilen.</returns>
    public override int ExecuteNonQuery() {
        DatabaseLog.Log(dbCommand);
        return dbCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Führt die Abfrage aus und gibt das erste Spaltenwert-Ergebnis der ersten Zeile zurück.
    /// </summary>
    /// <returns>Das erste Spaltenwert-Ergebnis oder null, wenn keine Zeilen vorhanden sind.</returns>
    public override object? ExecuteScalar() => dbCommand.ExecuteScalar();

    /// <summary>
    /// Bereitet den Befehl auf die Ausführung vor.
    /// </summary>
    public override void Prepare() => dbCommand.Prepare();

    /// <summary>
    /// Erstellt und gibt eine neue Instanz eines Parameters für diesen Befehl zurück.
    /// </summary>
    /// <returns>Ein neues <see cref="DbParameter"/>-Objekt.</returns>
    protected override DbParameter CreateDbParameter() => dbCommand.CreateParameter();

    /// <summary>
    /// Führt die Abfrage aus und gibt einen Datenleser zurück.
    /// </summary>
    /// <param name="behavior">Das <see cref="CommandBehavior"/>, das das Verhalten des Datenlesers steuert.</param>
    /// <returns>Ein <see cref="DbDataReader"/>, der die Abfrageergebnisse enthält.</returns>
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => dbCommand.ExecuteReader(behavior);
}
