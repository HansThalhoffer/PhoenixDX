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
        public HwndMouseEventArgs(MausEventArgs state) : base(state) 
        {
        }

        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        /// <param name="mouseWheelDelta">The mouse wheel rotation delta.</param>
        /// <param name="mouseHWheelDelta">The horizontal mouse wheel delta.</param>
        public HwndMouseEventArgs(MausEventArgs state, int mouseWheelDelta, int mouseHWheelDelta)
            : this(state)
        {
            WheelDelta = mouseWheelDelta;
            HorizontalWheelDelta = mouseHWheelDelta;
        }       
    }
}