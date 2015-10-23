// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event class for the Flick gesture.
    /// </summary>
    public sealed class GestureEventFlick : GestureEventTranslation
    {
        internal GestureEventFlick( int numberOfFingers, TimeSpan time, GestureShape shape, Vector2 startPos, Vector2 currPos)
            :base(GestureType.Flick, GestureState.Occurred, numberOfFingers, time, time, shape, startPos, currPos, currPos-startPos)
        {
        }
    }
}