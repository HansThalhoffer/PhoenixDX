using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {
    /// <summary>
    /// Statische Klasse zur Berechnung von Einnahmen aus verschiedenen Quellen.
    /// </summary>
    public static class EinnahmenView {
        /// <summary>
        /// Berechnet die Einnahmen basierend auf dem Terrain eines bestimmten Kleinfelds.
        /// </summary>
        /// <param name="gem">Das Kleinfeld, für das die Einnahmen berechnet werden.</param>
        /// <returns>Die Einnahmen aus dem Terrain.</returns>
        public static int GetTerrainEinnahmen(KleinFeld gem) {
            if (gem.Terrain != null) {
                return gem.Terrain.Einnahmen;
            }
            return 0;
        }

        /// <summary>
        /// Berechnet die Einnahmen aus einem Gebäude, das sich auf einem bestimmten Kleinfeld befindet.
        /// </summary>
        /// <param name="gem">Das Kleinfeld mit dem Gebäude.</param>
        /// <returns>Die Einnahmen aus dem Gebäude.</returns>
        public static int GetGebäudeEinnahmen(KleinFeld gem) {
            Rüstort? gebäude = BauwerkeView.GetRüstortNachKarte(gem);
            if (gebäude != null) {
                return GetGebäudeEinnahmen(gebäude);
            }
            return 0;
        }

        /// <summary>
        /// Berechnet die Einnahmen aus einem spezifischen Bauwerk.
        /// </summary>
        /// <param name="bauwerk">Das Bauwerk, für das die Einnahmen berechnet werden.</param>
        /// <returns>Die Einnahmen des Bauwerks.</returns>
        public static int GetGebäudeEinnahmen(BauwerkBasis bauwerk) {

            EinwohnerUndEinnahmenTabelle.Werte einwohnerUndEinnahmenTabelle;
            if (EinwohnerUndEinnahmenTabelle.EinwohnerUndEinnahmen.TryGetValue(bauwerk.Bauwerk, out einwohnerUndEinnahmenTabelle))
                return einwohnerUndEinnahmenTabelle.Einnahmen;

            ProgramView.LogError($"Das Gebäude {bauwerk.Bauwerk} hat keine Einnahmen in der EinnahmenView-Tabelle",
                "Durch einen Datenbankfehler hat das Gebäude keinen Eintrag in der Einnahmentabelle");
            return 0;
        }

        /// <summary>
        /// Berechnet die gesamten Einnahmen eines Kleinfelds, einschließlich Terrain- und Gebäude-Einnahmen.
        /// </summary>
        /// <param name="gem">Das Kleinfeld, für das die Einnahmen berechnet werden.</param>
        /// <returns>Die Gesamteinnahmen des Kleinfelds.</returns>
        public static int GetGesamtEinnahmen(KleinFeld gem) {
            return GetTerrainEinnahmen(gem) + GetGebäudeEinnahmen(gem);
        }

        /// <summary>
        /// Berechnet die gesamten Einnahmen eines Reichs, indem die Einnahmen aller zugehörigen Kleinfelder summiert werden.
        /// </summary>
        /// <param name="reich">Die Nation, für die die Einnahmen berechnet werden.</param>
        /// <returns>Die gesamten Einnahmen des Reichs.</returns>
        public static int GetReichEinnahmen(Nation reich) {
            int summe = 0;
            if (SharedData.Map != null) {
                foreach (var gemark in SharedData.Map.Values.Where(gem => gem.Nation == reich)) {
                    summe += GetGesamtEinnahmen(gemark);
                }
            }
            return summe;
        }
    }
}
