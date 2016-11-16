// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// State of a pointer event.
    /// </summary>
    public enum PointerEventType
    {
        /// <summary>
        /// The pointer just started to hit the digitizer.
        /// </summary>
        Pressed,

        /// <summary>
        /// The pointer is moving on the digitizer.
        /// </summary>
        Moved,

        /// <summary>
        /// The pointer just released pressure to the digitizer.
        /// </summary>
        Released,

        /// <summary>
        /// The pointer has been canceled.
        /// </summary>
        Canceled,
    }
}