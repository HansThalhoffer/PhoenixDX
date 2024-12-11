using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Karte.Terrain;

namespace PhoenixDX.Structures
{
    public class Reich : KleinfeldAdorner
    {
        public Color color;
        public string name;
        static Dictionary<string, Texture2D> reichsFarben = new Dictionary<string, Texture2D>();
        Texture2D hexTexture = null;

        public override Texture2D GetTexture()
        {
            return hexTexture;
        }

        public Reich(Nation nation)
        {
            HasDirections = false;

            color = new Color(nation.Farbe.Value.R, nation.Farbe.Value.G, nation.Farbe.Value.B);
            name = nation.Reich;
            if (reichsFarben.ContainsKey(nation.Farbname))
            {
                hexTexture = reichsFarben[nation.Farbname];
            }
        }

        public static void LoadContent(ContentManager contentManager) 
        {
            reichsFarben.Add("blau",contentManager.Load<Texture2D>("Images/Reichsfarben/reich_blau"));
            reichsFarben.Add("braun", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_braun"));
            reichsFarben.Add("gelb", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_gelb"));
            reichsFarben.Add("grau", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_grau"));
            reichsFarben.Add("gruen", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_gruen"));
            reichsFarben.Add("hellblau", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_hellblau"));
            reichsFarben.Add("lila", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_lila"));
            reichsFarben.Add("rot", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_rot"));
            reichsFarben.Add("schwarz", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_schwarz"));
            reichsFarben.Add("spielleitung", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_spielleitung"));
            reichsFarben.Add("tuerkis", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_tuerkis"));
            reichsFarben.Add("weiss", contentManager.Load<Texture2D>("Images/Reichsfarben/reich_weiss"));
        }

        public override AdornerTexture GetAdornerTexture()
        {
            return null;
        }

        public override List<Texture2D> GetTextures()
        {
            return new List<Texture2D>() { hexTexture };
        }

       
    }
}
