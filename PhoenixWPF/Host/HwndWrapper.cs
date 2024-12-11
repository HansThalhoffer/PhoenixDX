#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: 
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using PhoenixDX;
using PhoenixModel.Helper;
using System;
using System.ComponentModel;

using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace PhoenixWPF.Host
{
    /// <summary>
    /// A control that enables graphics rendering inside a WPF control through
    /// the use of a hosted child Hwnd.
    /// </summary>
    public abstract class HwndWrapper : HwndHost
    {
        #region Fields

        // The name of our window class
        private const string WindowClass = "GraphicsDeviceControlHostWindowClass";

        // The HWND we present to when rendering
        private IntPtr _hWnd;

        // For holding previous hWnd focus
        private IntPtr _hWndPrev;

        // Track if the application has focus
        private bool _applicationHasFocus;

        // Track if the mouse is in the window
        private bool _mouseInWindow;

        // Track the previous mouse position
        private Position? _previousPosition;

        // Track the mouse state
        private readonly MausEventArgs _mouseState = new MausEventArgs();

        // Tracking whether we've "capture" the mouse
        private bool _isMouseCaptured;

        #endregion

        #region Properties

        public new bool IsMouseCaptured
        {
            get { return _isMouseCaptured; }
        }

        #endregion

        #region Construction and Disposal

        protected HwndWrapper()
        {
            // We must be notified of the application foreground status for our mouse input events
            Application.Current.Activated += OnApplicationActivated;
            Application.Current.Deactivated += OnApplicationDeactivated;

            // We use the CompositionTarget.Rendering event to trigger the control to draw itself
            CompositionTarget.Rendering += OnCompositionTargetRendering;

            // Check whether the application is active (it almost certainly is, at this point).
            if (Application.Current.Windows.Cast<Window>().Any(x => x.IsActive))
                _applicationHasFocus = true;
        }

        protected override void Dispose(bool disposing)
        {
            // Unhook all events.
            CompositionTarget.Rendering -= OnCompositionTargetRendering;
            if (Application.Current != null)
            {
                Application.Current.Activated -= OnApplicationActivated;
                Application.Current.Deactivated -= OnApplicationDeactivated;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Captures the mouse, hiding it and trapping it inside the window bounds.
        /// </summary>
        /// <remarks>
        /// This method is useful for tooling scenarios where you only care about the mouse deltas
        /// and want the user to be able to continue interacting with the window while they move
        /// the mouse. A good example of this is rotating an object based on the mouse deltas where
        /// through capturing you can spin and spin without having the cursor leave the window.
        /// </remarks>
        public new void CaptureMouse()
        {
            // Don't do anything if the mouse is already captured
            if (_isMouseCaptured)
                return;

            NativeMethods.SetCapture(_hWnd);
            _isMouseCaptured = true;
        }

        /// <summary>
        /// Releases the capture of the mouse which makes it visible and allows it to leave the window bounds.
        /// </summary>
        public new void ReleaseMouseCapture()
        {
            // Don't do anything if the mouse is not captured
            if (!_isMouseCaptured)
                return;

            NativeMethods.ReleaseCapture();
            _isMouseCaptured = false;
        }

        #endregion

        #region Graphics Device Control Implementation

        private void OnCompositionTargetRendering(object? sender, EventArgs e)
        {
            // Get the current width and height of the control
            var width = (int)ActualWidth;
            var height = (int)ActualHeight;

            // If the control has no width or no height, skip drawing since it's not visible
            if (width < 1 || height < 1)
                return;

            Render(_hWnd);
        }

        protected abstract void Render(IntPtr windowHandle);
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_hWnd != IntPtr.Zero)
            {
                int width = Convert.ToInt32(sizeInfo.NewSize.Width);
                int height = Convert.ToInt32(sizeInfo.NewSize.Height);
               // SetWindowRegion(_hWnd, width, height);

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
        private void OnApplicationActivated(object? sender, EventArgs e)
        {
            _applicationHasFocus = true;
        }

        private void OnApplicationDeactivated(object? sender, EventArgs e)
        {
            _applicationHasFocus = false;
            CancelMouseState();

            if (_mouseInWindow)
            {
                _mouseInWindow = false;
                _mouseState.EventType = MausEventArgs.MouseEventType.MouseLeave;
                OnMouseEvent(new HwndMouseEventArgs(_mouseState));
            }

            ReleaseMouseCapture();
        }

        private void CancelMouseState()
        {
            // The mouse is no longer considered to be in our window
            _mouseInWindow = false;

            if (_mouseState.LeftButton == MausEventArgs.MouseButtonState.Pressed)
            {
                _mouseState.LeftButton = MausEventArgs.MouseButtonState.Released;
                _mouseState.EventType = MausEventArgs.MouseEventType.LeftButtonUp;
                OnMouseEvent(new HwndMouseEventArgs(_mouseState));
            }
            else if (_mouseState.MiddleButton == MausEventArgs.MouseButtonState.Pressed)
            {
                _mouseState.MiddleButton = MausEventArgs.MouseButtonState.Released;
                _mouseState.EventType = MausEventArgs.MouseEventType.MiddleButtonUp;
                OnMouseEvent(new HwndMouseEventArgs(_mouseState));
            }
            else if (_mouseState.RightButton == MausEventArgs.MouseButtonState.Pressed)
            {
                _mouseState.RightButton = MausEventArgs.MouseButtonState.Released;
                _mouseState.EventType = MausEventArgs.MouseEventType.RightButtonUp;
                OnMouseEvent(new HwndMouseEventArgs(_mouseState));
            }
            else if (_mouseState.X1Button == MausEventArgs.MouseButtonState.Pressed)
            {
                _mouseState.X1Button = MausEventArgs.MouseButtonState.Released;
                _mouseState.EventType = MausEventArgs.MouseEventType.X1ButtonUp;
                OnMouseEvent(new HwndMouseEventArgs(_mouseState));
            }
            else if (_mouseState.X2Button == MausEventArgs.MouseButtonState.Pressed)
            {
                _mouseState.X2Button = MausEventArgs.MouseButtonState.Released;
                _mouseState.EventType = MausEventArgs.MouseEventType.X2ButtonUp;
                OnMouseEvent(new HwndMouseEventArgs(_mouseState));
            }
        }

        #endregion

        #region HWND Management
        private PhoenixDX.MappaMundi? _map;
        private PhoenixWPF.Spiel _spiel = new PhoenixWPF.Spiel();
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            // Create the host window as a child of the parent
            // Check if the application is running in design mode
            
            _hWnd = CreateHostWindow(hwndParent.Handle);
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()) == false)
            {
                _map = new PhoenixDX.MappaMundi(_hWnd);
                _map.OnMapEvent += new MappaMundi.MapEventHandler(MapEventHandler);
                _map.Run();
            }
            return new HandleRef(this, _hWnd);
        }

        // dispatch events from the game engine thread back to UI
        public void MapEventHandler(object sender, MapEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _spiel.MapEventHandler(e);
            }));
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _map?.Exit();
            // Destroy the window and reset our hWnd value
            NativeMethods.DestroyWindow(hwnd.Handle);
            _hWnd = IntPtr.Zero;
        }

        /// <summary>
        /// Creates the host window as a child of the parent window.
        /// </summary>
        private IntPtr CreateHostWindow(IntPtr hWndParent)
        {
            // Register our window class
            RegisterWindowClass();

            // Create the window
            return NativeMethods.CreateWindowEx(0, WindowClass, "",
               NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
               0, 0, (int)Width, (int)Height, hWndParent, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Registers the window class.
        /// </summary>
        private void RegisterWindowClass()
        {
            var wndClass = new NativeMethods.WNDCLASSEX();
            wndClass.cbSize = (uint)Marshal.SizeOf(wndClass);
            wndClass.hInstance = NativeMethods.GetModuleHandle(null);
            wndClass.lpfnWndProc = NativeMethods.DefaultWindowProc;
            wndClass.lpszClassName = WindowClass;
            wndClass.hCursor = NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW);

            NativeMethods.RegisterClassEx(ref wndClass);
        }

        private void SetWindowRegion(IntPtr hwnd, int width, int height)
        {
            IntPtr rgn = NativeMethods.CreateRectRgn(0, 0, width, height);
            /*int r = NativeMethods.GetWindowRgn(hwnd, rgn);
            if (r == NativeMethods.ERROR)
                return ;
            var region = System.Drawing.Region.FromHrgn(rgn);
            Region region1 = new Region(new Rectangle(10, 10, 100, 100));*/
            NativeMethods.SetWindowRgn(hwnd, rgn, false);


        }

        #endregion

        #region WndProc Implementation
        
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeMethods.WM_MOUSEWHEEL:
                    if (_mouseInWindow)
                    {
                        int delta = NativeMethods.GetWheelDeltaWParam(wParam.ToInt32());
                        _mouseState.EventType = MausEventArgs.MouseEventType.MouseWheel;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState, delta, 0));
                    }
                    break;
                case NativeMethods.WM_LBUTTONDOWN:
                    _mouseState.LeftButton = MausEventArgs.MouseButtonState.Pressed;
                    _mouseState.EventType = MausEventArgs.MouseEventType.LeftButtonDown;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    _mouseState.LeftButton = MausEventArgs.MouseButtonState.Released;
                    _mouseState.EventType = MausEventArgs.MouseEventType.LeftButtonUp;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_LBUTTONDBLCLK:
                    _mouseState.EventType = MausEventArgs.MouseEventType.LeftButtonDoubleClick;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONDOWN:
                    _mouseState.RightButton = MausEventArgs.MouseButtonState.Pressed;
                    _mouseState.EventType = MausEventArgs.MouseEventType.RightButtonDown;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONUP:
                    _mouseState.RightButton = MausEventArgs.MouseButtonState.Released;
                    _mouseState.EventType = MausEventArgs.MouseEventType.RightButtonUp;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_RBUTTONDBLCLK:
                    _mouseState.EventType = MausEventArgs.MouseEventType.RightButtonDoubleClick;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONDOWN:
                    _mouseState.MiddleButton = MausEventArgs.MouseButtonState.Pressed;
                    _mouseState.EventType = MausEventArgs.MouseEventType.MiddleButtonDown;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    _mouseState.MiddleButton = MausEventArgs.MouseButtonState.Released;
                    _mouseState.EventType = MausEventArgs.MouseEventType.MiddleButtonUp;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_MBUTTONDBLCLK:
                    _mouseState.EventType = MausEventArgs.MouseEventType.MiddleButtonDoubleClick;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    break;
                case NativeMethods.WM_XBUTTONDOWN:
                    if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.X1Button = MausEventArgs.MouseButtonState.Pressed;
                        _mouseState.EventType = MausEventArgs.MouseEventType.X1ButtonDown;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.X2Button = MausEventArgs.MouseButtonState.Pressed;
                        _mouseState.EventType = MausEventArgs.MouseEventType.X1ButtonUp;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONUP:
                    if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.X1Button = MausEventArgs.MouseButtonState.Released;
                        _mouseState.EventType = MausEventArgs.MouseEventType.X2ButtonDown;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.X2Button = MausEventArgs.MouseButtonState.Released;
                        _mouseState.EventType = MausEventArgs.MouseEventType.X2ButtonUp;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONDBLCLK:
                    if (((int)wParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        _mouseState.EventType = MausEventArgs.MouseEventType.X1ButtonDoubleClick;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }
                    else if (((int)wParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        _mouseState.EventType = MausEventArgs.MouseEventType.X2ButtonDoubleClick;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }
                    break;
                case NativeMethods.WM_MOUSEMOVE:
                    // If the application isn't in focus, we don't handle this message
                    if (!_applicationHasFocus)
                        break;

                    // record the prevous and new position of the mouse
                    /*System.Windows.Point p = PointToScreen(new System.Windows.Point(
                        NativeMethods.GetXLParam((int)lParam),
                        NativeMethods.GetYLParam((int)lParam)));
                    _mouseState.ScreenPosition = new Position(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));*/
                    _mouseState.ScreenPosition = new Position(NativeMethods.GetXLParam((int)lParam), NativeMethods.GetYLParam((int)lParam));

                    if (!_mouseInWindow)
                    {
                        _mouseInWindow = true;
                        _mouseState.EventType = MausEventArgs.MouseEventType.MouseEnter;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));

                        // Track the previously focused window, and set focus to this window.
                        _hWndPrev = NativeMethods.GetFocus();
                        NativeMethods.SetFocus(_hWnd);

                        // send the track mouse event so that we get the WM_MOUSELEAVE message
                        var tme = new NativeMethods.TRACKMOUSEEVENT
                        {
                            cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT)),
                            dwFlags = NativeMethods.TME_LEAVE,
                            hWnd = hwnd
                        };
                        NativeMethods.TrackMouseEvent(ref tme);
                    }

                    if (_mouseState.ScreenPosition != _previousPosition)
                    {
                        _mouseState.EventType = MausEventArgs.MouseEventType.MouseMove;
                        _mouseState.ScreenPositionDelta = _mouseState.ScreenPosition - _previousPosition;
                        OnMouseEvent(new HwndMouseEventArgs(_mouseState));
                    }

                    _previousPosition = _mouseState.ScreenPosition;

                    break;
                case NativeMethods.WM_MOUSELEAVE:

                    // If we have capture, we ignore this message because we're just
                    // going to reset the cursor position back into the window
                    if (_isMouseCaptured)
                        break;

                    // Reset the state which releases all buttons and 
                    // marks the mouse as not being in the window.
                    CancelMouseState();
                    _mouseState.EventType = MausEventArgs.MouseEventType.MouseLeave;
                    OnMouseEvent(new HwndMouseEventArgs(_mouseState));

                    NativeMethods.SetFocus(_hWndPrev);

                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected virtual void OnMouseEvent(HwndMouseEventArgs args)
        {
            _map?.OnMouseEvent(args);
        }

       

        #endregion
    }
}