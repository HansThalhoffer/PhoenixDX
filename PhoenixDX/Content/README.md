# Content

## Überblick
Der **Content**-Ordner enthält alle Ressourcen des Spiels, darunter Schriftarten, Bilder und andere Medien. Diese Assets werden in verschiedene Unterordner unterteilt, um eine bessere Organisation zu gewährleisten.

## Struktur

```
Content/
 |-- Fonts/
 |-- Images/
```

### **Fonts/**
In diesem Ordner befinden sich alle **Schriftdateien (`.spritefont`)**, die im Spiel verwendet werden. Diese Schriftarten werden für verschiedene UI-Elemente und Texte im Spiel eingesetzt.

### **Images/**
Der **Images**-Ordner enthält alle **Bildressourcen** des Spiels. Diese sind weiter in spezifische Unterordner aufgeteilt:

- **Reichsfarben/** – Enthält Bilder, die unterschiedliche Reichsfarben repräsentieren.
- **Symbol/** – Beinhaltet Symbole und Icons, die im Spiel verwendet werden.
- **TilesetN/** – Eine Sammlung von Tileset-Grafiken für verschiedene Spielumgebungen.
- **TilesetV/** – Alternative Tileset-Grafiken zur weiteren Variation.

## Verwendung
Alle Ressourcen in diesem Ordner werden im Spiel automatisch geladen und in verschiedenen Komponenten verwendet. Die Schriftarten und Bilder werden hauptsächlich für die Benutzeroberfläche und das Rendering von Spielobjekten eingesetzt.

## Pflege und Erweiterung
- Neue Schriftarten sollten im **Fonts/**-Ordner hinzugefügt und im Code referenziert werden.
- Bilder müssen in die entsprechenden Unterordner eingefügt werden, um eine klare Struktur beizubehalten.
- Vermeide unnötige oder doppelte Ressourcen, um die Ladezeiten zu optimieren.

---
Falls neue Kategorien benötigt werden, sollte die Projektstruktur entsprechend erweitert und dokumentiert werden.

