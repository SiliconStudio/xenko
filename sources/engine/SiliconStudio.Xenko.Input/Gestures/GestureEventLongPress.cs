// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event class for the LongPress gesture.
    /// </summary>
    public sealed class GestureEventLongPress : GestureEvent
    {
        /// <summary>
        /// The position where the LongPress gesture happened.
        /// </summary>
        public Vector2 Position { get; internal set; }

        internal void Set(int numberOfFinger, TimeSpan time, Vector2 position)
        {
            State = GestureState.Occurred;
            Type = GestureType.LongPress;
            NumberOfFinger = numberOfFinger;
            DeltaTime = time;
            TotalTime = time;
            Position = position;
        }
    }
}
