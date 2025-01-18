# README - dbZugdaten

## Übersicht
Das Verzeichnis `dbZugdaten` im Projekt **PhoenixModel** innerhalb der Lösung **PhoenixDX** enthält Klassen zur Verwaltung und Verarbeitung der Zugdaten für das aktuelle Reich im aktuellen Zugmonat. Diese Datenbankdateien sind essenziell für die Simulation und Berechnung von wirtschaftlichen, militärischen und diplomatischen Ereignissen.

## Enthaltene Dateien

### Wirtschaft und Finanzen
- **BilanzEinnahmen.cs** – Enthält die Einnahmenbilanz des Reichs für den aktuellen Zug.
- **Schatzkammer.cs** – Verwaltet die finanzielle Lage des Reichs.
- **Schenkungen.cs** – Beinhaltet Informationen zu Geschenken und Zuwendungen innerhalb des Reichs.

### Charaktere und Einheiten
- **Character.cs** – Speichert Informationen über Charaktere im Spiel.
- **Kreaturen.cs** – Enthält Daten zu Kreaturen und Monstern.
- **Krieger.cs** – Beinhaltet Daten zu Kriegern und ihren Fähigkeiten.
- **Reiter.cs** – Enthält Daten zu Reitereinheiten.
- **Schiffe.cs** – Speichert Informationen über Schiffe und Flotten.
- **Zauberer.cs** – Enthält Informationen über Zauberer und magische Einheiten.
- **Units.cs** – Allgemeine Klasse für verschiedene Einheitstypen.

### Verwaltung und Politik
- **Diplomatiechange.cs** – Speichert Änderungen in den diplomatischen Beziehungen.
- **Lehensvergabe.cs** – Enthält Daten zur Vergabe von Lehen.
- **Personal.cs** – Beinhaltet Verwaltungsinformationen zu Bediensteten und Offizieren.
- **Ruestung.cs** – Verarbeitet Informationen zur Rüstung und militärischen Ausstattung.
- **RuestungBauwerke.cs** – Enthält Daten zu Befestigungsanlagen und Verteidigungsstrukturen.
- **RuestungRuestorte.cs** – Verarbeitet Informationen zu Rüstorten und ihrer Verwaltung.

### Systemeinstellungen
- **ZugdatenSettings.cs** – Konfigurationsdatei für die Verwaltung der Zugdaten.

## Zweck
Dieses Verzeichnis dient dazu, alle relevanten Informationen für das Reich während eines Zugmonats zu speichern und zu verarbeiten. Die enthaltenen Klassen ermöglichen Berechnungen zu Einnahmen, Diplomatie, militärischer Stärke sowie interner Verwaltung und Ressourcenmanagement.

## Nutzung
Die in diesem Verzeichnis enthaltenen Dateien werden automatisch in das Spielsystem integriert und genutzt, um den aktuellen Stand des Reichs abzubilden. Änderungen oder Erweiterungen sollten mit Bedacht vorgenommen werden, da sie direkt das Spielgeschehen beeinflussen.

---
**Projekt:** PhoenixModel  
**Lösung:** PhoenixDX
