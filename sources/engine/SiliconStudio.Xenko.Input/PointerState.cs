// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// State of a pointer event.
    /// </summary>
    public enum PointerState
    {
        /// <summary>
        /// The pointer just started to hit the digitizer.
        /// </summary>
        Down,
        
        /// <summary>
        /// The pointer is moving onto the digitizer.
        /// </summary>
        Move,

        /// <summary>
        /// The pointer just released pressure to the digitizer.
        /// </summary>
        Up,

        /// <summary>
        /// The pointer is out of the digitizer.
        /// </summary>
        Out,

        /// <summary>
        /// The pointer has been canceled.
        /// </summary>
        Cancel,
    }
}