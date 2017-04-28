// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Event class for the LongPress gesture.
    /// </summary>
    public sealed class LongPressEventArgs : PointerGestureEventArgs
    {
        public LongPressEventArgs(IPointerDevice pointerDevice, int fingerCount, TimeSpan time, Vector2 position)
            : base(pointerDevice)
        {
            EventType = PointerGestureEventType.Occurred;
            FingerCount = fingerCount;
            DeltaTime = time;
            TotalTime = time;
            Position = position;
        }

        /// <summary>
        /// The position (in pixels) where the LongPress gesture happened.
        /// </summary>
        public Vector2 Position { get; internal set; }
    }
}