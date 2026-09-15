# Offene Regelfragen

Sieben offene Punkte, bei denen die Quellen sich widersprechen, das Datenmodell etwas nicht kennt
oder die Daten sich selbst widersprechen. Beantwortete Fragen bleiben stehen, damit nachvollziehbar bleibt, warum die Anwendung
rechnet, wie sie rechnet.
Die Anwendung verhält sich jeweils so, dass nichts kaputtgeht, solange die Frage offen ist —
was das konkret heißt, steht unten bei "Was die Anwendung solange tut".

Hinweis: hier stehen bewusst keine Feldkoordinaten und keine Einheitennummern, weil das
Repository öffentlich ist.

---

## 1. ~~Haben Charaktere 21 oder 42 Bewegungspunkte?~~ — beantwortet

**Antwort der Spielleitung (September 2026):** **21, für alle Charaktere** — für den
Heerführercharakter wie für den Zauberer. Die 42 gilt nur zur See. Man geht davon aus, dass
jeder Charakter ein eigenes Schiff hat; ein Zauberer teleportiert bei Bedarf aufs Wasser und
nimmt erst dort die Schiffsbewegung auf.

Das Regelwerk trägt das inzwischen selbst, in der Korrekturliste: *"22. Charakterbewegung auf
Wasser (gefixt auf Treffen am 21.06.14) — Siehe dazu die Bewegungstabelle im Anhang. Unter 1.9:
und verfügen über ein eigenes Schiff (was nicht in die Heeresstärke eingerechnet wird), welches
Ihnen Bewegung auf Wasser ermöglicht. Und unter 0.3.2: (Charaktere verfügen über eigene Schiffe,
siehe 1.9)."*

**Umgesetzt** in `BewegungsRules`: die Regel steht als `BewegungspunkteCharakterNachRegelwerk`
= 21 im Code. Als Budget führt die Anwendung 42, weil die Bewegungstabelle die Landkosten
verdoppelt und damit dieselbe Reichweite meint — siehe Punkt 8, wo das ausgerechnet ist.

---

## 2. Welche Feindaufklärungsdatei gilt?

**Befund:** Im Datenbestand liegen zwei Dateien namens `Feindaufklaerung.dat`:

| Ort | Umfang |
|---|---|
| `_Data/Feindaufklaerung/` | wird von der Anwendung gelesen |
| `_Data/Zugdaten/Feindaufklaerung/` | wird nicht gelesen |

Sie unterscheiden sich deutlich — in der Zahl der Einträge, in den vertretenen Reichen und im
Inhalt. Auffällig: nur die **zweite** führt die eigenen Truppen an ihren tatsächlichen
Positionen, und nur sie kennt eines der Reiche überhaupt. So etwas entsteht nur aus dem
aktuellen Spielstand.

**Was die Anwendung solange tut:** Sie liest die erste Datei und schreibt beim Start in den
Infotab, **welche** Datei sie gelesen hat, wie viele Einträge darin standen, wie viele eigene
übersprungen wurden und wie viele Einheiten bekannt, aber nicht geortet sind. Eine veraltete
Datei fällt damit auf, statt sich als stille Lücke auf der Karte zu zeigen.

**Was sich mit der Antwort ändert:** Der Pfad in den Einstellungen — eine Zeile.

---

## 3. Gilt der Kampfvorteil der Kaianlage?

**Widerspruch:** Das Regelwerk gibt im Anhang 10.1 zwei Vorteile an:

* Verteidigung hinter einer Kaianlage, wenn kein Rüstort auf dem Feld steht: **+30 GP**
* Verteidigung, wenn der Angreifer direkt auf das Feld ausschifft: **+20 GP**

In der **Kampftabelle**, mit der die Spielleitung auswertet, gibt es diese Zeilen nicht. Deren
Vorteilsliste reicht von "hinter einer Brücke" bis "Nachbarunterstützung" und endet dort.

**Was die Anwendung solange tut:** Beide Vorteile gibt es als Kampfvorteil mit den Werten aus
dem Regelwerk, aber die Anwendung wendet sie **nicht von sich aus** an. Sonst würde sie anders
rechnen als die Auswertung der Spielleitung — und das wäre schlimmer als die Lücke.

**Was sich mit der Antwort ändert:** Gilt der Vorteil, wird er bei der Ermittlung der
Verteidigungsvorteile mit aufgenommen; die Bedingung "nur ohne Rüstort auf dem Feld" ist bereits
abgebildet.

