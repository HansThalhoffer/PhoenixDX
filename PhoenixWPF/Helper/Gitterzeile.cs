using System.Collections;

namespace PhoenixWPF.Helper {

    /// <summary>
    /// Findet die Zeile eines Objekts in einem Gitter.
    ///
    /// <b>Warum das eine eigene Funktion ist und nicht <c>IndexOf</c>:</b> eine
    /// <see cref="PhoenixModel.ViewModel.Spielfigur"/> erbt von
    /// <see cref="PhoenixModel.ViewModel.KleinfeldPosition"/> ein <c>Equals</c> nach gf und kf.
    /// Zwei verschiedene Figuren auf demselben Feld sind damit <c>Equals</c>, und jede Suche über
    /// <c>IndexOf</c>, <c>Contains</c> oder <c>Distinct</c> liefert die erste Figur <em>des
    /// Feldes</em> statt der gesuchten.
    ///
    /// In einer Figurenliste ist das ein Fehler mit Ansage: nach einer Bewegung steht die gesuchte
    /// Figur auf einem neuen Feld, auf dem schon etwas anderes stehen kann. Dann wurden die Werte
    /// in eine fremde Zeile geschrieben - und war die fremde Figur von einem anderen Typ, kam es
    /// nicht einmal so weit, weil Krieger und Zauberer unterschiedlich viele Eigenschaften haben.
    /// </summary>
    public static class Gitterzeile {

        /// <summary>
        /// Die Zeilennummer des gesuchten Objekts, über die <b>Identität</b> bestimmt.
        /// </summary>
        /// <param name="zeilen">
        /// die Zeilen des Gitters in der Reihenfolge, in der sie angezeigt werden - also
        /// <c>DataGrid.Items</c> und nicht die zugrunde liegende Liste. Die beiden unterscheiden
        /// sich, sobald jemand eine Spalte zum Sortieren angeklickt hat, und wer in die falsche
        /// Zeile schreibt, schreibt in eine fremde Figur.
        /// </param>
        /// <param name="gesucht">das Objekt, dessen Zeile gesucht wird</param>
        /// <returns>die Zeilennummer, oder -1, wenn das Objekt nicht angezeigt wird</returns>
        public static int Finde(IList? zeilen, object? gesucht) {
            if (zeilen == null || gesucht == null)
                return -1;
            for (int zeile = 0; zeile < zeilen.Count; zeile++) {
                if (ReferenceEquals(zeilen[zeile], gesucht))
                    return zeile;
            }
            return -1;
        }
    }
}
