#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL:
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using PhoenixModel.Helper;

namespace PhoenixWPF.Host
{
    public class HwndMouseEventArgs : MausEventArgs
    {
       
        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        public HwndMouseEventArgs(HwndMausState state)
        {
            LeftButton = state.LeftButton;
            RightButton = state.RightButton;
            MiddleButton = state.MiddleButton;
            X1Button = state.X1Button;
            X2Button = state.X2Button;
            ScreenPosition = state.ScreenPosition;
        }

        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        /// <param name="mouseWheelDelta">The mouse wheel rotation delta.</param>
        /// <param name="mouseHWheelDelta">The horizontal mouse wheel delta.</param>
        public HwndMouseEventArgs(HwndMausState state, int mouseWheelDelta, int mouseHWheelDelta)
            : this(state)
        {
            WheelDelta = mouseWheelDelta;
            HorizontalWheelDelta = mouseHWheelDelta;
        }

        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        /// <param name="doubleClickButton">The button that was double clicked.</param>
        public HwndMouseEventArgs(HwndMausState state, MouseButton doubleClickButton)
            : this(state)
        {
            DoubleClickButton = doubleClickButton;
        }
    }
}