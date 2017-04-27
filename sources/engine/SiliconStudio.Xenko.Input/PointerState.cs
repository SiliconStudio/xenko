// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
