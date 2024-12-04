using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixDX
{
    public class MappaMundi
    {
        private Spiel _game;
        IntPtr _hWnd;
        int _width = 3840;
        int _height = 2160;
        Thread _gameThread;
        CancellationTokenSource _cancellationTokenSource;

        public MappaMundi(IntPtr hWWnd, int width, int height)
        {
            _width = width;
            _height = height;
            _hWnd = hWWnd;
        }

        private void Start()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _game = new Spiel(_hWnd, _width, _height, _cancellationTokenSource.Token);
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

        public void OnMouseMove(nint wParam, nint lParam)
        {
            _game?.OnMouseMove(wParam, lParam);
        }

        public void Run()
        {
            _gameThread = new Thread(() => Start());
            _gameThread.SetApartmentState(ApartmentState.STA);
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        public void ShowKarte(Dictionary<string, PhoenixModel.Karte.Gemark> map)
        {
            
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
