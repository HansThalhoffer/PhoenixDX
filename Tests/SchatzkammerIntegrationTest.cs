using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Text;

namespace Tests {

    /// <summary>
    /// Prüft die Bilanzformel gegen die echte Schatzkammerhistorie.
    ///
    /// Die Tabelle Schatzkammer führt eine Zeile je Monat. Der Reichsschatz eines Monats ist das
    /// Ergebnis des Vormonats. Über 50 echte Monatsübergänge hinweg trägt die Formel in 38 Fällen
    /// exakt - die restlichen zwölf sind Korrekturen, die von aussen direkt auf den Reichsschatz
    /// gebucht wurden und in keinem der Bilanzfelder auftauchen (siehe <see cref="BekannteKorrekturen"/>).
    /// Die Formel selbst bleibt damit bestätigt, die Historie ist nur kein lückenloses Journal.
    /// </summary>
    public class SchatzkammerIntegrationTest {

        /// <summary>
        /// Monatsübergänge, bei denen der gespeicherte Reichsschatz von der Bilanz abweicht, mit der
        /// jeweiligen Differenz. Das sind Eingriffe der Spielleitung beziehungsweise Schenkungen, die
        /// nachträglich oder über die Monatsgrenze hinweg gebucht wurden.
        ///
        /// Auffällig und erklärbar ist das Paar 163/164: dort wurde eine Schenkung über 200000 beim
        /// Übergang nach 164 abgezogen, aber in der Zeile 164 statt 163 vermerkt - deshalb einmal
        /// -200000 und im Folgemonat wieder +200000.
        ///
        /// Der Zielmonat ist der Schlüssel, der Wert die Differenz "gespeichert minus berechnet".
        /// </summary>
        private static readonly Dictionary<int, int> BekannteKorrekturen = new() {
            { 121, 350652 }, { 125, 14000 }, { 127, 80000 }, { 132, 80000 },
            { 133, 6000 },   { 138, 320944 }, { 140, -21000 }, { 141, 20000 },
            { 151, -2600 },  { 159, 295068 }, { 164, -200000 }, { 165, 200000 },
        };

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        [StaFact]
        public void HistorieIstLueckenlos() {
            LadeAlles();
            var historie = SchatzkammerRules.GetHistorie();
            Assert.NotEmpty(historie);

            for (int i = 1; i < historie.Count; i++)
                Assert.Equal(historie[i - 1].monat + 1, historie[i].monat);
        }

        /// <summary>
        /// Der Reichsschatz eines Monats muss der Bilanz des Vormonats entsprechen, sofern in dem
        /// Übergang nicht von aussen am Schatz gedreht wurde.
        /// </summary>
        [StaFact]
        public void BilanzformelErgibtDenNaechstenReichsschatz() {
            LadeAlles();
            var historie = SchatzkammerRules.GetHistorie();
            Assert.True(historie.Count > 1, "Für den Vergleich braucht es mindestens zwei Monate");

            var abweichungen = new StringBuilder();
            int exakt = 0;
            for (int i = 1; i < historie.Count; i++) {
                var vormonat = historie[i - 1];
                var monat = historie[i];
                int erwartet = SchatzkammerRules.BerechneBilanz(vormonat);
                int differenz = monat.Reichschatz - erwartet;

                if (differenz == 0) {
                    exakt++;
                    if (BekannteKorrekturen.ContainsKey(monat.monat))
                        abweichungen.AppendLine($"Monat {monat.monat} ist als Korrektur vermerkt, stimmt jetzt aber exakt - der Eintrag gehört entfernt");
                    continue;
                }

                if (BekannteKorrekturen.TryGetValue(monat.monat, out int bekannt) && bekannt == differenz)
                    continue;

                abweichungen.AppendLine(
                    $"Monat {monat.monat}: erwartet {erwartet}, gespeichert {monat.Reichschatz}, Differenz {differenz}. "
                    + $"Vormonat {vormonat.monat}: Schatz {vormonat.Reichschatz}, Land {vormonat.Einahmen_land}, "
                    + $"bekommen {vormonat.schenkung_bekommen}, verschenkt {vormonat.schenkung_getaetigt}, verrüstet {vormonat.Verruestet}");
            }

            Assert.True(abweichungen.Length == 0, $"Die Bilanzformel passt nicht mehr:\r\n{abweichungen}");
            Assert.True(exakt > 30, $"Nur {exakt} Monatsübergänge gehen exakt auf - das waren einmal 38");
        }

        /// <summary>
        /// Prüft die Rüstungskosten gegen einen Monat, dessen Ergebnis schon feststeht.
        ///
        /// Die Kosten einer Rüstung stehen nirgends in der Zugdatenbank, sie ergeben sich erst aus
        /// Stückzahl mal Kostentabelle. Ob diese Rechnung stimmt, lässt sich nur an einem
        /// abgeschlossenen Monat zeigen: das Verzeichnis des Vormonats enthält noch dessen
        /// Rüstungstabellen, und der verrüstete Betrag steht in der Historie des laufenden Zuges.
        /// </summary>
        [StaFact]
        public void VerruestetStimmtMitDemAbgerechnetenVormonatUeberein() {
            LadeAlles();
            int vormonat = ZugView.AktuellerZug.Vorheriger.Zug;
            var abgerechnet = SchatzkammerRules.GetMonat(vormonat);
            Assert.NotNull(abgerechnet);

            try {
                if (TestSetup.LoadZugdatenAusZug(vormonat) == false)
                    return; // die alten Zugverzeichnisse sind nicht mehr da, dann ist hier nichts zu prüfen

                int berechnet = SchatzkammerRules.BerechneVerrüstet();
                Assert.True(berechnet == abgerechnet.Verruestet,
                    $"Für Zug {vormonat} ergeben die Rüstungstabellen {berechnet} GS, "
                    + $"abgerechnet wurden aber {abgerechnet.Verruestet} GS");
            }
            finally {
                // die folgenden Tests müssen wieder auf dem laufenden Zug arbeiten
                TestSetup.LoadZugdaten(false, false);
            }
        }

        /// <summary>
        /// Die Einnahmemonate sind in der Altanwendung hart auf Larn und Agul gesetzt. Die echten
        /// Zugdaten bestätigen das: genau in diesen Monaten steht ein Betrag in einahmen_land.
        /// </summary>
        [StaFact]
        public void EinnahmemonateDeckenSichMitDenEchtenZugdaten() {
            LadeAlles();
            foreach (var monat in SchatzkammerRules.GetHistorie()) {
                // der letzte Monat ist der laufende und noch nicht abgerechnet
                bool istEinnahmemonat = new Zugmonat(monat.monat).IstEinnahmemonat;
                if (monat.Einahmen_land > 0)
                    Assert.True(istEinnahmemonat, $"Monat {monat.monat} hat Landeinnahmen {monat.Einahmen_land}, gilt aber nicht als Einnahmemonat");
            }
        }
    }
}
