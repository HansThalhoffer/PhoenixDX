using PhoenixModel.Commands;
using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Befoerderung und Degradierung von Charakteren (Regelwerk 1.9.1).
    ///
    /// "Wird ein Charakter befoerdert so rueckt er Augenblicklich auf seine aktuelle Position auf,
    /// es aendert sich hier dann sofort sein Gutpunktmaxium, die aktuellen Gutpunkte bleiben ...
    /// Wird ein Charakter degradiert, so sinken seine maximalen und aktuellen Gutpunkte."
    /// </summary>
    public class BefoerderungTest {

        private static void LadeAlles() => TestSetup.LadeMitDiplomatie();

        private static Character Charakter(string beschriftung, int gpAkt, int gpGes)
            => new() {
                Nummer = 690, Beschriftung = beschriftung, GP_akt = gpAkt, GP_ges = gpGes,
                Nation = ProgramView.SelectedNation,
            };

        /// <summary>
        /// Die Gutpunktmaxima aus dem Beispiel des Regelwerks: Burgherr 24, Stadthalter 36,
        /// Herrscher 60.
        /// </summary>
        [Fact]
        public void DieGutpunktmaximaStehenSoImRegelwerk() {
            Assert.Equal(24, BeförderungsRules.GetGutpunktmaximum(Characterklasse.BUH));
            Assert.Equal(36, BeförderungsRules.GetGutpunktmaximum(Characterklasse.STH));
            Assert.Equal(60, BeförderungsRules.GetGutpunktmaximum(Characterklasse.HER));

            // die Reihenfolge der Aemter steigt durchgehend
            Assert.True(BeförderungsRules.GetGutpunktmaximum(Characterklasse.HF)
                      < BeförderungsRules.GetGutpunktmaximum(Characterklasse.BUH));
            Assert.True(BeförderungsRules.GetGutpunktmaximum(Characterklasse.FSH)
                      > BeförderungsRules.GetGutpunktmaximum(Characterklasse.STH));
            Assert.True(BeförderungsRules.GetGutpunktmaximum(Characterklasse.HER)
                      > BeförderungsRules.GetGutpunktmaximum(Characterklasse.FSH));
        }

        /// <summary>
        /// "es aendert sich hier dann sofort sein Gutpunktmaxium, die aktuellen Gutpunkte bleiben"
        /// - das Beispiel des Regelwerks: der Burgherr wird Stadthalter, 24/36.
        /// </summary>
        [StaFact]
        public void BeimBefoerdernSteigtNurDasMaximum() {
            LadeAlles();
            var burgherr = Charakter("BUH1", gpAkt: 24, gpGes: 24);

            BeförderungsRules.Befördere(burgherr, Characterklasse.STH);

            Assert.Equal(24, burgherr.GP_akt);
            Assert.Equal(36, burgherr.GP_ges);
            Assert.Equal(Characterklasse.STH, CharacterView.GetAssumedKlasse(burgherr));
            Assert.Equal("STH1", burgherr.Beschriftung);
        }

        /// <summary>
        /// "Wird ein Charakter degradiert, so sinken seine maximalen und aktuellen Gutpunkte."
        /// Der abgedankte Herrscher wird auf 24/24 gesetzt.
        /// </summary>
        [StaFact]
        public void BeimDegradierenSinktBeides() {
            LadeAlles();
            var herrscher = Charakter("HER1", gpAkt: 60, gpGes: 60);

            BeförderungsRules.Degradiere(herrscher, Characterklasse.BUH);

            Assert.Equal(24, herrscher.GP_akt);
            Assert.Equal(24, herrscher.GP_ges);
            Assert.Equal(Characterklasse.BUH, CharacterView.GetAssumedKlasse(herrscher));
            Assert.Equal("BUH1", herrscher.Beschriftung);
        }

        /// <summary>
        /// "Die Positionen ... sind in jedem Reich nur einmalig vorhanden" und "koennen nur
        /// besetzt werden, wenn fuer jeden Rang der erforderliche Ruestort vorhanden ist".
        /// </summary>
        [StaFact]
        public void EinmalJeReichUndNurMitDemPassendenRuestort() {
            LadeAlles();
            var reich = ProgramView.SelectedNation!;

            // das Reich hat seine Aemter schon besetzt - wer jetzt dorthin befoerdert werden
            // soll, stoesst auf den Amtsinhaber
            var besetzte = new[] { Characterklasse.BUH, Characterklasse.STH, Characterklasse.FSH, Characterklasse.HER }
                .Where(k => BeförderungsRules.IstAmtBesetzt(reich, k))
                .ToList();
            Assert.True(besetzte.Count > 0, "Das eigene Reich hat kein besetztes Amt");

            var kandidat = Charakter("HF9", gpAkt: 12, gpGes: 12);
            var besetzt = BeförderungsRules.PrüfeBeförderung(kandidat, besetzte[0]);
            Assert.True(besetzt.HasErrors);
            Assert.Contains("bereits besetzt", besetzt.Title);

            // der Heerfuehrer ist kein Amt, das nur einmal vorkommt
            Assert.False(BeförderungsRules.IstAmtBesetzt(reich, Characterklasse.HF));

            // und der Ruestort muss stehen
            Assert.True(BeförderungsRules.HatErforderlichenRüstort(reich, Characterklasse.HF));
            Assert.False(BeförderungsRules.HatErforderlichenRüstort(null, Characterklasse.BUH));
        }

        /// <summary>
        /// Eine Befoerderung fuehrt nach oben, eine Degradierung nach unten - andersherum nicht.
        /// </summary>
        [StaFact]
        public void DieRichtungMussStimmen() {
            LadeAlles();
            var stadthalter = Charakter("STH9", gpAkt: 30, gpGes: 36);

            var abwaerts = BeförderungsRules.PrüfeBeförderung(stadthalter, Characterklasse.HF);
            Assert.True(abwaerts.HasErrors);
            Assert.Contains("schon", abwaerts.Title);

            var aufwaerts = BeförderungsRules.PrüfeDegradierung(stadthalter, Characterklasse.HER);
            Assert.True(aufwaerts.HasErrors);

            // nach unten degradieren geht
            Assert.False(BeförderungsRules.PrüfeDegradierung(stadthalter, Characterklasse.HF).HasErrors);

            Assert.True(BeförderungsRules.PrüfeBeförderung(null, Characterklasse.BUH).HasErrors);
            Assert.True(BeförderungsRules.PrüfeBeförderung(stadthalter, Characterklasse.none).HasErrors);
        }

        /// <summary>
        /// "Wird der Ruestort eines Adligen durch aeussere Einwirkungen in der Ruestortklasse
        /// erniedrigt, so sinken die aktuellen Gutpunkte des Adligen auf die der Ruestortklasse
        /// entsprechenden. Der Rang des Adligen bleibt dennoch erhalten!" (Regelwerk 1.9.1)
        /// </summary>
        [StaFact]
        public void EinKleinererRuestortSenktDieGutpunkteNichtDenRang() {
            LadeAlles();
            var herrscher = Charakter("HER1", gpAkt: 60, gpGes: 60);

            // sein Sitz ist nur noch eine Burg
            var burg = SharedData.Map!.Values.First(kf =>
                BauwerkeView.GetRüstortNachKarte(kf)?.Ruestort == "Burg");

            Assert.True(BeförderungsRules.SenkeAufRüstortklasse(herrscher, burg));

            Assert.Equal(24, herrscher.GP_akt);
            Assert.Equal(60, herrscher.GP_ges);
            Assert.Equal(Characterklasse.HER, CharacterView.GetAssumedKlasse(herrscher));

            // ein zweites Mal sinkt nichts mehr
            Assert.False(BeförderungsRules.SenkeAufRüstortklasse(herrscher, burg));
        }

        /// <summary>
        /// Das Amt steht in der Beschriftung - die Nummer dahinter bleibt.
        /// </summary>
        [Fact]
        public void DasAmtStehtInDerBeschriftungDieNummerBleibt() {
            Assert.Equal("BUH0815", BeförderungsRules.SetzeAmt("HF0815", Characterklasse.BUH));
            Assert.Equal("HER", BeförderungsRules.SetzeAmt("STH", Characterklasse.HER));
            Assert.Equal("STH1", BeförderungsRules.SetzeAmt("BUH1", Characterklasse.STH));
            // was kein Amt vorne traegt, bekommt eines davor
            Assert.Equal("BUHKarl", BeförderungsRules.SetzeAmt("Karl", Characterklasse.BUH));
            Assert.Equal("HF", BeförderungsRules.SetzeAmt(null, Characterklasse.HF));
        }

        /// <summary>
        /// Der Befehl liest die Aemter ausgeschrieben und abgekuerzt, gebeugt und ungebeugt.
        /// </summary>
        [Fact]
        public void DerBefehlVerstehtDieAemter() {
            Assert.Equal(Characterklasse.BUH, BeförderungCommandParser.LiesAmt("Burgherrn"));
            Assert.Equal(Characterklasse.BUH, BeförderungCommandParser.LiesAmt("burgherr"));
            Assert.Equal(Characterklasse.BUH, BeförderungCommandParser.LiesAmt("BUH"));
            Assert.Equal(Characterklasse.STH, BeförderungCommandParser.LiesAmt("Stadthalter"));
            Assert.Equal(Characterklasse.STH, BeförderungCommandParser.LiesAmt("Statthalter"));
            Assert.Equal(Characterklasse.FSH, BeförderungCommandParser.LiesAmt("Festungsherrn"));
            Assert.Equal(Characterklasse.HER, BeförderungCommandParser.LiesAmt("Herrscher"));
            Assert.Equal(Characterklasse.HF, BeförderungCommandParser.LiesAmt("Heerführer"));
            Assert.Equal(Characterklasse.none, BeförderungCommandParser.LiesAmt("Kaiser von China"));
        }

        /// <summary>
        /// Der ganze Befehl: gelesen, geprueft, ausgefuehrt und zurueckgenommen.
        ///
        /// Degradiert wird, nicht befoerdert - eine Degradierung braucht weder einen freien Posten
        /// noch einen Ruestort und laeuft deshalb unabhaengig vom Datenbestand durch.
        /// </summary>
        [StaFact]
        public void DerBefehlLaeuftDurchUndLaesstSichZuruecknehmen() {
            LadeAlles();

            var amtstraeger = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation)
                .OfType<Character>()
                .FirstOrDefault(c => CharacterView.GetAssumedKlasse(c) != Characterklasse.HF
                                  && CharacterView.GetAssumedKlasse(c) != Characterklasse.none);
            Assert.True(amtstraeger != null, "Das eigene Reich hat keinen Amtstraeger");

            string beschriftung = amtstraeger!.Beschriftung;
            int gesamt = amtstraeger.GP_ges, aktuell = amtstraeger.GP_akt;
            try {
                var parser = new BeförderungCommandParser();
                Assert.True(parser.ParseCommand($"Degradiere Charakter {amtstraeger.Nummer} zum Heerführer",
                    out var befehl));
                var degradierung = Assert.IsType<BeförderungCommand>(befehl);
                Assert.Equal(Characterklasse.HF, degradierung.Amt);
                Assert.True(degradierung.IstDegradierung);

                Assert.False(degradierung.CheckPreconditions().HasErrors);
                Assert.False(degradierung.ExecuteCommand().HasErrors);

                Assert.Equal(12, amtstraeger.GP_ges);
                Assert.True(amtstraeger.GP_akt <= 12);
                Assert.Equal(Characterklasse.HF, CharacterView.GetAssumedKlasse(amtstraeger));

                Assert.True(degradierung.CanUndo);
                Assert.False(degradierung.UndoCommand().HasErrors);
                Assert.Equal(gesamt, amtstraeger.GP_ges);
                Assert.Equal(aktuell, amtstraeger.GP_akt);
                Assert.Equal(beschriftung, amtstraeger.Beschriftung);
            }
            finally {
                amtstraeger.Beschriftung = beschriftung;
                amtstraeger.GP_ges = gesamt;
                amtstraeger.GP_akt = aktuell;
            }
        }
    }
}
