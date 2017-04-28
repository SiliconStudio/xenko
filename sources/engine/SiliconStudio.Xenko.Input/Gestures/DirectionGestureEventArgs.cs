// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public class DirectionGestureEventArgs : GestureEventArgs
    {
        public DirectionGestureEventArgs(IInputDevice device, Vector2 state) : base(device)
        {
            this.State = state;
        }
        
        public Vector2 State { get; internal set; }
    }
}