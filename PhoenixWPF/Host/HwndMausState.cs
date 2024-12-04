#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: 
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System.Windows;
using PhoenixModel.Helper;

namespace PhoenixWPF.Host
{
    public class HwndMausState
    {
        /// <summary>
        /// The current state of the left mouse button.
        /// </summary>
        public MausEventArgs.MouseButtonState LeftButton;

        /// <summary>
        /// The current state of the right mouse button.
        /// </summary>
        public MausEventArgs.MouseButtonState RightButton;

        /// <summary>
        /// The current state of the middle mouse button.
        /// </summary>
        public MausEventArgs.MouseButtonState MiddleButton;

        /// <summary>
        /// The current state of the first extra mouse button.
        /// </summary>
        public MausEventArgs.MouseButtonState X1Button;

        /// <summary>
        /// The current state of the second extra mouse button.
        /// </summary>
        public MausEventArgs.MouseButtonState X2Button;

        /// <summary>
        /// The current position of the mouse in screen coordinates.
        /// </summary>
        public Position? ScreenPosition;
    }
}