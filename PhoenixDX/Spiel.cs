using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public Spiel(IntPtr windowHandle, int width, int height, CancellationToken token)
        {
            _cancellationToken = token;
            Content.RootDirectory = "Content";
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

            // TODO: use this.Content to load your game content here
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

            if (SharedData.Map != null && SharedData.Map.IsAddingCompleted)
            {
                Dictionary<string, Gemark> map = SharedData.Map.FirstOrDefault();
            }

            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        public void ShowKarte()
        {

        }
    }
}
