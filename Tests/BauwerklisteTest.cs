using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Bauwerkliste der Erkenfarakarte.mdb ist unvollstaendig - zu manchen Gemarken, auf denen
    /// laut Karte ein Gebaeude steht, fehlt dort der Eintrag. Die Karte ist die gepflegte Tabelle,
    /// deshalb ergaenzt die Anwendung die fehlenden Eintraege beim Laden.
    ///
    /// Frueher lief diese Reparatur nebenher in einem Task und wetteiferte mit dem Aufbau der
    /// Karte. Wer zuerst an ein Gemark kam, entschied darueber, ob die Meldung ueber das fehlende
    /// Gebaeude erschien - beim Start also mal ja, mal nein.
    /// </summary>
    public class BauwerklisteTest {

        /// <summary>
        /// Nach dem Laden der Karte muss zu jedem Gebaeude auf der Karte ein Eintrag in der
        /// Bauwerkliste stehen. Ist das so, kann niemand mehr ein fehlendes Gebaeude melden.
        /// </summary>
        [StaFact]
        public void NachDemLadenFehltKeinGebaeudeMehr() {
            TestSetup.Setup();
            TestSetup.LoadKarte(erzwingen: true);

            Assert.NotNull(SharedData.Map);
            Assert.NotNull(SharedData.Gebäude);

            var gebaeudeAufDerKarte = SharedData.Map!.Values.Where(gemark => gemark.Baupunkte > 0).ToList();
            Assert.NotEmpty(gebaeudeAufDerKarte);

            var ohneEintrag = gebaeudeAufDerKarte
                .Where(gemark => SharedData.Gebäude!.ContainsKey(gemark.Bezeichner) == false)
                .Select(gemark => gemark.Bezeichner)
                .ToList();

            Assert.True(ohneEintrag.Count == 0,
                $"Zu {ohneEintrag.Count} von {gebaeudeAufDerKarte.Count} Gebaeuden auf der Karte fehlt der Eintrag "
                + $"in der Bauwerkliste: {string.Join(", ", ohneEintrag.Take(20))}");
        }

        /// <summary>
        /// Der Kern der Sache: trifft der Zugriff auf ein Gemark auf einen fehlenden Eintrag, wird
        /// er ergaenzt statt nur gemeldet. Vorher lieferte GetGebaeude in dem Fall null und schrieb
        /// einen Fehler ins Protokoll - und ob es dazu kam, entschied das Wettrennen mit der
        /// Reparatur, die nebenher lief.
        ///
        /// Der Test nimmt einem Gemark seinen Eintrag weg und sieht nach, was danach passiert.
        /// </summary>
        [StaFact]
        public void EinFehlenderEintragWirdBeimZugriffErgaenzt() {
            TestSetup.Setup();
            // GetGebaeude braucht die Ruestort-Referenz, deshalb die Crossref vor der Karte -
            // dieselbe Reihenfolge wie in Main.StartInstance
            TestSetup.LoadCrossRef(false, false, erzwingen: true);
            TestSetup.LoadKarte(erzwingen: true);

            var gemark = SharedData.Map!.Values.First(g => g.Baupunkte > 0);
            Assert.True(SharedData.Gebäude!.TryRemove(gemark.Bezeichner, out var entfernt));
            try {
                var gebäude = BauwerkeView.GetGebäude(gemark);

                Assert.True(gebäude != null, $"Zu {gemark.Bezeichner} wurde kein Gebaeude ergaenzt");
                Assert.True(SharedData.Gebäude!.ContainsKey(gemark.Bezeichner),
                    "Das ergaenzte Gebaeude steht nicht in der Bauwerkliste");
                Assert.Equal(gemark.gf, gebäude!.gf);
                Assert.Equal(gemark.kf, gebäude.kf);
            }
            finally {
                // den geladenen Stand wiederherstellen, die Sammlung ist statisch
                if (entfernt != null)
                    SharedData.Gebäude![gemark.Bezeichner] = entfernt;
            }
        }

        /// <summary>
        /// Ein zweiter Aufruf der Reparatur darf nichts mehr aendern - sonst haengt das Ergebnis
        /// weiterhin davon ab, wie oft und wann sie laeuft.
        /// </summary>
        [StaFact]
        public void DieReparaturAendertBeimZweitenMalNichtsMehr() {
            TestSetup.Setup();
            TestSetup.LoadKarte(erzwingen: true);

            int vorher = SharedData.Gebäude!.Count;
            foreach (var gemark in SharedData.Map!.Values.Where(g => g.Baupunkte > 0))
                BauwerkeView.ErgänzeFehlendesGebäude(gemark);

            Assert.Equal(vorher, SharedData.Gebäude!.Count);
        }
    }
}
