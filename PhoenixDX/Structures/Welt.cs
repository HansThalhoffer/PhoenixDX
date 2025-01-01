using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Helper;
using PhoenixDX.Drawing;
using PhoenixModel.Helper;
using PhoenixModel.dbErkenfara;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static PhoenixModel.Helper.SharedData;
using PhoenixModel.View;

namespace PhoenixDX.Structures
{
    public class Welt
    {

        Dictionary<int, Provinz> Provinzen = [] ;
        Dictionary<int, Reich> Reiche = [];

        public Welt(SharedData.BlockingDictionary<KleinFeld> map) 
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

            foreach (KleinFeld gem in map.Values)
            {
                if (Plausibilität.IsValid(gem))
                {
                    if (Provinzen.ContainsKey(gem.gf) == false)
                    {
                        MappaMundi.Log(gem.gf, gem.kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, $"Großfeld {gem.gf} fehlt","In der Karte ist ein Großfeld eingetragen, das es rechnerisch nicht geben dürfte."));
                        continue;
                    }
                    var p = Provinzen[gem.gf];

                    if (p != null)
                    {
                        var k = p.GetOrCreateKleinfeld((int) gem.kf);
                        try
                        {
                            if (k.Initialize(gem) ==false)
                                continue; // TODO - doppelte Kleinfelder in der Datenbank
                        }
                        catch (Exception ex)
                        {
                            MappaMundi.Log(gem.gf, gem.kf,"Das Kleinfeld konnte nicht angelegt werden", ex);
                        }
                    }
                }
            }
        }

        public void UpdateGemark(KleinfeldPosition pos)
        {
            if (pos.gf < 0 || pos.kf < 0 || pos.kf > 48)
                return;
            try
            {
                var provinz = Provinzen[pos.gf];
                var gemark = SharedData.Map[KleinFeld.CreateBezeichner(pos.gf, pos.kf)];
                Gemark updatedKleinfeld = new Gemark(pos.gf, pos.kf);
                updatedKleinfeld.Initialize(gemark);

                // reich verändert?
                if (Reiche.ContainsKey(updatedKleinfeld.ReichID) == true)
                    updatedKleinfeld.Reich = Reiche[updatedKleinfeld.ReichID];

                provinz.Felder[pos.kf] = updatedKleinfeld;
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, $"Die Gemark {pos.gf}/{pos.kf} konnte nicht aktualisiert werden", ex);
            }

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

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Reich.LoadContent(contentManager);
            Gemark.LoadContent(contentManager);
        }

        public float TileTransparancy { get; set; } = 1f;

        public Gemark GetKleinfeld(int gf, int kf)
        {
            Provinz provinz;
            if (Provinzen.TryGetValue(gf, out provinz))
            {
                return provinz.GetKleinfeld(kf);
            }
            return null;
        }

        public Provinz GetProviz(int gf)
        {
            Provinz provinz;
            if (Provinzen.TryGetValue(gf, out provinz))
            {
                return provinz;
            }
            return null;
        }

        public Vector2? GetPosition(int gf, int kf, Vector2 scale)
        {
            Provinz provinz;
            if (Provinzen.TryGetValue(gf, out provinz))
            {
               return provinz.GetMapPosition(scale);
            }
            return null;
        }
      
        public Gemark Draw(SpriteBatch spriteBatch, Vector2 scale, Vector2? mousePos, bool isMoving, TimeSpan gameTime, Gemark selected, Rectangle visibleScreen)
        {
            return WeltDrawer.Draw(spriteBatch, scale, mousePos, isMoving, TileTransparancy, ref Provinzen, gameTime, selected, visibleScreen );
        }    
    }
}
