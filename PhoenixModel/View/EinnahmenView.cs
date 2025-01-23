using PhoenixModel.dbErkenfara;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using PhoenixModel.Program;
using System;
using PhoenixModel.dbCrossRef;
using PhoenixModel.dbPZE;


namespace PhoenixModel.View
{
    public static class EinnahmenView
    {
    
        public static int GetTerrainEinnahmen(KleinFeld gem)
        {
            if (gem.Terrain != null)
            {
                return gem.Terrain.Einnahmen;
            }
            return 0;
        }

        public static int GetGebäudeEinnahmen(KleinFeld gem)
        {
            Rüstort? gebäude = BauwerkeView.GetRüstortNachKarte(gem);
            if (gebäude != null)
            {
                return GetGebäudeEinnahmen(gebäude);
            }
            return 0;
        }

        public static int GetGebäudeEinnahmen(BauwerkBasis bauwerk) {
            
            EinwohnerUndEinnahmenTabelle.Werte einwohnerUndEinnahmenTabelle;
            if (EinwohnerUndEinnahmenTabelle.EinwohnerUndEinnahmen.TryGetValue(bauwerk.Bauwerk, out einwohnerUndEinnahmenTabelle))
                return einwohnerUndEinnahmenTabelle.Einnahmen;
            ProgramView.LogError($"Das Gebäude {bauwerk.Bauwerk} hat keine Einahmen in der EinnahmenView Tabelle", "Durch einen Datenbankfehler hat das Gebäude keinen Eintrag in der Einnahmentabelle");
            return 0;
        }

        public static int GetGesamtEinnahmen(KleinFeld gem) {
            
            return GetTerrainEinnahmen(gem) + GetGebäudeEinnahmen(gem);
        }

        public static int GetReichEinnahmen(Nation reich)
        {
            int summe = 0;
            if (SharedData.Map != null) 
            {
                foreach (var gemark in SharedData.Map.Values.Where( gem => gem.Nation == reich)) 
                {
                    summe += GetGesamtEinnahmen(gemark);
                }
            }
           
            return summe;
        }
    }
}
