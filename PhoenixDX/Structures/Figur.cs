using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    internal class Figur
    {
        public static List<Figur> Figuren =
        [
            new Figur(0, "Kreatur", "Monster"),
            new Figur(1, "Krieger","Krieger"),
            new Figur(2, "Reiter", "Reiter"),
            new Figur(4, "Charakter", "Charakter"),
            new Figur(5, "Zauberer", "Zauberer"),
            new Figur(6, "SchwereArtillerie", "SchweresKatapult"),
            new Figur(7, "LeichteArtillerie", "LeichtesKatapult"),
            new Figur(8, "BeritteneSchwereArtillerie", "ReiterSchweresKatapult"),
            new Figur(9, "BeritteneLeichteArtillerie", "ReiterLeichtesKatapult"),
            new Figur(10, "Schiff", "Schiff"),
            new Figur(11, "SchweresKriegsschiff", "SchweresKriegsschiff"),
            new Figur(12, "LeichtesKriegsschiff", "LeichtesKriegsschiff"),
            new Figur(13, "PiratenSchiff", "PiratenSchiff"),
            new Figur(14, "PiratenSchweresKriegsschiff", "PiratenSchweresKriegsschiff"),
            new Figur(15, "PiratenLeichtesKriegsschiff", "PiratenLeichtesKriegsschiff"),
            new Figur(16, "CharakterZauberer", "CharakterZauberer")
        ];

        Texture2D Texture { get; set; } = null;
        int _nummer;
        string _name;
        string _image;
        public Figur(int Nummer, string Name, string Image)
        {
            this._nummer = Nummer;
            this._name = Name;
            this._image = Image;
        }

        public static void LoadContent(ContentManager contentManager)
        {
            foreach (var item in Figuren)
            {
                item.Texture = contentManager.Load<Texture2D>($"Images/Symbol/{item._image}");
            }
        }

    }
}
