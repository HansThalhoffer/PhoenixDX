using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
using PhoenixModel.View;
using static PhoenixModel.ExternalTables.GeländeTabelle;

namespace PhoenixModel.ViewModel {
    public class ExpectedIncome {

        public string Reich { get; set; } = string.Empty;
        public int GesamtEinkommen { get; set; }
        public int TieflandFelder { get; set; }
        public int TieflandEinkommen { get; set; }
        public int TieflandwaldFelder { get; set; }
        public int TieflandwaldEinkommen { get; set; }
        public int TieflandwüsteFelder { get; set; }
        public int TieflandwüsteEinkommen { get; set; }
        public int TieflandsumpfFelder { get; set; }
        public int TieflandsumpfEinkommen { get; set; }
        public int HochlandFelder { get; set; }
        public int HochlandEinkommen { get; set; }
        public int BergFelder { get; set; }
        public int BergEinkommen { get; set; }
        public int GebirgFelder { get; set; }
        public int GebirgEinkommen { get; set; }
        public int Burgen { get; set; }
        public int BurgenEinkommen { get; set; }
        public int Städte { get; set; }
        public int StädteEinkommen { get; set; }
        public int Festungen { get; set; }
        public int FestungenEinkommen { get; set; }
        public int Hauptstädte { get; set; }
        public int HauptstädteEinkommen { get; set; }
        public int FestungsHauptstädte { get; set; }
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
