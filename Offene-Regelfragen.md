# Offene Regelfragen

Fünf Punkte, bei denen die Quellen sich widersprechen oder das Datenmodell etwas nicht kennt.
Die Anwendung verhält sich jeweils so, dass nichts kaputtgeht, solange die Frage offen ist —
was das konkret heißt, steht unten bei "Was die Anwendung solange tut".

Hinweis: hier stehen bewusst keine Feldkoordinaten und keine Einheitennummern, weil das
Repository öffentlich ist.

---

## 1. Haben Charaktere 21 oder 42 Bewegungspunkte?

**Widerspruch:** Das Regelwerk nennt in Kapitel 1.1 für den *Heerführercharakter* **21**
Bewegungspunkte. Die Zugdaten der Spielleitung führen dagegen für **alle** Charaktere
**42** — ausnahmslos.

**Was dafür spricht, dass 42 stimmt:** Ein Vergleich zweier aufeinanderfolgender Monate zeigt
einen Charakter, der in einem Monat sechs Felder weit gezogen ist und dabei genau 42 Punkte
verbraucht hat. Mit 21 Punkten wären das bei üblichen Geländekosten höchstens drei Schritte
gewesen. Vermutlich meint die 21 im Regelwerk den Charakter *im Heer*, der sich mit dem Heer
bewegt, nicht den allein reisenden.

Für Zauberer nennt das Regelwerk ausdrücklich 42; das ist bereits korrigiert.

**Was die Anwendung solange tut:** Namensfiguren behalten beim Zugwechsel ihren gespeicherten
Höchstwert. Würde sie ihn neu berechnen, nähme sie jedem Charakter lautlos die Hälfte seiner
Bewegungspunkte.

**Was sich mit der Antwort ändert:** Eine Zeile in `BewegungsRules.BerechneBewegungspunkte`.
Lautet die Antwort 42, kann `ZugendeRules` den Höchstwert auch für Namensfiguren wieder
mitberechnen.

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

## Nebenbefunde, die keine Frage sind, aber jemandem gehören

* **Eine Kaianlage steht auf einer Gemark der Höhenstufe 2.** Nach Regelwerk 1.5.2 werden
  Kaianlagen immer in Höhenstufe 1 errichtet, und die Bauprüfung verbietet es entsprechend.
  Vermutlich Altbestand.
* **Ein Rüstort in der Bauwerkliste hat keine Baupunkte, aber einen Rüstort-Eintrag in der
  Karte.** Das ist kein zerstörtes Bauwerk, sondern ein Widerspruch innerhalb der Karte; die
  Anwendung meldet es getrennt und lässt den Eintrag stehen.
* **Woran erkennt die Anwendung einen Ritter des Ritterordens?** Das Regelwerk gibt ihnen mit
  dem Ritterkampf (5.2) einen eigenen Schritt, der Charakterkampf und Rückzugsgefecht ersetzt.
  Im Datenbestand gibt es aber weder ein Reich dieses Namens noch ein Feld beim Charakter, das
  einen Orden festhielte. Die Regeln sind umgesetzt, aber wer ein Ritter ist, muss der Anwendung
  gesagt werden — solange bleibt der Ritterkampf ungenutzt.
* * **Wieviele Gutpunkte hat ein Festungsherr?** Das Regelwerk nennt im Beförderungsbeispiel
  (1.9.1) Burgherr 24, Stadthalter 36 und Herrscher 60 Gutpunkte, den Festungsherrn aber nirgends.
  Die Anwendung ordnet ihn seinem Rang entsprechend zwischen Stadthalter und Herrscher mit 48 ein.
  Der Wert dient nur dazu, das Amt eines Charakters zu schätzen, wenn seine Beschriftung es nicht
  verrät.
* * **Im Nahkampf verlieren Katapulte mit, obwohl sie sich ergeben.** Das Regelwerk sagt, dass
  Katapulte am Nahkampf nicht teilnehmen und sich immer ergeben (5.5); die Kampftabelle verteilt
  die Verluste aber auch auf ihre Zeilen. Die Anwendung rechnet die Verluste wie die Tabelle und
  zählt als Beute die Katapulte, die in den Nahkampf gegangen sind — bei einem aufgeriebenen Heer
  die Hälfte davon, wie es das Regelwerk vorsieht.
* **Die `settings`-Tabelle der Zugdatenbank läuft dem Zugverzeichnis voraus.** Die Anwendung
  rechnet mit dem Verzeichnis und der Schatzkammer, die übereinstimmen, und warnt.
