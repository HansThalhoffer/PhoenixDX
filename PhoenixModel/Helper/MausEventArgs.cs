#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL:
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

namespace PhoenixModel.Helper
{

    public class MausEventArgs : EventArgs
    {
        public enum MouseButtonState
        {
            Released = 0,
            Pressed = 1
        }

        public enum MouseEventType
        {
            None, LeftButtonDown, LeftButtonUp, LeftButtonDoubleClick,
            MiddleButtonDown, MiddleButtonUp, MiddleButtonDoubleClick,
            RightButtonDown, RightButtonUp, RightButtonDoubleClick,
            X1ButtonDown, X1ButtonUp, X1ButtonDoubleClick,
            X2ButtonDown, X2ButtonUp, X2ButtonDoubleClick,
            MouseMove, MouseEnter, MouseLeave, MouseWheel
        }
        public MouseEventType EventType { get; set; } = MouseEventType.None;
        public MouseButtonState LeftButton { get; set; } = MouseButtonState.Released;
        public MouseButtonState RightButton { get; set; } = MouseButtonState.Released;
        public MouseButtonState MiddleButton { get; set; } = MouseButtonState.Released;
        public MouseButtonState X1Button { get; set; } = MouseButtonState.Released;
        public MouseButtonState X2Button { get; set; } = MouseButtonState.Released;
      
        public int WheelDelta { get; set; } = 0;
        public int HorizontalWheelDelta { get; set; } = 0;
        public Position? ScreenPosition { get; set; } = null;
        public Position? ScreenPositionDelta { get; set; } = null;

        public bool Handled { get; set; } = false;

        public MausEventArgs()
        { }

        public MausEventArgs(MausEventArgs state)
        {
            EventType = state.EventType;
            LeftButton = state.LeftButton;
            RightButton = state.RightButton;
            MiddleButton = state.MiddleButton;
            X1Button = state.X1Button;
            X2Button = state.X2Button;
            WheelDelta = state.WheelDelta;
            HorizontalWheelDelta = state.HorizontalWheelDelta;
            ScreenPosition = state.ScreenPosition;
            ScreenPositionDelta = state.ScreenPositionDelta;
        }
    }
}