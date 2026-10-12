using PhoenixModel.Database;
using System.Data;
using System.Data.Common;

namespace Tests {

    /// <summary>
    /// Prüft den <see cref="Datenbankwaechter"/> - den Besitzer der Verbindung zu einer
    /// Datenbankdatei.
    ///
    /// <b>Ohne Datenbank und ohne Treiber.</b> Der Wächter kennt nur
    /// <see cref="DbConnection"/> und einen Erzeuger; was dieser baut, ist seine Sache. Hier baut
    /// er eine <see cref="Attrappe"/>, die mitzählt, wie oft sie geöffnet und geschlossen wurde.
    /// Das ist nicht Bequemlichkeit: an einer echten Access-Datei liesse sich gerade das nicht
    /// prüfen, was hier wichtig ist - dass nämlich <em>keine zweite</em> Verbindung entsteht und
    /// dass ein gescheitertes Öffnen das Schloss wieder hergibt. Beides wäre an einer .mdb nur als
    /// Absturz oder als Hänger zu sehen, also zu spät.
    ///
    /// Jeder Test benutzt einen eigenen Schlüssel: die Wächter stehen in einem statischen
    /// Verzeichnis und leben bis zum Ende des Prozesses.
    /// </summary>
    public class DatenbankwaechterTest {

        /// <summary>
        /// Eine Verbindung, die nichts verbindet und alles mitschreibt.
        /// </summary>
        private sealed class Attrappe : DbConnection {
            public int Geöffnet = 0;
            public int Geschlossen = 0;
            public bool WirftBeimÖffnen = false;
            public ConnectionState Zustand = ConnectionState.Closed;

            [System.Diagnostics.CodeAnalysis.AllowNull]
            public override string ConnectionString { get => _zeichenfolge; set => _zeichenfolge = value ?? string.Empty; }
            private string _zeichenfolge = string.Empty;
            public override string Database => "Attrappe";
            public override string DataSource => "Attrappe";
            public override string ServerVersion => "0";
            public override ConnectionState State => Zustand;

            public override void ChangeDatabase(string databaseName) { }

            public override void Close() {
                Geschlossen++;
                Zustand = ConnectionState.Closed;
            }

            public override void Open() {
                Geöffnet++;
                if (WirftBeimÖffnen)
                    throw new InvalidOperationException("Die Attrappe soll beim Öffnen scheitern");
                Zustand = ConnectionState.Open;
            }

            protected override DbCommand CreateDbCommand() => throw new NotSupportedException();
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
        }

        private static string NeuerSchlüssel()
            => $"Provider=Attrappe;Data Source=C:\\irgendwo\\{Guid.NewGuid():N}.mdb;Jet OLEDB:Database Password=geheim;";

        [Fact]
        public void ZweiZugriffeHintereinanderBenutzenDieselbeVerbindung() {
            var attrappen = new List<Attrappe>();
            var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => {
                var neu = new Attrappe();
                attrappen.Add(neu);
                return neu;
            });

            var erste = wächter.Betrete(out string fehler1);
            Assert.NotNull(erste);
            Assert.Equal(string.Empty, fehler1);
            wächter.Verlasse();

            var zweite = wächter.Betrete(out string fehler2);
            Assert.NotNull(zweite);
            Assert.Equal(string.Empty, fehler2);
            wächter.Verlasse();

            // Darum geht die ganze Übung: der Treiber stirbt an der Zahl der Verbindungen.
            Assert.Single(attrappen);
            Assert.Same(erste, zweite);
            Assert.Equal(1, attrappen[0].Geöffnet);
            Assert.Equal(0, attrappen[0].Geschlossen);

            wächter.Schliesse();
        }

        [Fact]
        public void DerselbeThreadDarfVerschachteln() {
            // Die Lader öffnen innerhalb eines Ladevorgangs mehrere Verbindungen zur selben Datei
            // nacheinander und teils verschachtelt. Wäre das Schloss nicht wiedereintrittsfähig,
            // würde die Anwendung sich selbst aussperren und nach der Geduld aufgeben.
            var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => new Attrappe());

            var aussen = wächter.Betrete(out _);
            Assert.NotNull(aussen);
            var innen = wächter.Betrete(out string fehler);
            Assert.NotNull(innen);
            Assert.Equal(string.Empty, fehler);
            Assert.Same(aussen, innen);

