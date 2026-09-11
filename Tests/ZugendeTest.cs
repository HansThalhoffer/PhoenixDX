using PhoenixModel.dbZugdaten;
using PhoenixModel.Rules;
using PhoenixModel.ViewModel;

namespace Tests {

    /// <summary>
    /// Die Regeln für den Übergang in den nächsten Spielzug, ohne Datenbank.
    /// </summary>
    public class ZugendeTest {

        private static Krieger ErzeugeKrieger() => new() {
            Nummer = 101,
            gf_von = 305,
            kf_von = 24,
            staerke = 1000,
            hf = 5,
            LKP = 1,
            SKP = 2,
            Pferde = 3,
            GS = 700,
            Kampfeinnahmen = 300,
            bp = 9,
            bp_max = 9,
        };

        /// <summary>
        /// Wer sich nicht bewegt hat, bleibt stehen. Ohne diese Korrektur würde die Figur beim
        /// Zugübergang die Position 0/0 erben und von der Karte verschwinden.
        /// </summary>
        [Fact]
        public void NichtBewegteFigurBleibtStehen() {
            var krieger = ErzeugeKrieger();
            Assert.Equal(0, krieger.gf_nach);

            Assert.True(ZugendeRules.SetzeAufAusgangsposition(krieger));
            Assert.Equal(305, krieger.gf_nach);
            Assert.Equal(24, krieger.kf_nach);

            // ein zweiter Aufruf ändert nichts mehr
            Assert.False(ZugendeRules.SetzeAufAusgangsposition(krieger));
        }

        /// <summary>
        /// Eine bewegte Figur wird nicht zurückgesetzt
        /// </summary>
        [Fact]
        public void BewegteFigurBehaeltIhrZiel() {
            var krieger = ErzeugeKrieger();
            krieger.gf_nach = 306;
            krieger.kf_nach = 12;

            Assert.False(ZugendeRules.SetzeAufAusgangsposition(krieger));
            Assert.Equal(306, krieger.gf_nach);
            Assert.Equal(12, krieger.kf_nach);
        }

        /// <summary>
        /// Beim Zugübergang wird die erreichte Position zur neuen Ausgangsposition, die
        /// Bewegungspunkte werden aufgefrischt und die Werte des Zugbeginns gesichert.
        /// </summary>
        [Fact]
        public void TruppeWirdInDenNaechstenZugGeschoben() {
            var krieger = ErzeugeKrieger();
            krieger.gf_nach = 306;
            krieger.kf_nach = 12;
            krieger.bp = 2;
            krieger.hoehenstufen = 2;
            krieger.Befehl_bew = "irgendwas";
            krieger.Befehl_erobert = "E:306/12;";
            krieger.Befehl_ang = "Angriff";
            krieger.spaltetab = "geteilt";
            krieger.fusmit = "fusioniert";
            krieger.Sonstiges = "Notiz";
            krieger.isbanned = 1;
            new Bewegungsspur(krieger).Add(new KleinfeldPosition(306, 12));

            ZugendeRules.SchiebeInNächstenZug(krieger);

            // die erreichte Position ist jetzt die Ausgangsposition
            Assert.Equal(306, krieger.gf_von);
            Assert.Equal(12, krieger.kf_von);
            Assert.Equal(0, krieger.gf_nach);
            Assert.Equal(0, krieger.kf_nach);
            Assert.Equal(306, krieger.gf);
            Assert.Equal(12, krieger.kf);

            // frische Bewegungspunkte, keine Höhenstufen, keine Spur
            Assert.Equal(krieger.bp_max, krieger.bp);
            Assert.Equal(0, krieger.hoehenstufen);
            Assert.Equal(0, krieger.schritt);
            Assert.Equal(0, krieger.x1);

            // alle Befehle geräumt
            Assert.Equal(string.Empty, krieger.Befehl_bew);
            Assert.Equal(string.Empty, krieger.Befehl_ang);
            Assert.Equal(string.Empty, krieger.Befehl_erobert);
            Assert.Equal(string.Empty, krieger.spaltetab);
            Assert.Equal(string.Empty, krieger.fusmit);
            Assert.Equal(string.Empty, krieger.Sonstiges);
            Assert.Equal(0, krieger.isbanned);

            // die Werte des Zugbeginns sind gesichert, die aktuellen unverändert
            Assert.Equal(1000, krieger.staerke_alt);
            Assert.Equal(5, krieger.hf_alt);
            Assert.Equal(1, krieger.LKP_alt);
            Assert.Equal(2, krieger.SKP_alt);
            Assert.Equal(3, krieger.pferde_alt);
            Assert.Equal(700, krieger.GS_alt);
            Assert.Equal(300, krieger.Kampfeinnahmen_alt);
            Assert.Equal(1000, krieger.staerke);
            Assert.Equal(700, krieger.GS);
        }