---

## 4. Soll die Belagerung ins Datenmodell?

**Befund:** Das Regelwerk nimmt belagerte Rüstorte vom Rüsten aus (Kapitel 3.3 mit Verweis auf
1.7). Im Datenmodell gibt es aber überhaupt keinen Belagerungszustand — weder in der Karte noch
in den Zugdaten. Die Rüstortreferenz kennt zwar ein Kennzeichen dafür, *ob* ein Rüstort
belagerbar ist, aber nirgends steht, ob einer gerade belagert **wird**.

**Was die Anwendung solange tut:** Sie prüft beim Rüsten alles andere — eigener Rüstort,
Zugphase, Rüstkapazität, vorhandene Mittel — und lässt die Belagerung aus. Der Vorbehalt steht
als Kommentar in `RuestRules`, damit er nicht in Vergessenheit gerät.

**Was sich mit der Antwort ändert:** Braucht es die Belagerung, muss zuerst geklärt werden, wo
sie herkommt: aus den Zugdaten der Spielleitung, oder leitet die Anwendung sie selbst aus der
Lage der Heere ab? Erst danach lässt sich die Prüfung ergänzen.

---

## 5. Wieviel Schaden zerstört ein Katapult — die Hälfte oder alles?

**Widerspruch:** Das Regelwerk ist bei allen vier Fernkampfwaffen eindeutig: ein LKP hat 200
Baupunkte und *"gilt mit nur noch 100 Baupunkten als zerstört"*, ein SKP hat 400 und ist bei 200
hin, ebenso LKS (200/100) und SKS (400/200). Die Hälfte genügt also.

Die **Kampftabelle** rechnet die Baupunktverluste anders in Stück zurück: sie teilt durch den
*vollen* Wert (Zeile 103: Verlust mal 0,005, also ein Stück je 200 Baupunkte; Zeile 106: mal
0,0025, ein Stück je 400). Nach der Tabelle braucht es doppelt so viel Beschuss, um dieselbe Zahl
an Katapulten auszuschalten.

Dazu passt, dass die Tabelle ein schweres Kriegsschiff in der Stärkespalte mit 200 Baupunkten
führt (Zeile 104), es aber mit 400 zurückrechnet — innerhalb derselben Zeile.

**Was dafür spricht, dass die Hälfte stimmt:** Regelwerk 1.5.11 beschreibt genau dazu die
Zwischenstufe: eine angeschlagene Waffe wird nicht sicher zerstört, sondern mit einer
Wahrscheinlichkeit, die dem Schadensanteil entspricht — *"Die Beschädigung einer Einheit wird in %
umgerechnet und dies ergibt die Chance mit welcher die Einheit zerstört wird"*. Beide Beispiele
dort rechnen gegen die **halben** Baupunkte. Und ausdrücklich: *"Um der SL die Arbeit zu
erleichtern wird die Chance und das Ergebnis durch die IT, in der Auswertung ermittelt."*

**Was die Anwendung solange tut:** Die Auswertung des Nahkampfes rechnet weiter wie die
Kampftabelle, damit ihre Zahlen mit denen der Spielleitung übereinstimmen. Die Zerstörungschance
nach 1.5.11 gibt es getrennt davon, mit der halben Grenze aus dem Regelwerk. Beides steht
nebeneinander, statt dass eines das andere still überschreibt.

**Was sich mit der Antwort ändert:** Gilt die Hälfte, ändert sich ein Faktor in der Rückrechnung
der Fernkampfwaffen; gilt die Tabelle, entfällt die Zerstörungschance für Beschuss.

---

## 6. Kann eine Flotte belagern?

**Widerspruch im selben Kapitel:** Regelwerk 1.7 sagt, die Belagerung werde eingeleitet, "wenn ein
feindliches **Heer oder Flotte** auf einer angrenzenden Gemark steht". 1.7.1 schränkt aber ein, dass
nur von den Gemarken aus belagert werden kann, "aus denen heraus sie auch betreten werden können".

Eine Flotte kann einen Rüstort an Land nie betreten. Nimmt man beide Sätze wörtlich, belagert eine
Flotte also nie etwas — und die Erwähnung der Flotte in 1.7 wäre sinnlos.

