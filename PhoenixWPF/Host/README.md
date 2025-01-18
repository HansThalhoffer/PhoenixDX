# Verzeichnis: PhoenixWPF/Host

Dieses Verzeichnis ist Teil des Projekts **PhoenixWPF** innerhalb der Lösung **PhoenixDX**. Es enthält Klassen zur Interaktion mit nativen Windows-APIs und zur Verwaltung von Fensterereignissen.

## Enthaltene Dateien

### `HwndMouseEventArgs.cs`
Diese Datei enthält die Definition der Ereignisargumente für Mausereignisse innerhalb des MonoGame Windows-Fensters (HWND-basiert).

### `HwndWrapper.cs`
Bietet eine Wrapper-Klasse für das MonGame Window basiert auf (HWND), die das Verwalten und Abfangen von Windows-Nachrichten erleichtert.

### `NativeMethods.cs`
Diese Datei enthält P/Invoke-Definitionen für den Zugriff auf native Windows-APIs. Sie dient zur Interaktion mit dem Betriebssystem auf niedriger Ebene.

## Zweck des Verzeichnisses
Das **Host**-Verzeichnis stellt Funktionen zur Verfügung, die für die Verwaltung von Fenstern und systemnahen Eingaben innerhalb von **PhoenixWPF** benötigt werden. Es ermöglicht die Kommunikation zwischen der WPF-Anwendung und dem Windows-Fenstersystem.

## Hinweise
- Änderungen an `NativeMethods.cs` sollten mit Vorsicht vorgenommen werden, da sie direkten Einfluss auf die Windows-API-Aufrufe haben.
- `HwndWrapper.cs` kann verwendet werden, um Nachrichten von Windows-Fenstern abzufangen und zu verarbeiten.
- `HwndMouseEventArgs.cs` wird für die Verwaltung von Mausereignissen genutzt, insbesondere für benutzerdefinierte Fensterelemente.

Dieses Verzeichnis ist ein wichtiger Bestandteil von **PhoenixWPF**, um eine tiefere Integration mit dem Windows-Betriebssystem zu ermöglichen.
