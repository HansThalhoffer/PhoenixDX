# PhoenixWPF - Converter

Dieses Verzeichnis gehört zum Projekt **PhoenixWPF** in der Lösung **PhoenixDX** und enthält verschiedene **Wertkonverter** für die Benutzeroberfläche. Diese Konverter werden hauptsächlich zur Umwandlung von Werten für die Darstellung in WPF-Controls verwendet.

## Enthaltene Dateien

### 1. AutorizedConverter.cs
Konvertiert Berechtigungen oder Statuswerte in boolesche Werte oder andere Formate zur Anzeige.

### 2. CellBackgroundConverter.cs
Ermöglicht die Anpassung der Hintergrundfarbe einer Tabellenzelle basierend auf bestimmten Bedingungen.

### 3. EditableToBackgroundConverter.cs
Ändert die Hintergrundfarbe eines Elements, je nachdem, ob es bearbeitbar ist oder nicht.

### 4. IntToBoolConverter.cs
Konvertiert Ganzzahlen (`int`) in boolesche Werte (`true` oder `false`), um beispielsweise Sichtbarkeiten oder Aktivierungen zu steuern.

### 5. LogTypeToColorConverter.cs
Weist verschiedenen Log-Typen spezifische Farben zu, um eine visuelle Unterscheidung in der Benutzeroberfläche zu ermöglichen.

## Verwendung
Diese Konverter werden in **WPF-XAML-Bindings** genutzt, um Daten entsprechend ihrer Logik in eine visuelle Darstellung zu überführen. Sie helfen, die Trennung zwischen **Datenlogik** und **Benutzeroberfläche** zu wahren.

## Speicherort
```
E:\PhoenixDX\PhoenixWPF\Pages\Converter
```

## Änderungen & Erweiterungen
Neue Konverter können hinzugefügt oder bestehende angepasst werden, um die Funktionalität der WPF-Oberfläche weiter zu verbessern. Änderungen sollten gut dokumentiert und getestet werden.