**Was die Anwendung solange tut:** Sie hält sich an 1.7.1 und zählt nur, wer den Rüstort auch
betreten könnte. Eine Flotte vor einer Küstenburg schnürt sie damit nicht ein.

**Was sich mit der Antwort ändert:** Zählt die Flotte, entfällt die Betretbarkeitsprüfung für
Schiffe — eine Zeile in `BelagerungsRules`.

---

## 7. Was wird bei der Hauptstadtverlegung aus den Baupunkten?

**Was das Regelwerk sagt:** "Im ersten Monat der Verlegung wird aus der alten Hauptstadt eine
Festung ... Im vierten Monat wird aus der zuvor bezeichneten Festung die neue Hauptstadt." Dazu
50.000 GS im ersten Monat (1.5.13). Über die Baupunkte steht dort nichts.

**Warum das eine Frage ist:** Eine Festung hat 3.000 Baupunkte, eine Hauptstadt 5.000 (1.5.7,
1.5.8), und der Ausbau einer Festung zur Hauptstadt kostet 100.000 GS (1.5.8). Spränge die
bezeichnete Festung im vierten Monat einfach auf 5.000 Baupunkte, wäre die Verlegung der halbe
Preis für dasselbe Bauwerk — und ein Reich mit zusammengeschossener Hauptstadt käme durch eine
Verlegung billiger zu einer heilen als durch Reparatur.

**Was die Anwendung solange tut:** Sie erfindet keine Baupunkte. Verlegt wird die Bezeichnung: die
alte Hauptstadt wird zur Festung und behält höchstens deren 3.000 Baupunkte, die neue Hauptstadt
behält die Baupunkte ihrer Festung. Das Reich hat danach eine Hauptstadt, der 2.000 Baupunkte
fehlen — für 100.000 GS aufzufüllen, genau der Preis aus 1.5.8. Die Verlegung kostet damit
insgesamt 150.000 GS.

**Was sich mit der Antwort ändert:** Soll die neue Hauptstadt sofort vollständig sein, wird aus
`SetzeBezeichnung` ein `SetzeStufe` — eine Zeile in `HauptstadtRules`.

---

## 8. ~~In welcher Skala steht BEW_Chars?~~ — beantwortet

**Antwort der Spielleitung (September 2026): Es bleibt bei der Tabelle.**

**Der Befund dahinter:** Die Bewegungstabelle der Charaktere in der crossref.mdb führt für jedes
Landgelände genau das **Doppelte** der Reiterkosten, für Wasser und Tiefsee dagegen die Kosten
eines Schiffes:

| Gelände | Reiter | Charaktere | Schiff |
|---|---|---|---|
| Tiefland, Hochland, Wüste | 7 | 14 | — |
| Wald, Sumpf | 10 | 20 | — |
| Bergland | 21 | 42 | — |
| auf Strasse | 4 / 5 / 7 | 8 / 10 / 14 | — |
| Gebirge | unpassierbar | 42 | — |
| Wasser | unpassierbar | 7 | 7 |
| Tiefsee | unpassierbar | 12 | 12 |

**Was daraus folgt:** In dieser Skala trägt **eine einzige Zahl beide Hälften der Regel**. Mit 42
Punkten kommt ein Charakter über Tiefland drei Felder weit — genau wie ein Reiter mit 21 — und
über Wasser sechs Felder weit, genau wie ein Schiff mit 42. Die 21 des Regelwerks und die 42 der
Zugdaten sind dasselbe, in zwei Skalen ausgedrückt.

**Umgesetzt:** `BewegungsRules.BewegungspunkteCharakter` = 2 × `BewegungspunkteCharakterNachRegelwerk`,
also 42, mit der Herleitung im Code. Weil berechneter und gespeicherter Wert damit
übereinstimmen — nachgeprüft an jeder Namensfigur des Bestands —, frischt `ZugendeRules` die
Bewegungspunkte jetzt auch für Charaktere und Zauberer wieder auf. Das war jahrelang abgeschaltet,
weil es ein lautloses Halbieren gewesen wäre.

**Falls die Tabelle je auf Reiterkosten umgestellt wird**, gehört in
`BewegungspunkteCharakter` die 21, und die Fahrt zur See braucht einen eigenen Wert.

---

## 9. Sind in einem vergangenen Zug 18.000 GS an Bauaufträgen nicht abgerechnet worden?

