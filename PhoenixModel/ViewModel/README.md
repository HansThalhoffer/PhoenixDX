# PhoenixModel - ViewModel

Dieses Verzeichnis ist Teil des **PhoenixModel**-Projekts in der **PhoenixDX**-Lösung. Es enthält verschiedene ViewModel-Klassen, die für die Verwaltung und Darstellung von Daten innerhalb der Anwendung genutzt werden.

## Inhalt der Dateien

### Kernklassen
- **Armee.cs**: Repräsentiert eine Armee im Spiel.
- **Spielfigur.cs**: Basisklasse für Spielfiguren im Spiel.
- **NamensSpielfigur.cs**: Erweiterung von `Spielfigur` für benannte Spielfiguren.
- **TruppenSpielfigur.cs**: Repräsentiert eine Truppe als spezielle Spielfigur.

### Unterstützende Klassen
- **BlockingDictionary.cs**: Thread-sicheres Wörterbuch zur Verwaltung von Daten.
- **DiplomatieView.cs**: Stellt diplomatische Beziehungen zwischen Entitäten dar.
- **EditableAttribute.cs**: Hilfsklasse zur Kennzeichnung von editierbaren Eigenschaften.
- **Eigenschaft.cs**: Verwaltet Eigenschaften von Spielfiguren.
- **ExpectedIncome.cs**: Berechnet erwartete Einkünfte.
- **TruppenStatus.cs**: Speichert und verwaltet den Status von Truppen.

### Koordinaten- und Positionsverwaltung
- **KartenKoordinaten.cs**: Enthält Methoden zur Umrechnung und Verwaltung von Kartenkoordinaten.
- **KleinfeldPosition.cs**: Definiert Positionen einzelner Felder auf der Karte.
- **Position.cs**: Allgemeine Positionsverwaltung für Spielobjekte.
- **Plausibilität.cs**: Enthält Prüfmechanismen zur Sicherstellung der Plausibilität von Spielaktionen.

### Gemeinsame Daten
- **SharedData.cs**: Verwaltung und Bereitstellung von gemeinsam genutzten Daten in der Anwendung.

## Zweck des Verzeichnisses
Dieses Verzeichnis fasst alle ViewModel-Klassen zusammen, die nicht direkt aus der Datenbank stammen, sondern zur strukturierten Verwaltung und Verarbeitung von Spieldaten dienen. Sie sind essenziell für die Darstellung und Berechnung innerhalb der Anwendung **PhoenixDX**.
