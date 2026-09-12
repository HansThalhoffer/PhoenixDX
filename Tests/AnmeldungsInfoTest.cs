using PhoenixDX.Helper;
using PhoenixModel.View;
using PhoenixModel.ViewModel;
using System.Xml.Linq;

namespace Tests {

    /// <summary>
    /// Der Zustandsbericht des XmlStateServer soll sagen, welches Reich angemeldet ist und
    /// welcher Zug vorliegt.
    /// </summary>
    public class AnmeldungsInfoTest {

        private static void LadeAlles() {
            TestSetup.Setup();
            TestSetup.LoadCrossRef(false, false);
            TestSetup.LoadKarte();
            TestSetup.LoadPZE(false, false);
            TestSetup.LoadZugdaten(false, false);
        }

        /// <summary>Das Fragment allein ist kein Dokument - zum Prüfen bekommt es eine Wurzel</summary>
        private static XElement Lies(string fragment)
            => XElement.Parse("<XmlStateServer>" + fragment + "</XmlStateServer>").Element("Anmeldung")!;

        [StaFact]
        public void DerBerichtNenntReichUndZug() {
            LadeAlles();
            var anmeldung = Lies(AnmeldungsInfo.AlsXml());
            Assert.NotNull(anmeldung);
            Assert.Equal("true", anmeldung.Attribute("angemeldet")?.Value);

            var reich = anmeldung.Element("Reich");
            Assert.NotNull(reich);
            Assert.Equal(ProgramView.SelectedNation!.Reich, reich!.Value);
            Assert.Equal(ProgramView.SelectedNation.Nummer.ToString(), reich.Attribute("Nummer")?.Value);

            var zug = anmeldung.Element("Zug");
            Assert.NotNull(zug);
            Assert.Equal(ZugView.AktuellerZug.Zug.ToString(), zug!.Attribute("Nummer")?.Value);
            Assert.Equal(ZugView.AktuellerZug.Beschreibung, zug.Value);
            Assert.Equal(ZugView.AktuellerZug.Jahr.ToString(), anmeldung.Element("Jahr")?.Value);
            Assert.Equal(ZugView.Phase.ToString(), anmeldung.Element("Phase")?.Value);
        }

        /// <summary>
        /// Die Phase wechselt im Betrieb, der Bericht liest deshalb bei jedem Abruf neu.
        /// </summary>
        [StaFact]
        public void DerBerichtFolgtDerPhase() {
            LadeAlles();
            Assert.NotNull(ZugView.Settings);
            int phaseVorher = ZugView.Settings!.Phase;
            try {
                ZugView.Settings.Phase = (int)Zugphase.Rüstphase;
                Assert.Equal("Rüstphase", Lies(AnmeldungsInfo.AlsXml()).Element("Phase")?.Value);

                ZugView.Settings.Phase = (int)Zugphase.Bewegungsphase;
                Assert.Equal("Bewegungsphase", Lies(AnmeldungsInfo.AlsXml()).Element("Phase")?.Value);
            }
            finally {
                ZugView.Settings.Phase = phaseVorher;
            }
        }

        /// <summary>
        /// Der Zug steht im Verzeichnisnamen und in der Tabelle settings der Zugdatenbank. Laufen
        /// beide auseinander, liegt die falsche Datei im Verzeichnis - das gehört in den Bericht.
        /// </summary>
        [StaFact]
        public void EineAbweichendeMonatsangabeDerDatenbankStehtImBericht() {
            LadeAlles();
            Assert.NotNull(ZugView.Settings);
            int monatVorher = ZugView.Settings!.Monat;
            try {
                ZugView.Settings.Monat = ZugView.AktuellerZug.Zug;
                Assert.Null(Lies(AnmeldungsInfo.AlsXml()).Element("MonatLautDatenbank"));

                ZugView.Settings.Monat = ZugView.AktuellerZug.Zug + 1;
                Assert.Equal((ZugView.AktuellerZug.Zug + 1).ToString(),
                    Lies(AnmeldungsInfo.AlsXml()).Element("MonatLautDatenbank")?.Value);
            }
            finally {
                ZugView.Settings.Monat = monatVorher;
            }
        }

        /// <summary>
        /// Vor der Anmeldung steht noch kein Reich fest. Der Bericht muss trotzdem lesbares XML
        /// liefern, sonst ist das ganze Dokument kaputt.
        /// </summary>
        [StaFact]
        public void OhneAnmeldungBleibtDerBerichtLesbar() {
            LadeAlles();
            var nationVorher = ProgramView.SelectedNation;
            int monatVorher = ProgramView.SelectedMonth;
            try {
                ProgramView.SelectedNation = null;
                ProgramView.SelectedMonth = 0;

                var anmeldung = Lies(AnmeldungsInfo.AlsXml());
                Assert.Equal("false", anmeldung.Attribute("angemeldet")?.Value);
                Assert.Null(anmeldung.Element("Reich"));
                Assert.Null(anmeldung.Element("Zug"));
            }
            finally {
                ProgramView.SelectedNation = nationVorher;
                ProgramView.SelectedMonth = monatVorher;
            }
        }

        /// <summary>
        /// Der Server antwortet jedem, der den Port erreicht. Datenbankname und Passwort des
        /// Reiches haben darin nichts verloren.
        /// </summary>
        [StaFact]
        public void DerBerichtVerraetKeineZugangsdaten() {
            LadeAlles();
            string xml = AnmeldungsInfo.AlsXml();
            var nation = ProgramView.SelectedNation!;

            Assert.DoesNotContain("DBname", xml);
            Assert.DoesNotContain("DBpass", xml);
            Assert.DoesNotContain("Passwor", xml);
            if (string.IsNullOrEmpty(nation.DBpass) == false)
                Assert.DoesNotContain(nation.DBpass, xml);
        }

        /// <summary>
        /// Reichsnamen sind freier Text aus der Datenbank. Kaeme ein Sonderzeichen darin vor,
        /// duerfte es das Dokument nicht zerreissen.
        /// </summary>
        [StaFact]
        public void SonderzeichenImReichsnamenZerreissenDasDokumentNicht() {
            LadeAlles();
            var nationVorher = ProgramView.SelectedNation;
            try {
                ProgramView.SelectedNation = new PhoenixModel.dbPZE.Nation("Haus <Rot> & \"Grün\"") { Nummer = 99 };
                var anmeldung = Lies(AnmeldungsInfo.AlsXml());
                Assert.Equal("Haus <Rot> & \"Grün\"", anmeldung.Element("Reich")?.Value);
            }
            finally {
                ProgramView.SelectedNation = nationVorher;
            }
        }
    }
}
