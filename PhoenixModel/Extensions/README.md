# Erweiterungen (Extensions)

Um die Datenbankklassen möglichst kompakt und übersichtlich zu halten, werden funktionale Erweiterungen (Extensions) verwendet.  
Diese enthalten zusätzliche Methoden und Funktionalitäten, die wiederverwendbar sind und den Code sauberer sowie wartbarer machen.


## 📌 Namenskonvention
- **Namen der Datenbank-Klasse + 'Extensions'
- **Wiederverwendbarkeit**: Methoden können an mehreren Stellen genutzt werden, ohne Redundanzen zu erzeugen.
- **Verbesserte Lesbarkeit und Wartbarkeit**: Logik wird gekapselt und ist leichter verständlich.

## 📌 Zweck der Erweiterungen
- **Kürzere und klarere Datenbankklassen**: Durch die Auslagerung von Funktionen bleiben die Kernklassen schlank.
- **Wiederverwendbarkeit**: Methoden können an mehreren Stellen genutzt werden, ohne Redundanzen zu erzeugen.
- **Verbesserte Lesbarkeit und Wartbarkeit**: Logik wird gekapselt und ist leichter verständlich.

## 🔧 Enthaltene Funktionen
Diese Erweiterungen können beispielsweise beinhalten:
- Methoden zur **Datenbankabfrage** und -verarbeitung
- **Hilfsfunktionen** zur Formatierung oder Konvertierung von Daten
- **Erweiterungsmethoden für bestehende C#-Typen**

## 📂 Struktur der Erweiterungen
Die Dateien innerhalb dieses Ordners sind nach ihrem jeweiligen Einsatzzweck organisiert.

💡 **Tipp:** Falls eine Klasse häufig wiederverwendet wird und generelle Funktionen benötigt, könnte sie eine eigene Erweiterungsmethode in diesem Ordner erhalten.

---
🚀 **Ziel:** Eine modulare und flexible Codebasis, die eine effiziente Entwicklung und Wartung ermöglicht.
