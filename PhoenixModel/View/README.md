# PhoenixModel - View

Dieses Verzeichnis gehört zum **PhoenixModel**-Projekt innerhalb der **PhoenixDX**-Lösung und enthält verschiedene View-Klassen zur Darstellung und Verwaltung von Spielelementen.

## Übersicht der Dateien

### BauwerkeView.cs
Diese Klasse verwaltet die Darstellung von Bauwerken im Spiel. Sie enthält Methoden zur Visualisierung und zur Verwaltung von Bauwerken in der Spielwelt.

### CharacterView.cs
Diese Klasse verwaltet die Darstellung und Steuerung von Charaktern innerhalb des Spiels. 

### EinnahmenView.cs
Diese Klasse ist für die Berechnung und Darstellung von Einnahmen der Spieler verantwortlich. Sie aggregiert relevante Daten und stellt sie für die GUI oder andere Module bereit.

### KleinfeldView.cs
Verwaltet die Darstellung einzelner Kleinfelder in der Spielwelt. Diese Klasse wird zur Anzeige und Interaktion mit den kleinsten Spielfeldeinheiten verwendet.

### LehenView.cs
Unterstützt bei der Verwaltung und der Anlage von Lehen. Liefert eine Liste von Charakteren, die noch kein Lehen haben.

### NationenView.cs
Bietet eine Übersicht über die verschiedenen Nationen im Spiel und stellt deren Eigenschaften für die Anzeige bereit.

### SpielfigurenView.cs
Enthält die Logik zur Darstellung und Verwaltung von Spielfiguren. Dies umfasst das Laden, Aktualisieren und Anzeigen von Charakteren und Truppen.

### ZaubererView.cs
Diese Klasse verwaltet die Darstellung und Steuerung von Zauberern innerhalb des Spiels. 

## Zweck dieses Verzeichnisses
Das **View**-Verzeichnis enthält Klassen, die für die Darstellung von Spielfeldelementen, Einheiten und Spielmechaniken zuständig sind. Diese Views werden verwendet, um Daten aus den Modellen im Spiel sichtbar und interaktiv zu machen.

---

Falls Änderungen oder Erweiterungen an der Darstellung vorgenommen werden müssen, sollten diese innerhalb der entsprechenden **View-Klassen** erfolgen.