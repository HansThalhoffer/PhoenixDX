# Landkarte: Regelwerk gegen Anwendung

Ein Abgleich Kapitel für Kapitel: was die Anwendung kann, was halb da ist, was fehlt und was
nur deshalb nicht läuft, weil die Daten es nicht hergeben.

**Wie diese Liste entstanden ist:** das Inhaltsverzeichnis des Regelwerks durchgegangen und zu
jedem Punkt im Code nachgesehen — Regelklassen, Befehle, Sichten, Dialoge und die Testdateien.
Gelesen wurden Klassen, Methoden und ihre Kommentare, **nicht jede Zeile**. Wo unten "steht"
sagt, heisst das: es gibt eine Umsetzung und Tests dazu, nicht dass sie in jedem Sonderfall
richtig ist. Gegen die Altanwendung PZE.NET ist hier nichts verglichen worden.

**Legende**

| | |
|---|---|
| **steht** | umgesetzt, mit Tests |
| **halb** | teilweise da, oder nicht erreichbar, oder nicht angewendet |
| **fehlt** | nicht umgesetzt |
| **blockiert** | umsetzbar, aber es fehlen Daten oder eine Antwort |
| **ausserhalb** | gehört nicht in diese Anwendung |

---

## 0 Die Welt — Gelände

| Punkt | Stand | Bemerkung |
|---|---|---|
| Geländearten, Höhenstufen, Flüsse | **steht** | `GeländeTabelle`, `TerrainType`, Karte zeichnet sie |
| Küstengewässer | **steht** | erkannt über `IsWasser` / `IsKüste` |

---

## 1 Rüstgüter und Charaktere

| Punkt | Stand | Bemerkung |
|---|---|---|
| 1.1 Krieger, Pferd, Reiter, Schiff, Heerführer | **steht** | Datenmodell, Rüsten, Bewegung, Kampf |
| 1.2 Fernkampfwaffen (LKP, SKP, LKS, SKS) | **steht** | `FernkampfRules`, Beschussbefehl, Auswertung |
| 1.2 Schadenstabelle (Tabelle 8) | **blockiert** | liegt im Datenbestand nicht vor; die Trefferpunkte werden eingetragen |
| 1.3 Zauberer, Klassen, Zauberkraft | **steht** | `ZaubereiRules`, `ZaubererView` |
| 1.3 Abrüsten von Nichtcharakterzauberern | **fehlt** | |
| 1.4 Teleportation mit Rüstgütern | **steht** | `TeleportRules`, `TeleportCommand` |
| 1.4 Magische Wand errichten und einreissen | **steht** | `CastSpellCommando` |
| 1.4 Bannen von Rüstgütern | **steht** | Kosten und Wirkung gerechnet |
| 1.5 Strasse, Kaianlage, Brücke, Wall | **steht** | `ConstructRules`, `ConstructCommand` |
| 1.5 Rüstorte bauen und ausbauen | **steht** | `RuestortRules`, `UpgradeCommand` |
| 1.5.10–1.5.12 Reparieren | **halb** | Rüstorte und Bauwerke ja; beschädigte **Rüstgüter** (1.5.11) nur als Zerstörungschance gerechnet, nicht als Reparatur |
| 1.5.13 Hauptstadtverlegung | **fehlt** | 50.000 GS, mehrmonatig — kommt im Code nicht vor |
| 1.6 Bauwerke durch eigene Heere zerstören | **fehlt** | |
| 1.6 Eigene Heere auflösen | **halb** | `ZugendeRules.IstAufgelöst` entfernt Heere ohne Heerführer; einen Auflösungsbefehl gibt es nicht |
| 1.7 Belagerung | **steht** | `BelagerungsRules`; mindert Rüstkapazität und Großbaustelle |
| 1.8 Heere: Mindestgrösse, Nummernkreise, Teilen, Fusionieren | **steht** | `HeeresRules`, `SplitCommand`, `MergeCommand` |
| 1.8 Höchstgrenze 100.000 Raumpunkte | **steht** | `ÜberbesetzungRules` (Regelwerk 5.7) |
| 1.8 "eroberungsfähiges Heer" (1000 RP + HF) | **steht** | `HeeresRules.IstEroberungsfähig`; benutzt beim Erobern, Stören und Unterstützen |
| 1.9 Charaktere, Ämter, Gutpunkte | **steht** | `CharacterView`, Klassenstufen in `CharakterkampfRules` |
| 1.9 Beförderung und Degradierung | **fehlt** | offenes Issue |
| 1.9 Charakterzauberer, Herrscherzauberer | **halb** | erkannt und im Kampf behandelt; eigene Regeln (Notteleportation) fehlen |

