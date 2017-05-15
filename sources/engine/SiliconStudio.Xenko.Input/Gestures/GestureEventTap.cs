// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event class for the Tap gesture.
    /// </summary>
    public sealed class GestureEventTap : GestureEvent
    {
        /// <summary>
        /// The number of time the use successively touched the screen.
        /// </summary>
        public int NumberOfTaps { get; internal set; }

        /// <summary>
        /// The position of the tap.
        /// </summary>
        public Vector2 TapPosition { get; internal set; }


        internal GestureEventTap(TimeSpan takenTime, int numberOfFingers, int numberOfTaps, Vector2 position)
        {
            Type = GestureType.Tap;
            State = GestureState.Occurred;
            DeltaTime = takenTime;
            TotalTime = takenTime;
            NumberOfFinger = numberOfFingers;
            NumberOfTaps = numberOfTaps;
            TapPosition = position;
        }
    }
}
