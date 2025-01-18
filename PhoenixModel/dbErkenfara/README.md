# README - dbErkenfara

Dieses Verzeichnis gehört zum Projekt **PhoenixModel** in der Lösung **PhoenixDX** und enthält Klassen zur Verwaltung der Kartendaten.

## Inhalt

### Bestiarium.cs
Diese Datei verwaltet Informationen zu Kreaturen und Monstern in der Spielwelt.

### Gebäude.cs
Enthält Daten und Definitionen für Gebäude innerhalb der Weltkarte.

### KleinFeld.cs
Verantwortlich für die Verwaltung aller Felder auf der Karte, einschließlich ihrer Eigenschaften und Beziehungen. Die Summe aller Felder bildet die Karte in SharedData.Map

### ReichCrossref.cs
Bietet eine Referenztabelle für Reiche und deren Zuordnungen zu anderen Spielelementen.

### Weltbilanz.cs
Enthält Mechanismen zur Berechnung der Gesamtbilanz der Spielwelt, z. B. Ressourcen und wirtschaftliche Entwicklungen.

### Zugreihenfolge.cs
Definiert die Reihenfolge, in der die Züge der Spieler oder KI abgearbeitet werden.

## Zweck
Das Verzeichnis `dbErkenfara` enthält Datenbank-Klassen zur Verwaltung der Kartendaten, einschließlich Gelände, Gebäude, Einheiten und ökonomischer Aspekte. Diese Klassen interagieren mit der zugrunde liegenden Datenbank und unterstützen die Spiellogik in **PhoenixDX**.

## Abhängigkeiten
- **PhoenixModel**: Bietet allgemeine Modelle und Mechaniken für die Spiellogik.
- **PhoenixDX**: Die Hauptlösung, in die dieses Modul eingebunden ist.

Falls Änderungen an diesen Dateien vorgenommen werden, sollte überprüft werden, ob bestehende Abhängigkeiten innerhalb der Lösung **PhoenixDX** betroffen sind.