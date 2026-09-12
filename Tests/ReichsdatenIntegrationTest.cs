using PhoenixModel.dbPZE;
using PhoenixModel.dbZugdaten;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using PhoenixWPF.Database;
using System.IO;

namespace Tests {

    /// <summary>
    /// Baut einmal je Testklasse ein Zugverzeichnis mit mehreren Reichen auf und liest es ein.
    ///
    /// Einmal, nicht je Test: jede Access-Verbindung traegt ein Risiko von etwa 1:400, den Prozess
    /// mit einer Zugriffsverletzung im Treiber zu beenden (siehe SpeichernIntegrationTest). Fuenf
    /// Tests mit je einem eigenen Aufbau haben den Testlauf messbar wieder zum Absturz gebracht -
    /// ohne diese Klasse 3 von 3 Laeufen sauber, mit ihr 2 von 3 abgestuerzt.
    /// </summary>
    public class Mehrreichumgebung : IDisposable {

        /// <summary>Wieviele Reiche nachgebaut werden</summary>
        public const int AnzahlReiche = 3;

        public string Zugverzeichnis { get; }
        public string Spielwiese { get; }
        public List<Nation> Reiche { get; }
        public ReichsdatenErgebnis Ergebnis { get; }

        /// <summary>Das Passwort der Zugdatenbank im Klartext - die Kopien tragen dasselbe</summary>
        public static string Klartext()
            => new PhoenixModel.Database.PasswordHolder(TestSetup.ZugdatenPasswort).DecryptedPassword ?? string.Empty;

        public Mehrreichumgebung() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);

            Spielwiese = Path.Combine(Path.GetTempPath(), "PhoenixDX_Reiche_" + Guid.NewGuid().ToString("N"));
            Zugverzeichnis = Path.Combine(Spielwiese, ProgramView.SelectedMonth.ToString());
            Directory.CreateDirectory(Zugverzeichnis);

            Reiche = SharedData.Nationen!
                .Where(n => string.IsNullOrWhiteSpace(n.DBname) == false)
                .Take(AnzahlReiche)
                .ToList();
            foreach (var reich in Reiche)
                File.Copy(TestSetup.ZugdatenPfad, Path.Combine(Zugverzeichnis, $"{reich.DBname}.mdb"));