**Befund:** Für einen abgeschlossenen Zug ergeben die Rüstungstabellen der Zugdatenbank einen
anderen Betrag als die Schatzkammer als verrüstet führt:

| Posten | Betrag |
|---|---|
| Truppenrüstung | 217.830 GS |
| Bauwerke (16 Aufträge) | 56.000 GS |
| Rüstorte | 0 GS |
| **Summe der Tabellen** | **273.830 GS** |
| **Abgerechnet laut Schatzkammer** | **255.830 GS** |

Die Differenz beträgt **18.000 GS** und ist genau **sechsmal 3.000 GS**. Unter den sechzehn
Bauaufträgen stehen vier Wälle zu je 5.000 GS und zwölf Aufträge zu je 3.000 GS — Strassen und
eine Brücke. Sechs dieser zwölf stecken also nicht im abgerechneten Betrag.

**Warum das eine Frage ist:** Beide Zahlen stammen aus derselben Zugdatenbank, nur aus
verschiedenen Tabellen. Entweder sind Bauaufträge nachgetragen worden, nachdem der Monat schon
abgerechnet war, oder die Abrechnung der Spielleitung zählt bestimmte Aufträge nicht mit, oder in
einzelnen Zeilen steht ein anderer Preis als der, der gebucht wurde. Welches davon zutrifft, kann
die Anwendung nicht entscheiden — sie sieht nur das Ergebnis.

**Ein Vorbehalt gehört dazu:** Die Zugdatenbank dieses Monats hat sich während der Arbeit an der
Anwendung verändert; der Fingerabdruck der Datei weicht von dem ab, der zuvor genommen wurde. Es
lässt sich deshalb nicht ausschliessen, dass die überzähligen Aufträge erst kürzlich entstanden
sind und nicht von jeher in den Daten standen. Wer die Frage beantwortet, sollte das mitprüfen.

**Was die Anwendung solange tut:** Sie rechnet, wie sie rechnet, und meldet den Widerspruch. Der
Test `SchatzkammerIntegrationTest.VerruestetStimmtMitDemAbgerechnetenVormonatUeberein` schlägt
deshalb fehl und bleibt rot, bis die Frage geklärt ist. Das ist Absicht: ein grüner Test würde den
Befund verstecken.

**Was sich mit der Antwort ändert:** Liegt es an den Daten, ändert sich am Programm nichts. Zählt
die Abrechnung bestimmte Aufträge bewusst nicht mit, gehört diese Regel in
`SchatzkammerRules.BerechneVerrüstet`.

---

## Nebenbefunde, die keine Frage sind, aber jemandem gehören

* **Eine Kaianlage steht auf einer Gemark der Höhenstufe 2.** Nach Regelwerk 1.5.2 werden
  Kaianlagen immer in Höhenstufe 1 errichtet, und die Bauprüfung verbietet es entsprechend.
  Vermutlich Altbestand.
* **Ein Rüstort in der Bauwerkliste hat keine Baupunkte, aber einen Rüstort-Eintrag in der
  Karte.** Die Anwendung meldet es getrennt und lässt den Eintrag stehen. Seit dem Geradeziehen der
  beschädigten Rüstorte bringt so ein Feld keine Einnahmen und keine Rüstkapazität mehr: was für
  keine Ausbaustufe reicht, steht nicht. Die Sollstufe bleibt in der Karte, damit der Wiederaufbau
  weiss, was dort stand.
* **Woran erkennt die Anwendung einen Ritter des Ritterordens?** Das Regelwerk gibt ihnen mit
  dem Ritterkampf (5.2) einen eigenen Schritt, der Charakterkampf und Rückzugsgefecht ersetzt.
  Im Datenbestand gibt es aber weder ein Reich dieses Namens noch ein Feld beim Charakter, das
  einen Orden festhielte. Die Regeln sind umgesetzt, aber wer ein Ritter ist, muss der Anwendung
  gesagt werden — solange bleibt der Ritterkampf ungenutzt.
* **Wieviele Gutpunkte haben Heerführer und Festungsherr?** Das Regelwerk nennt im
  Beförderungsbeispiel (1.9.1) Burgherr 24, Stadthalter 36 und Herrscher 60 Gutpunkte — den
  Heerführer und den Festungsherrn nirgends. Die Anwendung setzt sie ihrem Rang entsprechend an:
  Heerführer 12, Festungsherr 48 zwischen Stadthalter und Herrscher. Beide Werte schätzen nicht
  nur das Amt eines Charakters, wenn seine Beschriftung es nicht verrät, sondern sind seit den
  Beförderungen auch das Maximum, das eine Beförderung in dieses Amt setzt (`BeförderungsRules`).
