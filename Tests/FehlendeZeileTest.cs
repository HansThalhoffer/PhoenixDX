using PhoenixModel.Commands;
using PhoenixModel.dbPZE;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Was passiert, wenn eine Zeile fehlt, die der Befehl erwartet.
    ///
    /// An fuenf Stellen stand dafuer .First() - viermal sogar mit einer Null-Pruefung darunter, die
    /// gar nicht greifen kann, weil First() wirft statt null zu liefern. Aus der Oberflaeche heraus
    /// endet so eine Ausnahme die Anwendung; gemeldet wurde genau das beim Zuruecknehmen gebauter
    /// Mauern:
    ///
    ///     System.InvalidOperationException: Sequence contains no elements
    ///        at System.Linq.Enumerable.First[TSource](IEnumerable`1 source)
    ///        at PhoenixWPF.Pages.CommandPage.Commands_CollectionChanged(...)
    ///
    /// Zwei der fuenf Stellen liegen in der Oberflaeche - dort war der Absturz, und dort laesst er
    /// sich hier nicht pruefen.
    ///
    /// Die drei im Diplomatiebefehl sind dagegen gar nicht erreichbar: CheckPreconditions fragt
    /// vorher mit Any() nach, ob es die Zeile gibt, und jeder Weg dorthin - ExecuteCommand,
    /// UndoCommand, CanUndo - geht durch diese Pruefung. Das First() dahinter war also eine Falle,
    /// die niemand zuschnappen sah; entschaerft ist sie trotzdem besser. Geprueft wird hier die
    /// Absicherung, die wirklich greift.
    /// </summary>
    public class FehlendeZeileTest {

        private static void LadeAlles() {
            TestSetup.LadeMitDiplomatie();
            TestSetup.SetzePhase(Zugphase.Bewegungsphase);
        }

        /// <summary>
        /// Sucht ein Reichspaar, zu dem die Diplomatietabelle keine Zeile fuehrt.
        ///
        /// Die Tabelle fuehrt eine Zeile je Paar, aber nicht jedes denkbare Paar kommt darin vor.
        /// </summary>
        private static (Nation Geber, Nation Nehmer) FindeFehlendesPaar() {
            var nationen = SharedData.Nationen!.ToList();
            foreach (var geber in nationen) {
                foreach (var nehmer in nationen) {
                    bool gibtEs = SharedData.Diplomatiechange!.Any(
                        d => d.ReferenzNation == geber && d.Nation == nehmer);
                    if (gibtEs == false)
                        return (geber, nehmer);
                }
            }
            Assert.Fail("Die Diplomatietabelle fuehrt jedes denkbare Reichspaar");
            return default;
        }

        /// <summary>
        /// Ohne Zeile antwortet der Befehl - und zwar schon in der Vorbedingung.
        /// </summary>
        [StaFact]
        public void OhneDiplomatiezeileAntwortetDerBefehlStattZuWerfen() {
            LadeAlles();
            Assert.NotNull(SharedData.Diplomatiechange);
            var (geber, nehmer) = FindeFehlendesPaar();

            var befehl = new DiplomacyCommand("Gebe Niemand Wegerecht") {
                ReferenzNation = geber,
                Nation = nehmer,
                Recht = DiplomacyCommand.BewegungsRecht.Wegerecht,
                // ohne RemoveRecht schlaegt schon die Vorbedingung fehl, und der Test waere
                // aus dem falschen Grund gruen
                RemoveRecht = false,
            };

            // Hier greift die Pruefung, nicht erst der Zugriff auf die Zeile
            var geprueft = befehl.CheckPreconditions();
            Assert.True(geprueft.HasErrors);
            Assert.Contains("Diplomatiechange", geprueft.Title);

            Assert.True(befehl.ExecuteCommand().HasErrors);
            Assert.True(befehl.UndoCommand().HasErrors);
            Assert.False(befehl.CanUndo);
        }

        /// <summary>
        /// Und der Regelfall geht weiterhin: mit Zeile wird das Recht gesetzt.
        /// </summary>
        [StaFact]
        public void MitDiplomatiezeileLaeuftDerBefehlDurch() {
            LadeAlles();
            var zeile = SharedData.Diplomatiechange!.FirstOrDefault();
            Assert.True(zeile != null, "Die Zugdaten fuehren keine Diplomatieaenderungen");

            int wegerechtVorher = zeile!.Wegerecht;
            try {
                var befehl = new DiplomacyCommand("Gebe Wegerecht") {
                    ReferenzNation = zeile.ReferenzNation,
                    Nation = zeile.Nation,
                    Recht = DiplomacyCommand.BewegungsRecht.Wegerecht,
                    RemoveRecht = false,
                };

                var ausgefuehrt = befehl.ExecuteCommand();
                Assert.False(ausgefuehrt.HasErrors, $"{ausgefuehrt.Title}: {ausgefuehrt.Message}");
                Assert.Equal(1, zeile.Wegerecht);
            }
            finally {
                zeile.Wegerecht = wegerechtVorher;
            }
        }
    }
}
