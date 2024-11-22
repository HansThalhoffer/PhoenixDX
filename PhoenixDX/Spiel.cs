using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace PhoenixDX
{
    public class Spiel : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private IntPtr _windowHandle;

        public Spiel(IntPtr windowHandle)
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _windowHandle = windowHandle;
            _graphics = new GraphicsDeviceManager(this);

            // Set the window handle
            _graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;

            // Optional: Set the preferred back buffer size to match the control
            _graphics.PreferredBackBufferWidth = 0; // Set appropriate width
            _graphics.PreferredBackBufferHeight = 0; // Set appropriate height

            // Hide the MonoGame window when the game starts
            Activated += Spiel_Activated;
           
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
           IntPtr hWnd,
           IntPtr hWndInsertAfter,
           int X,
           int Y,
           int cx,
           int cy,
           uint uFlags);

        private const int SW_HIDE = 0;

        private void Spiel_Activated(object sender, EventArgs e)
        {
            // Hide the MonoGame window
            var windowHandle = this.Window.Handle;
            if (windowHandle != IntPtr.Zero)
            {
                const int SWP_NOZORDER = 0x0004;
                const int SWP_NOACTIVATE = 0x0010;
                ShowWindow(windowHandle, SW_HIDE);
                SetWindowPos(windowHandle, IntPtr.Zero, -400, -400, 0, 0, SWP_NOZORDER | SWP_NOACTIVATE);
            }
        }

        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Redirect rendering to the WPF control's window handle
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = _windowHandle;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
