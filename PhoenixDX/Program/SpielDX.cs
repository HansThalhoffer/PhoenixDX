using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhoenixDX.Drawing;
using PhoenixDX.Helper;
using PhoenixDX.Structures;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.ExternalTables;
using PhoenixModel.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Vektor = Microsoft.Xna.Framework.Vector2;

namespace PhoenixDX.Program {
    /// <summary>
    /// Hauptklasse des Spiels, die von der MonoGame Game-Klasse erbt.
    /// Verarbeitet Spielereignisse, Eingaben und Darstellungen.
    /// </summary>
    internal class SpielDX : Game
    {
        /// <summary>
        /// zum Beenden der Engine
        /// </summary>
        private CancellationToken _cancellationToken;
        /// <summary>
        /// Das Handle des Host Fensters als Rederziel
        /// </summary>
        private nint _windowHandle;
        private readonly ConcurrentQueue<Action> _actionQueue = new();
        /// <summary>
        /// wird von der Kamera benutzt, um nicht endlos zu bewegen
        /// </summary>
        bool _isMoving = false; 
        /// <summary>
        /// Mausevents zum Verarbeiten
        /// </summary>
        MausEventArgs _maus = new MausEventArgs();
        /// <summary>
        /// aktuelle Kamerapositon
        /// </summary>
        Position _cameraPosition = new Position(0, 0);
        /// <summary>
        /// virtuelle Breite des Fenster in 4K
        /// </summary>
        const int _virtualWidth = 3840;
        /// <summary>
        /// virtuelle Höhe des Fenster in 4K
        /// </summary>
        const int _virtualHeight = 2160;
        /// <summary>
        /// tatsächliche Breite
        /// </summary>
        int _clientWidth = _virtualWidth;
        /// <summary>
        /// tatsächliche Höhe
        /// </summary>
        int _clientHeight = _virtualHeight;
        /// <summary>
        /// Skalierung die sich aus sichtbaren Bereich und Zoom errechnet
        /// </summary>
        Vektor _scale = Vektor.Zero;
        /// <summary>
        /// Zoomfaktor - Backing member für das Property
        /// </summary>
        float _zoom = 0f;
        /// <summary>
        /// Tranzparenz der Terrainfelder - Backing member für das Property
        /// </summary>
        private float _opacity = 1f;
        /// <summary>
        /// die Batch zum Zeichnen in DirectX
        /// </summary>
        private SpriteBatch _spriteBatch;
        /// <summary>
        /// Die Welt als Sammlung von Strukturen, die sich zeichnen lassen
        /// Sie wird aus den Datenstrukturen von SharedData.Map gebildet 
        /// und ist daher eine art Kopie. Änderungen in den Daten müssen per Update
        /// an die Engine herangetragen werden
        /// </summary>
        public Welt Weltkarte;
        /// <summary>
        /// aktuell ausgewähltes Kleinfeld. Wird durch MapEvents aktualisiert
        /// </summary>
        private Gemark _selected = null;
        /// <summary>
        /// aktuelles Feld, über dem sich die Maus befindet
        /// </summary>
        private Gemark _mouseOver = null;
        /// <summary>
        /// die API zur Benutzeroberfläche 
        /// die DirectX Engine verarbeitet keine vom Benutzer ausgelösten Events
        /// </summary>
        private MappaMundi _wpfBridge;
        /// <summary>
        /// Updates an der Karte werden asynchron verarbeitet
        /// Damit ist der DirectX Thread weitgehend frei
        /// lediglich das Rendern von zusammengeführten Texturen blockiert ihn noch.
        /// </summary>
        private BackgroundUpdater _backgroundUpdater = null;

        /// <summary>
        /// Singleton-Instanz des Spiels.
        /// </summary>
        public static SpielDX Instance;

