using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixDX.Drawing
{
    /// <summary>
    /// Aktuell wird die Klasse nicht genutzt. Sie kann aber später sinnvoll sein, um die Truppen oder andere Texturen zu cashen 
    /// und mit unterschiedlichen Farben darzustellen
    /// </summary>
    public class GameTexture
    {
        public Color Color { get; set; }
        public Texture2D Texture { get; set; }
        public GameTexture(Color color, Texture2D texture) 
        {  
            this.Color = color; 
            this.Texture = texture;
        }
    }
}
