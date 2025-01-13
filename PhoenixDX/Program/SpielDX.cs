using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Helper;
using PhoenixDX.Structures;
using PhoenixModel.dbErkenfara;
using PhoenixModel.Helper;
using PhoenixModel.Program;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Program
{
    public class SpielDX : Game
    {
        public static SpielDX Instance;

        private GraphicsDeviceManager _graphics;
        public GraphicsDeviceManager Graphics { get => _graphics; private set => _graphics = value; }

        private CancellationToken _cancellationToken;
        private nint _windowHandle;
        private readonly ConcurrentQueue<Action> _actionQueue = new();

        Position _cameraPosition = new Position(0, 0);
        const int _virtualWidth = 3840;
        const int _virtualHeight = 2160;
        int _clientWidth = _virtualWidth;
        int _clientHeight = _virtualHeight;
        Vektor _scale = Vektor.Zero;
        private SpriteBatch _spriteBatch;

        public Welt Weltkarte;
        private Gemark _selected = null;
        private Gemark _mouseOver = null;
        private MappaMundi _wpfBridge;
        private BackgroundUpdater _backgroundUpdater = null;

        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        public SpielDX(nint windowHandle, CancellationToken token, MappaMundi bridge)
        {
            Instance = this;
            _wpfBridge = bridge;
            _clientWidth = 10;
            _clientHeight = 10;
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
            _updateFunction = DoInitialization;
        }

        protected override void Initialize()
        {
            // Disable vertical sync
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            base.Initialize();
        }


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        // Hide the MonoGame window 
        private void HideGameWindow()
        {
            var windowHandle = Window.Handle;
            if (windowHandle != nint.Zero)
            {
                const int SW_HIDE = 0;
                ShowWindow(windowHandle, SW_HIDE);
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

        void _RecalcScale()
        {
            _scale.X = _virtualWidth / (float)_clientWidth * Zoom;
            _scale.Y = _virtualHeight / (float)_clientHeight * Zoom;
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


        bool _isMoving = false;
        void MoveCamera(Position delta)
        {
            _cameraPosition += delta;
            _isMoving = true;
        }
        public Vektor ClientToVirtualScreen(Position pos)
        {
            return new Vektor(pos.X - _cameraPosition.X, pos.Y - _cameraPosition.Y);
        }

        MausEventArgs _maus = new MausEventArgs();
        public void OnMouseEvent(MausEventArgs args)
        {
            EnqueueAction(() =>
            {
                _maus = args;
            });
        }

        // wenn sich die Daten geändert haben, werden die jeweiligen Gemarken neu gezeichnet
        ConcurrentQueue<KleinfeldPosition> _updateQueue = [];
        public void OnUpdateEvent(MapEventArgs args)
        {
            
            _updateQueue.Enqueue(new KleinfeldPosition(args.GF, args.KF));
          
        }

        private void _OnKeyEvent(PhoenixModel.Helper.KeyEventArgs args)
        {
            if (args.State == KeyEventArgs.KeyState.Down)
            {
                switch (args.Key)
                {
                    case 0x41: //   A key page left
                        _cameraPosition.X += 200;
                        break;
                    case 0x44://   D key page right
                        _cameraPosition.X -= 200;
                        break;
                    case 0x57: //   W key page up
                        _cameraPosition.Y += 200;
                        break;
                    case 0x53: //   S key page down
                        _cameraPosition.Y -= 200;
                        break;
                }
            }
        }

        public void OnKeyEvent(PhoenixModel.Helper.KeyEventArgs args)
        {
            EnqueueAction(() =>
            {
                _OnKeyEvent(args);
            });
        }


        void _goto(KleinfeldPosition pos)
        {
            Provinz provinz = Weltkarte.GetProviz(pos.gf);
            if (provinz == null) 
                return;
            Gemark kleinfeld = provinz.GetKleinfeld(pos.kf);
            if (kleinfeld == null)
                return;

            Vektor posP = provinz.GetMapPosition(_scale);
            var posG = kleinfeld.GetMapPosition(posP, _scale); // aktualisiert die MapSize - Reihenfolge wichtig
            // var sizeG = Gemark.GetMapSize();
            posG *= -1;
            Vektor offset = new Vektor(_clientWidth / 2, _clientHeight / 2);
            posG += offset;
            _cameraPosition.SetFromVector2(posG);
            _selected = kleinfeld;
            //_wpfBridge.SelectKleinfeld(_selected.Koordinaten.gf, _selected.Koordinaten.kf, MausEventArgs.MouseEventType.None);
        }

        public void Goto(KleinfeldPosition pos)
        {
            EnqueueAction(() =>
            {
                _goto(pos);
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
                            if (_maus.RightButton == MausEventArgs.MouseButtonState.Pressed)
                            {
                                Position delta = _maus.ScreenPositionDelta * 18;
                                MoveCamera(delta);
                            }
                            break;
                        }
                    case MausEventArgs.MouseEventType.MouseWheel:
                        {
                            if (Zoom >= 2.6f && _maus.WheelDelta > 0)
                                return;
                            if (Zoom > 0.2f || _maus.WheelDelta > 0)
                                Zoom = Zoom + _maus.WheelDelta / 1000f;
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
            WeltDrawer.LoadContent(Content);
            Welt.LoadContent(Content);
            FontManager.LoadContent(Content);
            RuestortSymbol.LoadContent(Content);
            Truppen.LoadContent(Content);
            Marker.LoadContent(Content); // marker für den Layer 2
        }

        public delegate void UpdateFunction();
        private UpdateFunction _updateFunction;

        
        private void DoInitialization()
        {
            if (Weltkarte == null && SharedData.Map != null && SharedData.Map.IsAddingCompleted)
                Weltkarte = new Welt(SharedData.Map);
            else if (Weltkarte != null)
            {
                if (Weltkarte.ReicheInitalized == false && SharedData.Nationen != null && SharedData.Nationen.IsAddingCompleted)
                    Weltkarte.AddNationen(SharedData.Nationen);
                // sobald minimal alles initialisiert, die Funktion auf HandleInput umstellen
                if (Weltkarte.ReicheInitalized)
                {
                    _updateFunction = HandleInput;
                    _backgroundUpdater = new BackgroundUpdater(ref Weltkarte, ref _updateQueue);   
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (_cancellationToken.IsCancellationRequested)
                Exit();

            // Process queued actions
            while (_actionQueue.TryDequeue(out var action))
                action();
            /*while (_updateQueue.TryDequeue(out var gemarkPosition))
                Weltkarte.UpdateGemark(gemarkPosition);*/
            _backgroundUpdater?.Update();
            // entweder DoInitialization oder HandleInput
            _updateFunction();

            base.Update(gameTime);
        }

        float _zoom = 0f;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                _RecalcScale();
                _wpfBridge.OnZoomChanged(Zoom);
            }
        }
            
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);

            Vektor offset = new Vektor(30f * _scale.X, 10f * _scale.Y);
            _graphics.GraphicsDevice.Viewport = new Viewport
            {
                X = _cameraPosition.X,
                Y = _cameraPosition.Y,
                Width = _virtualWidth - _cameraPosition.X,
                Height = _virtualHeight - _cameraPosition.Y,
                MinDepth = 0,
                MaxDepth = 1
            };

            if (Weltkarte != null)
            {
                Vektor? mousePos = _maus.ScreenPosition == null ? null : ClientToVirtualScreen(_maus.ScreenPosition);
                Rectangle visibleScreen = new Rectangle(_cameraPosition.X * -1, _cameraPosition.Y * -1, _clientWidth, _clientHeight);
                _mouseOver = Weltkarte.Draw(_spriteBatch, _scale, mousePos, _isMoving, gameTime.TotalGameTime, _selected, visibleScreen);
            }

            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

    }
}
