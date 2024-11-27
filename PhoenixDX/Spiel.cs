using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhoenixDX.Structures;
using PhoenixModel.Helper;
using PhoenixModel.Karte;


// using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace PhoenixDX
{
    public class Spiel : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private CancellationToken _cancellationToken;
        private IntPtr _windowHandle;
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();
       
        public Welt Weltkarte { get; private set; }

        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public Spiel(IntPtr windowHandle, int width, int height, CancellationToken token)
        {
            _cancellationToken = token;
           IsMouseVisible = true;

            _windowHandle = windowHandle;
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            _graphics.PreferredBackBufferWidth = width; 
            _graphics.PreferredBackBufferHeight = height; 

            Activated += Spiel_Activated;
        }

        protected override void Initialize()
        {
            // Disable vertical sync
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            base.Initialize();
        }


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y,int cx,int cy,uint uFlags);


        // Hide the MonoGame window as we use the 
        private void HideGameWindow()
        {
            var windowHandle = this.Window.Handle;
            if (windowHandle != IntPtr.Zero)
            {
                const int SWP_NOZORDER = 0x0004;
                const int SWP_NOACTIVATE = 0x0010;
                const int SW_HIDE = 0;
                ShowWindow(windowHandle, SW_HIDE);
                SetWindowPos(windowHandle, IntPtr.Zero, -400, -400, 0, 0, SWP_NOZORDER | SWP_NOACTIVATE);
            }
        }
        
        private void Spiel_Activated(object sender, EventArgs e)
        {
            HideGameWindow();
        }

        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Redirect rendering to the WPF control's window handle
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = _windowHandle;
        }

       public void Resize(int width, int height)
        {
            EnqueueAction(() =>
            {
                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
                _graphics.ApplyChanges();
                HideGameWindow();
            });
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Content.RootDirectory = "Content";
            Welt.LoadContent(Content);
            FontManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                Exit();
            }

            // Process queued actions
            while (_actionQueue.TryDequeue(out var action))
            {
                action();
            }

            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted && Weltkarte == null)
            {
                Dictionary<string, Gemark> map = SharedData.Map.FirstOrDefault();
                Weltkarte = new Welt(map);
            }

            // TODO: Add your update logic here
            base.Update(gameTime);
        }
       
        
        Vector2 cameraPosition = Vector2.Zero;
        float zoom = 1f;

        private static Texture2D _texture;
        private static Texture2D GetTexture(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _texture.SetData(new[] { Color.White });
            }

            return _texture;
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }

        protected override void Draw(GameTime gameTime)
        {
            // GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            // TODO: Add your drawing code here
            base.Draw(gameTime);

            if (Weltkarte != null)
            {
                Weltkarte.Draw(this.GraphicsDevice, _spriteBatch);
            }
        }
              
    }
}
