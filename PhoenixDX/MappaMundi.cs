using PhoenixDX.Drawing;
using PhoenixDX.Program;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Threading;

namespace PhoenixDX {
    // stellt die API zur Verfügung für die WPF Welt
    public class MappaMundi {
        private SpielDX _game;
        private readonly IntPtr _hWnd;

        Thread _gameThread;
        CancellationTokenSource _cancellationTokenSource;

        public MappaMundi(IntPtr hWWnd) {
            _hWnd = hWWnd;
        }
        #region Threading
        private void Start() {
            try {
                _cancellationTokenSource = new CancellationTokenSource();
                _game = new SpielDX(_hWnd, _cancellationTokenSource.Token, this);
                _game?.Run();
            }
            catch (Exception ex) {
                MappaMundi.Log(0, 0, "Beim Start der DirectX Engine kam es zu einem Fehler", ex);
            }
        }

        public void Run() {
            _gameThread = new Thread(() => this.Start());
            _gameThread.SetApartmentState(ApartmentState.STA);
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        public void Exit() {
            // Signal the game thread to exit
            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
            }

            // Optionally, wait for the game thread to finish
            if (_gameThread != null && _gameThread.IsAlive) {
                _gameThread.Join();
            }
            // _game.Exit();
            _game.Dispose();
        }
        #endregion

        #region EventsFromWpfToDirectX
        public void Resize(int width, int height) {
            _game?.Resize(width, height);
        }

        public bool ReichOverlay {
            get { return WeltDrawer.ShowReichOverlay; }
            set { WeltDrawer.ShowReichOverlay = value; }
        }

        public void OnMouseEvent(MausEventArgs args) {
            _game?.OnMouseEvent(args);
        }

        public void OnKeyEvent(KeyEventArgs args) {
            _game?.OnKeyEvent(args);
        }

        public void Goto(KleinfeldPosition pos) {
            _game?.Goto(pos);
        }

        /// <summary>
        /// Zoom change von WPF zu DirectX
        /// </summary>
        /// <param name="val"></param>
        public void SetZoom(float val) {
            if (_game != null)
                _game.Zoom = val;
        }

        /// <summary>
        /// Zoom holen - wird benötigt für das Abspeichern des Zooms in den Settings
        /// </summary>
        /// <returns></returns>
        public float GetZoom() {
            if (_game != null)
                return _game.Zoom;
            return 0f;
        }
        #endregion

        #region EventsFromDirectXToWpf
        /// <summary>
        /// Zoom change von DirectX zu WPF
        /// </summary>
        /// <param name="val"></param>
        internal void OnZoomChanged(float val) {
            _OnMapEvent(new MapEventArgs(0, 0, MapEventArgs.MapEventType.Zoom, _game?.Zoom));
        }

        /// <summary>
        /// Zoom change von DirectX zu WPF
        /// </summary>
        /// <param name="val"></param>
        internal void OnLoaded() {
            _OnMapEvent(new MapEventArgs(MapEventArgs.MapEventType.Loaded));
        }


        public delegate void MapEventHandler(object sender, MapEventArgs e);
        public static event MapEventHandler OnMapEvent;
        private static void _OnMapEvent(MapEventArgs args) {
            if (OnMapEvent == null)
                return;
            OnMapEvent(null, args);
        }

        public void SelectKleinfeld(int gf, int kf, MausEventArgs.MouseEventType eventType) {
            _OnMapEvent(new MapEventArgs(gf, kf, MapEventArgs.MapEventType.SelectGemark));
        }
        #endregion

        #region Logs
        public static void Log(LogEntry logentry) {
            _OnMapEvent(new MapEventArgs(logentry));
        }
        public static void Log(int gf, int kf, LogEntry logentry) {
            _OnMapEvent(new MapEventArgs(gf, kf, logentry));
        }
        public static void Log(int gf, int kf, string titel, Exception ex) {
            MappaMundi.Log(gf, kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, titel, ex.Message));
        }
        #endregion
    }
}
