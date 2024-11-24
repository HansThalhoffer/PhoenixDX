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
        private Spiel? _game;
        IntPtr _hWnd;
        int _width = 800;
        int _height = 600;
        Thread? _gameThread;

        public MappaMundi(IntPtr hWWnd)
        {
            _hWnd = hWWnd;
        }

        private void Start()
        {
            try
            {
                _game = new Spiel(_hWnd, _width, _height);
                _game?.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
            _game?.Resize(width, height);
        }

        public void Run()
        {
            _gameThread = new Thread(() => Start());
            _gameThread.SetApartmentState(ApartmentState.STA);
            _gameThread.IsBackground = true;
            _gameThread.Start();
        }

        public void Exit()
        {
            _game.Exit();
            _game.Dispose();
        }
    }
}
