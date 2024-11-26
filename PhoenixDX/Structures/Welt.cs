using Microsoft.Xna.Framework.Content;
using PhoenixModel.Karte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    public class Welt
    {
        Dictionary<int, Provinz> Provinzen {  get; set; }

        public Welt(Dictionary<string, Gemark> map) 
        {
            Provinzen = new Dictionary<int, Provinz>();

            foreach (Gemark gem in map.Values)
            {
                if (gem != null && gem.gf > 0 && gem.kf > 0)
                {
                    if (gem.gf == null)
                        continue;
                    var p = GetProvinz((int) gem.gf);
                    if (p != null)
                    {
                        var k = p.GetPKleinfeld((int) gem.kf);
                        k.Initialize(gem);
                    }
                }
            }
        }

        public static void LoadContent(ContentManager contentManager)
        {
            Gelaende.LoadContent(contentManager);
            Kleinfeld.LoadContent(contentManager);
        }


        public Provinz? GetProvinz(int gf)
        {
            if (Provinzen.ContainsKey(gf)) 
                return Provinzen[gf];
            var p = new Provinz(gf);
            Provinzen.Add(gf, p);
            return p;
        }
    
    }
}
