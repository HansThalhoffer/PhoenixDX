using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Classes;
using PhoenixDX.Drawing;
using PhoenixModel.Helper;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static PhoenixModel.Helper.SharedData;

namespace PhoenixDX.Structures
{
    public class Welt
    {

        Dictionary<int, Provinz> Provinzen = [] ;
        Dictionary<int, Reich> Reiche = [];

        public Welt(SharedData.BlockingDictionary<Gemark> map) 
        {
            Provinzen.Add(701, new Provinz(7, 0, 701));
            Provinzen.Add(901, new Provinz(9, 0, 901));
            Provinzen.Add(712, new Provinz(7, 11, 712));
            Provinzen.Add(912, new Provinz(9, 11, 912));
            Provinzen.Add(6666, new Provinz(15, 11, 6666));
            Provinzen.Add(2001, new Provinz(1, 1, 2001));
            Provinzen.Add(4001, new Provinz(15, 1, 4001));

            for (int x = 1; x <= 15; x++)
            {
                for (int y = 1; y <= 11; y++)
                {
                    if ((x == 1 || x== 15) && y > 6)
                        continue;
                    if ((x == 2 || x == 14) && y > 7)
                        continue;
                    if ((x == 3 || x == 13) && y > 8)
                        continue;
                    if ((x == 4 || x == 12) && y > 9)
                        continue;
                    if ((x == 5 || x == 11) && y > 10)
                        continue;
                    int gf = x * 100 + y;
                    int yPos = y ;
                    int xPos = x; 
                    if (x < 6 )
                    {
                        yPos += 3 - x / 2 - x%2;
                    }
                    if (x > 6 && x <10)
                    {
                        yPos -= x %2;
                    }
                    if (x > 10)
                    {
                        yPos +=  x / 2 -5;
                    }
                    if (Provinzen.ContainsKey(gf) == false)
                        Provinzen.Add(gf, new Provinz(xPos,yPos, gf));
                }
            }

            foreach (Gemark gem in map.Values)
            {
                if (gem != null && gem.gf > 0 && gem.kf > 0 && gem.kf < 50)
                {
                    if (Provinzen.ContainsKey(gem.gf) == false)
                    {
                        MappaMundi.Log(gem.gf, gem.kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Großfeld {gem.gf} fehlt"));
                        continue;
                    }
                    var p = Provinzen[gem.gf];

                    if (p != null)
                    {
                        var k = p.GetKleinfeld((int) gem.kf);
                        try
                        {
                            if (k.Initialize(gem) ==false)
                                continue; // TODO - doppelte Kleinfelder in der Datenbank
                        }
                        catch (Exception ex)
                        {
                            MappaMundi.Log(gem.gf, gem.kf, ex);
                        }
                    }
                }
            }
        }

        public Gemark FindGemarkByPosition(Vector2 position)
        {
            return null;
        }

        public bool ReicheInitalized = false;
        public void AddNationen(BlockingCollection<Nation> nations)
        {
            ReicheInitalized = true;
            foreach (var nation in nations)
            {
                if (Reiche.ContainsKey(nation.Nummer.Value) == false)
                    Reiche.Add(nation.Nummer ?? -1, new Reich(nation));
                else
                    break;
            }

            foreach (var province in Provinzen.Values)
            {
                foreach (var kleinfeld in province.Felder.Values)
                {
                    if (Reiche.ContainsKey(kleinfeld.ReichID) == true)
                        kleinfeld.Reich = Reiche[kleinfeld.ReichID];

                }
            }
        }

        public bool RüstorteInitalized = false;
        public void AddBauwerke(BlockingDictionary<Gebäude> gebäude)
        {
            RüstorteInitalized = true;
            foreach (var g in gebäude.Values)
            {
                if (Provinzen.ContainsKey(g.gf) == false)
                    continue;

                var provinz = Provinzen[g.gf];
                var kleinfeld = provinz.GetKleinfeld(g.kf);
                if (kleinfeld == null)
                    continue;
                string typ =  g.Bauwerknamem.Split(' ')[0];
                if (RuestortSymbol.Ruestorte.ContainsKey(typ) == true)
                    kleinfeld.Adorner.Add("Rüstort",RuestortSymbol.Ruestorte[typ]);
                else
                {
                    MappaMundi.Log(g.gf,g.kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error,$"Unbekanntes Gebäude {typ}"));
                }

            }
            
        }

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Reich.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }

        public float TileTransparancy { get; set; } = 1f;

      
        public Kleinfeld Draw(SpriteBatch spriteBatch, Vector2 scale, Vector2? mousePos, bool isMoving, TimeSpan gameTime, Kleinfeld selected, Rectangle visibleScreen)
        {
           
            return WeltDrawer.Draw(spriteBatch, scale, mousePos, isMoving, TileTransparancy, ref Provinzen, gameTime, selected, visibleScreen );
        }    
    }
}
