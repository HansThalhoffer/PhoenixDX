# PhoenixModel - Database

Dieses Verzeichnis gehört zum **PhoenixModel**-Projekt in der Lösung **PhoenixDX**. Es enthält Klassen zur Verwaltung und Interaktion mit der Datenbank.

## Enthaltene Dateien

### DatabaseConverter.cs
Diese Datei enthält die Logik zur Umwandlung und Verarbeitung von Datenbankeinträgen.

### DatabaseInterfaces.cs
Definiert Schnittstellen für die Kommunikation mit der Datenbank.

### DatabaseLog.cs
Diese Datei speichert Commandos zum Insert und Update als Log in einen Cache, der asynchron ausgelesen werden kann

### DbCommandFacade.cs
DbCommandFacade ist eine Fassade an das Common Data Objekt, um ohne Überladung zwischen die Funktionsaufrufe zu kommen (zB für Logs)

### PasswordHolder.cs
Verwaltet und schützt Passwörter, die für den Zugriff auf Datenbanken erforderlich sind.

## Zweck
Das `Database`-Verzeichnis stellt essentielle Funktionen zur Verfügung, die für den Zugriff und die Verwaltung von Daten innerhalb der PhoenixDX-Anwendung benötigt werden. Änderungen an diesen Dateien sollten sorgfältig geprüft werden, um die Integrität der Datenbankoperationen zu gewährleisten.