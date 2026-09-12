using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Karte, PZE und Crossreferenzen werden im Testlauf nur einmal geladen, weil jedes Laden
    /// Access-Verbindungen kostet und der Treiber davon sporadisch abstürzt (siehe
    /// <see cref="SpeichernIntegrationTest"/>).
    ///
    /// Das setzt voraus, dass kein Test diese Nachschlagewerke verändert - sonst sieht der
    /// nächste Test die Änderung. Diese Prüfung vergleicht den gemeinsam genutzten Stand mit
    /// einem frisch geladenen.
    ///
    /// Die Reihenfolge der Tests liegt nicht fest; läuft dieser Test zufällig als erster, findet
    /// er nichts. Über mehrere Läufe hinweg fällt eine Verunreinigung trotzdem auf.
    /// </summary>
    public class ReferenzdatenTest {

        [StaFact]
        public void DieKarteWirdVonKeinemTestVeraendert() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();

            int felder = SharedData.Map!.Count;
            int gebäude = SharedData.Gebäude!.Count;
            long baupunkte = SharedData.Map.Values.Sum(k => (long)k.Baupunkte);
            long nationen = SharedData.Map.Values.Sum(k => (long)(k.Nation?.Nummer ?? 0));

            TestSetup.LoadKarte(erzwingen: true);

            Assert.Equal(felder, SharedData.Map!.Count);
            Assert.Equal(gebäude, SharedData.Gebäude!.Count);
            Assert.Equal(baupunkte, SharedData.Map.Values.Sum(k => (long)k.Baupunkte));
            Assert.Equal(nationen, SharedData.Map.Values.Sum(k => (long)(k.Nation?.Nummer ?? 0)));
        }

        [StaFact]
        public void DieCrossreferenzenWerdenVonKeinemTestVeraendert() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);

            int rüstorte = SharedData.RüstortReferenz!.Count;
            int kosten = SharedData.Kosten!.Count;
            int teleport = SharedData.Crossref_zauberer_teleport!.Count;
            long baupunkte = SharedData.RüstortReferenz.Sum(r => (long)(r.Baupunkte ?? 0));

            TestSetup.LoadCrossRef(false, false, erzwingen: true);

            Assert.Equal(rüstorte, SharedData.RüstortReferenz!.Count);
            Assert.Equal(kosten, SharedData.Kosten!.Count);
            Assert.Equal(teleport, SharedData.Crossref_zauberer_teleport!.Count);
            Assert.Equal(baupunkte, SharedData.RüstortReferenz.Sum(r => (long)(r.Baupunkte ?? 0)));
        }

        /// <summary>
        /// Und die Abkürzung muss auch wirklich abkürzen - sonst ist der ganze Aufwand umsonst.
        /// </summary>
        [StaFact]
        public void EinZweitesLadenDerKarteWirdUebersprungen() {
            TestSetup.Setup();
            TestSetup.LoadKarte();

            var uhr = System.Diagnostics.Stopwatch.StartNew();
            TestSetup.LoadKarte();
            uhr.Stop();

            Assert.True(uhr.ElapsedMilliseconds < 100,
                $"Das zweite Laden der Karte dauerte {uhr.ElapsedMilliseconds} ms - es wurde nicht übersprungen.");
        }
    }
}
