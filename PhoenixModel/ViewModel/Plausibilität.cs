namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Statische Klasse zur Überprüfung der Plausibilität von Spielfiguren und Positionen.
    /// </summary>
    public static class Plausibilität {

        /// <summary>
        /// Überprüft, ob eine Spielfigur eine gültige Position hat.
        /// </summary>
        /// <param name="spielfigur">Die zu überprüfende Spielfigur.</param>
        /// <returns>True, wenn die Position gültig ist, sonst false.</returns>
        public static bool IsValid(Spielfigur spielfigur) {
            return IsValid(spielfigur as KleinfeldPosition);
        }

        /// <summary>
        /// Überprüft, ob eine gegebene KleinfeldPosition auf der Karte existiert.
        /// </summary>
        /// <param name="pos">Die zu überprüfende Position.</param>
        /// <returns>True, wenn die Position auf der Karte existiert, sonst false.</returns>
        public static bool IsOnMap(KleinfeldPosition? pos) {
            if (!IsValid(pos) || pos == null)
                return false;
            if (SharedData.Map == null)
                return false;
            return SharedData.Map.ContainsKey(pos.CreateBezeichner());
        }

        /// <summary>
        /// Überprüft, ob eine KleinfeldPosition gültig ist.
        /// </summary>
        /// <param name="position">Die zu überprüfende Position.</param>
        /// <returns>True, wenn die Position gültig ist, sonst false.</returns>
        public static bool IsValid(KleinfeldPosition? position) {
            return position != null && position.gf > 0 && position.kf > 0 && position.kf <= 48;
        }
    }
}
