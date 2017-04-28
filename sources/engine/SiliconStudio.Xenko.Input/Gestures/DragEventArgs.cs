// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Event class for the Drag gesture.
    /// </summary>
    public sealed class DragEventArgs : TranslationEventArgs
    {
        public DragEventArgs(IPointerDevice pointerDevice, PointerGestureEventType eventType, int fingerCount, TimeSpan deltaTime, TimeSpan totalTime,
            GestureShape shape, Vector2 startPos, Vector2 currPos, Vector2 deltaTrans)
            : base(pointerDevice, eventType, fingerCount, deltaTime,totalTime,shape,startPos,currPos,deltaTrans)
        {
        }
    }
}