---

## 2 Spielbeginn

| Punkt | Stand | Bemerkung |
|---|---|---|
| Aufstellung zu Spielbeginn | **ausserhalb** | einmalige Arbeit der Spielleitung |
| Testdaten erzeugen | **steht** | Menü *Extras*, Zug 999 |

---

## 3 Spielablauf

| Punkt | Stand | Bemerkung |
|---|---|---|
| 3.1 Zugreihenfolge, Auftauchpunkt | **steht** | eigener Dialog, liest die Kartendatenbank |
| 3.2.1 Gewöhnliche Einnahmen (Einwohner, Gelände, Bauwerke) | **steht** | `EinnahmenView`, Bericht des Kämmerers |
| 3.2.2 Sonstige Einnahmen (Verkauf von Rüstgütern, Gefangenen, Gelände) | **fehlt** | kein Verkauf im Code |
| 3.2.3 Plündereinnahmen | **halb** | der Befehl prüft jetzt die Eroberungsfähigkeit; die doppelte Einnahme und die acht Monate Sperre fehlen |
| 3.2 Besondere Einnahmen | **steht** | Kampfeinnahmen, Transport in den Rüstort, Schenkung |
| 3.3 Rüsten, Rüstkapazität, Rüstmonat | **steht** | `RuestRules` |
| 3.3 Rüsten ausserhalb des Rüstmonats | **steht** | aus besonderen Einnahmen |
| 3.3 Rüsten: Oberfläche | **halb** | nur über die Befehlseingabe; vertagtes Issue |
| 3.4 Zugphasen, Zugabgabe, Zugwechsel, Zug holen | **steht** | `ZugView`, `Zugabgabe`, Archiv und Serverablage |

---

## 4 Bewegung

| Punkt | Stand | Bemerkung |
|---|---|---|
| Bewegungspunkte, Geländekosten, Höhenstufen | **steht** | `BewegungsRules`, mit Erklärung der Unerreichbarkeit |
| 4.1 Küstengewässerregel | **steht** | über `DiplomatieRules` |
| 4.2 Wegerecht, Strassen | **steht** | seit der Berichtigung der Diplomatiesicht |
| 4.3 Transport und Ladung, Ein- und Ausschiffen | **steht** | `SchifffahrtsRules`, `EmbarkCommand` |
| 4.3 Gold und Kampfeinnahmen umladen | **steht** | `VerschiebeRules` |
| Erobern und Plündern beim Betreten | **steht** | nur ein eroberungsfähiges Heer nimmt Gelände; die Einnahme daraus fehlt noch |

---

## 5 Der Kampf

Vollständig gerechnet, in der Reihenfolge des Regelwerks. Gewürfelt wird nicht — die Würfe der
Spielleitung werden eingetragen.

| Punkt | Stand | Bemerkung |
|---|---|---|
| 5.1 Fernkampf: Befehl, Reichweite, Auswertung | **steht** | |
| 5.2 Ritterkampf | **blockiert** | gerechnet, aber die Daten kennzeichnen keinen Ritter |
| 5.3 Charakterkampf, Zauberduell | **steht** | Würfelrunde, Paarungen, Gutpunkte, Folgen |
| 5.4 Rückzugsgefecht | **steht** | Zulassung, freier Rückzug, Angriffssperre |
| 5.4 Notteleportation der Charakterzauberer | **fehlt** | |
| 5.5 Nahkampf | **steht** | Beispiel des Regelwerks geht durch |
| 5.5.1 Nachbarunterstützung | **steht** | `KampfRules.FindeUnterstützer` |
| 5.5.2 Standardgelände (+50) | **blockiert** | welches Gelände ein Reich hat, steht nicht im Datenbestand |
| 5.6 Kampfeinnahmen | **steht** | |
| 5.7 Überbesetzung | **steht** | |
| 5.8 Beute (Rüstort, See, Land, Katapulte) | **steht** | |
| 5.8.4 Freilassung gefangener Rüstgüter | **fehlt** | Gefangene gibt es im Datenmodell nicht |
| Ablauf über eine Gemark, Bericht, Übernehmen | **steht** | `KampfablaufRules`, Fenster der Spielleitung |
| Charakter- und Ritterkampf im Ablauf | **halb** | gehen als Gutpunkte von Hand ein, laufen nicht mit |

---

## 6 Sonstige Regeln

