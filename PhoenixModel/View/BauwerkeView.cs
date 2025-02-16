using PhoenixModel.dbCrossRef;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ViewModel;

namespace PhoenixModel.View {
    /// <summary>
    /// Statische Klasse zur Verarbeitung und Abfrage von Bauwerken.
    /// </summary>
    public static class BauwerkeView {
        /// <summary>
        /// Gibt die Rüstort-Referenz für eine gegebene Nummer zurück.
        /// </summary>
        /// <param name="nummer">Die Nummer des Rüstorts.</param>
        /// <returns>Der zugehörige Rüstort oder null, falls nicht gefunden.</returns>
        public static Rüstort? GetRuestortReferenz(int? nummer) {
            if (nummer == null || nummer < 1 || SharedData.RüstortReferenz == null)
                return null;
            return SharedData.RüstortReferenz.ElementAt(nummer.Value - 1);
        }

        /// <summary>
        /// Gibt einen Rüstort basierend auf der Anzahl der Baupunkte zurück.
        /// </summary>
        /// <param name="baupunkte">Die Anzahl der Baupunkte.</param>
        /// <returns>Der entsprechende Rüstort oder null, falls nicht gefunden.</returns>
        public static Rüstort? GetRuestortNachBaupunkten(int baupunkte) {
            if (baupunkte < 1 || SharedData.RüstortReferenz == null)
                return null;

            Rüstort? rnbp = null;
            if (Rüstort.NachBaupunkten.ContainsKey(baupunkte))
                rnbp = Rüstort.NachBaupunkten[baupunkte];
            else {
                int bp = baupunkte - baupunkte % 250;
                while (Rüstort.NachBaupunkten.ContainsKey(bp) == false && bp > 0)
                    bp -= 250;

                if (bp > 0) {
                    rnbp = Rüstort.NachBaupunkten[bp];
                }
            }
            return rnbp;
        }

        /// <summary>
        /// Gibt eine Liste von Gebäuden einer bestimmten Nation zurück.
        /// </summary>
        /// <param name="nation">Die Nation, für die die Gebäude gesucht werden.</param>
        /// <returns>Eine Liste der Gebäude oder null, falls keine gefunden wurden.</returns>
        public static List<Gebäude>? GetGebäude(Nation nation) {
            if (SharedData.Gebäude != null)
                return SharedData.Gebäude.Values?.Where(s => s.Nation == nation).ToList();
            return null;
        }

        /// <summary>
        /// Ermittelt die Anzahl der Baupunkte für eine bestimmte Kleinfeld-Position.
        /// </summary>
        /// <param name="pos">Die Position auf der Karte.</param>
        /// <returns>Die Anzahl der Baupunkte oder null, falls nicht vorhanden.</returns>
        public static int? GetBaupunkteNachKarte(KleinfeldPosition pos) {
            if (SharedData.Map == null)
                return null;
            try {
                return SharedData.Map[pos.CreateBezeichner()].Baupunkte;
            }
            catch (Exception ex) {
                ProgramView.LogError(pos, "Kleinfeld existiert nicht", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gibt den Rüstort basierend auf einer Kartenposition zurück.
        /// </summary>
        /// <param name="pos">Die Position auf der Karte.</param>
        /// <returns>Der entsprechende Rüstort oder null, falls nicht gefunden.</returns>
        public static Rüstort? GetRüstortNachKarte(KleinfeldPosition pos) {
            if (SharedData.Map == null)
                return null;

            var gemark = SharedData.Map[pos.CreateBezeichner()];
            Rüstort? rüstortLautKarte = BauwerkeView.GetRuestortReferenz(gemark.Ruestort);

            if (rüstortLautKarte != null)
                return rüstortLautKarte;

            if (Rüstort.NachBaupunkten.ContainsKey(gemark.Baupunkte))
                return Rüstort.NachBaupunkten[gemark.Baupunkte];
            else {
                int bp = gemark.Baupunkte - gemark.Baupunkte % 250;
                while (Rüstort.NachBaupunkten.ContainsKey(bp) == false && bp > 0)
                    bp -= 250;

                if (bp > 0) {
                    return Rüstort.NachBaupunkten[bp];
                }
                return null;
            }
        }

        /// <summary>
        /// Ermittelt das Gebäude für eine bestimmte Kartenposition.
        /// </summary>
        /// <param name="gemark">Die Position auf der Karte.</param>
        /// <returns>Das Gebäude oder null, falls nicht vorhanden.</returns>
        public static Gebäude? GetGebäude(KleinFeld gemark) {
            if (SharedData.Gebäude == null)
                throw new Exception("Die Bauwerkliste muss vor den Kartendaten geladen werden.");
            if (SharedData.RüstortReferenz == null)
                throw new Exception("Die Rüstort-Referenzdaten müssen vor den Kartendaten geladen werden.");
            if (gemark.Baupunkte == 0)
                return null;

            try {
                Gebäude? gebäude = null;

                if (SharedData.Gebäude.ContainsKey(gemark.Bezeichner))
                    gebäude = SharedData.Gebäude[gemark.Bezeichner];

                if (gebäude == null) {
                    ProgramView.LogError(gemark, $"Fehlendes Gebäude in der Bauwerktabelle mit dem Namen {gemark.Bauwerknamen}",
                        "Durch einen Datenbankfehler hat das Gebäude keinen Eintrag in der Tabelle [bauwerkliste] in der Datenbank Ekrenfarakarte.mdb");
                }
                return gebäude;
            }
            catch (Exception ex) {
                throw new Exception($"Ausnahme bei der Festlegung des Gebäudes auf Kleinfeld {gemark.Bezeichner}", ex);
            }
        }
    }
}
