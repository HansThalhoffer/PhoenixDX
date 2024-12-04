using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using System.Windows;
using System.ComponentModel;
using System.IO;

// <local:MonoGameHost Grid.Column="0" Grid.Row="0" Width="800" Height="600"/>

namespace PhoenixWPF
{
    public class MonoGameHost : HwndHost
    {

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        private PhoenixDX.MappaMundi? _map;
        private IntPtr _hWnd;
        int hostHeight = 600, hostWidth= 800;

     
        public MonoGameHost()
        {
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_hWnd != IntPtr.Zero)
            {
                int width = Convert.ToInt32(sizeInfo.NewSize.Width);
                int height = Convert.ToInt32(sizeInfo.NewSize.Height);
                _map?.Resize(width, height);
            }
        }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
           base.OnWindowPositionChanged(rcBoundingBox);
            if (_hWnd != IntPtr.Zero)
             {
                 int width = Convert.ToInt32(rcBoundingBox.Width);
                 int height = Convert.ToInt32(rcBoundingBox.Height);
                 _map?.Resize(width, height);
             }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _map?.Exit();
            }
            base.Dispose(disposing);
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            const int
             WS_CHILD = 0x40000000,
             WS_VISIBLE = 0x10000000,
             HOST_ID = 0x00000002,
            WS_CLIPSIBLINGS = 0x04000000, WS_CLIPCHILDREN = 0x02000000,
             WS_EX_TRANSPARENT = 0x00000020;
            // Create a child window to host MonoGame
            _hWnd = CreateWindowEx(0, "STATIC", "Host",
                 WS_CHILD | WS_VISIBLE | WS_EX_TRANSPARENT | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
                 0, 0,
                 hostWidth, hostHeight,
                 hwndParent.Handle,
                 (IntPtr)HOST_ID,
                 IntPtr.Zero,
                 0);

            if (_hWnd == IntPtr.Zero)
             {
                 int error = Marshal.GetLastWin32Error();
                 throw new System.ComponentModel.Win32Exception(error, "Failed to create window.");
             }

            // Check if the application is running in design mode
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()) == false)
            {
                // Exit the function if in design mode
                // Initialize MonoGame with the window handle
                
                _map = new PhoenixDX.MappaMundi(_hWnd,1600,1000);
                _map.Run();
            }
            

             return new HandleRef(this, _hWnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _map?.Exit();
            if (_hWnd != IntPtr.Zero)
            {
                DestroyWindow(_hWnd);
            }
        }

        protected override nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            const int WM_MOUSEMOVE = 0x0200;
            if (msg == WM_MOUSEMOVE)
            {
               _map?.OnMouseMove(wParam, lParam);
                
            }


            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }               

        // PInvoke declarations and constants go here
    }
}
