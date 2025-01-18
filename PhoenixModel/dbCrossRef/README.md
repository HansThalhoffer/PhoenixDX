# PhoenixModel - dbCrossRef

Dieses Verzeichnis gehört zum Projekt **PhoenixModel** in der Lösung **PhoenixDX** und enthält Referenztabellen für verschiedene Spielelemente. Diese Dateien sind Teil der Datenbankstruktur und helfen bei der Zuordnung und Verwaltung von Einheiten, Bauwerken, Geländearten, Teleportpunkten und anderen Spielmechaniken.

## Inhalt des Verzeichnisses

### 1. Bauwerke
- **Bauwerke.cs**: Enthält Informationen zu verschiedenen Bauwerken im Spiel.

### 2. Einheiten und Bewegungen (BEW)
Diese Dateien enthalten Referenztabellen für verschiedene Einheitentypen und Bewegungsmuster:
- **BEW.cs**: Allgemeine Bewegungsreferenzen.
- **BEW_chars.cs**: Bewegungsdaten für Charaktere.
- **BEW_Kreaturen.cs**: Bewegungsdaten für Kreaturen.
- **BEW_Krieger.cs**: Bewegungsdaten für Krieger.
- **BEW_LKP.cs**: Bewegungsdaten für leichte Katapulte.
- **BEW_LKS.cs**: Bewegungsdaten für leichte Kriegsschiffe.
- **BEW_PiratenChars.cs**: Bewegungsdaten für Piraten-Charaktere.
- **BEW_PiratenLKS.cs**: Bewegungsdaten für Piraten-Leichte-Kriegsschiffe.
- **BEW_PiratenSchiffe.cs**: Bewegungsdaten für Piratenschiffe.
- **BEW_PiratenSKS.cs**: Bewegungsdaten für Piraten-Schwere-Kriegsschiffe.
- **BEW_Reiter.cs**: Bewegungsdaten für Reitereinheiten.
- **BEW_Schiffe.cs**: Bewegungsdaten für Schiffe.
- **BEW_SKP.cs**: Bewegungsdaten für schwere Katapulte.
- **BEW_SKS.cs**: Bewegungsdaten für schwere Kriegsschiffe.

### 3. Magie und Teleportation
- **Crossref_zauberer_teleport.cs**: Referenztabelle für Teleportzauberer und ihre Fähigkeiten.
- **Teleportpunkte.cs**: Enthält Daten zu verfügbaren Teleportpunkten im Spiel.

### 4. Gelände und Kosten
- **Gelaendetypen_crossref.cs**: Referenztabelle für verschiedene Geländetypen.
- **Kosten.cs**: Enthält Kostenübersichten für verschiedene Aktionen und Einheiten.

### 5. Einheiten und Strukturen
- **Units_crossref.cs**: Referenztabelle für verschiedene Einheiten im Spiel.
- **Wall_crossref.cs**: Referenztabelle für Wände und Befestigungen.

## Zweck dieses Verzeichnisses
Das **dbCrossRef**-Verzeichnis enthält zentrale Referenztabellen für das Spielsystem. Diese Tabellen ermöglichen eine effiziente Verwaltung von Spieleinheiten, deren Bewegungsmuster, Bauwerken, Geländearten und magischen Fähigkeiten. Änderungen an diesen Dateien können sich direkt auf das Spielgeschehen auswirken.

## Hinweis
Beim Bearbeiten dieser Dateien ist darauf zu achten, dass die Datenbankreferenzen konsistent bleiben, um Fehler in der Anwendung zu vermeiden.
