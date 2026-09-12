using System.Text;
using PhoenixModel.View;

namespace PhoenixDX.Helper {

    /// <summary>
    /// Der Ausschnitt des Zustandsberichts, der sagt, wer angemeldet ist und mit welchem Zug
    /// gearbeitet wird. Geliefert wird er vom <see cref="XmlStateServer"/>.
    ///
    /// Eigene Klasse, damit sich der Bericht ohne den HTTP-Server erzeugen und prüfen lässt -
    /// der Server bindet beim ersten Zugriff seinen Port.
    /// </summary>
    public static class AnmeldungsInfo {

        /// <summary>
        /// Meldet, welches Reich angemeldet ist und mit welchem Zug die Anwendung arbeitet.
        ///
        /// Beides wird in der Anmeldemaske gewählt und kann sich im Betrieb noch ändern, wenn der
        /// Zug gewechselt wird - deshalb wird bei jedem Abruf neu gelesen und nichts gemerkt.
        ///
        /// Der Zug steht an zwei Stellen: im Namen des Zugdatenverzeichnisses, mit dem die
        /// Anwendung arbeitet, und in der Tabelle settings der Zugdatenbank. Laufen beide
        /// auseinander, liegt die falsche Datei im Verzeichnis; deshalb wird eine abweichende
        /// Angabe der Datenbank mitgemeldet.
        ///
        /// Absichtlich nicht enthalten sind Datenbankname und Passwort des Reiches. Der Server
        /// antwortet jedem, der den Port erreicht.
        /// </summary>
        /// <example>
        /// &lt;Anmeldung angemeldet="true"&gt;
        ///   &lt;Reich Nummer="12"&gt;Theostelos&lt;/Reich&gt;
        ///   &lt;Zug Nummer="170"&gt;Rim (Frühling)&lt;/Zug&gt;
        ///   &lt;Jahr&gt;21&lt;/Jahr&gt;
        ///   &lt;Phase&gt;Bewegungsphase&lt;/Phase&gt;
        /// &lt;/Anmeldung&gt;
        /// </example>
        public static string AlsXml() {
            StringBuilder sb = new StringBuilder();
            var nation = ProgramView.SelectedNation;
            var zug = ZugView.AktuellerZug;
            bool angemeldet = nation != null && zug.IstGültig;

            sb.Append("<Anmeldung angemeldet=\"" + (angemeldet ? "true" : "false") + "\">");
            if (nation != null)
                sb.Append("<Reich Nummer=\"" + nation.Nummer + "\">" + Escape(nation.Reich) + "</Reich>");
            if (zug.IstGültig) {
                sb.Append("<Zug Nummer=\"" + zug.Zug + "\">" + Escape(zug.Beschreibung) + "</Zug>");
                sb.Append("<Jahr>" + zug.Jahr + "</Jahr>");
                sb.Append("<Phase>" + Escape(ZugView.Phase.ToString()) + "</Phase>");

                var lautDatenbank = ZugView.MonatLautDatenbank;
                if (lautDatenbank != null && lautDatenbank.Equals(zug) == false)
                    sb.Append("<MonatLautDatenbank>" + lautDatenbank.Zug + "</MonatLautDatenbank>");
            }
            sb.Append("</Anmeldung>");

            return sb.ToString();
        }

        /// <summary>
        /// Reichsnamen und Beschreibungen sind freier Text und können Zeichen enthalten, die in XML
        /// eine Bedeutung haben
        /// </summary>
        private static string Escape(string? text)
            => string.IsNullOrEmpty(text) ? string.Empty : System.Security.SecurityElement.Escape(text);
    }
}
