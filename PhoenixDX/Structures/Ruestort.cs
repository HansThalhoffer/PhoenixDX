using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixModel.dbPZE.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Structures
{
    internal class Ruestort
    {
        public static List<Ruestort> Ruestorte =
        [
            new Ruestort(11, "Burg-I","Burg-I"),
            new Ruestort(12, "Burg-II", "Burg-II"),
            new Ruestort(13, "Burg-III", "Burg-III"),
            new Ruestort(14, "Stadt-I", "Stadt-I"),
            new Ruestort(15, "Stadt-II", "Stadt-II"),
            new Ruestort(16, "Stadt-III", "Stadt-III"),
            new Ruestort(6, "Audvacar", "Audvacar"),
            new Ruestort(1, "Festungshauptstadt", "Festungshauptstadt"),
            new Ruestort(2, "Hauptstadt", "Hauptstadt"),
            new Ruestort(3, "Festung", "Festung"),
            new Ruestort(4, "Stadt", "Stadt"),
            new Ruestort(5, "Burg", "Burg"),
            new Ruestort(7, "Dorf", "Dorf"),
            new Ruestort(8, "Dorf-I", "Dorf-I"),
            new Ruestort(9, "Dorf-II", "Dorf-II"),
            new Ruestort(10, "Dorf-III", "Dorf-III")
        ];

        Texture2D Texture { get; set; } = null;
        int _nummer;
        string _name;
        string _image;
        public Ruestort(int Nummer, string Name, string Image)
        {
            this._nummer = Nummer;
            this._name = Name;
            this._image = Image;
        }

        public static void LoadContent(ContentManager contentManager)
        {
            foreach (var item in Ruestorte)
            {
                item.Texture = contentManager.Load<Texture2D>($"Images/Symbol/{item._image}");
            }
        }
        
    }
}
