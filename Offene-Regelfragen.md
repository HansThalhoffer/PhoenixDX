# Offene Regelfragen

Vier Punkte, bei denen die Quellen sich widersprechen oder das Datenmodell etwas nicht kennt.
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

## Nebenbefunde, die keine Frage sind, aber jemandem gehören

* **Eine Kaianlage steht auf einer Gemark der Höhenstufe 2.** Nach Regelwerk 1.5.2 werden
  Kaianlagen immer in Höhenstufe 1 errichtet, und die Bauprüfung verbietet es entsprechend.
  Vermutlich Altbestand.
* **Ein Rüstort in der Bauwerkliste hat keine Baupunkte, aber einen Rüstort-Eintrag in der
  Karte.** Das ist kein zerstörtes Bauwerk, sondern ein Widerspruch innerhalb der Karte; die
  Anwendung meldet es getrennt und lässt den Eintrag stehen.
* **Die `settings`-Tabelle der Zugdatenbank läuft dem Zugverzeichnis voraus.** Die Anwendung
  rechnet mit dem Verzeichnis und der Schatzkammer, die übereinstimmen, und warnt.
