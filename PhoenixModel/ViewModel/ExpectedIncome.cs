using PhoenixModel.dbPZE;
using PhoenixModel.ExternalTables;
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

        public ExpectedIncome(Nation reich) {
            this.Reich = reich.Reich;
            if (SharedData.Map != null) {
                var kleinfelder = SharedData.Map.Values.Where(k => k.Nation == reich);
                this.BergFelder = kleinfelder.Where(k => k.Terrain.Typ == TerrainType.Bergland).Count();
                this.BergEinkommen = BergFelder * Terrains[(int)TerrainType.Bergland].Einnahmen;
            }

        }
    }
}
