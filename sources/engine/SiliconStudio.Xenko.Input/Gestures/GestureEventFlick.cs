// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event class for the Flick gesture.
    /// </summary>
    public sealed class GestureEventFlick : GestureEventTranslation
    {
        internal void Set( int numberOfFingers, TimeSpan time, GestureShape shape, Vector2 startPos, Vector2 currPos)
        {
            base.Set(GestureType.Flick, GestureState.Occurred, numberOfFingers, time, time, shape, startPos, currPos, currPos - startPos);
        }
    }
}
