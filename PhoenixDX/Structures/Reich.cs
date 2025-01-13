using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhoenixDX.Helper;
using PhoenixModel.dbPZE;
using System;
using System.Collections.Generic;

namespace PhoenixDX.Structures
{
    public class Reich : GemarkAdorner
    {
        public Color color;
        public string name;
        static Dictionary<string, Texture2D> reichsFarben = [];
        Texture2D hexTexture = null;
    
        public Reich(Nation nation)
        {
            HasDirections = false;

            color = Kolor.Convert(nation.Farbe);
            name = nation.Reich;
            if (reichsFarben.ContainsKey(nation.Farbname))
            {
                hexTexture = reichsFarben[nation.Farbname];
            }
            else
            {
                _ = MessageBox.Show("Fehler", "Das Nation hat keine Farbe gefunden " + nation.Farbname, ["OK"]);
            }
        }

        public static void LoadContent(ContentManager contentManager) 
        {

            try
            {
                reichsFarben.Add("blau",contentManager.Load<Texture2D>("Images/Reichsfarben/reich_blau"));
                reichsFarben.Add("braun", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_braun"));
                reichsFarben.Add("gelb", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_gelb"));
                reichsFarben.Add("grau", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_grau"));
                reichsFarben.Add("dunkelgrün", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_gruen"));
                reichsFarben.Add("hellgrün", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_hellgruen"));
                reichsFarben.Add("hellblau", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_hellblau"));
                reichsFarben.Add("lila", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_lila"));
                reichsFarben.Add("orange", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_orange"));
                reichsFarben.Add("rot", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_rot"));
                reichsFarben.Add("schwarz", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_schwarz"));
                reichsFarben.Add("spielleitung", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_spielleitung"));
                reichsFarben.Add("türkis", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_tuerkis"));
                reichsFarben.Add("weiß", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_weiss"));
            }
            catch (Exception ex)
            {
                MappaMundi.Log(0, 0, "Die Textur für die Farben einer Nation konnte nicht geladen werden", ex);
            }
        }

        protected override Drawing.DirectionTexture GetDirectionTexture()
        {
            return null;
        }

        public override List<Texture2D> GetTextures()
        {
            return new List<Texture2D>() { hexTexture };
        }

        public override Texture2D GetTexture()
        {
            return hexTexture;
        }


    }
}
