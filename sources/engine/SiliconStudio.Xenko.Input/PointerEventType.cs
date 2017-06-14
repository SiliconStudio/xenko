// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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