using PhoenixModel.dbErkenfara;
using PhoenixModel.dbErkenfara.Defaults;
using PhoenixModel.Helper;
using System;


namespace PhoenixModel.View
{
    public static class Einnahmen
    {
        public static Dictionary<string, int[]> SonstigeEinnahmen = new Dictionary<string, int[]>
        {
            // name,              max einwohner, einnahmen
            { "Burg", new int[] { 10000, 1000 } },
            { "Stadt", new int[] { 30000, 2000 } },
            { "Festung", new int[] { 70000, 3000 } },
            { "Hauptstadt", new int[] { 100000, 5000 } },
            { "Festungshauptstadt", new int[] { 100000, 6000 } }
        };


        public static int GetTerrainEinnahmen(Gemark gem)
        {
            int summe = 0;
            summe += gem.Terrain.Einnahmen;
            Gebäude? gebäude = gem.Gebäude;
            return summe;
        }

        public static int GetGebäudeEinnahmen(Gemark gem)
        {
            Gebäude? gebäude = gem.Gebäude;
            int summeGebäude = 0;
            if (gebäude != null)
            {
                if (gebäude.Bauwerknamen != null && SonstigeEinnahmen.ContainsKey(gebäude.Bauwerknamen))
                {
                    summeGebäude += SonstigeEinnahmen[gebäude.Bauwerknamen][1];
                }
            }
            return summeGebäude;
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