* **Im Nahkampf verlieren Katapulte mit, obwohl sie sich ergeben.** Das Regelwerk sagt, dass
  Katapulte am Nahkampf nicht teilnehmen und sich immer ergeben (5.5); die Kampftabelle verteilt
  die Verluste aber auch auf ihre Zeilen. Die Anwendung rechnet die Verluste wie die Tabelle und
  zählt als Beute die Katapulte, die in den Nahkampf gegangen sind — bei einem aufgeriebenen Heer
  die Hälfte davon, wie es das Regelwerk vorsieht.
* **Wann genau fällt ein beschädigter Rüstort eine Stufe?** Seit dem Geradeziehen zählt der
  tatsächliche Stand: ein Rüstort ist die höchste Stufe, die seine Baupunkte tragen (Regelwerk 1.5).
  Die Referenztabelle ist dafür gebaut — Burg-I bis Burg-III tragen die Werte der fertigen Burg, wie
  es das Beispiel in 1.5 verlangt. Die Bauwerkkapitel nennen aber andere Schwellen: "Eine
  beschossene Festung wird mit 2499 Baupunkten zur Stadt" (1.5.7), "Eine Hauptstadt ... wird mit
  3.499 Baupunkten zur Festung" (1.5.8), "Eine Festungshauptstadt wird mit 5499 Baupunkten zur
  Hauptstadt" (1.5.9). Diese drei Zahlen liegen jeweils 500 über der nächstniedrigeren Stufe — nach
  ihnen bliebe eine Festung bis 2500 Baupunkte eine Festung, nach der Tabelle ist sie ab 2999
  eine Stadt. Der Unterschied ist eine Stufe im Kampf und bei den Einnahmen. Die Anwendung folgt der
  Tabelle und dem Beispiel aus 1.5.
* **Die `settings`-Tabelle der Zugdatenbank läuft dem Zugverzeichnis um sieben Monate voraus,
  und ihre Phase steht überall auf Bewegungsphase.** Der Befund über drei Züge:

  | Zugverzeichnis | `settings.Monat` | Schatzkammer | `settings.Phase` |
  |---|---|---|---|
  | 168 | 175 | 168 | 1 |
  | 169 | 176 | 169 | 1 |
  | 170 | 177 | 170 | 1 |

  Verzeichnis und Schatzkammer stimmen überein, die settings-Zeile gehört zu einem anderen Monat.
  Damit sagt auch ihre Phase nichts über diesen Zug aus — und weil dort überall 1 steht, also
  Bewegungsphase, war die Rüstphase nie erreichbar: es liess sich in keinem Zug etwas bauen oder
  rüsten.

  Die Anwendung rechnet deshalb mit dem Verzeichnis und der Schatzkammer, warnt über den
  Widerspruch und führt die Zugphase selbst, beginnend mit der Rüstphase
  (`ZugView.PhaseStehtInDenZugdaten`). Ein gespeichertes *abgeschlossen* gilt weiterhin, das setzt
  niemand versehentlich.

  **Die Anwendung stellt den Monat inzwischen selbst richtig**, und zwar genau dann, wenn der
  Spieler die Phase wechselt (`ZugView.SetzePhase`). Sie muss es: sonst hätte die beendete
  Rüstphase keinen Ort, an dem sie überlebt — beim nächsten Start stünde der Zug wieder in der
  Rüstphase, obwohl es laut Regelwerk kein Zurück gibt. Aufgefallen ist es als "Züge 168, 169, 170:
  immer nur Rüstphase", und damit liess sich keine Figur bewegen.

  Je Zugdatenbank gibt es genau eine `settings`-Zeile, und die Datei gehört zu genau einem Zug; der
  Monat, den sie nennt, ist also schlicht falsch, wenn er vom Verzeichnis abweicht. Geschrieben
  wird der berichtigte Wert mit einer Meldung, die beide Zahlen nennt. **Falls die Spielleitung die
  177 in Verzeichnis 170 doch braucht, muss diese Richtigstellung wieder heraus** — dann wird ein
  anderer Ort für die Phase gebraucht.
