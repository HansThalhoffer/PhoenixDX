using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using static PhoenixModel.ExternalTables.GeländeTabelle;

namespace PhoenixModel.ViewModel {
    /// <summary>
    /// Diese Klasse speichert das erwartete Einkommen eines Reiches basierend auf verschiedenen Geländearten und Gebäudetypen.
    /// </summary>
    public class ExpectedIncome {

        /// <summary>
        /// Der Name des Reiches.
        /// </summary>
        public string Reich { get; set; } = string.Empty;

        /// <summary>
        /// Das gesamte Einkommen des Reiches.
        /// </summary>
        public int GesamtEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Tieflandfelder.
        /// </summary>
        public int TieflandFelder { get; set; }

        /// <summary>
        /// Einkommen aus Tieflandfeldern.
        /// </summary>
        public int TieflandEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Tiefland-Waldfelder.
        /// </summary>
        public int TieflandwaldFelder { get; set; }

        /// <summary>
        /// Einkommen aus Tiefland-Waldfeldern.
        /// </summary>
        public int TieflandwaldEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Tiefland-Wüstenfelder.
        /// </summary>
        public int TieflandwüsteFelder { get; set; }

        /// <summary>
        /// Einkommen aus Tiefland-Wüstenfeldern.
        /// </summary>
        public int TieflandwüsteEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Tiefland-Sumpffelder.
        /// </summary>
        public int TieflandsumpfFelder { get; set; }

        /// <summary>
        /// Einkommen aus Tiefland-Sumpffeldern.
        /// </summary>
        public int TieflandsumpfEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Hochlandfelder.
        /// </summary>
        public int HochlandFelder { get; set; }

        /// <summary>
        /// Einkommen aus Hochlandfeldern.
        /// </summary>
        public int HochlandEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Bergfelder.
        /// </summary>
        public int BergFelder { get; set; }

        /// <summary>
        /// Einkommen aus Bergfeldern.
        /// </summary>
        public int BergEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Gebirgsfelder.
        /// </summary>
        public int GebirgFelder { get; set; }

        /// <summary>
        /// Einkommen aus Gebirgsfeldern.
        /// </summary>
        public int GebirgEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Burgen.
        /// </summary>
        public int Burgen { get; set; }

        /// <summary>
        /// Einkommen aus Burgen.
        /// </summary>
        public int BurgenEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Städte.
        /// </summary>
        public int Städte { get; set; }

        /// <summary>
        /// Einkommen aus Städten.
        /// </summary>
        public int StädteEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Festungen.
        /// </summary>
        public int Festungen { get; set; }

        /// <summary>
        /// Einkommen aus Festungen.
        /// </summary>
        public int FestungenEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Hauptstädte.
        /// </summary>
        public int Hauptstädte { get; set; }

        /// <summary>
        /// Einkommen aus Hauptstädten.
        /// </summary>
        public int HauptstädteEinkommen { get; set; }

        /// <summary>
        /// Anzahl der Festungs-Hauptstädte.
        /// </summary>
        public int FestungsHauptstädte { get; set; }

        /// <summary>
        /// Einkommen aus Festungs-Hauptstädten.
        /// </summary>
        public int FestungsHauptstädteEinkommen { get; set; }

        /// <summary>
        /// erwartete Einnahmen des Reiches
        /// </summary>
        /// <param name="reich"></param>
        public ExpectedIncome(Nation reich) {
            this.Reich = reich.Reich;
            if (SharedData.Map != null) {
                var kleinfelder = SharedData.Map.Values.Where(k => k.Nation == reich);
                this.GesamtEinkommen = EinnahmenView.GetReichEinnahmen(reich);
                // Einnahmen der Felder
                this.BergFelder = kleinfelder.Where(k => k.Terrain.Typ == TerrainType.Bergland).Count();
                this.BergEinkommen = BergFelder * Terrains[(int)TerrainType.Bergland].Einnahmen;
                this.TieflandFelder = kleinfelder.Where(k => k.TerrainType == TerrainType.Tiefland).Count();
                this.TieflandEinkommen = TieflandFelder * Terrains[(int)TerrainType.Tiefland].Einnahmen;
                this.TieflandwaldFelder = kleinfelder.Where(k => k.TerrainType == TerrainType.Wald).Count();
                this.TieflandwaldEinkommen = TieflandwaldFelder * Terrains[(int)TerrainType.Wald].Einnahmen;
                this.TieflandwüsteFelder = kleinfelder.Where(k => k.TerrainType == TerrainType.Wüste).Count();
                this.TieflandwüsteEinkommen = TieflandwüsteFelder * Terrains[(int)TerrainType.Wüste].Einnahmen;
                this.TieflandsumpfFelder = kleinfelder.Where(k => k.TerrainType == TerrainType.Sumpf).Count();
                this.TieflandsumpfEinkommen = TieflandsumpfFelder * Terrains[(int)TerrainType.Sumpf].Einnahmen;
                this.HochlandFelder = kleinfelder.Where(k => k.TerrainType == TerrainType.Hochland).Count();
                this.HochlandFelder = HochlandFelder * Terrains[(int)TerrainType.Tiefland].Einnahmen;
                this.GebirgFelder = kleinfelder.Where(k => k.TerrainType == TerrainType.Gebirge).Count();
                this.GebirgEinkommen = GebirgFelder * Terrains[(int)TerrainType.Gebirge].Einnahmen;

                // Einnahmen der Bauten - die Bauten können auch Burg-I etc sein, daher StartsWith
                foreach (KleinFeld k in kleinfelder) {
                    if (k.Gebäude == null || k.Gebäude.Rüstort == null)
                        continue;
                    if (k.Gebäude.Rüstort.Bauwerk.StartsWith("Dorf")) 
                        continue;
                    if (EinnahmenView.GetGebäudeEinnahmen(k)==0)
                        continue;
                    if (k.Gebäude.Rüstort.Bauwerk.StartsWith("Burg")) {
                        this.Burgen++;
                        this.BurgenEinkommen += EinnahmenView.GetGebäudeEinnahmen(k);
                    }
                    else if (k.Gebäude.Rüstort.Bauwerk.StartsWith("Stadt")) {
                        this.Städte++; this.StädteEinkommen += EinnahmenView.GetGebäudeEinnahmen(k);
                    } // muss vor StartsWith("Festung") stehen 
                    else if (k.Gebäude.Rüstort.Bauwerk.StartsWith("Festungshauptstadt")) {
                        this.FestungsHauptstädte++;
                        this.FestungsHauptstädteEinkommen += EinnahmenView.GetGebäudeEinnahmen(k);
                    }
                    else if (k.Gebäude.Rüstort.Bauwerk.StartsWith("Festung")) {
                        this.Festungen++;
                        this.FestungenEinkommen += EinnahmenView.GetGebäudeEinnahmen(k);
                    }
                    else if (k.Gebäude.Rüstort.Bauwerk.StartsWith("Hauptstadt")) {
                        this.Hauptstädte++;
                        this.HauptstädteEinkommen += EinnahmenView.GetGebäudeEinnahmen(k);
                    }

                }
                // jedes Kleinfeld hat Terrain, Gebäude usw. schon als Member. Mit einer foreach lassen sich die Daten auch sammeln - siehe auch EinnahmenView
            }

        }
    }
}
