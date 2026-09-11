namespace PhoenixModel.ViewModel {

    /// <summary>
    /// Eine Momentaufnahme aller Werte einer Spielfigur, die durch eine Bewegung verändert werden.
    ///
    /// Damit lässt sich eine Bewegung exakt zurücknehmen, ohne die einzelnen Schritte rückwärts
    /// nachrechnen zu müssen. Das ist bewusst so gelöst, weil in der Altanwendung genau diese
    /// Rückrechnung die Ursache für doppelte oder verlorene Truppen beim Undo war.
    /// </summary>
    public class BewegungsZustand {

        private readonly Spielfigur _figur;
        private readonly int _gf_nach;
        private readonly int _kf_nach;
        private readonly int _bp;
        private readonly int _hoehenstufen;
        private readonly int _schritt;
        private readonly string _ph_xy;
        private readonly string? _befehl_bew;
        private readonly string? _befehl_erobert;
        private readonly int[] _wegpunkteGf;
        private readonly int[] _wegpunkteKf;

        private BewegungsZustand(Spielfigur figur) {
            _figur = figur;
            _gf_nach = figur.gf_nach;
            _kf_nach = figur.kf_nach;
            _bp = figur.bp;
            _hoehenstufen = figur.hoehenstufen;
            _schritt = figur.schritt;
            _ph_xy = figur.ph_xy;

            if (figur is TruppenSpielfigur truppe) {
                _befehl_bew = truppe.Befehl_bew;
                _befehl_erobert = truppe.Befehl_erobert;
            }

            var spur = new Bewegungsspur(figur);
            _wegpunkteGf = new int[Bewegungsspur.MaxWegpunkteTruppe];
            _wegpunkteKf = new int[Bewegungsspur.MaxWegpunkteTruppe];
            for (int i = 0; i < Bewegungsspur.MaxWegpunkteTruppe; i++) {
                var punkt = spur[i];
                _wegpunkteGf[i] = punkt?.gf ?? 0;
                _wegpunkteKf[i] = punkt?.kf ?? 0;
            }
        }

        /// <summary>
        /// Erzeugt den Zustand, den die Figur zu Beginn des Zuges hatte
        /// </summary>
        private BewegungsZustand(Spielfigur figur, bool zugStart) {
            _figur = figur;
            _gf_nach = 0;
            _kf_nach = 0;
            _bp = figur.bp_max;
            _hoehenstufen = 0;
            _schritt = 0;
            _ph_xy = figur.ph_xy;
            _befehl_bew = figur is TruppenSpielfigur truppe ? truppe.Befehl_bew : null;
            _befehl_erobert = string.Empty;
            _wegpunkteGf = new int[Bewegungsspur.MaxWegpunkteTruppe];
            _wegpunkteKf = new int[Bewegungsspur.MaxWegpunkteTruppe];
        }

        /// <summary>
        /// Sichert den aktuellen Bewegungszustand der Figur
        /// </summary>
        public static BewegungsZustand Sichern(Spielfigur figur) => new(figur);

        /// <summary>
        /// Erzeugt den Zustand, in dem sich die Figur zu Beginn des Zuges befunden hat.
        /// Wird benötigt, um eine aus der Datenbank rekonstruierte Bewegung zurücknehmen zu können.
        /// Dabei wird davon ausgegangen, dass die Bewegungspunkte zu Zugbeginn vollständig und
        /// die Höhenstufen sowie die Eroberungsbefehle leer waren.
        /// </summary>
        public static BewegungsZustand ZugStart(Spielfigur figur) => new(figur, true);

        /// <summary>
        /// Die Figur, zu der dieser Zustand gehört
        /// </summary>
        public Spielfigur Figur => _figur;

        /// <summary>
        /// Die Position, auf der die Figur zum Zeitpunkt der Sicherung stand
        /// </summary>
        public KleinfeldPosition Position => new(_gf_nach == 0 ? _figur.gf_von : _gf_nach, _kf_nach == 0 ? _figur.kf_von : _kf_nach);

        /// <summary>
        /// Setzt die Figur exakt auf den gesicherten Zustand zurück
        /// </summary>
        public void Wiederherstellen() {
            _figur.gf_nach = _gf_nach;
            _figur.kf_nach = _kf_nach;
            _figur.bp = _bp;
            _figur.hoehenstufen = _hoehenstufen;
            _figur.ph_xy = _ph_xy;

            if (_figur is TruppenSpielfigur truppe) {
                truppe.Befehl_bew = _befehl_bew ?? string.Empty;
                truppe.Befehl_erobert = _befehl_erobert ?? string.Empty;
            }

            var spur = new Bewegungsspur(_figur);
            spur.Clear();
            for (int i = 0; i < Bewegungsspur.MaxWegpunkteTruppe; i++) {
                if (_wegpunkteGf[i] == 0)
                    break;
                spur.Add(new KleinfeldPosition(_wegpunkteGf[i], _wegpunkteKf[i]));
            }
            // der Schrittzähler wird durch Add hochgezählt, daher zum Schluss auf den Originalwert setzen
            _figur.schritt = _schritt;
        }
    }
}