| Punkt | Stand | Bemerkung |
|---|---|---|
| 6.1 Handel und Schenkung | **steht** | Befehl, Seite, Prüfung im fremden Rüstort |
| 6.2 Putschistenregel | **fehlt** | kommt im Code nicht vor |
| 6.3 Kreaturen | **halb** | Datenklasse und Bewegung ja; das Bestiarium wird **nicht geladen** |
| 6.3 Artefakte | **fehlt** | |
| 6.4 Audvacar, Handelskontor | **halb** | Audvacargeld ist in den Einstellungen und im Rüstort berücksichtigt; Handel dort fehlt |
| 6.5 Invasoren | **halb** | nur ein Kennzeichen am Reich |
| 6.6 Piraten, Piratennest, Reisen, Teleportverluste | **steht** | Teleportfelder, Auftauchpunkt, Reichsalias |
| 6.9 Einnahmebonus, Untote, Götterschutz | **fehlt** | |
| 6.9 Garde | **halb** | Gardevorteil und Gardedrittel im Kampf; die Rüstbedingung (nur in der Hauptstadt oder beim Herrscher) fehlt |
| 6.10 Abwesende Reiche, Ausscheiden | **fehlt** | Sache der Spielleitung, bisher nicht abgebildet |
| 6.11 Events | **fehlt** | |
| Lehen | **halb** | Anlegen und Verwalten gibt es als Seiten, das Menü dazu ist auskommentiert ("noch nicht vollständig umgesetzt") |
| Personal | **fehlt** | Datenklasse vorhanden, wird **nicht geladen** |

---

## 7 Rollenspiel · 8 Wagenrennen · 9 Einrichtungen

**ausserhalb.** Questen sind laut Errata gestrichen; Wagenrennen und Rollenspiel werden am Tisch
gespielt, nicht in der Zugeingabe. Die Einrichtungen (Rat der Grauen, Spielleitung, Gilden) sind
organisatorisch.

---

## 10 Anhänge und Tabellen

| Tabelle | Stand |
|---|---|
| 1 Gelände, 2 Bauwerke, 3 Rüstgüter, 4 Gutpunkte, 5 BP im Gelände | **steht** — in der Crossreferenz |
| 7 Zaubersprüche | **steht** |
| 8 Fernkampf | **blockiert** — liegt nicht vor |
| 9 Wann kommt es zu was | **halb** — die Konflikterkennung deckt den Kampfteil ab |
| 10.2 Einzig gültige Sonderbefehle | **halb** — der Befehlsparser kennt einen Teil davon |

---

## Was die Anwendung kann, das nicht im Regelwerk steht

Werkzeuge, die es in der Altanwendung so nicht gab: Zugabgabe mit Archiv und Serverablage, Zug
von der Spielleitung holen, USB-Stick für die Installation, Datenbankvergleich zweier Monate,
Kartenbereinigung über `/cleanup`, Feindaufklärung auf der Karte, Historienberichte, die
Erklärung, warum ein Feld unerreichbar ist, und die Kampfauswertung der Spielleitung.

---

## Die grössten Lücken, nach Nutzen sortiert

1. ~~Belagerung (1.7).~~ **Erledigt** — `BelagerungsRules`, sie wird nicht befohlen, sondern aus
   der Lage der Heere gesucht. Offen blieb nur die Frage, ob eine Flotte belagern kann (Punkt 6
   der offenen Regelfragen).

2. ~~Eroberungsfähiges Heer (1.8).~~ **Erledigt** — `HeeresRules.IstEroberungsfähig`, benutzt beim
   Erobern und Plündern, beim Stören von Bau und Reparatur und bei der Nachbarunterstützung. Für
   das Zerstören von Bauwerken (1.6) fehlt weiterhin der Befehl selbst.

3. ~~Nachbarunterstützung (5.5.1).~~ **Erledigt** — `KampfRules.FindeUnterstützer`.

4. **Sonstige Einnahmen und Plündereinnahmen (3.2.2, 3.2.3).** Der Plünderbefehl wird vermerkt,
   die Einnahme daraus nicht gerechnet; Verkäufe gibt es gar nicht.

5. **Bestiarium und Personal.** Beide Tabellen sind im Datenmodell, beide werden nicht geladen —
   eine auskommentierte Zeile je Tabelle plus die Sichten darauf.

6. **Beförderungen (1.9), Hauptstadtverlegung (1.5.13), Heere auflösen (1.6).** Kleine,
   klar umrissene Befehle.

7. **Gefangene (5.5, 5.8.4).** Beim Überrennen werden Truppen gefangen genommen; das Datenmodell
   kennt keine Gefangenen. Das ist der grösste der kleinen Punkte, weil es eine neue Spalte
   braucht.
