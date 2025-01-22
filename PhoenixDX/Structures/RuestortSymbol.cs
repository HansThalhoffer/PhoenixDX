using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using System.Collections.Generic;

namespace PhoenixDX.Structures {
    /// <summary>
    /// Stellt ein Rüstortsymbol dar, das eine visuelle Darstellung für verschiedene Orte im Spiel bietet.
    /// </summary>
    internal class RuestortSymbol : GemarkAdorner {
        /// <summary>
        /// Enthält eine Sammlung vordefinierter Rüstortsymbole.
        /// </summary>
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

        private SimpleTexture _hexTexture = null;
        private int _nummer;
        private string _name;
        private string _image;

        /// <summary>
        /// Erstellt eine neue Instanz eines Rüstortsymbols.
        /// </summary>
        /// <param name="Nummer">Die eindeutige TargetID des Symbols.</param>
        /// <param name="Name">Der Name des Symbols.</param>
        /// <param name="Image">Der Bildpfad des Symbols.</param>
        public RuestortSymbol(int Nummer, string Name, string Image) {
            this._nummer = Nummer;
            this._name = Name;
            this._image = Image;
        }

        /// <summary>
        /// Lädt die Inhalte (Texturen) für alle definierten Rüstortsymbole.
        /// </summary>
        /// <param name="contentManager">Der Content-Manager zum Laden der Texturen.</param>
        public static void LoadContent(ContentManager contentManager) {
            foreach (var item in Ruestorte.Values) {
                item._hexTexture = new SimpleTexture(contentManager.Load<Texture2D>($"Images/Symbol/{item._image}"));
            }
        }

        /// <summary>
        /// Gibt die Textur des Rüstortsymbols zurück.
        /// </summary>
        /// <returns>Die Textur des Symbols.</returns>
        public override BaseTexture GetTexture() {
            return _hexTexture;
        }
    }
}
