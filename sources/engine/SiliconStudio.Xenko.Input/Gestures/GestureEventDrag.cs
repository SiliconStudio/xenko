// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event class for the Drag gesture.
    /// </summary>
    public sealed class GestureEventDrag : GestureEventTranslation
    {
        internal void Set(GestureState state, int numberOfFingers, TimeSpan deltaTime, TimeSpan totalTime,
                                    GestureShape shape, Vector2 startPos, Vector2 currPos, Vector2 deltaTrans)
        {
            base.Set(GestureType.Drag, state, numberOfFingers, deltaTime, totalTime, shape, startPos, currPos, deltaTrans);
        }
    }
}
