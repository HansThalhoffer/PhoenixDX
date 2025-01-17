# Projektstruktur
Dieses Modul enthält das Datenmodell (1:1 aus der Datenbank), das Viewmodel (Sicht der Anwendung auf die Daten) und Backend-Logik zur Unterstützung der PhoenixDX-Anwendung.
Es bildet die Anwendung vollständig ab, mit allen Regeln und Datenmodifikationen.
Das Modul hat keine weiteren Abhängigkeiten außer dem .NETCore Framework.

## Ordnerstruktur

### Database
Beinhaltet Klassen zur Interaktion mit der Datenbank, wie:
- `DatabaseConverter.cs`
- `DatabaseInterfaces.cs`
Die Datenbank-Klassen erwarten primitives SQL zum Laden und Speichern. Die SQL Engine sollte 'Select ...Order BY, Update ...Where und Insert' als Befehle unterstützen
Verwendet werden die Datenbank Befehle aus System.Data.Common 'DBCommand' und 'DBReader'

### dbCrossRef
Referenztabellen der Access-Datenbank für Spielelemente wie Einheiten und Kosten, z. B.:
- `Units_crossref.cs`
- `Kosten.cs`

### dbErkenfara
Enthält Kartendaten aus der Access-Datenbank.

### dbPZE
Enthält zentrale Daten für die alte Anwendung aus der Access-Datenbank.

### dbZugdaten
Verwaltet die Zugdaten des aktuellen Reichs für den aktuellen Zugmonat in der Access-Datenbank.

### EventsAndArgs
Definiert Ereignisse und die dazu passenden Argumente für das Event-Handling.

### ExternalTables
Enthält externe Lookup-Tabellen für die Spielmechanik, z. B.:
- `ReichTabelle.cs`
Diese Tabellen sind aktuell als Listen oder Dictionaries im Code implementiert.

### Extensions
Funktionale Erweiterungen für Datenbankklassen zur Vereinfachung und besseren Lesbarkeit.

### Helper
Hilfsklassen für:
- Datenmanipulation
- Ereignisargumente
- Geteilte Eigenschaften

### Program
Beinhaltet verschiedene Klassen für:
- Benutzereinstellungen
- Anwendungstati in (ProgramView) - wie zB die gerade ausgwählte Nation
- Logging

### View
Hier findet die Datenmanipulation und Verarbeitung der Anwendungsdaten statt.

### ViewModel
Datenklassen, die nicht 1:1 aus der Datenbank stammen. Dazu gehören:
- Basisklassen für Datenbankklassen
- Berechnungsklassen für die Darstellung


