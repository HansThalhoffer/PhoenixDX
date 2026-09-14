using PhoenixModel.Commands;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;

namespace Tests {

    /// <summary>
    /// Die Kampfeinnahmen des Siegers (Regelwerk 5.6).
    ///
    /// "Die Kampfeinnahmen des Siegers bestehen aus 50 % der Summe aller im Nahkampf dieser
    /// Schlacht gefallenen gegnerischen Truppen und 25 % aller in diesem Nahkampf gefallenen
    /// eigenen Truppen."
    /// </summary>
    public class KampfeinnahmenTest {

        private static void LadeKosten() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
        }

        private static KampfRules.Verluste Gefallene(int krieger = 0, int reiter = 0, int hf = 0,
                int schiffe = 0, int lkp = 0, int skp = 0, int pferde = 0)
            => new() {
                Krieger = krieger, Reiter = reiter, Heerführer = hf,
                Schiffe = schiffe, LKP = lkp, SKP = skp, Pferde = pferde,
            };

        /// <summary>
        /// Bewertet wird zu Ruestkosten - dieselben Zahlen, die auch die Kampftabelle als
        /// Faktoren einsetzt (Zeilen 326 und 327).
        /// </summary>
        [StaFact]
        public void GefalleneWerdenZuRuestkostenBewertet() {
            LadeKosten();

            int krieger = KostenView.GetGSKosten(ConstructionElementType.K);
            int heerführer = KostenView.GetGSKosten(ConstructionElementType.HF);
            int schwer = KostenView.GetGSKosten(ConstructionElementType.SKP);
            Assert.True(krieger > 0 && heerführer > 0 && schwer > 0, "Die Kostentabelle ist leer");

            var gefallen = Gefallene(krieger: 1000, hf: 10, skp: 2);
            Assert.Equal(1000 * krieger + 10 * heerführer + 2 * schwer,
                         KampfeinnahmenRules.BerechneWert(gefallen));

            // jede Gattung zaehlt mit ihrem eigenen Preis
            Assert.Equal(KostenView.GetGSKosten(ConstructionElementType.R),
                         KampfeinnahmenRules.BerechneWert(Gefallene(reiter: 1)));
            Assert.Equal(KostenView.GetGSKosten(ConstructionElementType.P),
                         KampfeinnahmenRules.BerechneWert(Gefallene(pferde: 1)));
            Assert.Equal(KostenView.GetGSKosten(ConstructionElementType.S),
                         KampfeinnahmenRules.BerechneWert(Gefallene(schiffe: 1)));
            Assert.Equal(KostenView.GetGSKosten(ConstructionElementType.LKP),
                         KampfeinnahmenRules.BerechneWert(Gefallene(lkp: 1)));

            Assert.Equal(0, KampfeinnahmenRules.BerechneWert((KampfRules.Verluste?)null));
            Assert.Equal(0, KampfeinnahmenRules.BerechneWert((IEnumerable<KampfRules.Verluste>?)null));
        }

        /// <summary>
        /// Der Gegner zaehlt zur Haelfte, die eigenen Gefallenen zu einem Viertel.
        /// </summary>
        [StaFact]
        public void DieHaelfteVomGegnerUndEinViertelVomEigenen() {
            LadeKosten();
            int proKrieger = KostenView.GetGSKosten(ConstructionElementType.K);

            var gegner = Gefallene(krieger: 1000);
            var eigene = Gefallene(krieger: 1000);

            Assert.Equal(1000 * proKrieger / 2, KampfeinnahmenRules.Berechne([gegner], null));
            Assert.Equal(1000 * proKrieger / 4, KampfeinnahmenRules.Berechne(null, [eigene]));
            Assert.Equal(1000 * proKrieger / 2 + 1000 * proKrieger / 4,
                         KampfeinnahmenRules.Berechne([gegner], [eigene]));

            // die eigenen Gefallenen bringen halb soviel wie dieselbe Zahl beim Gegner
            Assert.Equal(0.5, (double)KampfeinnahmenRules.Berechne(null, [eigene])
                            / KampfeinnahmenRules.Berechne([gegner], null), 6);

            Assert.Equal(0, KampfeinnahmenRules.Berechne(null, null));
        }

