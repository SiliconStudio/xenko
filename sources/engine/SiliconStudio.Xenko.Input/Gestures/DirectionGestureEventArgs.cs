// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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