# PhoenixModel - Database

Dieses Verzeichnis gehört zum **PhoenixModel**-Projekt in der Lösung **PhoenixDX**. Es enthält Klassen zur Verwaltung und Interaktion mit der Datenbank.

## Enthaltene Dateien

### DatabaseConverter.cs
Diese Datei enthält die Logik zur Umwandlung und Verarbeitung von Datenbankeinträgen.

### DatabaseInterfaces.cs
Definiert Schnittstellen für die Kommunikation mit der Datenbank: `ILoadableDatabase` für eine ladbare Datenbank und `IDatabaseTable` für eine Tabellenzeile.

### IDatenbank.cs
Die Schnittstelle einer offenen Verbindung zu *einer* Datenbankdatei. Davor war die Anwendung an die Klasse `AccessDatabase` namentlich gebunden; wer den Unterbau austauschen will, kommt jetzt hier hinein. Welche Datei von welchem Unterbau bedient wird, entscheidet `PhoenixWPF/Database/Datenbankfabrik.cs`.

### Datenbankwaechter.cs
Der Besitzer der Verbindung zu einer Datenbankdatei: er hält sie offen und gibt sie nur einem Ablauf zur Zeit. Grund ist der Access-Treiber, der an der *Zahl* der Verbindungen stirbt und nicht an der Menge der Abfragen. `SchliesseAlle()` gibt die Dateien wieder frei und **muss** vor jedem Kopieren, Packen, Verschieben oder Ersetzen einer Datenbankdatei gerufen werden.

### DatabaseLog.cs
Diese Datei speichert Commandos zum Insert und Update als Log in einen Cache, der asynchron ausgelesen werden kann

### DbCommandFacade.cs
DbCommandFacade ist eine Fassade an das Common Data Objekt, um ohne Überladung zwischen die Funktionsaufrufe zu kommen (zB für Logs)

### PasswordHolder.cs
Verwaltet und schützt Passwörter, die für den Zugriff auf Datenbanken erforderlich sind.

## Zweck
Das `Database`-Verzeichnis stellt essentielle Funktionen zur Verfügung, die für den Zugriff und die Verwaltung von Daten innerhalb der PhoenixDX-Anwendung benötigt werden. Änderungen an diesen Dateien sollten sorgfältig geprüft werden, um die Integrität der Datenbankoperationen zu gewährleisten.