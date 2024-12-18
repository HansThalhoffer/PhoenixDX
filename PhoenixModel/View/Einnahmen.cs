using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;


namespace PhoenixModel.View
{
    public static class Einnahmen
    {
    
        public static int GetTerrainEinnahmen(Gemark gem)
        {
            if (gem.Terrain != null)
            {
                if (gem.Terrain.Name != null && EinwohnerUndEinnahmenTabelle.EinwohnerUndEinnahmen.ContainsKey(gem.Terrain.Name))
                {
                    return EinwohnerUndEinnahmenTabelle.EinwohnerUndEinnahmen[gem.Terrain.Name].MultiplikatorEinnahmen;
                }
                else
                {
                    ViewModel.LogError(gem.gf, gem.kf, $"Das Gelände {gem.Terrain.Name} hat keine Einahmen in der Einnahmen Tabelle");
                }
            }
            return 0;
        }

        public static int GetGebäudeEinnahmen(Gemark gem)
        {
            Gebäude? gebäude = gem.Gebäude;
            if (gebäude != null)
            {
                if (gebäude.Bauwerknamen != null && EinwohnerUndEinnahmenTabelle.EinwohnerUndEinnahmen.ContainsKey(gebäude.Bauwerknamen))
                {
                    return EinwohnerUndEinnahmenTabelle.EinwohnerUndEinnahmen[gebäude.Bauwerknamen].MultiplikatorEinnahmen;
                }
                else
                {
                    ViewModel.LogError(gem.gf, gem.kf, $"Das Gebäude {gebäude.Bauwerknamen} hat keine Einahmen in der Einnahmen Tabelle");
                }
            }
            return 0;
        }

        public static int GetReichEinnahmen(int reich)
        {
            int summe = 0;
            if (SharedData.Map != null) 
            {
                int einnahmenTerrain =0;
                int einnahmenGebäude =0;
                foreach (var gemark in SharedData.Map.Values) 
                {
                    if (gemark.Reich == reich)
                    {
                        einnahmenTerrain += GetTerrainEinnahmen(gemark);
                        einnahmenGebäude += GetTerrainEinnahmen(gemark);
                    }
                }
                summe += einnahmenTerrain;
                summe += einnahmenGebäude;
            }
           
            return summe;
        }
    }
}