            Spielleitungsdaten.Leere();
            Ergebnis = Reichsdaten.LadeAlleReiche(Zugverzeichnis, ProgramView.SelectedMonth, Klartext());
        }

        public void Dispose() {
            Spielleitungsdaten.Leere();
            TestSetup.RäumeAuf(Spielwiese);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Die Spielleitung liest die Zugdaten aller Reiche.
    ///
    /// Geprueft wird gegen Kopien einer einzigen Reichsdatenbank, die unter fremden Reichsnamen
    /// abgelegt werden - echte Daten mehrerer Reiche liegen nur einer Spielleitungsinstallation
    /// vor. Damit laesst sich pruefen, dass mehrere Datenbanken nebeneinander gelesen werden, dass
    /// die Figuren beim richtigen Reich landen und dass die Spielerseite unbeschaedigt bleibt.
    /// Was sich damit nicht pruefen laesst, sind echte Zusammentreffen echter Reiche.
    /// </summary>
    public class ReichsdatenIntegrationTest : IClassFixture<Mehrreichumgebung> {

        private readonly Mehrreichumgebung _umgebung;

        public ReichsdatenIntegrationTest(Mehrreichumgebung umgebung) {
            _umgebung = umgebung;
        }

        [StaFact]
        public void MehrereReicheWerdenNebeneinanderGelesen() {
            var ergebnis = _umgebung.Ergebnis;
            Assert.True(ergebnis.Erfolgreich, $"{ergebnis.Meldung} {ergebnis.Details}");
            Assert.Equal(Mehrreichumgebung.AnzahlReiche, ergebnis.GeladeneReiche.Count);
            Assert.True(ergebnis.Figuren > 0, "Es wurden keine Figuren gelesen");

            // jedes Reich haelt seine eigenen Figuren, und sie tragen seinen Namen
            foreach (var reich in _umgebung.Reiche) {
                var armee = Spielleitungsdaten.GetFiguren(reich);
                Assert.True(armee.Count > 0, $"{reich.Reich} hat keine Figuren");
                Assert.All(armee, figur => Assert.Equal(reich, figur.Nation));
            }

            // und die Reiche, deren Datenbank fehlt, sind vermerkt statt verschwiegen
            Assert.Equal(SharedData.Nationen!.Count - Mehrreichumgebung.AnzahlReiche, ergebnis.FehlendeReiche.Count);
        }

        /// <summary>
        /// Der Leser der Spielleitung laesst die Spielerseite unberuehrt: weder der Speicherort
        /// noch das gewaehlte Reich noch SharedData veraendern sich.
        ///
        /// Warum das nicht selbstverstaendlich ist, zeigt
        /// <see cref="DerGewoehnlicheLadewegVerstelltDenSpeicherortSehrWohl"/>.
        /// </summary>
        [StaFact]
        public void DasLesenFremderReicheLaesstDieSpielerseiteInRuhe() {
            // der Aufbau der Umgebung hat bereits drei fremde Reiche gelesen
            Assert.True(_umgebung.Ergebnis.Erfolgreich);

            Assert.Equal(TestSetup.ZugdatenPfad, Krieger.DatabaseName);
            Assert.Equal(TestSetup.ZugdatenPfad, Zauberer.DatabaseName);
            Assert.Equal("Theostelos", ProgramView.SelectedNation?.Reich);

            // und die eigenen Figuren stehen unveraendert in SharedData
            Assert.True(SharedData.Krieger!.Count > 0);
            Assert.True(SpielfigurenView.GetSpielfiguren(ProgramView.SelectedNation).Count > 0);
        }

        /// <summary>
        /// Die Figuren mehrerer Reiche auf derselben Gemark - die Grundlage der Konflikterkennung.
        /// Weil alle Kopien dieselben Daten tragen, steht hier garantiert jedes Reich auf
        /// denselben Feldern.
        /// </summary>
        [StaFact]
        public void FigurenMehrererReicheStehenAufDerselbenGemark() {
            var irgendeine = Spielleitungsdaten.GetFiguren(_umgebung.Reiche[0])
                .First(f => Plausibilität.IsValid(f));
            var gemark = new KleinfeldPosition(irgendeine.gf, irgendeine.kf);
            var alle = Spielleitungsdaten.GetFigurenAufGemark(gemark);

            Assert.True(alle.Count >= Mehrreichumgebung.AnzahlReiche,
                $"Auf {gemark.CreateBezeichner()} stehen nur {alle.Count} Figuren");
            Assert.Equal(Mehrreichumgebung.AnzahlReiche, alle.Select(f => f.Nation).Distinct().Count());
            Assert.All(alle, figur => {
                Assert.Equal(gemark.gf, figur.gf);
                Assert.Equal(gemark.kf, figur.kf);
            });
        }

        /// <summary>
        /// Belegt die Gefahr, gegen die der eigene Leser gebaut ist.
        ///
        /// DatabaseName ist eine statische Eigenschaft je Tabellenklasse, und der Speicherlauf
        /// entscheidet an ihr, in welche Datei ein Datensatz geht. Der gewoehnliche Ladeweg setzt
        /// sie bei jedem Laden neu - ein fremdes Reich darueber zu holen wuerde also die Zuege des
        /// Spielers in die fremde Datenbank umleiten. Genau deshalb liest die Spielleitung ueber
        /// Reichsdaten und nicht ueber Zugdaten.
        ///
        /// Ohne diesen Test staende die Begruendung nur im Kommentar.
        /// </summary>
        [StaFact]
        public void DerGewoehnlicheLadewegVerstelltDenSpeicherortSehrWohl() {
            string echt = Krieger.DatabaseName;
            Assert.False(string.IsNullOrEmpty(echt), "Vor dem Test ist kein Speicherort gesetzt");

            string kopie = Path.Combine(_umgebung.Zugverzeichnis, $"{_umgebung.Reiche[0].DBname}.mdb");
            try {
                Assert.True(TestSetup.LadeZugdatenAusDatei(kopie, ProgramView.SelectedMonth));

                // der Speicherort zeigt jetzt auf die Kopie - ab hier ginge jeder Zug dorthin
                Assert.NotEqual(echt, Krieger.DatabaseName);
                Assert.Equal(kopie, Krieger.DatabaseName);
            }
            finally {
                // zurueck auf die echten Zugdaten, sonst arbeiten die folgenden Tests auf der Kopie
                TestSetup.LoadZugdaten(false, false);
            }
            Assert.Equal(TestSetup.ZugdatenPfad, Krieger.DatabaseName);
        }

        /// <summary>
        /// Ein Zugverzeichnis ohne Reichsdatenbanken ist kein Grund abzustuerzen. Hier wird nichts
        /// geoeffnet, also kostet der Test auch keine Verbindung.
        /// </summary>
        [StaFact]
        public void EinLeeresZugverzeichnisMeldetSichVerstaendlich() {
            string leer = Path.Combine(_umgebung.Spielwiese, "leer");
            Directory.CreateDirectory(leer);

            var ergebnis = Reichsdaten.LadeAlleReiche(leer, 170, Mehrreichumgebung.Klartext());
            Assert.False(ergebnis.Erfolgreich);
            Assert.Contains("keine Reichsdatenbank", ergebnis.Meldung);

            var fehlt = Reichsdaten.LadeAlleReiche(Path.Combine(leer, "gibtesnicht"), 170, Mehrreichumgebung.Klartext());
            Assert.False(fehlt.Erfolgreich);
            Assert.Contains("gibt es nicht", fehlt.Meldung);
        }

        /// <summary>
        /// Der Speicher liefert auch ohne geladene Daten brauchbare Antworten. Laeuft zum Schluss,
        /// weil er den Speicher leert und ihn danach wieder fuellt.
        /// </summary>
        [StaFact]
        public void OhneDatenBleibtDerSpeicherLeerAberBrauchbar() {
            try {
                Spielleitungsdaten.Leere();
                Assert.False(Spielleitungsdaten.IstGeladen);
                Assert.Empty(Spielleitungsdaten.GeladeneReiche);
                Assert.Empty(Spielleitungsdaten.GetAlleFiguren());
                Assert.Empty(Spielleitungsdaten.GetFiguren(null));
                Assert.Empty(Spielleitungsdaten.GetFigurenAufGemark(null));
                Assert.Empty(Spielleitungsdaten.GetFigurenAufGemark(new KleinfeldPosition(1, 1)));
                Assert.Equal(0, Spielleitungsdaten.Zug);
            }
            finally {
                // die uebrigen Tests der Klasse erwarten die geladene Umgebung wieder
                Reichsdaten.LadeAlleReiche(_umgebung.Zugverzeichnis, ProgramView.SelectedMonth,
                    Mehrreichumgebung.Klartext());
            }
        }
    }
}
