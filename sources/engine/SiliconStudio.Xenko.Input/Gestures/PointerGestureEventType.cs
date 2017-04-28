// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// The different possible states of a gestures.
    /// </summary>
    public enum PointerGestureEventType
    {
        /// <summary>
        /// A discrete gesture has occurred.
        /// </summary>
        Occurred,

        /// <summary>
        /// A continuous gesture has started.
        /// </summary>
        Began,

        /// <summary>
        /// A continuous gesture parameters changed.
        /// </summary>
        Changed,

        /// <summary>
        /// A continuous gesture has stopped.
        /// </summary>
        Ended,
    }
}