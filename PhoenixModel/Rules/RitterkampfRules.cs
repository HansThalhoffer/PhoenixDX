using PhoenixModel.dbErkenfara;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace PhoenixModel.Rules {

    /// <summary>
    /// Der Ritterkampf (Regelwerk 5.2).
    ///
    /// "Der Ritterkampf ist der Charakterkampf der Ritter des Ritterordens. Befinden sich in den
    /// gegnerischen Parteien Charaktere des Ritterordens, so führen diese einen Ritterkampf nach
    /// den Regeln des Charakterkampfes durch, der anstatt eines Charakterkampfes und
    /// Rückzugsgefechtes durchgeführt wird."
    ///
    /// Gerechnet wird er wie eine Runde Charakterkampf (<see cref="CharakterkampfRules"/>); eigen
    /// sind ihm nur die Folgen:
    ///
    /// * Er ersetzt für die beteiligten Ritter den Charakterkampf - "und können am Charterkampf
    ///   der anderen nicht mehr teilnehmen" - und das Rückzugsgefecht (5.4).
    /// * "Ritter bringen nur den Rest ihrer nach dem Ritterkampf verbliebenen Gutpunkte in den
    ///   Nahkampf ein."
    /// * "Ungeachtet des Ausgangs der Schlacht kann der am Ende unterlegene Ritter ohne
    ///   Rückzugsgefecht in ein benachbartes Gemark abziehen." Auch wenn seine Seite die Schlacht
    ///   gewinnt, darf er gehen.
    /// * "Nach einem freien Rückzug können Ritter im kommenden Zug ebenfalls nicht mehr
    ///   angreifen." (5.4)
    ///
    /// Die Reihenfolge des Kampfes steht in 5.5: "Dann wird der Fernkampf, anschließend der
    /// ( ggf. Ritterkampf, dann ) Charakterkampf und dann der Nahkampf ausgewürfelt und
    /// berechnet." Der Ritterkampf liegt also zwischen Beschuss und Charakterkampf.
    ///
    /// Wer ein Ritter ist, sagen die Daten nicht: unter den Reichen gibt es keinen Ritterorden,
    /// und die Zugdaten führen bei einem Charakter kein Feld für einen Orden. Deshalb bekommt
    /// jede Suche hier die Ritter oder eine Erkennung übergeben, statt sie zu raten. Die Frage
    /// steht in Offene-Regelfragen.md.
    /// </summary>
    public static class RitterkampfRules {

        /// <summary>
        /// Die Ritter, die auf einer Gemark gegeneinander antreten.
        /// </summary>
        /// <param name="Gemark">wo</param>
        /// <param name="Ritter">alle beteiligten Ritter, beider Seiten</param>
        public record class Ritterkampf(KleinfeldPosition Gemark, IReadOnlyList<NamensSpielfigur> Ritter) {
            public override string ToString()
                => $"{Gemark.CreateBezeichner()}: {string.Join(", ", Ritter.Select(r => r.Bezeichner))}";
        }

        /// <summary>
        /// Sucht die Ritterkämpfe.
        ///
        /// "Befinden sich in den gegnerischen Parteien Charaktere des Ritterordens, so führen
        /// diese einen Ritterkampf durch" - es braucht also auf beiden Seiten einen. Ein Ritter
        /// allein auf weiter Flur kämpft nicht gegen sich selbst.
        /// </summary>
        /// <param name="figuren">die Figuren aller Reiche</param>
        /// <param name="istRitter">
        /// woran ein Ritter des Ritterordens zu erkennen ist - die Daten sagen es nicht von selbst
        /// </param>
        public static List<Ritterkampf> FindeRitterkämpfe(IEnumerable<Spielfigur>? figuren,
                Func<NamensSpielfigur, bool>? istRitter) {
            List<Ritterkampf> ergebnis = [];
            if (figuren == null || istRitter == null)
                return ergebnis;

            var ritter = figuren
                .OfType<NamensSpielfigur>()
                .Where(figur => figur.Nation != null && Plausibilität.IsValid(figur) && istRitter(figur))
                .ToList();

            foreach (var gruppe in ritter.GroupBy(figur => figur.Key)) {
                var aufDerGemark = gruppe.ToList();
                var gemark = KleinfeldView.GetKleinfeld(aufDerGemark[0]);

                var beteiligte = aufDerGemark
                    .Where(einer => aufDerGemark.Any(anderer
                        => DiplomatieRules.SindVerfeindet(einer.Nation, anderer.Nation, gemark)))
                    .ToList();
                if (beteiligte.Count < 2)
                    continue;

                ergebnis.Add(new Ritterkampf(
                    new KleinfeldPosition(aufDerGemark[0].gf, aufDerGemark[0].kf), beteiligte));
            }
            return [.. ergebnis.OrderBy(k => k.Gemark.gf).ThenBy(k => k.Gemark.kf)];
        }

        /// <summary>
        /// Nimmt dieser Charakter noch am Charakterkampf der anderen teil?
        ///
        /// "Das heißt, Ritter ... können am Charterkampf der anderen nicht mehr teilnehmen."
        /// Wer keinen Ritterkampf geführt hat, ist davon nicht betroffen - für ihn gelten die
        /// gewöhnlichen Regeln aus 5.3.
        /// </summary>
        public static bool NimmtAmCharakterkampfTeil(NamensSpielfigur? figur, bool hatRitterkampfGeführt)
            => hatRitterkampfGeführt == false && CharakterkampfRules.NimmtAmCharakterkampfTeil(figur);

        /// <summary>
        /// Die Gutpunkte, die ein Ritter in den Nahkampf einbringt.
        ///
        /// "Ritter bringen nur den Rest ihrer nach dem Ritterkampf verbliebenen Gutpunkte in den
        /// Nahkampf ein." Der Ritterkampf liegt davor, also zählt, was danach übrig ist - nicht
        /// der Wert, mit dem der Ritter in den Monat gegangen ist.
        /// </summary>
        public static int GetGutpunkteFürNahkampf(int gutpunkteNachRitterkampf)
            => Math.Max(0, gutpunkteNachRitterkampf);

        /// <summary>
        /// Darf dieser Ritter ohne Rückzugsgefecht abziehen?
        ///
        /// "Ungeachtet des Ausgangs der Schlacht kann der am Ende unterlegene Ritter ohne
        /// Rückzugsgefecht in ein benachbartes Gemark abziehen." Es kommt also allein darauf an,
        /// wie der Ritterkampf ausging, nicht wie die Schlacht ausging - ein Ritter darf selbst
        /// dann gehen, wenn seine Seite gewinnt.
        /// </summary>
        /// <param name="vorsprungImRitterkampf">seine Treffer minus die des Gegners</param>
        public static bool DarfAbziehen(int vorsprungImRitterkampf) => vorsprungImRitterkampf < 0;

        /// <summary>
        /// Die Gemarken, in die ein unterlegener Ritter abziehen darf.
        ///
        /// Das Regelwerk sagt hier nur "in ein benachbartes Gemark". Es gelten dieselben Felder
        /// wie beim Rückzugsgefecht: ein Abzug auf ein von Gegnern besetztes Feld wäre kein Abzug,
        /// sondern der nächste Kampf.
        /// </summary>
        public static List<KleinFeld> FindeAbzugsfelder(NamensSpielfigur? ritter,
                IEnumerable<Spielfigur>? fremdeFiguren = null)
            => RückzugsRules.FindeRückzugsfelder(ritter, fremdeFiguren);

        /// <summary>
        /// "Nach einem freien Rückzug können Ritter im kommenden Zug ebenfalls nicht mehr
        /// angreifen." (Regelwerk 5.4)
        ///
        /// Dieselbe Sperre wie beim Rückzugsgefecht.
        /// </summary>
        public static bool DarfAngreifen(int zugDesAbzugs, int aktuellerZug)
            => RückzugsRules.DarfAngreifen(zugDesAbzugs, aktuellerZug);
    }
}