            wächter.Verlasse();
            wächter.Verlasse();
            wächter.Schliesse();
        }

        [Fact]
        public async Task EinFremderZugriffWartetUndGibtAuf() {
            var vorher = Datenbankwaechter.Geduld;
            Datenbankwaechter.Geduld = TimeSpan.FromMilliseconds(150);
            try {
                var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => new Attrappe());
                Assert.NotNull(wächter.Betrete(out _));
                try {
                    // Ein anderer Thread, weil das Schloss je Thread wiedereintrittsfähig ist -
                    // auf demselben Thread käme er durch, und der Test würde nichts prüfen.
                    string fehler = string.Empty;
                    DbConnection? ergebnis = null;
                    var fremder = Task.Run(() => { ergebnis = wächter.Betrete(out fehler); });
                    var geduld = Task.Delay(TimeSpan.FromSeconds(10));
                    Assert.Same(fremder, await Task.WhenAny(fremder, geduld));

                    // Aufgeben heisst: null und eine Begründung. Früher stand hier das Gegenteil -
                    // es wurde eine eigene Verbindung geöffnet, also genau die Betriebsart, an der
                    // der Treiber stirbt, und zwar dann, wenn ohnehin schon viel los ist.
                    Assert.Null(ergebnis);
                    Assert.Contains("belegt", fehler);
                    // Der Dateiname gehört in die Meldung, das Passwort nicht.
                    Assert.Contains(".mdb", fehler);
                    Assert.DoesNotContain("geheim", fehler);
                }
                finally {
                    wächter.Verlasse();
                    wächter.Schliesse();
                }
            }
            finally {
                Datenbankwaechter.Geduld = vorher;
            }
        }

        [Fact]
        public async Task EinGescheitertesOeffnenSperrtDieDateiNichtAus() {
            // Der Fall, der ohne diesen Test unbemerkt bliebe: scheitert das Öffnen, muss das
            // Schloss wieder hergegeben werden. Sonst wäre die Datei nach einem einzigen
            // Fehlversuch bis zum Ende des Prozesses für jeden anderen Ablauf unerreichbar - und
            // der nächste Versuch würde nicht scheitern, sondern die volle Geduld lang hängen.
            //
            // Der zweite Versuch läuft ausdrücklich auf einem <b>anderen Thread</b>. Auf demselben
            // käme er auch durch ein nie hergegebenes Schloss hindurch, weil der Monitor je Thread
            // wiedereintrittsfähig ist - der Test hätte dann nichts geprüft. Mit dem Versuch auf
            // demselben Thread ist er stillschweigend grün geblieben, als das Hergeben entfernt
            // wurde. Und der fremde Thread ist ohnehin der wirkliche Fall: gelesen wird in der
            // Oberfläche, geschrieben im Hintergrund.
            var vorher = Datenbankwaechter.Geduld;
            Datenbankwaechter.Geduld = TimeSpan.FromMilliseconds(150);
            try {
                bool scheitern = true;
                var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => new Attrappe { WirftBeimÖffnen = scheitern });

                Assert.Null(wächter.Betrete(out string fehler));
                Assert.Contains("liess sich nicht öffnen", fehler);

                scheitern = false;
                DbConnection? zweiterVersuch = null;
                string fehler2 = string.Empty;
                var fremder = Task.Run(() => {
                    zweiterVersuch = wächter.Betrete(out fehler2);
                    if (zweiterVersuch != null)
                        wächter.Verlasse();
                });
                var geduld = Task.Delay(TimeSpan.FromSeconds(10));
                Assert.Same(fremder, await Task.WhenAny(fremder, geduld));

                Assert.NotNull(zweiterVersuch);
                Assert.Equal(string.Empty, fehler2);
                wächter.Schliesse();
            }
            finally {
                Datenbankwaechter.Geduld = vorher;
            }
        }

        [Fact]
        public void EineKaputteVerbindungWirdErsetzt() {
            var attrappen = new List<Attrappe>();
            var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => {
                var neu = new Attrappe();
                attrappen.Add(neu);
                return neu;
            });

            var erste = (Attrappe)wächter.Betrete(out _)!;
            wächter.Verlasse();

            // Der Treiber bricht unter uns weg. Ein Zugriff auf diese Verbindung würde an einer
            // Stelle werfen, die nichts dafür kann.
            erste.Zustand = ConnectionState.Broken;

            var zweite = wächter.Betrete(out string fehler);
            Assert.NotNull(zweite);
            Assert.Equal(string.Empty, fehler);
            Assert.NotSame(erste, zweite);
            Assert.Equal(2, attrappen.Count);
            wächter.Verlasse();
            wächter.Schliesse();
        }

        [Fact]
        public void SchliessenGibtDieDateiFrei() {
            // Das ist der Preis des Offenhaltens: solange die Verbindung steht, hält sie die Datei,
            // und Access legt eine .ldb daneben. Wer die Datei kopieren oder packen will, muss sie
            // freigeben können - sonst wäre die Zugabgabe nicht möglich.
            var attrappe = new Attrappe();
            var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => attrappe);

            Assert.NotNull(wächter.Betrete(out _));
            wächter.Verlasse();
            Assert.True(wächter.IstOffen);

            wächter.Schliesse();

            Assert.False(wächter.IstOffen);
            Assert.Equal(1, attrappe.Geschlossen);

            // Und danach arbeitet alles weiter: die nächste Benutzung öffnet eine neue.
            Assert.NotNull(wächter.Betrete(out _));
            wächter.Verlasse();
            Assert.True(wächter.IstOffen);
            wächter.Schliesse();
        }

        [Fact]
        public void EinOffenerWaechterWirdGezaehlt() {
            var wächter = Datenbankwaechter.Hole(NeuerSchlüssel(), () => new Attrappe());
            int vorher = Datenbankwaechter.AnzahlOffenerVerbindungen;

            Assert.NotNull(wächter.Betrete(out _));
            wächter.Verlasse();
            Assert.Equal(vorher + 1, Datenbankwaechter.AnzahlOffenerVerbindungen);

            wächter.Schliesse();
            Assert.Equal(vorher, Datenbankwaechter.AnzahlOffenerVerbindungen);
        }
    }
}
