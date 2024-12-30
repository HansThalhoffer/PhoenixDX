using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PhoenixDX.Structures
{
    internal class RuestortSymbol : GemarkAdorner
    {
        public static Dictionary<string, RuestortSymbol> Ruestorte = new Dictionary<string, RuestortSymbol>
        {
            {"Burg-I", new RuestortSymbol(11, "Burg-I","Burg-I")},
            {"Burg-II",new RuestortSymbol(12, "Burg-II", "Burg-II") },
            {"Burg-III",new RuestortSymbol(13, "Burg-III", "Burg-III") },
            {"Stadt-I",new RuestortSymbol(14, "Stadt-I", "Stadt-I") },
            {"Stadt-II",new RuestortSymbol(15, "Stadt-II", "Stadt-II") },
            {"Stadt-III",new RuestortSymbol(16, "Stadt-III", "Stadt-III") },
            {"Audvacar",new RuestortSymbol(6, "Audvacar", "Audvacar") },
            {"Festungshauptstadt",new RuestortSymbol(1, "Festungshauptstadt", "Festungshauptstadt") },
            {"Hauptstadt",new RuestortSymbol(2, "Hauptstadt", "Hauptstadt") },
            {"Festung",new RuestortSymbol(3, "Festung", "Festung") },
            {"Stadt",new RuestortSymbol(4, "Stadt", "Stadt") },
            {"Burg",new RuestortSymbol(5, "Burg", "Burg") },
            {"Dorf",new RuestortSymbol(7, "Dorf", "Dorf") },
            {"Dorf-I",new RuestortSymbol(8, "Dorf-I", "Dorf-I") },
            {"Dorf-II",new RuestortSymbol(9, "Dorf-II", "Dorf-II") },
            {"Dorf-III",new RuestortSymbol(10, "Dorf-III", "Dorf-III") }
        };

        Texture2D hexTexture = null;
        int _nummer;
        string _name;
        string _image;
        public RuestortSymbol(int Nummer, string Name, string Image)
        {
            this._nummer = Nummer;
            this._name = Name;
            this._image = Image;
            HasDirections = false;
        }

        public static void LoadContent(ContentManager contentManager)
        {
            foreach (var item in Ruestorte.Values)
            {
                item.hexTexture = contentManager.Load<Texture2D>($"Images/Symbol/{item._image}");
            }
        }

        public override AdornerTexture GetAdornerTexture()
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
