using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhoenixModel.Helper
{
    internal class CashedTexture
    {

        RenderTarget2D _renderTarget;
        GraphicsDevice _graphicsDevice;

        public CashedTexture(GraphicsDevice graphicsDevice)
        {
            this._graphicsDevice = graphicsDevice;
        }

        public void Cash(List<Texture2D> textureList)
        {
            _renderTarget = new RenderTarget2D( _graphicsDevice, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight,false,
                _graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            // Set the render target
            _graphicsDevice.SetRenderTarget(_renderTarget);

            _graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            // Draw the scene
            _graphicsDevice.Clear(Color.Transparent);
           
            // Drop the render target
            _graphicsDevice.SetRenderTarget(null);
        }

        
    }
}
