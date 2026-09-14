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
        /// fügt eine Baustelle 
        /// </summary>
        /// <param name="kf"></param>
        public static void AddBaustelle(KleinFeld kf) {
            if (kf != null && kf.Nation != null && SharedData.Gebäude != null) {
                kf.Baupunkte = -1;
                var gebäude = new Gebäude() { Bauwerknamen = "Baustelle", gf = kf.gf, kf = kf.kf, Reich = kf.Nation.DBname, IsNew = true  };
                SharedData.Gebäude.Add(gebäude.CreateBezeichner(), gebäude);
            }
        }


        /// <summary>
        /// Gibt die Rüstort-Referenz für eine gegebene Nummer zurück.
        /// </summary>
        /// <param name="nummer">Die Nummer des Rüstorts.</param>
        /// <returns>Der zugehörige Rüstort oder null, falls nicht gefunden.</returns>
        public static Rüstort? GetRuestortReferenz(int? nummer) {
            if (nummer == null || nummer < 1 || SharedData.RüstortReferenz == null)
                return null;
            // Gesucht wird über die Nummer, nicht über die Position in der Sammlung.
            //
            // Vorher stand hier ElementAt(nummer - 1). Das setzt voraus, dass die Referenztabelle
            // lückenlos bei 1 beginnt und genau in dieser Reihenfolge geladen wurde. Stimmt das
            // nicht, kam die falsche Referenz heraus - und sobald die Nummer grösser war als die
            // Zeilenzahl, eine Ausnahme mitten in der Anzeige eines Feldes.
            return SharedData.RüstortReferenz.FirstOrDefault(rüstort => rüstort.Nummer == nummer.Value);
        }

        /// <summary>
        /// Gibt einen Rüstort basierend auf der Anzahl der Baupunkte zurück.
        /// </summary>
        /// <param name="baupunkte">Die Anzahl der Baupunkte.</param>
        /// <returns>Der entsprechende Rüstort oder null, falls nicht gefunden.</returns>
        public static Rüstort? GetRuestortNachBaupunkten(int baupunkte) {
            if (baupunkte == -1 || SharedData.RüstortReferenz == null) {
                return Rüstort.NachBaupunkten[baupunkte];
            }

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

                return gebäude ?? ErgänzeFehlendesGebäude(gemark);
            }
            catch (Exception ex) {
                throw new Exception($"Ausnahme bei der Festlegung des Gebäudes auf Kleinfeld {gemark.Bezeichner}", ex);
            }
        }

        /// <summary>
        /// Die ergänzten Bauwerke, die noch auf ihr Reich warten.
        ///
        /// Beim Laden der Karte sind die Nationen noch nicht da (Main lädt CrossRef, Karte, PZE),
        /// und ohne Reich darf der Eintrag nicht in die Datenbank - die Tabelle führt genau vier
        /// Spalten. Sie sammeln sich deshalb hier, bis alles geladen ist.
        /// </summary>
        private static readonly List<Gebäude> _wartenAufReich = [];

        /// <summary>
        /// Wie viele ergänzte Bauwerke noch auf ihr Reich warten
        /// </summary>
        public static int AnzahlWartenderBauwerke { get { lock (_wartenAufReich) return _wartenAufReich.Count; } }

        /// <summary>
        /// Trägt bei den wartenden Bauwerken das Reich nach und stellt sie zum Schreiben ein.
        ///
        /// Aufzurufen, sobald die Nationen geladen sind. Was dann immer noch kein Reich hat, bleibt
        /// liegen: die Karte nennt für diese Gemark keines, und ein unvollständiger Eintrag gehört
        /// nicht in die Datenbank.
        /// </summary>
        /// <returns>die eingestellten Bauwerke und die Bezeichner derer ohne Reich</returns>
        public static (List<Gebäude> Eingestellt, List<string> OhneReich) SchreibeWartendeBauwerke() {
            List<Gebäude> wartende;
            lock (_wartenAufReich) {
                wartende = [.. _wartenAufReich];
                _wartenAufReich.Clear();
            }

            // Wird die Karte erneut geladen, entsteht die Bauwerkliste neu. Die alten Objekte
            // stehen dann nicht mehr darin, und sie zu schreiben brächte nichts - maßgeblich ist,
            // was jetzt in der Liste steht. Das hält die Warteliste auch über mehrere Ladevorgänge
            // frei von Karteileichen.
            wartende = [.. wartende.Where(gebäude =>
                SharedData.Gebäude != null
                && SharedData.Gebäude.TryGetValue(gebäude.Bezeichner, out var aktuell)
                && ReferenceEquals(aktuell, gebäude))];

            var (vollständig, ohneReich) = VervollständigeReiche(wartende);
            foreach (var gebäude in vollständig)
                SharedData.StoreQueue.Insert(gebäude);
            return (vollständig, ohneReich);
        }

        /// <summary>
        /// Trägt bei nachgetragenen Bauwerken das Reich aus der Karte nach.
        ///
        /// Beim Laden der Karte ist das Reich noch nicht zu ermitteln: KleinFeld.Nation braucht die
        /// Nationen aus der PZE, und die sind zu diesem Zeitpunkt nicht geladen. Ein Eintrag ohne
        /// Reich wäre in der Tabelle [bauwerkliste] unvollständig - die führt genau vier Spalten -
        /// und darf so nicht in die Datenbank. Deshalb wird das Reich nachgereicht, sobald alles
        /// geladen ist, und erst dann geschrieben.
        ///
        /// Die Spalte Reich führt den Reichsnamen, so wie ihn auch die vorhandenen Einträge tragen.
        /// </summary>
        /// <returns>
        /// die Bauwerke, die damit vollständig sind und geschrieben werden können, und die
        /// Bezeichner derer, zu denen die Karte kein Reich nennt
        /// </returns>
        public static (List<Gebäude> Vollständig, List<string> OhneReich) VervollständigeReiche(IEnumerable<Gebäude>? nachgetragene) {
            List<Gebäude> vollständig = [];
            List<string> ohneReich = [];
            if (nachgetragene == null || SharedData.Map == null)
                return (vollständig, ohneReich);

            foreach (var gebäude in nachgetragene) {
                if (SharedData.Map.TryGetValue(gebäude.Bezeichner, out var gemark) == false)
                    continue;
                if (gemark.Nation != null)
                    gebäude.Reich = gemark.Nation.Reich;
                if (string.IsNullOrEmpty(gebäude.Reich))
                    ohneReich.Add(gebäude.Bezeichner);
                else
                    vollständig.Add(gebäude);
            }
            return (vollständig, ohneReich);
        }

        /// <summary>
        /// Legt einen fehlenden Eintrag in der Bauwerkliste an.
        ///
        /// In der Karte steht ein Gebäude, in der Tabelle [bauwerkliste] der Erkenfarakarte.mdb
        /// fehlt es - die Karte ist die gepflegte Tabelle und gibt den Ausschlag. Der Eintrag wird
        /// deshalb ergänzt statt den Fehler nur zu melden.
        ///
        /// Wer den Mangel zuerst bemerkt, behebt ihn: die Reparatur beim Laden der Karte oder der
        /// erste Zugriff auf das Gemark. Vorher hing es vom Zufall ab, wer zuerst an das Gemark kam,
        /// und die Meldung erschien beim Start mal als Warnung, mal als Fehler, mal gar nicht.
        /// </summary>
        /// <returns>das ergänzte Gebäude, oder das bereits vorhandene, wenn ein anderer schneller war</returns>
        public static Gebäude? ErgänzeFehlendesGebäude(KleinFeld gemark, bool stillschweigend = false) {
            if (SharedData.Gebäude == null)
                return null;

            var gebäude = new Gebäude {
                gf = gemark.gf,
                kf = gemark.kf,
                Bauwerknamen = gemark.Bauwerknamen,
                // Das Reich stand bisher nicht im ergänzten Eintrag. Für die Anzeige genügte das,
                // für einen Eintrag in der Datenbank nicht - die Tabelle führt genau vier Spalten.
                Reich = gemark.Nation?.DBname ?? string.Empty,
            };

            if (SharedData.Gebäude.TryAdd(gebäude.Bezeichner, gebäude) == false)
                return SharedData.Gebäude[gebäude.Bezeichner];

            // Der Eintrag soll nicht nur diese Sitzung überleben, sondern in die Datenbank. Steht
            // das Reich schon fest, kann er sofort eingestellt werden; sonst wartet er darauf, dass
            // die Nationen geladen sind. Fehlte das hier, ergänzte der Einzelfall den Eintrag nur
            // im Speicher - und beim nächsten Start stünde dieselbe Meldung wieder da.
            bool wirdGeschrieben = string.IsNullOrEmpty(gebäude.Reich) == false;
            if (wirdGeschrieben)
                SharedData.StoreQueue.Insert(gebäude);
            else
                lock (_wartenAufReich) _wartenAufReich.Add(gebäude);

            // Die Sammelreparatur beim Laden meldet selbst, und zwar einmal statt einundzwanzigmal.
            // Hier meldet nur der Einzelfall, der beim Zugriff auffällt - der ist selten und
            // deshalb eine Meldung wert.
            if (stillschweigend == false)
                ProgramView.LogWarning(gemark, $"Fehlendes Gebäude in der Bauwerktabelle mit dem Namen {gemark.Bauwerknamen}",
                    $"In der Karte steht auf {gemark.Bezeichner} ein Gebäude, in der Tabelle [bauwerkliste] der "
                    + "Erkenfarakarte.mdb fehlt der Eintrag.\r\rDer Eintrag wurde ergänzt und "
                    + (wirdGeschrieben
                        ? "wird in die Datenbank geschrieben."
                        : "wird geschrieben, sobald die Reiche geladen sind."));
            return gebäude;
        }
    }
}
