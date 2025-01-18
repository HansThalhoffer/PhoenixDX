# PhoenixWPF - Verzeichnis `Database`

Dieses Verzeichnis ist ein zentraler Bestandteil des Projekts **PhoenixWPF** innerhalb der Lösung **PhoenixDX**. Es enthält Klassen zur Verwaltung und Interaktion mit der Datenbank, einschließlich Ladevorgängen, Einstellungen und Datenkonvertierung.

## Inhalt der `Database`

### Wichtige Dateien

- **AccessDatabase.cs**  
  Enthält Funktionen zur Verbindung und Interaktion mit einer Access-Datenbank.

- **AppSettings.cs**  
  Verwaltet die Anwendungseinstellungen, die für den Zugriff auf Datenbanken und andere Konfigurationswerte benötigt werden.

- **Backgroundsave.cs**  
  Implementiert eine Funktionalität zum Speichern von Daten im Hintergrund.

- **CrossRef.cs**  
  Verknüpfungstabellen zur Referenzierung von Spielelementen innerhalb der Datenbank.

- **DatabaseLoader.cs**  
  Lädt die Datenbankinhalte und stellt die Verbindungen zu den relevanten Tabellen her.

- **ErkenfaraKarte.cs**  
  Verwaltet Kartendaten und deren Interaktion mit der Datenbank.

- **ObjectStore.cs**  
  Dient als Zwischenspeicher für Objekte und reduziert wiederholte Datenbankabfragen.

- **PasswortProvider.cs**  
  Verwaltung von Passwörtern und Authentifizierungsmechanismen.

- **PZE.cs**  
  Verwaltung zentraler Spielmechaniken und Prozesse.

- **Zugdaten.cs**  
  Enthält und verarbeitet die Daten zu Spielzügen, einschließlich historischer Informationen.

### Unterverzeichnisse

- **Generatoren/**  
  Enthält Klassen zur Generierung und Manipulation von Daten innerhalb der Datenbank.

## Verwendung
Dieses Verzeichnis stellt die wesentlichen Datenbankoperationen für **PhoenixWPF** bereit. Die Klassen arbeiten zusammen, um Daten aus der Datenbank effizient zu laden, zu speichern und zu verwalten. Änderungen sollten mit Bedacht vorgenommen werden, um die Konsistenz der Daten sicherzustellen.

---

📌 **Hinweis:** Dieses Projekt ist Teil der **PhoenixDX**-Lösung. Änderungen in diesem Verzeichnis können Auswirkungen auf andere Module haben. Bei Änderungen sollte geprüft werden, ob Abhängigkeiten betroffen sind.
