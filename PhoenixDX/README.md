# PhoenixDx

## Überblick
**PhoenixDx** ist eine zentrale Komponente der **PhoenixDX**-Lösung. Sie enthält die Darstellung der Karte auf DirectX.
Die Implementierung basiert auf **MonoGame** und wird ausschließlich für das Zeichnen der Spielkarte und deren Inhalte verwendet.

## Anforderungen
- **.NET** (empfohlene Version: .NET 6 oder höher)
- **MonoGame Framework** (erforderlich für Rendering und Spielmechaniken)
- **Visual Studio 2022** oder höher (empfohlen)


## Wichtige Dateien
- `MappaMundi.cs`: Die einzige Schnittstelle zur Außenwelt


## Projektstruktur
Die Projektstruktur ist wie folgt organisiert:

```
PhoenixDx/
 |-- Content/
 |-- Drawing/
 |-- Helper/
 |-- Program/
 |-- Structures/
```

### Ordner und ihre Bedeutung

**Unterordner:**
*   **Content**: Ressourcen des Spiels wie Schriftarten, Bilder und andere Medien.
    *   **Fonts**: Enthält Schriftdateien (`.spritefont`), die im Spiel verwendet werden.
    *   **Images**: Enthält Bildressourcen, darunter Unterordner für **Reichsfarben**, **Symbol**, **TilesetN** und **TilesetV**.
*   **Drawing**: Verantwortlich für das Rendern von grafischen Komponenten. Dateien wie `TextureCache.cs` und `WeltDrawer.cs` verwalten die grafische Darstellung.
*   **Helper**: Hilfsklassen wie `FontManager.cs` und `VectorExtensions.cs`, die grafische und mathematische Operationen vereinfachen.
*   **Program**: Einstiegspunkt für die PhoenixDX-Anwendung (`SpielDX.cs`).
*   **Structures**: Definiert Datenstrukturen wie `Bruecke.cs`, `Figur.cs` und `Terrain.cs`, die die optischen Komponenten der Anwendung repräsentieren.

**Wichtige Dateien:**
*   `MappaMundi.cs`: API als Schnittstelle zu WPF 
*   `Program/SpielDX.cs`: DirectX Hauptklasse
*   `Structures/Welt.cs`: Repräsentation der Karte
*   `PhoenixDX.csproj`: Projektdatei für das Hauptprojekt PhoenixDX.


