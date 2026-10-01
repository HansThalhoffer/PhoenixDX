using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using static PhoenixModel.View.KartenanzeigeView;

namespace Tests {

    /// <summary>
    /// Die zusaetzlichen Kartenanzeigen.
    ///
    /// Fuenf Anzeigen, die die Altanwendung in ihren Menues fuehrt und von denen dort keine
    /// einzige angeschlossen ist - die Schalter stehen im Formular und werden nirgends gelesen,
    /// der Folienzug ist als "wird erstmal nicht mehr unterstuetzt" vermerkt. Uebernommen ist die
    /// Absicht, nicht der Code; geprueft wird deshalb, was diese Anwendung daraus macht.
    ///
    /// Gezeichnet wird in MonoGame und laesst sich hier nicht pruefen. Pruefbar ist, was auf die
    /// Karte gehoert.
    /// </summary>
    public class KartenanzeigeTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>
        /// Ohne Beschriftung steht nichts auf der Karte - und es wird auch nichts gerechnet.
        /// </summary>
        [StaFact]
        public void OhneBeschriftungStehtNichtsDa() {
            LadeAlles();
            var irgendeine = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).First(Plausibilität.IsValid);
            var gemark = KleinfeldView.GetKleinfeld(irgendeine);

            Assert.Equal(string.Empty, GetBeschriftung(gemark, Feldbeschriftung.Keine));
            Assert.Equal(string.Empty, GetBeschriftung(null, Feldbeschriftung.Stärke));
            Assert.Empty(GetBeschriftungen(Feldbeschriftung.Keine));
        }

        /// <summary>
        /// Die Nummern stehen auf dem Feld, auf dem die Figur steht - und zwar ihre.
        /// </summary>
        [StaFact]
        public void DieNummernStehenAufDemRichtigenFeld() {
            LadeAlles();
            var figur = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).First(Plausibilität.IsValid);
            var gemark = KleinfeldView.GetKleinfeld(figur);
            Assert.True(gemark != null, "Die Figur steht auf keinem bekannten Feld");

            string text = GetBeschriftung(gemark, Feldbeschriftung.Nummer);
            Assert.Contains(figur.Nummer.ToString(), text);

            // mehrere Figuren auf einem Feld werden getrennt aufgezaehlt
            var zusammen = SpielfigurenView.GetSpielfiguren(gemark!)
                .Where(f => f.Nation == ProgramView.SelectedNation && Plausibilität.IsValid(f))
                .ToList();
            Assert.Equal(zusammen.Count, text.Split('/').Length);
        }

        /// <summary>
        /// Die Staerke zaehlt die Truppen zusammen; Namensfiguren sind Personen und werden
        /// angehaengt, nicht addiert - ein Zauberer ist keine Staerke von 1.
        /// </summary>
        [StaFact]
        public void DieStaerkeZaehltTruppenUndNenntNamensfigurenGetrennt() {
            LadeAlles();

            var mitTruppe = SharedData.Map!.Values.FirstOrDefault(gemark =>
                SpielfigurenView.GetSpielfiguren(gemark)
                    .Any(f => f.Nation == ProgramView.SelectedNation && Plausibilität.IsValid(f)
                              && f is TruppenSpielfigur truppe && truppe.staerke > 0));
            Assert.True(mitTruppe != null, "Keine eigene Truppe auf der Karte");

            int summe = SpielfigurenView.GetSpielfiguren(mitTruppe!)
                .Where(f => f.Nation == ProgramView.SelectedNation && Plausibilität.IsValid(f))
                .OfType<TruppenSpielfigur>()
                .Sum(truppe => truppe.staerke);
            int namensfiguren = SpielfigurenView.GetSpielfiguren(mitTruppe!)
                .Count(f => f.Nation == ProgramView.SelectedNation && Plausibilität.IsValid(f)
                            && f is NamensSpielfigur);

            string text = GetBeschriftung(mitTruppe, Feldbeschriftung.Stärke);
            Assert.StartsWith(summe.ToString(), text);
            if (namensfiguren > 0)
                Assert.Contains($"+{namensfiguren}", text);
        }

        /// <summary>
        /// Fremde Einheiten werden nicht beschriftet: von ihnen kennt man aus der Feindaufklaerung
        /// die Nummer, aber nicht die Staerke. Beides nebeneinander liesse eine Vermutung wie eine
        /// Tatsache aussehen.
        /// </summary>
        [StaFact]
        public void FremdeEinheitenWerdenNichtBeschriftet() {
            LadeAlles();
            var fremdeFigur = SharedData.Krieger?.FirstOrDefault(k =>
                k.Nation != null && k.Nation != ProgramView.SelectedNation && Plausibilität.IsValid(k));
            if (fremdeFigur == null)
                return; // in diesen Zugdaten sind keine fremden Figuren geladen

            var gemark = KleinfeldView.GetKleinfeld(fremdeFigur);
            bool eigeneAuchDa = SpielfigurenView.GetSpielfiguren(gemark!)
                .Any(f => f.Nation == ProgramView.SelectedNation && Plausibilität.IsValid(f));
            if (eigeneAuchDa)
                return; // dann steht dort zu Recht etwas

            Assert.Equal(string.Empty, GetBeschriftung(gemark, Feldbeschriftung.Stärke));
            Assert.Equal(string.Empty, GetBeschriftung(gemark, Feldbeschriftung.Nummer));
        }

        /// <summary>
        /// Die Sammelabfrage liefert genau die Felder, auf denen auch einzeln etwas steht.
        /// </summary>
        [StaFact]
        public void DieSammelabfrageStimmtMitDerEinzelnenUeberein() {
            LadeAlles();
            var beschriftungen = GetBeschriftungen(Feldbeschriftung.Nummer);
            Assert.NotEmpty(beschriftungen);

            foreach (var eintrag in beschriftungen) {
                var gemark = SharedData.Map![eintrag.Key];
                Assert.Equal(eintrag.Value, GetBeschriftung(gemark, Feldbeschriftung.Nummer));
                Assert.False(string.IsNullOrEmpty(eintrag.Value));
            }

            // und kein Feld fehlt
            int erwartet = SharedData.Map!.Values
                .Count(gemark => GetBeschriftung(gemark, Feldbeschriftung.Nummer).Length > 0);
            Assert.Equal(erwartet, beschriftungen.Count);
        }

        /// <summary>
        /// Beschaedigt ist ein Ruestort, dessen Baupunkte seine Sollstufe nicht mehr tragen - und
        /// nur ein eigener.
        /// </summary>
        [StaFact]
        public void BeschaedigtIstWerSeineStufeNichtMehrTraegt() {
            LadeAlles();
            var beschädigt = FindeBeschädigte();

            Assert.All(beschädigt, eintrag => Assert.Equal(ProgramView.SelectedNation, eintrag.Gemark.Nation));
            // Soll und Ist sind verschieden - sonst waere es keine Beschaedigung
            Assert.All(beschädigt, eintrag => Assert.NotEqual(eintrag.Soll, eintrag.Ist));
            Assert.All(beschädigt, eintrag => Assert.False(string.IsNullOrWhiteSpace(eintrag.Soll)));
            Assert.All(beschädigt, eintrag => Assert.False(string.IsNullOrWhiteSpace(eintrag.Beschreibung)));

            // nach Bezeichnung sortiert
            var namen = beschädigt.Select(eintrag => eintrag.Gemark.Bezeichner).ToList();
            Assert.Equal(namen.OrderBy(name => name, StringComparer.OrdinalIgnoreCase), namen);
        }

        /// <summary>
        /// Fest stationiert ist, wer in diesem Zug keinen Schritt getan hat.
        ///
        /// Nicht gemeint ist, wer keine Bewegungspunkte mehr hat - der ist erschoepft, nicht
        /// stationiert. Unterschieden wird an der Bewegungsspur.
        /// </summary>
        [StaFact]
        public void StationiertIstWerKeinenSchrittGetanHat() {
            LadeAlles();
            var stehengeblieben = FindeStationierte();

            Assert.All(stehengeblieben, figur => Assert.Equal(0, new Bewegungsspur(figur).Count));
            Assert.All(stehengeblieben, figur => Assert.Equal(ProgramView.SelectedNation, figur.Nation));

            // und umgekehrt: wer gezogen ist, steht nicht in der Liste
            var gezogen = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .Where(Plausibilität.IsValid)
                .Where(figur => new Bewegungsspur(figur).Count > 0)
                .ToList();
            Assert.True(gezogen.Count > 0, "In diesen Zugdaten ist niemand gezogen");
            // ReferenceEquals, nicht Contains: Spielfigur erbt von KleinfeldPosition ein Equals
            // nach gf/kf - zwei verschiedene Figuren desselben Feldes sind damit gleich, und
            // Contains faende die falsche. Dieselbe Falle steht im Kartenkontextmenue vermerkt.
            foreach (var figur in gezogen)
                Assert.False(stehengeblieben.Any(stehend => ReferenceEquals(stehend, figur)),
                    $"{figur.Bezeichner} ist gezogen und gilt trotzdem als stationiert");

            // zusammen ergibt es alle gueltigen eigenen Figuren
            int alle = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).Count(Plausibilität.IsValid);
            Assert.Equal(alle, stehengeblieben.Count + gezogen.Count);
        }

        /// <summary>
        /// Der Folienzug zeigt die Wege dieses Zuges - mit dem Startfeld voran.
        /// </summary>
        [StaFact]
        public void DerFolienzugZeigtDieWegeDesZuges() {
            LadeAlles();
            var spuren = FindeZugspuren();
            Assert.NotEmpty(spuren);

            foreach (var spur in spuren) {
                Assert.True(spur.Felder.Count >= 2,
                    $"{spur.Figur.Bezeichner} hat eine Spur aus {spur.Felder.Count} Feldern");
                // das erste Feld ist das Startfeld der Figur
                Assert.Equal(spur.Figur.gf_von, spur.Felder[0].gf);
                Assert.Equal(spur.Figur.kf_von, spur.Felder[0].kf);
                // und das letzte ist, wo sie jetzt steht
                var letztes = spur.Felder[^1];
                Assert.Equal(spur.Figur.gf, letztes.gf);
                Assert.Equal(spur.Figur.kf, letztes.kf);
            }

            // ohne Startfeld faengt die Spur beim ersten Wegpunkt an
            var ohneStart = FindeZugspuren(new Folienzugeinstellung(MitStartfeld: false));
            Assert.Equal(spuren.Count, ohneStart.Count);
            Assert.All(ohneStart, spur => Assert.Equal(new Bewegungsspur(spur.Figur).Count, spur.Felder.Count));
        }

        /// <summary>
        /// Abgeschaltete Gattungen bleiben draussen - jede einzeln.
        /// </summary>
        [StaFact]
        public void AbgeschalteteGattungenBleibenDraussen() {
            LadeAlles();
            var alle = FindeZugspuren();

            var ohneReiter = FindeZugspuren(new Folienzugeinstellung(Reiter: false));
            Assert.DoesNotContain(ohneReiter, spur => spur.Gattung == FigurType.Reiter);
            Assert.True(ohneReiter.Count <= alle.Count);

            // nichts angehakt heisst nichts auf der Karte
            var garnichts = FindeZugspuren(new Folienzugeinstellung(
                Krieger: false, Reiter: false, Schiffe: false,
                Kreaturen: false, Zauberer: false, Charaktere: false));
            Assert.Empty(garnichts);
        }

        /// <summary>
        /// Die Beschriftung kommt bis auf die Karte an - und laesst sich wieder abraeumen.
        ///
        /// Gezeichnet wird hier nichts, dafuer braucht es MonoGame. Geprueft wird der Weg bis
        /// dorthin: dass die Kartenstruktur die Koordinaten wiederfindet und kein Feld verliert.
        /// </summary>
        [StaFact]
        public void DieBeschriftungKommtAufDerKarteAn() {
            LadeAlles();
            var welt = new PhoenixDX.Structures.Welt(SharedData.Map!);

            var texte = GetBeschriftungen(Feldbeschriftung.Nummer);
            Assert.NotEmpty(texte);

            // Die Karte findet ihre Felder ueber die Koordinaten, nicht ueber den Bezeichner
            var mitKoordinaten = texte
                .Select(eintrag => (Feld: (KleinfeldPosition)SharedData.Map![eintrag.Key], Text: eintrag.Value))
                .ToList();
            int beschriftet = welt.Beschrifte(mitKoordinaten);
            Assert.True(beschriftet > 0, "Kein einziges Feld wurde beschriftet");
            Assert.Equal(beschriftet, welt.AnzahlBeschriftungen);

            // Nicht jedes Kleinfeld der Datenbank liegt in der Kartenstruktur: Welt ueberspringt
            // doppelte Eintraege (siehe das TODO dort). Verlangt wird deshalb nicht Gleichheit,
            // sondern dass nichts erfunden wird.
            Assert.True(beschriftet <= texte.Count,
                $"Die Karte beschriftet {beschriftet} Felder, angefordert waren {texte.Count}");

            // eine zweite Zuordnung ersetzt die erste vollstaendig
            welt.Beschrifte(mitKoordinaten.Take(1).ToList());
            Assert.True(welt.AnzahlBeschriftungen <= 1);

            welt.Beschrifte(null);
            Assert.Equal(0, welt.AnzahlBeschriftungen);
        }

        /// <summary>
        /// Mehrere Mengen liegen nebeneinander auf der Karte - der Folienzug braucht das, weil er
        /// je Gattung eine eigene Farbe zeichnet.
        /// </summary>
        [StaFact]
        public void MehrereEinfaerbungenLiegenNebeneinander() {
            LadeAlles();
            var welt = new PhoenixDX.Structures.Welt(SharedData.Map!);
            var spuren = FindeZugspuren();
            Assert.True(spuren.Count >= 2, "Zu wenige Wege in diesen Zugdaten");

            var erste = spuren[0].Felder.Select(f => new KleinfeldPosition(f.gf, f.kf)).ToList();
            var zweite = spuren[1].Felder.Select(f => new KleinfeldPosition(f.gf, f.kf))
                .Where(f => erste.Any(e => e.gf == f.gf && e.kf == f.kf) == false).ToList();

            welt.HebeHervor(erste, new Microsoft.Xna.Framework.Color(255, 0, 255));
            int nachDerErsten = welt.AnzahlHervorhebungen;
            Assert.True(nachDerErsten > 0);

            // FaerbeEin loescht nicht - sonst waere der Folienzug am Ende einfarbig
            welt.FärbeEin(zweite, new Microsoft.Xna.Framework.Color(0, 0, 0));
            Assert.Equal(nachDerErsten + zweite.Select(f => $"{f.gf}/{f.kf}").Distinct().Count(),
                welt.AnzahlHervorhebungen);

            // HebeHervor dagegen schon
            welt.HebeHervor(erste, new Microsoft.Xna.Framework.Color(255, 0, 255));
            Assert.Equal(nachDerErsten, welt.AnzahlHervorhebungen);

            welt.LöscheHervorhebung();
            Assert.Equal(0, welt.AnzahlHervorhebungen);
        }

        /// <summary>
        /// Die Artillerie faehrt bei den Kriegern mit, die Kriegsschiffe bei den Schiffen - sonst
        /// fiele sie beim Abschalten durch jedes Raster.
        /// </summary>
        [Fact]
        public void JedeGattungGehoertZuEinemSchalter() {
            var alles = Folienzugeinstellung.Alles;
            FigurType[] beweglich = [
                FigurType.Krieger, FigurType.Reiter, FigurType.Kreatur,
                FigurType.LeichteArtillerie, FigurType.SchwereArtillerie,
                FigurType.BeritteneLeichteArtillerie, FigurType.BeritteneSchwereArtillerie,
                FigurType.Schiff, FigurType.LeichtesKriegsschiff, FigurType.SchweresKriegsschiff,
                FigurType.PiratenSchiff, FigurType.PiratenLeichtesKriegsschiff,
                FigurType.PiratenSchweresKriegsschiff,
                FigurType.Zauberer, FigurType.Charakter, FigurType.CharakterZauberer,
            ];
            foreach (var typ in beweglich)
                Assert.True(alles.Zeigt(typ), $"{typ} gehoert zu keinem Schalter des Folienzugs");
        }
    }
}
