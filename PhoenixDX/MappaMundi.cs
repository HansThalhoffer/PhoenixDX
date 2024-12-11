using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using PhoenixModel.Helper;
using SharpDX.Direct3D9;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PhoenixDX
{
    public class MappaMundi
    {
        private Spiel _game;
        private readonly IntPtr _hWnd;
     
        Thread _gameThread;
        CancellationTokenSource _cancellationTokenSource;

        public MappaMundi(IntPtr hWWnd)
        {
          
            _hWnd = hWWnd;
        }

        private void Start()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _game = new Spiel(_hWnd, _cancellationTokenSource.Token, this);
                _game?.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void Resize(int width, int height)
        {
            _game?.Resize(width, height);
        }

        public void OnMouseEvent(MausEventArgs args)
        {
            _game?.OnMouseEvent(args);
        }

        public void Run()
        {
            _gameThread = new Thread(() => this.Start());
            _gameThread.SetApartmentState(ApartmentState.STA);
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        public void Update()
        {
            
        }

        public delegate void MapEventHandler(object sender, MapEventArgs e);
        public event MapEventHandler OnMapEvent;

        private void _OnMapEvent(MapEventArgs args)
        {
            if (OnMapEvent == null)
                return;
            OnMapEvent(this, args);
        }

        public void SelectKleinfeld(int gf, int kf, MausEventArgs.MouseEventType eventType)
        {
            _OnMapEvent(new MapEventArgs(gf, kf, MapEventArgs.MapEventType.SelectGemark));
        }

        public void Exit()
        {
            // Signal the game thread to exit
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            // Optionally, wait for the game thread to finish
            if (_gameThread != null && _gameThread.IsAlive)
            {
                _gameThread.Join();
            }
            // _game.Exit();
            _game.Dispose();
        }
    }
}