        /// <summary>
        /// Die Einnahmen einer ganzen Schlacht: der Sieger bekommt sie, sonst niemand.
        /// </summary>
        [StaFact]
        public void NurDerSiegerBekommtKampfeinnahmen() {
            LadeKosten();

            var sieger = new NahkampfRules.Seite([
                new NahkampfRules.Kämpfer(new Krieger { staerke = 4000, Nummer = 1 }, 100)]);
            var verlierer = new NahkampfRules.Seite([
                new NahkampfRules.Kämpfer(new Krieger { staerke = 1000, hf = 10, Nummer = 2 })]);

            var schlacht = NahkampfRules.WerteNahkampfAus(sieger, verlierer);
            Assert.Equal(NahkampfRules.Ausgang.Angreifer, schlacht.Sieger);

            int einnahmen = KampfeinnahmenRules.Berechne(schlacht);

            // der Verlierer ist aufgerieben, also zaehlt sein ganzes Heer zur Haelfte, dazu ein
            // Viertel der eigenen Gefallenen
            int erwartet = KampfeinnahmenRules.Berechne(
                schlacht.Verteidiger.Select(a => a.Verluste),
                schlacht.Angreifer.Select(a => a.Verluste));
            Assert.Equal(erwartet, einnahmen);
            Assert.True(einnahmen > 0);

            // ohne Sieger gibt es nichts
            var gleichstand = NahkampfRules.WerteNahkampfAus(
                new NahkampfRules.Seite([new NahkampfRules.Kämpfer(new Krieger { staerke = 1000, Nummer = 1 })]),
                new NahkampfRules.Seite([new NahkampfRules.Kämpfer(new Krieger { staerke = 1000, Nummer = 2 })]));
            Assert.Equal(NahkampfRules.Ausgang.Unentschieden, gleichstand.Sieger);
            Assert.Equal(0, KampfeinnahmenRules.Berechne(gleichstand));

            Assert.Equal(0, KampfeinnahmenRules.Berechne((NahkampfRules.Schlachtergebnis?)null));
        }

        /// <summary>
        /// "Fuer im Fernkampf vernichtete Ruestgueter gibt es keine Kampfeinnahmen!"
        /// (Regelwerk 5.1)
        ///
        /// Wer vor dem Nahkampf unter Beschuss faellt, bringt nichts ein: in den Nahkampf geht
        /// nur, was uebrig ist, und nur daraus entstehen Einnahmen.
        /// </summary>
        [StaFact]
        public void ImFernkampfGefalleneBringenNichtsEin() {
            LadeKosten();

            var angreifer = new NahkampfRules.Seite([
                new NahkampfRules.Kämpfer(new Krieger { staerke = 4000, Nummer = 1 }, 100)]);

            // derselbe Verteidiger, einmal unversehrt und einmal nach dem Beschuss
            var unversehrt = new NahkampfRules.Seite([
                new NahkampfRules.Kämpfer(new Krieger { staerke = 1000, Nummer = 2 })]);
            var beschossen = new NahkampfRules.Seite([
                new NahkampfRules.Kämpfer(new Krieger { staerke = 600, Nummer = 2 })]);

            int ohneBeschuss = KampfeinnahmenRules.Berechne(NahkampfRules.WerteNahkampfAus(angreifer, unversehrt));
            int mitBeschuss = KampfeinnahmenRules.Berechne(NahkampfRules.WerteNahkampfAus(angreifer, beschossen));

            Assert.True(mitBeschuss < ohneBeschuss,
                "Die im Beschuss gefallenen Truppen haben trotzdem Kampfeinnahmen gebracht");

            // und zwar genau um den Wert der 400 Mann, die den Nahkampf nicht mehr erlebt haben
            int proKrieger = KostenView.GetGSKosten(ConstructionElementType.K);
            int unterschiedDurchGegner = 400 * proKrieger / 2;
            Assert.True(ohneBeschuss - mitBeschuss >= unterschiedDurchGegner,
                $"Erwartet mindestens {unterschiedDurchGegner} Unterschied, gemessen {ohneBeschuss - mitBeschuss}");
        }

        /// <summary>
        /// Abgerundet wird, wie die Kampftabelle es tut (D329: TRUNC).
        /// </summary>
        [StaFact]
        public void AngebrocheneGoldstueckeGibtEsNicht() {
            LadeKosten();
            int proKrieger = KostenView.GetGSKosten(ConstructionElementType.K);

            // ein einzelner Krieger beim Gegner: die Haelfte seines Preises, abgerundet
            int einnahmen = KampfeinnahmenRules.Berechne([Gefallene(krieger: 1)], null);
            Assert.Equal(proKrieger / 2, einnahmen);
            Assert.Equal(einnahmen, (int)einnahmen);
        }
    }
}