        /// <summary>
        /// Ein Heer ohne Heerführer existiert nicht mehr und verschwindet von der Karte
        /// </summary>
        [Fact]
        public void HeerOhneHeerfuehrerVerschwindet() {
            var krieger = ErzeugeKrieger();
            krieger.hf = 0;
            krieger.gf_nach = 306;
            krieger.kf_nach = 12;

            Assert.True(ZugendeRules.IstAufgelöst(krieger));
            ZugendeRules.SchiebeInNächstenZug(krieger);

            Assert.Equal(0, krieger.gf_von);
            Assert.Equal(0, krieger.kf_von);
            Assert.Equal(0, krieger.gf);
        }

        /// <summary>
        /// Charaktere regenerieren pauschal fünf Gutpunkte, aber nie über ihren Höchstwert hinaus
        /// </summary>
        [Fact]
        public void CharakterRegeneriertGutpunkte() {
            var charakter = new Character { Nummer = 601, GP_ges = 20, GP_akt = 12, gf_von = 305, kf_von = 24, bp = 0, bp_max = 21 };
            charakter.gf_nach = 305;
            charakter.kf_nach = 24;

            ZugendeRules.SchiebeInNächstenZug(charakter);

            Assert.Equal(12, charakter.GP_akt_alt);
            Assert.Equal(20, charakter.GP_ges_alt);
            Assert.Equal(17, charakter.GP_akt);
            Assert.Equal(21, charakter.bp);

            // ein zweiter Monat füllt nur noch bis zum Höchstwert auf
            charakter.gf_nach = 305;
            charakter.kf_nach = 24;
            ZugendeRules.SchiebeInNächstenZug(charakter);
            Assert.Equal(20, charakter.GP_akt);
        }

        /// <summary>
        /// Ein Charakter ohne Gutpunkte verschwindet ebenfalls von der Karte
        /// </summary>
        [Fact]
        public void CharakterOhneGutpunkteVerschwindet() {
            var charakter = new Character { Nummer = 601, GP_ges = 0, GP_akt = 0, gf_von = 305, kf_von = 24 };
            charakter.gf_nach = 305;
            charakter.kf_nach = 24;

            Assert.True(ZugendeRules.IstAufgelöst(charakter));
            ZugendeRules.SchiebeInNächstenZug(charakter);
            Assert.Equal(0, charakter.gf);
        }

        /// <summary>
        /// Die Teleportpunkte der Nichtspielerfiguren werden jeden Monat zurückgesetzt,
        /// die der Spielerfiguren ab Nummer 600 nicht.
        /// </summary>
        [Fact]
        public void TeleportpunkteWerdenNurBeiNichtspielernZurueckgesetzt() {
            var nichtspieler = new Zauberer { Nummer = 501, GP_ges = 10, GP_akt = 10, gf_von = 305, kf_von = 24, tp = 3, tp_alt = 2 };
            nichtspieler.gf_nach = 305;
            nichtspieler.kf_nach = 24;
            ZugendeRules.SchiebeInNächstenZug(nichtspieler);
            Assert.Equal(0, nichtspieler.tp);
            Assert.Equal(0, nichtspieler.tp_alt);

            var spieler = new Character { Nummer = 601, GP_ges = 10, GP_akt = 10, gf_von = 305, kf_von = 24, tp = 3, tp_alt = 2 };
            spieler.gf_nach = 305;
            spieler.kf_nach = 24;
            ZugendeRules.SchiebeInNächstenZug(spieler);
            Assert.Equal(3, spieler.tp);
            Assert.Equal(2, spieler.tp_alt);
        }

        /// <summary>
        /// Die Reihenfolge ist entscheidend: wird erst geschoben und dann korrigiert, verliert eine
        /// nicht bewegte Figur ihre Position.
        /// </summary>
        [Fact]
        public void ReihenfolgeIstEntscheidend() {
            var richtig = ErzeugeKrieger();
            ZugendeRules.SetzeAufAusgangsposition(richtig);
            ZugendeRules.SchiebeInNächstenZug(richtig);
            Assert.Equal(305, richtig.gf);
            Assert.Equal(24, richtig.kf);

            // die falsche Reihenfolge zeigt, warum die richtige nötig ist
            var falsch = ErzeugeKrieger();
            ZugendeRules.SchiebeInNächstenZug(falsch);
            Assert.Equal(0, falsch.gf);
        }
    }
}
