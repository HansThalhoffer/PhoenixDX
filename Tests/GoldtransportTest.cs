using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Gold transportieren und transferieren - Issue #47, die beiden Wege, die noch fehlten.
    ///
    /// Umladen zwischen Heeren und die Buchungen gegen den Reichsschatz gab es bereits. Offen
    /// waren: wann transportiertes Geld im Ruestort als besondere Einnahme bereitsteht, und die
    /// Schenkung aus einem fremden Ruestort heraus.
    /// </summary>
    public class GoldtransportTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
            if (ZugView.Settings != null)
                ZugView.Settings.Phase = (int)Zugphase.Rüstphase;
        }

        private static PhoenixModel.dbErkenfara.KleinFeld FindeEigenenRuestort() {
            var gemark = SharedData.Map!.Values.FirstOrDefault(kf =>
                kf.Nation == ProgramView.SelectedNation && RuestRules.GetKapazität(kf).Goldstücke > 0);
            Assert.True(gemark != null, "Das eigene Reich hat keinen Rüstort");
            return gemark!;
        }

        /// <summary>
        /// Bereit steht nur, was zu Monatsbeginn schon im Ruestort stand.
        ///
        /// "Diese Gelder muessen erst in einen eigenen Ruestort transportiert werden, um zur
        /// Verfuegung zu stehen. Nur in diesem koennen sie im naechsten Monat verruestet werden."
        /// </summary>
        [StaFact]
        public void BereitStehtNurWasZuMonatsbeginnSchonDaWar() {
            LadeAlles();
            var rüstort = FindeEigenenRuestort();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().FirstOrDefault(Plausibilität.IsValid);
            Assert.True(truppe != null, "Das eigene Reich hat keine Truppen");

            int gfVon = truppe!.gf_von, kfVon = truppe.kf_von;
            int einnahmenVorher = truppe.Kampfeinnahmen_alt;
            int jetztVorher = truppe.Kampfeinnahmen;
            try {
                // die Truppe stand zu Monatsbeginn im Rüstort und hatte dort Kampfeinnahmen
                truppe.gf_von = rüstort.gf;
                truppe.kf_von = rüstort.kf;
                truppe.Kampfeinnahmen_alt = 12000;
                Assert.True(VerschiebeRules.BerechneBesondereEinnahmen(rüstort) >= 12000);

                // dieselbe Truppe, aber erst in diesem Monat angekommen: zaehlt noch nicht
                truppe.gf_von = gfVon;
                truppe.kf_von = kfVon;
                truppe.Kampfeinnahmen = 12000;
                if (gfVon != rüstort.gf || kfVon != rüstort.kf)
                    Assert.True(VerschiebeRules.BerechneBesondereEinnahmen(rüstort) < 12000,
                        "Geld, das erst in diesem Monat ankam, wurde schon mitgezählt");
            }
            finally {
                truppe.gf_von = gfVon;
                truppe.kf_von = kfVon;
                truppe.Kampfeinnahmen_alt = einnahmenVorher;
                truppe.Kampfeinnahmen = jetztVorher;
            }
        }

        /// <summary>
        /// Aus besonderen Einnahmen laesst sich nur ruesten, soweit welche da sind. Vorher wurde
        /// bei besonderen Einnahmen ueberhaupt nicht auf Mittel geprueft.
        /// </summary>
        [StaFact]
        public void AusBesonderenEinnahmenNurSoweitWelcheDaSind() {
            LadeAlles();
            var rüstort = FindeEigenenRuestort();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().FirstOrDefault(Plausibilität.IsValid);
            Assert.True(truppe != null);

            int gfVon = truppe!.gf_von, kfVon = truppe.kf_von;
            int einnahmenVorher = truppe.Kampfeinnahmen_alt;
            int proKrieger = KostenView.GetGSKosten(PhoenixModel.Commands.ConstructionElementType.K);
            try {
                truppe.gf_von = rüstort.gf;
                truppe.kf_von = rüstort.kf;
                truppe.Kampfeinnahmen_alt = 0;

                // ohne Kampfeinnahmen geht gar nichts
                var ohne = new Ruestung { gf = rüstort.gf, kf = rüstort.kf, K = 10 };
                var ergebnisOhne = RuestRules.Prüfe(rüstort, ohne, ausBesonderenEinnahmen: true);
                Assert.True(ergebnisOhne.HasErrors, "Ohne besondere Einnahmen wurde gerüstet");
                Assert.Contains("besondere Einnahmen", ergebnisOhne.Title);

                // mit genug Kampfeinnahmen geht es
                truppe.Kampfeinnahmen_alt = 10 * proKrieger + 1000;
                var ergebnisMit = RuestRules.Prüfe(rüstort, ohne, ausBesonderenEinnahmen: true);
                Assert.False(ergebnisMit.HasErrors, $"{ergebnisMit.Title}: {ergebnisMit.Message}");
            }
            finally {
                truppe.gf_von = gfVon;
                truppe.kf_von = kfVon;
                truppe.Kampfeinnahmen_alt = einnahmenVorher;
            }
        }

        /// <summary>
        /// Mitgefuehrtes Gold aus dem Reichsschatz wird nicht zur besonderen Einnahme - sonst
        /// liesse sich Schatzgold ueber den Umweg einer Truppe ausserhalb der Ruestmonate
        /// verruesten. "Besondere Einnahmen sind Kampfeinnahmen."
        /// </summary>
        [StaFact]
        public void MitgefuehrtesGoldWirdNichtZurBesonderenEinnahme() {
            LadeAlles();
            var rüstort = FindeEigenenRuestort();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().FirstOrDefault(Plausibilität.IsValid);
            Assert.True(truppe != null);

            int gfVon = truppe!.gf_von, kfVon = truppe.kf_von;
            int gsVorher = truppe.GS_alt;
            int einnahmenVorher = truppe.Kampfeinnahmen_alt;
            try {
                truppe.gf_von = rüstort.gf;
                truppe.kf_von = rüstort.kf;
                truppe.Kampfeinnahmen_alt = 0;
                truppe.GS_alt = 100000;

                Assert.Equal(0, VerschiebeRules.BerechneBesondereEinnahmen(rüstort));
            }
            finally {
                truppe.gf_von = gfVon;
                truppe.kf_von = kfVon;
                truppe.GS_alt = gsVorher;
                truppe.Kampfeinnahmen_alt = einnahmenVorher;
            }
        }

        /// <summary>
        /// Verschenkt wird in einem fremden Ruestort - nicht im eigenen und nicht auf freiem Feld.
        /// </summary>
        [StaFact]
        public void VerschenktWirdNurInEinemFremdenRuestort() {
            LadeAlles();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().FirstOrDefault(Plausibilität.IsValid);
            Assert.True(truppe != null);

            int gf = truppe!.gf, kf = truppe.kf;
            int einnahmen = truppe.Kampfeinnahmen;
            try {
                truppe.Kampfeinnahmen = 5000;

                // im eigenen Rüstort: nein
                var eigener = FindeEigenenRuestort();
                truppe.gf_nach = eigener.gf; truppe.kf_nach = eigener.kf;
                var imEigenen = VerschiebeRules.PrüfeSchenkung(truppe, Verschiebbar.Kampfeinnahmen, 1000);
                Assert.True(imEigenen.HasErrors);
                Assert.Contains("eigener Rüstort", imEigenen.Title);

                // in einem fremden Rüstort: ja
                var fremder = SharedData.Map!.Values.First(k =>
                    k.Nation != null && k.Nation != ProgramView.SelectedNation
                    && RuestRules.GetKapazität(k).Goldstücke > 0);
                truppe.gf_nach = fremder.gf; truppe.kf_nach = fremder.kf;
                var imFremden = VerschiebeRules.PrüfeSchenkung(truppe, Verschiebbar.Kampfeinnahmen, 1000);
                Assert.False(imFremden.HasErrors, $"{imFremden.Title}: {imFremden.Message}");
                Assert.Contains("befreundet", imFremden.Message);

                // auf freiem Feld: nein
                var freiesFeld = SharedData.Map!.Values.First(k =>
                    k.Nation != null && k.Nation != ProgramView.SelectedNation
                    && RuestRules.GetKapazität(k).Goldstücke == 0);
                truppe.gf_nach = freiesFeld.gf; truppe.kf_nach = freiesFeld.kf;
                var imFreien = VerschiebeRules.PrüfeSchenkung(truppe, Verschiebbar.Kampfeinnahmen, 1000);
                Assert.True(imFreien.HasErrors);
                Assert.Contains("kein Rüstort", imFreien.Title);
            }
            finally {
                truppe.gf_nach = gf; truppe.kf_nach = kf;
                truppe.Kampfeinnahmen = einnahmen;
            }
        }

        /// <summary>
        /// Mehr verschenken als vorhanden geht nicht, und Heerfuehrer lassen sich nicht verschenken.
        /// </summary>
        [StaFact]
        public void NurWasDaIstUndNurGeld() {
            LadeAlles();
            var truppe = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<TruppenSpielfigur>().FirstOrDefault(Plausibilität.IsValid);
            Assert.True(truppe != null);

            int einnahmen = truppe!.Kampfeinnahmen;
            try {
                truppe.Kampfeinnahmen = 100;
                var zuViel = VerschiebeRules.PrüfeSchenkung(truppe, Verschiebbar.Kampfeinnahmen, 5000);
                Assert.True(zuViel.HasErrors);

                var heerführer = VerschiebeRules.PrüfeSchenkung(truppe, Verschiebbar.Heerführer, 1);
                Assert.True(heerführer.HasErrors);
                Assert.Contains("nicht verschenken", heerführer.Title);

                Assert.True(VerschiebeRules.PrüfeSchenkung(null, Verschiebbar.Gold, 100).HasErrors);
            }
            finally {
                truppe.Kampfeinnahmen = einnahmen;
            }
        }
    }
}
