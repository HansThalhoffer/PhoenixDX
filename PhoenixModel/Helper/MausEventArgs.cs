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
        //
        // Zusammenfassung:
        //     Specifies the possible states of a mouse button.
        public enum MouseButtonState
        {
            //
            // Zusammenfassung:
            //     The button is released.
            Released = 0,
            //
            // Zusammenfassung:
            //     The button is pressed.
            Pressed = 1
        }

        //
        // Zusammenfassung:
        //     Defines values that specify the buttons on a mouse device.
        public enum MouseButton
        {
            //
            // Zusammenfassung:
            //     The left mouse button.
            Left = 0,
            //
            // Zusammenfassung:
            //     The middle mouse button.
            Middle = 1,
            //
            // Zusammenfassung:
            //     The right mouse button.
            Right = 2,
            //
            // Zusammenfassung:
            //     The first extended mouse button.
            XButton1 = 3,
            //
            // Zusammenfassung:
            //     The second extended mouse button.
            XButton2 = 4
        }
        /// <summary>
        /// Gets the state of the left mouse button.
        /// </summary>
        public MouseButtonState LeftButton { get; protected set; }

        /// <summary>
        /// Gets the state of the right mouse button.
        /// </summary>
        public MouseButtonState RightButton { get; protected set; }

        /// <summary>
        /// Gets the state of the middle mouse button.
        /// </summary>
        public MouseButtonState MiddleButton { get; protected set; }

        /// <summary>
        /// Gets the state of the first extra mouse button.
        /// </summary>
        public MouseButtonState X1Button { get; protected set; }

        /// <summary>
        /// Gets the state of the second extra mouse button.
        /// </summary>
        public MouseButtonState X2Button { get; protected set; }

        /// <summary>
        /// Gets the button that was double clicked.
        /// </summary>
        public MouseButton? DoubleClickButton { get; protected set; }

        /// <summary>
        /// Gets the mouse wheel delta.
        /// </summary>
        public int WheelDelta { get; protected set; }

        /// <summary>
        /// Gets the horizontal mouse wheel delta.
        /// </summary>
        public int HorizontalWheelDelta { get; protected set; }

        /// <summary>
        /// Gets the position of the mouse in screen coordinates.
        /// </summary>
        public Position? ScreenPosition { get; protected set; }

        /// <summary>
        ///  Calculates the position of the mouse relative to a particular element. 
        /// </summary>
        /*public Position GetPosition(UIElement relativeTo)
        {
            return relativeTo.PointFromScreen(ScreenPosition);
        }*/

        
    }
}