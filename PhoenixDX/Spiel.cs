using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhoenixDX.Classes;
using PhoenixDX.Structures;
using PhoenixModel.Helper;
using PhoenixModel.Karte;
using SharpDX.Direct2D1.Effects;


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
        private CancellationToken _cancellationToken;
        private IntPtr _windowHandle;
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        Vector2 cameraPosition = Vector2.Zero;
        const int _virtualWidth = 3840;
        const int _virtualHeight = 2160;
        int _clientWidth = _virtualWidth;
        int _clientHeight = _virtualHeight;

        public Welt Weltkarte { get; private set; }

        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public Spiel(IntPtr windowHandle, CancellationToken token)
        {
            _clientWidth = _virtualWidth;
            _clientHeight = _virtualWidth;
            Zoom = 0.4f;
            _cancellationToken = token;
           IsMouseVisible = true;

            _windowHandle = windowHandle;
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            _graphics.PreferredBackBufferWidth = _virtualWidth; 
            _graphics.PreferredBackBufferHeight = _virtualHeight;
            _graphics.ApplyChanges();

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
                const int SW_HIDE = 0;
                ShowWindow(windowHandle, SW_HIDE);
                //const int SWP_NOZORDER = 0x0004;
                //const int SWP_NOACTIVATE = 0x0010;
                // SetWindowPos(windowHandle, IntPtr.Zero, -400, -400, 0, 0, SWP_NOZORDER | SWP_NOACTIVATE);
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
                _clientHeight = height;
                _clientWidth = width;
                _RecalcScale();
                HideGameWindow();
            });
        }

        void _RecalcScale()
        {
            _scaleX = (float) _virtualWidth / (float) _clientWidth * Zoom;
             _scaleY = (float) _virtualHeight / (float) _clientHeight * Zoom;
        }

        public Vector2 ScreeToMap(Position pos)
        {
            return new Vector2(pos.X / _scaleX, pos.Y / _scaleY);
        }

        MausEventArgs _maus = new MausEventArgs();
        public void OnMouseEvent(MausEventArgs args)
        {
            EnqueueAction(() =>
            {
                _maus= args;
            });
        }
        
        Position _cameraPosition = new Position(0,0);
        void MoveCamera(Position delta)
        {
            _cameraPosition += delta; 
        }

        private void HandleInput()
        {
            // Font for status text
            SpriteFont font = FontManager.Fonts["Default"];
            if (_maus?.Handled == false)
            {
                _maus.Handled = true;
                switch (_maus.EventType)
                {
                    case MausEventArgs.MouseEventType.LeftButtonDown:
                        {
                            Vector2 mousePos = ScreeToMap(_maus.ScreenPosition);
                            Gemark gem = Weltkarte.FindGemarkByPosition(mousePos);
                            break;
                        }
                    case MausEventArgs.MouseEventType.MiddleButtonDown:
                        {
                            break;
                        }
                    case MausEventArgs.MouseEventType.RightButtonDown:
                        {
                            break;
                        }
                    case MausEventArgs.MouseEventType.MouseMove:
                        {
                            if (_maus.LeftButton == MausEventArgs.MouseButtonState.Pressed)
                            {
                                Position delta = _maus.ScreenPositionDelta * 16;
                                MoveCamera(delta);
                            }
                            break;
                        }
                    case MausEventArgs.MouseEventType.MouseWheel:
                        {
                            if (Zoom > 0.2f || _maus.WheelDelta>0)
                                Zoom = Zoom + (float) _maus.WheelDelta / 1000f;
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
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

            if (Weltkarte == null && SharedData.Map != null && SharedData.Map.IsAddingCompleted && Weltkarte == null)
            {
                Dictionary<string, Gemark> map = SharedData.Map.FirstOrDefault();
                Weltkarte = new Welt(map);
            }

            HandleInput();

            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        float _scaleX = 0f;
        float _scaleY = 0f;


        private SpriteBatch _spriteBatch;
        float _zoom = 0f;
        public float Zoom { get => _zoom; set {  _zoom = value; _RecalcScale(); } }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);
            
            if (_scaleX > 0)
            {
                int offsetX = (int)((_clientWidth - _virtualWidth * _scaleX) / 2);
                int offsetY = (int)((_clientHeight - _virtualHeight * _scaleY) / 2);
            }
             _graphics.GraphicsDevice.Viewport = new Viewport
            {
                X = _cameraPosition.X,
                Y = _cameraPosition.Y,
                Width = _virtualWidth- _cameraPosition.X,
                Height = _virtualHeight - _cameraPosition.Y,
                MinDepth = 0,
                MaxDepth = 1
            };

            if (Weltkarte != null)
            {
                Weltkarte.Draw(_spriteBatch, _scaleX,_scaleY);
            }

            // Draw status text
            /*
            _spriteBatch.Begin();

            string statusText = "Breite " + _virtualWidth.ToString() + "  Client " + _clientWidth.ToString() + " ScaleX " + scaleX.ToString();

            _spriteBatch.DrawString(font, statusText, new Vector2(10, 10), Color.Violet);

            _spriteBatch.End();*/

            // TODO: Add your drawing code here
            base.Draw(gameTime);

            
        }
              
    }
}
