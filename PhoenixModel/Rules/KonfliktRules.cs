using PhoenixModel.dbPZE;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Wo kommt es in diesem Monat zum Kampf? (Regelwerk 5)
    ///
    /// "Im Spielverlauf können Konflikte entstehen, die durch Waffengewalt auf der Karte
    /// ausgetragen werden sollen. Die bereits gerüsteten Heere können dabei in fremdes
    /// Reichsgebiet eindringen oder im eigenen Reichsgebiet auf gegnerische Truppen stoßen - oder
    /// es kommt zur Seeschlacht."
    ///
    /// Die Suche braucht die Figuren aller Reiche und läuft deshalb über
    /// <see cref="Spielleitungsdaten"/>. Sie findet nur, was in den geladenen Zugdaten steht: ein
    /// Reich, dessen Zug noch nicht abgegeben ist, taucht mit seinem alten Stand auf.
    ///
    /// Gefunden wird die Begegnung, nicht der Ausgang. Wer gegen wen steht, entscheidet
    /// <see cref="DiplomatieRules"/>; was daraus wird, rechnen <see cref="FernkampfRules"/>,
    /// <see cref="KampfRules"/> und <see cref="BeuteRules"/>.
    /// </summary>
    public static class KonfliktRules {

        /// <summary>
        /// Um welche Art Kampf es sich handelt.
        /// </summary>
        public enum Kampfart {
            /// <summary>Heere treffen an Land aufeinander</summary>
            Nahkampf,
            /// <summary>Flotten treffen auf dem Wasser aufeinander (Regelwerk 5: "oder es kommt zur Seeschlacht")</summary>
            Seeschlacht,
            /// <summary>
            /// Nur Charaktere treffen aufeinander: "Zum Charakterkampf kommt es, wenn sich
            /// Charaktere verfeindeter Reiche in der selben Gemark befinden." (Regelwerk 5.3)
            /// </summary>
            Charakterkampf,
        }

        /// <summary>
        /// Ein Reich mit den Figuren, die es auf dieser Gemark stehen hat.
        /// </summary>
        public record class Partei(Nation Reich, IReadOnlyList<Spielfigur> Figuren) {
            /// <summary>Die Heeresstärke aller Truppen dieser Partei auf der Gemark</summary>
            public double Heeresstärke => Figuren
                .OfType<TruppenSpielfigur>()
                .Sum(truppe => KampfRules.BerechneHeeresstärke(truppe, truppe.isbanned));

            public override string ToString() => $"{Reich.Reich}: {Figuren.Count} Figuren";
        }

        /// <summary>
        /// Zwei Reiche, die auf derselben Gemark als Gegner stehen.
        /// </summary>
        public record class Gegnerschaft(Nation Eines, Nation Anderes) {
            public override string ToString() => $"{Eines.Reich} gegen {Anderes.Reich}";
        }

        /// <summary>
        /// Eine Gemark, auf der es zum Kampf kommt.
        /// </summary>
        /// <param name="Gemark">wo</param>
        /// <param name="Art">Nahkampf, Seeschlacht oder Charakterkampf</param>
        /// <param name="Parteien">die beteiligten Reiche mit ihren Figuren</param>
        /// <param name="Gegnerschaften">welche der Reiche gegeneinander stehen</param>
        public record class Konflikt(KleinfeldPosition Gemark, Kampfart Art,
                IReadOnlyList<Partei> Parteien, IReadOnlyList<Gegnerschaft> Gegnerschaften) {

            /// <summary>
            /// Eine Zeile für die Anzeige
            /// </summary>
            public string Beschreibung
                => $"{Gemark.CreateBezeichner()}: {Art} - {string.Join(", ", Gegnerschaften)}";
        }

        /// <summary>
        /// Alle Konflikte aus den Daten, die die Spielleitung geladen hat
        /// </summary>
        public static List<Konflikt> FindeKonflikte() => FindeKonflikte(Spielleitungsdaten.GetAlleFiguren());

        /// <summary>
        /// Alle Konflikte unter diesen Figuren.
        ///
        /// Sortiert nach Gemark, damit die Liste bei gleichem Datenstand gleich aussieht.
        /// </summary>
        public static List<Konflikt> FindeKonflikte(IEnumerable<Spielfigur>? figuren) {
            List<Konflikt> ergebnis = [];
            if (figuren == null)
                return ergebnis;

            Dictionary<int, List<Spielfigur>> nachGemark = [];
            foreach (var figur in figuren) {
                if (figur == null || figur.Nation == null || Plausibilität.IsValid(figur) == false)
                    continue;
                if (nachGemark.TryGetValue(figur.Key, out var besatzung) == false)
                    nachGemark[figur.Key] = besatzung = [];
                besatzung.Add(figur);
            }

            foreach (var besatzung in nachGemark.Values) {
                var konflikt = PrüfeGemark(besatzung);
                if (konflikt != null)
                    ergebnis.Add(konflikt);
            }
            return [.. ergebnis.OrderBy(k => k.Gemark.gf).ThenBy(k => k.Gemark.kf)];
        }

        /// <summary>
        /// Kommt es auf dieser Gemark zum Kampf?
        ///
        /// Geprüft wird jedes Reichspaar einzeln: auf einer Gemark können Reiche stehen, die
        /// miteinander verbündet und mit einem dritten verfeindet sind.
        /// </summary>
        /// <param name="besatzung">alle Figuren auf einer Gemark</param>
        /// <returns>der Konflikt, oder null wenn dort niemand gegeneinander steht</returns>
        public static Konflikt? PrüfeGemark(IReadOnlyList<Spielfigur>? besatzung) {
            if (besatzung == null || besatzung.Count < 2)
                return null;

            var gemark = KleinfeldView.GetKleinfeld(besatzung[0]);
            List<Nation> reiche = [];
            foreach (var figur in besatzung) {
                if (figur.Nation != null && reiche.Any(reich => reich.Equals(figur.Nation)) == false)
                    reiche.Add(figur.Nation);
            }
            if (reiche.Count < 2)
                return null;

            List<Gegnerschaft> gegnerschaften = [];
            for (int i = 0; i < reiche.Count; i++) {
                for (int j = i + 1; j < reiche.Count; j++) {
                    if (DiplomatieRules.SindVerfeindet(reiche[i], reiche[j], gemark))
                        gegnerschaften.Add(new Gegnerschaft(reiche[i], reiche[j]));
                }
            }
            if (gegnerschaften.Count == 0)
                return null;

            List<Partei> parteien = [];
            foreach (var reich in reiche) {
                bool beteiligt = gegnerschaften.Any(g => g.Eines.Equals(reich) || g.Anderes.Equals(reich));
                if (beteiligt == false)
                    continue;
                parteien.Add(new Partei(reich, [.. besatzung.Where(f => f.Nation != null && f.Nation.Equals(reich))]));
            }

            return new Konflikt(new KleinfeldPosition(besatzung[0].gf, besatzung[0].kf),
                BestimmeKampfart(parteien, gemark?.IsWasser ?? false), parteien, gegnerschaften);
        }

        /// <summary>
        /// Ein Zauberduell: zwei Zauberer verfeindeter Reiche in Reichweite zueinander.
        /// </summary>
        /// <param name="Eines">der eine Zauberer</param>
        /// <param name="Anderes">der andere</param>
        /// <param name="Entfernung">0, wenn sie auf derselben Gemark stehen, sonst 1</param>
        public record class Zauberduell(NamensSpielfigur Eines, NamensSpielfigur Anderes, int Entfernung) {
            public override string ToString()
                => $"{Eines.Bezeichner} gegen {Anderes.Bezeichner} ({Entfernung} Gemarken)";
        }

        /// <summary>
        /// Alle Zauberduelle aus den Daten, die die Spielleitung geladen hat
        /// </summary>
        public static List<Zauberduell> FindeZauberduelle() => FindeZauberduelle(Spielleitungsdaten.GetAlleFiguren());

        /// <summary>
        /// Sucht die Zauberduelle (Regelwerk 5.3).
        ///
        /// "Zum Zauberduell kommt es, wenn Zauberer von verfeindeten Reichen in der gleichen oder
        /// in benachbarten Gemarken stehen."
        ///
        /// Anders als der Charakterkampf reicht das über die Gemark hinaus - deshalb steht es
        /// neben <see cref="FindeKonflikte(IEnumerable{Spielfigur}?)"/> und nicht darin. Ob zwei
        /// Reiche verfeindet sind, hängt am Gelände; hier zählt, ob sie es auf einer der beiden
        /// Gemarken sind - wer sich über die Gemarkgrenze hinweg duelliert, steht nicht auf
        /// demselben Boden.
        ///
        /// Der dritte Fall des Regelwerks bleibt aussen vor: ein Zauberspruch, der auf eine Gemark
        /// mit einem Zauberer ausgesprochen wird oder dort wirkt. Das weiss die Zauberei, nicht
        /// die Karte.
        /// </summary>
        public static List<Zauberduell> FindeZauberduelle(IEnumerable<Spielfigur>? figuren) {
            List<Zauberduell> ergebnis = [];
            if (figuren == null)
                return ergebnis;

            var zauberer = figuren
                .OfType<NamensSpielfigur>()
                .Where(figur => figur.BaseTyp == ExternalTables.FigurType.Zauberer
                             && figur.Nation != null && Plausibilität.IsValid(figur))
                .ToList();

            for (int i = 0; i < zauberer.Count; i++) {
                for (int j = i + 1; j < zauberer.Count; j++) {
                    var eines = zauberer[i];
                    var anderes = zauberer[j];
                    if (eines.Nation!.Equals(anderes.Nation))
                        continue;

                    int entfernung = FernkampfRules.GetEntfernung(eines, anderes, 1);
                    if (entfernung < 0)
                        continue;

                    var hier = KleinfeldView.GetKleinfeld(eines);
                    var dort = KleinfeldView.GetKleinfeld(anderes);
                    if (DiplomatieRules.SindVerfeindet(eines.Nation, anderes.Nation, hier) == false
                     && DiplomatieRules.SindVerfeindet(eines.Nation, anderes.Nation, dort) == false)
                        continue;

                    ergebnis.Add(new Zauberduell(eines, anderes, entfernung));
                }
            }
            return ergebnis;
        }

        /// <summary>
        /// Welche Art Kampf hier stattfindet.
        ///
        /// Stehen sich nur Charaktere gegenüber, ist es ein Charakterkampf (Regelwerk 5.3); sonst
        /// entscheidet das Gelände zwischen Nahkampf und Seeschlacht.
        ///
        /// Nicht erfasst ist das Zauberduell: dazu kommt es auch, "wenn Zauberer von verfeindeten
        /// Reichen in der gleichen oder in benachbarten Gemarken stehen" (Regelwerk 5.3) - das
        /// reicht über die Gemark hinaus und ist ein eigener Schritt.
        /// </summary>
        private static Kampfart BestimmeKampfart(IReadOnlyList<Partei> parteien, bool wasser) {
            bool truppen = parteien.All(partei => partei.Figuren.OfType<TruppenSpielfigur>().Any());
            if (truppen == false)
                return Kampfart.Charakterkampf;
            return wasser ? Kampfart.Seeschlacht : Kampfart.Nahkampf;
        }
    }
}
