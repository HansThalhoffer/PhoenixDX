# PhoenixWPF - Generatoren

Dieses Verzeichnis ist Teil des **PhoenixWPF**-Projekts in der **PhoenixDX**-Lösung. Es enthält Klassen zur automatischen Erstellung und Generierung von Datenmodellen und Testdaten.

## Enthaltene Dateien

### ModelGenerator.cs
- Verantwortlich für die automatische Generierung von Modellen.
- Nutzt die Datenbank zur Erstellung von strukturierten Datenmodellen.
- Wird verwendet, um sicherzustellen, dass Datenmodelle konsistent und effizient erstellt werden.

### TestDataGenerator.cs
- Erstellt Testdaten für verschiedene Module des PhoenixWPF-Projekts.
- Dient zur Validierung und Überprüfung von Geschäftslogik und Datenintegrität.
- Erlaubt das einfache Generieren großer Datenmengen zur Testzwecken.

## Zweck des Verzeichnisses
Die hier enthaltenen Generatoren unterstützen die Entwicklung durch Automatisierung der Modellerstellung und Generierung von Testdaten. Dies reduziert manuellen Aufwand und verbessert die Konsistenz innerhalb der Datenverarbeitung.

## Nutzung
- **ModelGenerator**: Wird typischerweise bei der Entwicklung neuer Features oder bei der Migration von Datenmodellen verwendet.
- **TestDataGenerator**: Wird zur Durchführung von Unit-Tests oder Integrationstests mit simulierten Daten eingesetzt.

## Weiterführende Informationen
Für Details zur Implementierung und Nutzung der Generatoren siehe die jeweiligen Quellcodedateien oder die Projektdokumentation.
