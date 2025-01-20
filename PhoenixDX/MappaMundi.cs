using PhoenixDX.Drawing;
using PhoenixDX.Program;
using PhoenixModel.EventsAndArgs;
using PhoenixModel.Program;
using PhoenixModel.ViewModel;
using System;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PhoenixDX {
    // stellt die API zur Verfügung für die WPF Welt
    public class MappaMundi {
        /// <summary>
        /// die DirectX Engine
        /// </summary>
        private SpielDX _game;
        /// <summary>
        /// Handle auf das Host Fenster als Child in WPF
        /// </summary>
        private readonly IntPtr _hWnd;

        /// <summary>
        /// der Thread, in dem die DirectX Engine gehostet wird
        /// </summary>
        Thread _gameThread;

        /// <summary>
        /// Token, um den DirectX Thread zu stoppen
        /// </summary>
        CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Konstruktor der API 
        /// </summary>
        /// <param name="hWWnd"></param>
        public MappaMundi(IntPtr hWWnd) {
            _hWnd = hWWnd;
        }

        #region Threading
        /// <summary>
        /// Erzeugung und Start der Game Engine 
        /// </summary>
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
        
        /// <summary>
        /// Schnittstelle, um die Game Engine zu starten
        /// </summary>
        public void Run() {
            _gameThread = new Thread(() => this.Start());
            _gameThread.SetApartmentState(ApartmentState.STA);
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        /// <summary>
        /// Schnittstelle, um die Game Engine zu stoppen und zu entsorgen
        /// </summary>
        public void Exit() {
            // Signal the game thread to exit
            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
            }

            // Optionally, wait for the game thread to finish
            if (_gameThread != null && _gameThread.IsAlive) {
                _gameThread.Join();
            }
            _game.Dispose();
        }
        #endregion

        #region EventsFromWpfToDirectX
        /// <summary>
        /// Größenanpassung durch das Host Fenster in der UI
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(int width, int height) {
            _game?.Resize(width, height);
        }

        /// <summary>
        /// Schnittstelle, um das ReichOverlay zu schalten
        /// </summary>
        public bool ReichOverlay {
            get { return WeltDrawer.ShowReichOverlay; }
            set { WeltDrawer.ShowReichOverlay = value; }
        }

        /// <summary>
        /// Schnittstelle, um das Zeigen der Küsten, ein und auszuschalten, reicht es nicht hier den boolschen Parameter umzuschalten
        /// es muss in der SharedData.UpdateQueue auch alle betroffenen Kleinfelder abgelegt werden
        /// </summary>
        public bool Küsten {
            get { return WeltDrawer.ShowKüsten; }
            set { WeltDrawer.ShowKüsten = value; }
        }

        /// <summary>
        /// Schnittstelle für das Empfangen von Maus Ereignissen aus der Oberfläche
        /// die DirectX Engine kannn und soll keine User Inputs direkt verarbeiten
        /// </summary>
        /// <param name="args"></param>
        public void OnMouseEvent(MausEventArgs args) {
            _game?.OnMouseEvent(args);
        }
        /// <summary>
        /// Schnittstelle für das Empfangen von Tastatur Ereignissen aus der Oberfläche
        /// die DirectX Engine kannn und soll keine User Inputs direkt verarbeiten
        /// </summary>
        /// <param name="args"></param>
        public void OnKeyEvent(KeyEventArgs args) {
            _game?.OnKeyEvent(args);
        }

        /// <summary>
        /// Die DirectX Engine soll sich zu einer bestimmten Kleinfeld Position begeben
        /// </summary>
        /// <param name="pos"></param>
        public void Goto(KleinfeldPosition pos) {
            _game?.Goto(pos);
        }


        /// <summary>
        /// Zoom change von WPF zu DirectX, beispielsweise aus den UserSettings oder aus dem Sliden von den Optionen
        /// </summary>
        /// <param name="val"></param>
        public void SetTerrainOpacity(float val) {
            if (val < 0 || val > 1) {
                Log(new LogEntry(LogEntry.LogType.Error, $"Der übergebene Wert {val} für Transparenz ist ungültig", "Bitte den Programmierfehler beheben. Der Wert muss > 0  und <1 sein."));
                return;
            }
            if (_game != null)
                _game.Opacity = val;
        }

        /// <summary>
        /// Zoom change von WPF zu DirectX, beispielsweise aus den UserSettings oder aus dem Sliden von den Optionen
        /// </summary>
        /// <param name="val"></param>
        public float GetTerrainOpacity() {
            
            if (_game != null)
                return _game.Opacity;
            return 1f;
        }

        /// <summary>
        /// Zoom change von WPF zu DirectX, beispielsweise aus den UserSettings oder aus dem Sliden von den Optionen
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
        /// Dieses geschieht durch die Verarbeitung von <see cref="OnMouseEvent"/> aus der UI
        /// so führt das Drehen des Mausrades zu einer Veränderung des Zooms, die dann zurückgespielt wird
        /// </summary>
        /// <param name="val"></param>
        internal void OnZoomChanged(float val) {
            _OnMapEvent(new MapEventArgs(0, 0, MapEventArgs.MapEventType.Zoom, _game?.Zoom));
        }

        /// <summary>
        /// Nachricht der DirectX Engine, dass sie vollständig konstruiert wurde
        /// </summary>
        /// <param name="val"></param>
        internal void OnLoaded() {
            _OnMapEvent(new MapEventArgs(MapEventArgs.MapEventType.Loaded));
        }


        /// <summary>
        /// Delegate für Map-Ereignisse.
        /// </summary>
        /// <param name="sender">Das auslösende Objekt.</param>
        /// <param name="e">Die Ereignisargumente.</param>
        public delegate void MapEventHandler(object sender, MapEventArgs e);

        /// <summary>
        /// Ereignis, das ausgelöst wird, wenn eine Kartenaktion stattfindet.
        /// Hier kann sich die UI dranhängen, um Ereignisse aus der Darstellung zu empfangen
        /// </summary>
        public static event MapEventHandler OnMapEvent;

        /// <summary>
        /// Interne Methode zum Auslösen des Map-Ereignisses
        /// </summary>
        /// <param name="args">Die Argumente des Kartenereignisses.</param>
        private static void _OnMapEvent(MapEventArgs args) {
            if (OnMapEvent == null)
                return;
            OnMapEvent(null, args);
        }

        /// <summary>
        /// Wählt ein Kleinfeld in der Karte durch die Maus aus und löst ein entsprechendes Ereignis aus.
        /// </summary>
        /// <param name="gf">Die ID des großen Feldes.</param>
        /// <param name="kf">Die ID des kleinen Feldes.</param>
        /// <param name="eventType">Der Typ des Mausereignisses.</param>
        public void SelectKleinfeld(int gf, int kf, MausEventArgs.MouseEventType eventType) {
            _OnMapEvent(new MapEventArgs(gf, kf, MapEventArgs.MapEventType.SelectGemark));
        }
        #endregion

        #region Logs
        /// <summary>
        /// Protokolliert eine Log-Nachricht mit einem LogEntry-Objekt.
        /// </summary>
        /// <param name="logentry">Das zu protokollierende Log-Objekt.</param>
        public static void Log(LogEntry logentry) {
            _OnMapEvent(new MapEventArgs(logentry));
        }

        /// <summary>
        /// Protokolliert eine Log-Nachricht für ein bestimmtes Feld in der Karte.
        /// </summary>
        /// <param name="gf">Die ID des großen Feldes.</param>
        /// <param name="kf">Die ID des kleinen Feldes.</param>
        /// <param name="logentry">Das Log-Objekt mit der Meldung.</param>
        public static void Log(int gf, int kf, LogEntry logentry) {
            _OnMapEvent(new MapEventArgs(gf, kf, logentry));
        }

        /// <summary>
        /// Protokolliert eine Fehlermeldung mit einem Titel und einer Ausnahme für ein bestimmtes Feld in der Karte.
        /// </summary>
        /// <param name="gf">Die ID des großen Feldes.</param>
        /// <param name="kf">Die ID des kleinen Feldes.</param>
        /// <param name="titel">Der Titel der Fehlermeldung.</param>
        /// <param name="ex">Die ausgelöste Ausnahme.</param>
        public static void Log(int gf, int kf, string titel, Exception ex) {
            MappaMundi.Log(gf, kf, new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, titel, ex.Message));
        }

        /// <summary>
        /// Protokolliert eine allgemeine Fehlermeldung mit einem Titel und einer Ausnahme.
        /// </summary>
        /// <param name="titel">Der Titel der Fehlermeldung.</param>
        /// <param name="ex">Die ausgelöste Ausnahme.</param>
        public static void Log(string titel, Exception ex) {
            MappaMundi.Log(new PhoenixModel.Program.LogEntry(PhoenixModel.Program.LogEntry.LogType.Error, titel, ex.Message));
        }

        #endregion
    }
}
