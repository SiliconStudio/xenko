// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Base class for the gesture events.
    /// </summary>
    public abstract class PointerGestureEventArgs : GestureEventArgs
    {
        protected PointerGestureEventArgs(IPointerDevice pointerDevice) : base(pointerDevice)
        {
        }

        /// <summary>
        /// The state of the gesture.
        /// </summary>
        public PointerGestureEventType EventType { get; internal set; }

        /// <summary>
        /// The number of fingers involved in the gesture.
        /// </summary>
        public int FingerCount { get; internal set; }

        /// <summary>
        /// The time elapsed between the two last events of the gesture.
        /// </summary>
        /// <remarks>This value is equal to <see cref="TotalTime"/> for discrete gestures.</remarks>
        public TimeSpan DeltaTime { get; internal set; }

        /// <summary>
        /// The time elapsed since the beginning of the gesture.
        /// </summary>
        public TimeSpan TotalTime { get; internal set; }

        /// <summary>
        /// The pointer device
        /// </summary>
        public IPointerDevice Pointer => (IPointerDevice)Device;
    }
}