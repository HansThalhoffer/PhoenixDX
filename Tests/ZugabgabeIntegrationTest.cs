using PhoenixModel.Rules;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Fährt eine komplette Zugabgabe gegen eine Kopie der echten Zugdaten.
    ///
    /// Der Test fasst die echten Spieldaten nicht an: er kopiert die Zugdatenbank in ein eigenes
    /// Verzeichnis unterhalb des Temp-Ordners und gibt dort ab. Am Ende wird wieder auf die echte
    /// Datenbank umgestellt, damit die folgenden Tests nicht auf der Kopie weiterarbeiten.
    /// </summary>
    public class ZugabgabeIntegrationTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>
        /// Legt eine Spielwiese an: ein eigenes Zugdatenverzeichnis mit einer Kopie der Datenbank
        /// des laufenden Zuges.
        /// </summary>
        private static string ErstelleKopie(int zug) {
            string quelle = TestSetup.ZugdatenPfad;
            string spielwiese = Path.Combine(Path.GetTempPath(), "PhoenixDX_Zugabgabe_" + Guid.NewGuid().ToString("N"));
            string verzeichnis = Path.Combine(spielwiese, zug.ToString());
            Directory.CreateDirectory(verzeichnis);
            string ziel = Path.Combine(verzeichnis, Path.GetFileName(quelle));
            File.Copy(quelle, ziel);
            return ziel;
        }

        [StaFact]
        public void ZugabgabeLegtDenFolgezugAn() {
            LadeAlles();
            int zug = ZugView.AktuellerZug.Zug;
            int folgeZug = zug + 1;

            string kopie = ErstelleKopie(zug);
            string spielwiese = Path.GetDirectoryName(Path.GetDirectoryName(kopie))!;
            try {
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug), $"Die Kopie {kopie} liess sich nicht laden");

                // den Stand vor der Abgabe festhalten
                var vorher = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                int anzahlVorher = vorher.Count;
                int aufgelösteVorher = vorher.Count(ZugendeRules.IstAufgelöst);
                var zielpositionen = vorher.Where(f => ZugendeRules.IstAufgelöst(f) == false)
                                           .ToDictionary(f => f.Nummer, f => (f.gf_nach, f.kf_nach));
                Assert.True(anzahlVorher > 0, "Für den Test braucht es Figuren");

                var ergebnis = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug);
                Assert.True(ergebnis.Erfolgreich, $"{ergebnis.Meldung} {ergebnis.Details}");
                Assert.NotNull(ergebnis.NeueDatenbank);
                Assert.True(File.Exists(ergebnis.NeueDatenbank), $"{ergebnis.NeueDatenbank} wurde nicht angelegt");
                Assert.Equal(anzahlVorher - aufgelösteVorher, ergebnis.ÜbernommeneFiguren);

                // die Quelle bleibt als Archiv erhalten und ist als abgegeben markiert
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug));
                Assert.Equal(Zugphase.Abgeschlossen, ZugView.Phase);

                // und nun die frisch angelegte Datenbank des Folgezuges
                Assert.True(TestSetup.LadeZugdatenAusDatei(ergebnis.NeueDatenbank!, folgeZug));

                var settings = ZugView.Settings;
                Assert.NotNull(settings);
                Assert.Equal(folgeZug, settings!.Monat);
                Assert.Equal(Zugphase.Rüstphase, ZugView.Phase);

                // die Rüstung des Vorzuges ist bezahlt und abgeräumt
                Assert.Empty(SharedData.Ruestung!);
                Assert.Empty(SharedData.RuestungBauwerke!);
                Assert.Empty(SharedData.RuestungRuestorte!);

                // der Reichsschatz des Folgemonats ist die Bilanz des abgegebenen Monats
                var abgerechnet = SchatzkammerRules.GetMonat(zug);
                var neu = SchatzkammerRules.GetMonat(folgeZug);
                Assert.NotNull(abgerechnet);
                Assert.NotNull(neu);
                Assert.Equal(SchatzkammerRules.BerechneBilanz(abgerechnet), neu!.Reichschatz);
                Assert.Equal(ergebnis.NeuerReichsschatz, neu.Reichschatz);
                Assert.Equal(0, neu.Verruestet);
                Assert.Equal(0, neu.schenkung_getaetigt);
                Assert.Equal(0, neu.schenkung_bekommen);

                // die Figuren stehen da, wo sie im Vorzug angekommen sind, und haben wieder
                // volle Bewegungspunkte
                var nachher = SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation);
                Assert.Equal(ergebnis.ÜbernommeneFiguren, nachher.Count);
                foreach (var figur in nachher) {
                    Assert.True(zielpositionen.TryGetValue(figur.Nummer, out var ziel),
                        $"Figur {figur.Nummer} taucht im Folgezug auf, war aber vorher nicht da");
                    Assert.True(figur.gf_von == ziel.gf_nach && figur.kf_von == ziel.kf_nach,
                        $"Figur {figur.Nummer} startet auf {figur.gf_von}/{figur.kf_von}, "
                        + $"angekommen war sie aber auf {ziel.gf_nach}/{ziel.kf_nach}");
                    Assert.True(figur.gf_von > 0, $"Figur {figur.Nummer} ist von der Karte gefallen");
                }
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                try { Directory.Delete(spielwiese, true); } catch { /* die Spielwiese ist im Temp, das räumt notfalls Windows auf */ }
            }
        }

        /// <summary>
        /// Ein zweites Abgeben desselben Zuges darf die bereits begonnene Arbeit im Folgezug nicht
        /// stillschweigend wegwerfen.
        /// </summary>
        [StaFact]
        public void ZugabgabeUeberschreibtDenFolgezugNichtUngefragt() {
            LadeAlles();
            int zug = ZugView.AktuellerZug.Zug;

            string kopie = ErstelleKopie(zug);
            string spielwiese = Path.GetDirectoryName(Path.GetDirectoryName(kopie))!;
            try {
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug));
                Assert.True(Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug).Erfolgreich);

                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, zug));
                Assert.True(Zugabgabe.ZielExistiert(kopie, zug + 1));

                // die erste Sperre ist die Phase: der Zug ist als abgegeben vermerkt
                var zweiterVersuch = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug);
                Assert.False(zweiterVersuch.Erfolgreich);
                Assert.Equal(Zugphase.Abgeschlossen, ZugView.Phase);

                // die zweite Sperre ist die vorhandene Datei. Sie greift auch dann, wenn die
                // Spielleitung den Zug wieder geöffnet hat, weil im Folgezug schon gearbeitet
                // worden sein kann.
                ZugView.Settings!.Phase = (int)Zugphase.Bewegungsphase;
                var dritterVersuch = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug);
                Assert.False(dritterVersuch.Erfolgreich);
                Assert.Contains("gibt es bereits", dritterVersuch.Meldung);

                // mit ausdrücklicher Zustimmung geht es dann doch
                var vierterVersuch = Zugabgabe.Durchführen(kopie, TestSetup.ZugdatenPasswort, zug, zielÜberschreiben: true);
                Assert.True(vierterVersuch.Erfolgreich, $"{vierterVersuch.Meldung} {vierterVersuch.Details}");
            }
            finally {
                TestSetup.LoadZugdaten(false, false);
                try { Directory.Delete(spielwiese, true); } catch { /* siehe oben */ }
            }
        }
    }
}