        private GraphicsDeviceManager _graphics;
        /// <summary>
        /// Verwaltet die Grafikkonfiguration.
        /// </summary>
        public GraphicsDeviceManager Graphics { get => _graphics; private set => _graphics = value; }

        /// <summary>
        /// Fügt eine Aktion zur Warteschlange hinzu, die später ausgeführt wird.
        /// </summary>
        /// <param name="action">Die auszuführende Aktion.</param>
        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Erstellt eine neue Instanz des Spiels.
        /// </summary>
        public SpielDX(nint windowHandle, CancellationToken token, MappaMundi bridge)
        {
            Instance = this;
            _wpfBridge = bridge;
            _clientWidth = 10;
            _clientHeight = 10;

            _zoom = 0.4f;
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

        /// <summary>
        /// Initialisiert das Spiel. Herausgelöst aus dem Konstruktor für späte Initalisierung
        /// </summary>
        protected override void Initialize()
        {
            // Disable vertical sync
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            base.Initialize();
            // der WPF Anwendung sagen, dass alles bereit ist
            _wpfBridge.OnLoaded();
        }

        /// <summary>
        /// Importiert die Windows-API-Funktion zum Anzeigen oder Verbergen eines Fensters.
        /// </summary>
        /// <param name="hWnd">Das Handle des Fensters.</param>
        /// <param name="nCmdShow">Der Befehl zum Anzeigen oder Verbergen des Fensters.</param>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        /// <summary>
        /// Versteckt das MonoGame-Fenster, da es in einem ChildWindow gerendert wird
        /// </summary>
        private void HideGameWindow()
        {
            var windowHandle = Window.Handle;
            if (windowHandle != nint.Zero)
            {
                const int SW_HIDE = 0;
                ShowWindow(windowHandle, SW_HIDE);
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn das Spiel aktiviert wird.
        /// </summary>
        private void Spiel_Activated(object sender, EventArgs e)
        {
            HideGameWindow();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Grafikeinstellungen vorbereitet werden.
        /// </summary>
        /// <param name="sender">Das auslösende Objekt.</param>
        /// <param name="e">Ereignisdaten mit Informationen zu den Grafikeinstellungen.</param>
        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Redirect rendering to the WPF control's window handle
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = _windowHandle;
        }
        /// <summary>
        /// Berechnet die Skalierung basierend auf der virtuellen und der Client-Größe sowie dem Zoomfaktor.
        /// </summary>
        void _RecalcScale()
        {
            _scale.X = _virtualWidth / (float)_clientWidth * Zoom;
            _scale.Y = _virtualHeight / (float)_clientHeight * Zoom;
        }

        /// <summary>
        /// Ändert die Größe des Client-Fensters und passt die Grafikeinstellungen an.
        /// </summary>
        /// <param name="width">Die neue Breite des Fensters.</param>
        /// <param name="height">Die neue Höhe des Fensters.</param>
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

        /// <summary>
        /// Verschiebt die Kamera um eine bestimmte Distanz.
        /// </summary>
        /// <param name="delta">Die Verschiebung der Kamera.</param>
        void MoveCamera(Position delta)
        {
            _cameraPosition += delta;
            _isMoving = true;
        }
        /// <summary>
        /// Konvertiert eine Client-Position in die virtuelle Bildschirmkoordinaten.
        /// </summary>
        /// <param name="pos">Die Position im Client-Koordinatensystem.</param>
        /// <returns>Die Position im virtuellen Bildschirmkoordinatensystem.</returns>
        public Vektor ClientToVirtualScreen(Position pos)
        {
            return new Vektor(pos.X - _cameraPosition.X, pos.Y - _cameraPosition.Y);
        }

        /// <summary>
        /// Verarbeitet ein Mausereignis und speichert die Argumente.
        /// Sendet es damit in den Thread der Game Engine
        /// </summary>
        /// <param name="args">Die Mausereignis-Argumente.</param>
        public void OnMouseEvent(MausEventArgs args)
        {
            EnqueueAction(() =>
            {
                _maus = args;
            });
        }
        /// <summary>
        /// Verarbeitet Tastatureingaben und bewegt die Kamera entsprechend.
        /// </summary>
        /// <param name="args">Die Tasteneingabe-Argumente.</param>
        private void _OnKeyEvent(KeyEventArgs args)
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
        /// <summary>
        /// Sendet ein Tastenereignis in den Thread der Game Engine
        /// </summary>
        /// <param name="args">Die Tastenereignis-Argumente.</param>
        public void OnKeyEvent(KeyEventArgs args)
        {
            EnqueueAction(() =>
            {
                _OnKeyEvent(args);
            });
        }

        /// <summary>
        /// Bewegt die Kamera zur angegebenen Position auf der Karte.
        /// </summary>
        /// <param name="pos">Die Zielposition auf der Karte.</param>
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

        /// <summary>
        /// Schickt das Verschieben der Kamera in den Game Engine Thread
        /// </summary>
        /// <param name="pos">Die Zielposition auf der Karte.</param>
        public void Goto(KleinfeldPosition pos)
        {
            EnqueueAction(() =>
            {
                _goto(pos);
            });
        }

        /// <summary>
        /// Gibt an, ob das Reichs-Overlay angezeigt wird.
        /// Da dies im Drawing passiert und bool Threadsafe ist, kann hier direkt zugegriffen werden
        /// </summary>
        public bool ReichOverlay
        {
            get { return WeltDrawer.ShowReichOverlay; }
            set { WeltDrawer.ShowReichOverlay = value; }
        }

        /// <summary>
        /// Verarbeitet die Eingaben der Maus.
        /// Wird im Udate der GameEngine aufgerufen
        /// </summary>
        private void HandleInput()
        {
            // Font for status text
            SpriteFont font = FontManager.Fonts["Default"];
            // wenn der Event noch nicht verarbeitet wurde, dann jetzt bitte
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

        /// <summary>
        /// Lädt den Inhalt der Spielressourcen.
        /// </summary>
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

        /// <summary>
        /// Delegat für die Update-Funktion.
        /// </summary>
        public delegate void UpdateFunction();
        /// <summary>
        /// Die Update-Funktion wird je anch Status der Welt umgeschaltet
        /// zuerst soll die Initialisierung abgeschlossen sein
        /// dann werden der Input und die Queues verarbeitet
        /// </summary>
        private UpdateFunction _updateFunction;

        /// <summary>
        /// Initialisiert die Weltkarte und setzt die Update-Funktion um, damit die Inputs verarbeitet werden können
        /// </summary>
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
                    _backgroundUpdater = new BackgroundUpdater(ref Weltkarte, ref SharedData.UpdateQueue);   
                }
            }
        }

        /// <summary>
        /// Aktualisiert das Spiel in jedem Frame.
        /// </summary>
        /// <param name="gameTime">Spielzeit-Informationen.</param>
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


        /// <summary>
        /// Steuerung des Zoom-Faktors mit Begrenzung der Werte.
        /// </summary>
        public float Opacity {
            get => _opacity;
            set { 
                if (value < 0 || value > 1 || _opacity == value)
                    return;

                _opacity = value;
                EnqueueAction(() =>
                {
                    Gelaende.ChangeOpacity(Opacity);
                });
            }
        }

        /// <summary>
        /// Steuerung des Zoom-Faktors mit Begrenzung der Werte.
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set  { // aus technischen Gründen sind die Zoomwerte limitiert
                if (value <= 0.01 || value > 2.6 || _zoom == value)
                    return;
                _zoom = value;
                _RecalcScale();
                _wpfBridge.OnZoomChanged(Zoom);
            }
        }
        /// <summary>
        /// Zeichnet das aktuelle Frame.
        /// </summary>
        /// <param name="gameTime">Spielzeit-Informationen.</param> 
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
