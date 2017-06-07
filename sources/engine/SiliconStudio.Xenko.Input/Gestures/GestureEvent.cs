// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for the gesture events.
    /// </summary>
    public abstract class GestureEvent
    {
        internal GestureEvent()
        {}

        /// <summary>
        /// The state of the gesture.
        /// </summary>
        public GestureState State { get; internal set; }
        
        /// <summary>
        /// The type of the gesture.
        /// </summary>
        public GestureType Type { get; internal set; }

        /// <summary>
        /// The number of fingers involved in the gesture.
        /// </summary>
        public int NumberOfFinger { get; internal set; }

        /// <summary>
        /// The time elapsed between the two last events of the gesture.
        /// </summary>
        /// <remarks>This value is equal to <see cref="TotalTime"/> for discrete gestures.</remarks>
        public TimeSpan DeltaTime { get; internal set; }

        /// <summary>
        /// The time elapsed since the beginning of the gesture.
        /// </summary>
        public TimeSpan TotalTime { get; internal set; }
    }
}
