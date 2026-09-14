using PhoenixModel.Database;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.View;

namespace Tests {

    /// <summary>
    /// Der Umgang mit verschluesselten Passwoertern.
    ///
    /// Hier lag eine Falle: EncryptedString laesst sich implizit aus einem string bilden, und
    /// PasswordHolder hatte zusaetzlich einen Konstruktor mit string, der sein Argument als
    /// Klartext ansah und verschluesselte. Bei einem Aufruf mit einer Zeichenkette gewann immer
    /// dieser - auch dort, wo die Zeichenkette bereits ein verschluesseltes Passwort war. Das
    /// Passwort wurde dann ein zweites Mal verschluesselt, DecryptedPassword lieferte den
    /// Base64-Text statt des Klartextes, und die Datenbank liess sich damit nicht oeffnen.
    /// </summary>
    public class PasswortTest {

        private static List<LogEntry> SammleBeim(Action aktion) {
            List<LogEntry> gesammelt = [];
            void Horcher(object? sender, ViewEventArgs e) {
                if (e.LogEntry != null)
                    lock (gesammelt) gesammelt.Add(e.LogEntry);
            }
            ProgramView.OnViewEvent += Horcher;
            try { aktion(); }
            finally { ProgramView.OnViewEvent -= Horcher; }
            return gesammelt;
        }

        /// <summary>
        /// Ein Klartextpasswort ueberlebt Verschluesseln und Entschluesseln unveraendert.
        /// </summary>
        [Fact]
        public void KlartextUeberstehtDenHinUndRueckweg() {
            const string klartext = "MeinGeheimes!42";
            var verschluesselt = PasswordHolder.AusKlartext(klartext).EncryptedPasswordBase64;

            Assert.NotEqual(klartext, (string)verschluesselt);
            Assert.Equal(klartext, new PasswordHolder(verschluesselt).DecryptedPassword);
        }

        /// <summary>
        /// Der Kern der Sache: ein verschluesseltes Passwort, das als gewoehnliche Zeichenkette
        /// daherkommt, darf nicht noch einmal verschluesselt werden.
        /// </summary>
        [Fact]
        public void EinVerschluesseltesPasswortWirdNichtNochEinmalVerschluesselt() {
            const string klartext = "MeinGeheimes!42";
            string verschluesselt = PasswordHolder.AusKlartext(klartext).EncryptedPasswordBase64;

            // Genau so steht es an vielen Stellen im Programm: ein string aus den Einstellungen.
            PasswordHolder holder = new(verschluesselt);

            Assert.Equal(klartext, holder.DecryptedPassword);
            // frueher kam hier der Base64-Text heraus
            Assert.NotEqual(verschluesselt, holder.DecryptedPassword);
        }

        /// <summary>
        /// Laesst sich ein hinterlegtes Passwort nicht entschluesseln, sagt das jemand. Vorher ging
        /// die Meldung auf die Konsole, die niemand sieht, und zurueck kam eine leere Zeichenkette -
        /// mit der sich keine Datenbank oeffnet, ohne dass klar wuerde warum.
        /// </summary>
        [StaFact]
        public void EinUnlesbaresPasswortMeldetSich() {
            PasswordHolder.EncryptedString unsinn = "kein gueltiger Block";
            string? ergebnis = null;
            var meldungen = SammleBeim(() => ergebnis = new PasswordHolder(unsinn).DecryptedPassword);

            Assert.Equal(string.Empty, ergebnis);
            Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Warning);
            Assert.Contains(meldungen, m => m.Message.Contains("Rechnernamen"));
        }

        /// <summary>
        /// Ohne hinterlegtes Passwort gibt es nichts zu entschluesseln - das ist der Normalfall beim
        /// ersten Start und darf niemanden behelligen.
        /// </summary>
        [StaFact]
        public void EinLeeresPasswortMeldetSichNicht() {
            PasswordHolder.EncryptedString leer = string.Empty;
            var meldungen = SammleBeim(() => Assert.Equal(string.Empty, new PasswordHolder(leer).DecryptedPassword));
            Assert.Empty(meldungen);
        }

        /// <summary>
        /// Klartext, der als verschluesseltes Passwort abgelegt wird, faellt sofort auf.
        ///
        /// Genau das ist passiert: StartDialog.Password ist Klartext, und nachdem der
        /// string-Konstruktor weg war, landete er unveraendert in den Einstellungen. Beim naechsten
        /// Start liess er sich nicht entschluesseln, und die Anwendung fragte das Passwort jedes
        /// Mal neu ab - ohne einen Hinweis, woran es liegt.
        /// </summary>
        [StaFact]
        public void KlartextAlsVerschluesseltesPasswortFaelltAuf() {
            PasswordHolder.EncryptedString klartext = "geheim123";
            var meldungen = SammleBeim(() => new PasswordHolder(klartext));

            Assert.Contains(meldungen, m => m.Type == LogEntry.LogType.Warning);
            Assert.Contains(meldungen, m => m.Titel.Contains("unverschlüsselt"));
        }

        /// <summary>
        /// Ein richtig verschluesseltes Passwort loest keine Meldung aus - sonst waere die Warnung
        /// bei jedem Start zu sehen und damit wertlos.
        /// </summary>
        [StaFact]
        public void EinRichtigVerschluesseltesPasswortMeldetSichNicht() {
            var verschluesselt = PasswordHolder.AusKlartext("MeinGeheimes!42").EncryptedPasswordBase64;
            var meldungen = SammleBeim(() => new PasswordHolder(verschluesselt));
            Assert.Empty(meldungen);
        }
    }
}
