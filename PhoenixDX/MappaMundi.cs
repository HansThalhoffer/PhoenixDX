using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhoenixDX
{
    public class MappaMundi
    {
        private Spiel _game;
        IntPtr _hWnd;

        public MappaMundi(IntPtr hWWnd)
        {
            _hWnd = hWWnd;
        }

        private void Start()
        {
            try
            {
                _game = new Spiel(_hWnd);
                _game.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }


        public void Run()
        {
            Task.Run(() => Start());
        }

        public void Exit()
        {
            _game.Exit();
            _game.Dispose();
        }
    }
}
