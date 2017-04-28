// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Event class for the Flick gesture.
    /// </summary>
    public sealed class FlickEventArgs : TranslationEventArgs
    {
        public FlickEventArgs(IPointerDevice pointerDevice, int fingerCount, TimeSpan time, GestureShape shape, Vector2 startPos, Vector2 currPos)
            : base(pointerDevice, PointerGestureEventType.Occurred, fingerCount, time, time, shape, startPos, currPos, currPos-startPos)
        {
        }
    }
}