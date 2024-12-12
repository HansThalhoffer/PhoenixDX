﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhoenixDX.Classes;
using PhoenixDX.Drawing;
using PhoenixDX.Structures;
using PhoenixModel.Helper;


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

        Position _cameraPosition = new Position(0, 0);
        const int _virtualWidth = 3840;
        const int _virtualHeight = 2160;
        int _clientWidth = _virtualWidth;
        int _clientHeight = _virtualHeight;

        public Welt Weltkarte { get; private set; }
        private Kleinfeld _selected = null;
        private Kleinfeld _mouseOver = null;
        private MappaMundi _wpfBridge;

        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public Spiel(IntPtr windowHandle, CancellationToken token, MappaMundi bridge)
        {
            _wpfBridge = bridge;
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
                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
                _graphics.ApplyChanges();
                _RecalcScale();
                HideGameWindow();
            });
        }

        void _RecalcScale()
        {
            _scale.X =  (float) _virtualWidth / (float) _clientWidth * Zoom;
            _scale.Y = (float) _virtualHeight / (float) _clientHeight * Zoom;
        }

        bool _isMoving = false;
        void MoveCamera(Position delta)
        {
            _cameraPosition += delta;
            _isMoving = true;
        }
        public Vector2 ClientToVirtualScreen(Position pos)
        {
            return new Vector2(pos.X - _cameraPosition.X , pos.Y - _cameraPosition.Y);
        }

        MausEventArgs _maus = new MausEventArgs();
        public void OnMouseEvent(MausEventArgs args)
        {
            EnqueueAction(() =>
            {
                 _maus= args;
            });
        }

        public bool ReichOverlay
        {
            get { return WeltDrawer.ShowReichOverlay; }
            set { WeltDrawer.ShowReichOverlay = value; }
        }

        private void HandleInput()
        {
            // Font for status text
            SpriteFont font = FontManager.Fonts["Default"];
            if (_maus?.Handled == false)
            {
                _maus.Handled = true;
                _isMoving = false;
                switch (_maus.EventType)
                {
                    case MausEventArgs.MouseEventType.LeftButtonDown:
                        { // single click

                            if (_selected != null)
                                _selected.IsSelected = false;

                            _selected = _mouseOver;
                            if (_selected != null)
                            {
                                _selected.IsSelected = true;
                                _wpfBridge.SelectKleinfeld(_selected.Koordinaten.gf, _selected.Koordinaten.kf, _maus.EventType);
                            }
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
                                Position delta = _maus.ScreenPositionDelta*18;
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
            _spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(_graphics.GraphicsDevice);
            Content.RootDirectory = "Content";
            WeltDrawer.LoadContent(Content);
            Welt.LoadContent(Content);
            FontManager.LoadContent(Content);
            Ruestort.LoadContent(Content);
            Figur.LoadContent(Content);

        }

        protected override void Update(GameTime gameTime)
        {
            if (_cancellationToken.IsCancellationRequested)
                Exit();

            // Process queued actions
            while (_actionQueue.TryDequeue(out var action))
                action();

            if (Weltkarte == null && SharedData.Map != null && SharedData.Map.IsAddingCompleted)
                Weltkarte = new Welt(SharedData.Map);
            else if (Weltkarte != null && Weltkarte.IsInitalized == false && SharedData.Nationen != null && SharedData.Nationen.IsAddingCompleted)
            {
                Weltkarte.AddNationen(SharedData.Nationen);

            }

            HandleInput();
            if (_lastDrawTime == null)
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        Vector2 _scale = Vector2.Zero;


        private Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch;
        float _zoom = 0f;
        public float Zoom { get => _zoom; set {  _zoom = value; _RecalcScale(); } }
        GameTime _lastDrawTime = null;
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);

            Vector2 offset = new Vector2(30f * _scale.X, 10f * _scale.Y);
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
                Vector2? mousePos = _maus.ScreenPosition == null ? null : ClientToVirtualScreen(_maus.ScreenPosition);
                Rectangle visibleScreen = new Rectangle(_cameraPosition.X * -1, _cameraPosition.Y *-1, _clientWidth, _clientHeight);
                _mouseOver = Weltkarte.Draw(_spriteBatch, _scale, mousePos, _isMoving, gameTime.TotalGameTime, _selected, visibleScreen);
            }
            
            /*if (_maus.ScreenPosition != null)
            {
                Vector2 mousePos = ClientToVirtualScreen(_maus.ScreenPosition);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
              
                _selected.Draw(_spriteBatch, mousePos, gameTime);

                string statusText = "Maus " + mousePos.ToString();
                SpriteFont font = FontManager.Fonts["Large"];
                _spriteBatch.DrawString(font, statusText, new Vector2(10, 10), Color.Azure);
                _spriteBatch.End();
            }*/

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
