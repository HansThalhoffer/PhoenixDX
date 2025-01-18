# PhoenixModel - ExternalTables

Dieses Verzeichnis gehört zum Projekt **PhoenixModel** in der Lösung **PhoenixDX** und enthält externe Lookup-Tabellen, die für verschiedene Spielmechaniken verwendet werden.
Diese wurden in der Altanwendung im Source per IF-ELSE verwendet. In einer späteren Version dieser Anwendung mit einer neuen Datenbank, sollen diese Tabellen als solche abgelegt werden.

## Inhalt

### 1. EinwohnerUndEinnahmenTabelle.cs
Diese Datei definiert eine Tabelle zur Verwaltung der Einwohnerzahl und deren Einfluss auf die Einnahmen eines Reiches.

### 2. FigurType.cs
Diese Datei enthält eine Enumeration für die verschiedenen Typen von Spielfiguren, die im Spiel existieren.

### 3. GeländeTabelle.cs
Enthält eine Tabelle zur Beschreibung der Geländearten und deren Auswirkungen auf Bewegung, Bau und andere Spielmechaniken.

### 4. ReichTabelle.cs
Speichert Informationen zu den Reichen im Spiel, einschließlich relevanter Eigenschaften und Beziehungen.

## Zweck der ExternalTables
Diese Dateien dienen als Nachschlagetabellen und stellen sicher, dass das Spiel konsistente und zentrale Definitionswerte für verschiedene Spielfunktionen verwendet. Änderungen an diesen Tabellen haben direkte Auswirkungen auf das Verhalten und die Berechnungen im Spiel.

## Weitere Informationen
Für Details zur Nutzung dieser Tabellen in der Anwendung siehe die zugehörige Dokumentation oder den Code in den relevanten Modulen von **PhoenixModel**.