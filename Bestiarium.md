# Das Bestiarium und das Wiki

Stand: September 2026

Die Frage war, ob sich aus dem Weltwiki <https://www.erkenfara.com/> Elemente für das
Bestiarium der Anwendung übernehmen lassen. Dieses Papier hält fest, was dabei herauskam.
Geändert wurde nichts.

Kurzfassung: **beides heisst Bestiarium, meint aber nicht dasselbe.** Übernehmen lässt sich
Text und Bild, nicht ein einziger Zahlenwert — und für neue Einträge müsste die Spielleitung
Spielwerte erfinden. Deshalb steht am Ende der Vorschlag, zu verlinken statt zu kopieren.

## 1. Was unser Bestiarium ist

Die Tabelle `bestiarium` steht in der Kartendatenbank und gilt für alle Reiche. Sie wird seit
dem Zug, in dem sie geladen wurde, im Menü *Nachschlagewerk* angezeigt
(`NachschlagewerkView`, `Bestiarium`).

Sie hat acht Einträge, und das sind **keine Tiere, sondern Sonderfiguren des Strategiespiels**:

* Armee der Templer
* Bote des Feuers
* Bote des Todes
* Bote des Windes
* Imnuteph der Seelenverkäufer
* Kartograph
* Skelettdrache
* Werwolf

Je Eintrag führt die Tabelle:

| Feld | Inhalt |
|---|---|
| `Kreaturenname` | der Name, zugleich der Schlüssel |
| `Waffengattung` | Heer, Kriegerheer oder Zauberer |
| `GP`, `HF`, `Stärke`, `BP` | die Spielwerte: Gutpunkte, Heerführer, Stärke, Baupunkte |
| `Beschreibung1` bis `Beschreibung4` | vier kurze Zeilen, meist leer |
| `IMG` | der Name einer Bilddatei |

Die Beschreibungen sind Einzeiler zwischen 20 und 67 Zeichen; von 32 möglichen Feldern sind
weniger als die Hälfte gefüllt. **Hier wäre also Platz** — das ist der Grund, warum die Frage
überhaupt aufkam.

## 2. Was das Wiki ist

Ein gewachsenes MediaWiki mit 89 Kategorien und mehreren tausend Seiten, das die Welt
beschreibt: Reiche, Charaktere, Erzählungen, Religion, Landeskunde. Einschlägig wären

| Kategorie | Seiten |
|---|---|
| Flora & Fauna | 55 |
| NSC | 48 |
| Artefakt | 25 |
| Elementar | 23 |
| Mythische Wesen | 13 |

Darin steht ordentlich Stoff: Drache, Greif, Eisenwolf, Frostspinner, Remorhaz, Martigora,
Riesenkrake, Waldschleicher, dazu die nordische Reihe um Fenris, Jørmungandr, Nidhøgg und
Sleipnir. Übersichtsseiten wie *Tierwelt Erkenfaras* oder *Tiere und andere Kreaturen in
Nordheim* fassen zusammen.

## 3. Der Befund

Zwei Dinge entscheiden die Sache.

### Die Namen überschneiden sich fast nicht

Von den acht Einträgen der Tabelle hat genau **einer** eine Wiki-Seite: *Imnuteph*, und zwar
als Charakter aus dem Rollenspiel um Yaromo. Skelettdrache, Werwolf, Kartograph, Armee der
Templer und die drei Boten kommen im Wiki nicht vor; die Suche findet zu ihnen nichts.

Umgekehrt steht keine der 55 Kreaturen aus *Flora & Fauna* in unserer Tabelle.

Es gibt also keinen Bestand, den man abgleichen könnte. Es gibt zwei getrennte Listen.

### Die Werte passen nicht

Wo das Wiki Kreaturen beziffert, tut es das in **Midgard-5-Rollenspielwerten**. Auf der
Drachenseite stehen Lebenspunkte, Ausdauerpunkte, Stärke, Gewandtheit, Bewegungsweite und
Würfelschäden — Angaben der Art `45 LP, 60 AP, St 140, Gw 80, B 32, Biss +13 (3W6+3)`.

Unser Bestiarium braucht Gutpunkte, Heerführer, Stärke, Baupunkte und eine Waffengattung.
Zwischen beiden Reihen gibt es keine Umrechnung: Midgard misst einen einzelnen Kämpfer in
einer Rollenspielrunde, das Strategiespiel misst ein Heer auf einer Gemark. **Das sind zwei
Spiele, die sich eine Welt teilen.**

## 4. Was sich also übernehmen liesse

**Text und Bild — für Einträge, die es schon gibt.** Das betrifft genau einen: Imnuteph. Für
die übrigen sieben gibt es keine Quelle.

**Neue Einträge aus dem Wiki.** Ein Greif, ein Eisenwolf, ein Frostspinner — die Beschreibung
stünde bereit, die Spielwerte nicht. Die müsste jemand festlegen: wieviele Gutpunkte hat ein
Greif, wieviele Heerführer ersetzt er, welche Stärke bringt er ins Gefecht, was kostet er an
Baupunkten. Das ist **eine Entscheidung über Spielbalance und gehört der Spielleitung**, nicht
dem Programm. Aus dem Wiki ableiten lässt sie sich nicht.

**Bilder.** Das Feld `IMG` nennt schon heute Bilddateien. Bilder aus dem Wiki liessen sich
verwenden, das ist aber eine Frage an die Urheber und keine technische.

## 5. Der Vorschlag: verlinken statt kopieren

Kopierter Text kostet dauerhaft Pflege und veraltet ab dem Tag der Kopie. Für acht Einzeiler
lohnt das nicht.

Ein Verweis lohnt sich sofort:

* ein Feld je Eintrag mit dem Wiki-Titel — oder, wo der Name übereinstimmt, ganz ohne Feld
  über den Namen selbst
* im Nachschlagewerk ein Knopf, der die Seite im Browser öffnet
* die Beschreibung bleibt dort, wo sie gepflegt wird, und ist immer aktuell

Aufwand: eine Spalte und ein Knopf. Nutzen: die ganze Landeskunde hängt am Eintrag, nicht
67 Zeichen davon.

Sinnvoll wird das erst, wenn die Tabelle mehr als acht Einträge hat — siehe Punkt 4. Die
Reihenfolge wäre also: erst entscheidet die Spielleitung, welche Kreaturen ins Spiel gehören
und was sie wert sind, dann trägt jemand sie ein, dann lohnt der Verweis.

## 6. Was offen bleibt

1. **Sollen neue Kreaturen ins Spiel?** Wenn ja, welche, und mit welchen Werten? Das ist die
   einzige Frage, an der alles Weitere hängt.
2. **Woran erkennt eine Figur auf der Karte ihren Eintrag?** Eine Kreaturenfigur führt keinen
   Namen, über den sich nachschlagen liesse. Das steht schon als Lücke in
   [Landkarte.md](Landkarte.md) und wäre für ein grösseres Bestiarium zu lösen — sonst ist die
   Tabelle ein Lexikon, das nie jemand aufschlägt.
3. **Dürfen Bilder aus dem Wiki verwendet werden?** Frage an die Urheber.

## 7. Nebenbefunde aus derselben Sichtung

Gehört nicht zum Bestiarium, fiel aber beim Durchsehen des Wikis an und ist sonst nirgends
festgehalten:

* **Das Wiki hat eine Kategorie *Regeln* mit 45 Seiten** — Burg, Stadt, Festung, Hauptstadt,
  Rüstort, Raumpunkte, Heerführer, Herrscher, Burgherr, Festungsherr, Stadtherr,
  Einnahmemonat, Rüstmonat, Position, Gemark, Provinz, Putsch. Also genau unsere Begriffe.
  Zwei davon habe ich geprüft: *Festung* stimmt mit dem Regelwerk überein, *Festungsherr*
  nennt keine Gutpunkte — die offene Frage dazu bleibt offen. Die übrigen 43 wären eine
  Quelle für die anderen Fragen in [Offene-Regelfragen.md](Offene-Regelfragen.md).
* **Das Amt heisst im Wiki *Stadtherr*.** Das Regelwerk benutzt beide Formen, Stadthalter
  neunmal und Stadtherr viermal. Der Beförderungsbefehl versteht Stadthalter, Statthalter und
  STH — Stadtherr nicht. Eine Zeile in `BeförderungCommandParser`.
* **Die Stadt hat 40.000 Einwohner, in `EinwohnerUndEinnahmenTabelle` stehen 30.000.**
  Regelwerk 1.5.6 und Wiki sagen übereinstimmend 40.000. Der Wert ist zurzeit folgenlos —
  gelesen werden aus dieser Tabelle nur die Einnahmen —, aber falsch.
