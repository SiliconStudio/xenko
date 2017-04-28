// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A gesture that generates a direction from mouse X and Y movement
    /// </summary>
    [DataContract]
    [Display("Mouse Movement")]
    public class MouseMovementGesture : DirectionGestureBase, IInputEventListener<PointerEvent>
    {
        private Vector2 lastValue = Vector2.Zero;
        private IInputDevice lastDevice;
        
        public override void PreUpdate(TimeSpan elapsedTime)
        {
            base.PreUpdate(elapsedTime);

            // Reset value every frame
            lastValue = Vector2.Zero;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (lastDevice != null)
                UpdateDirection(lastValue, lastDevice);
        }
        public void ProcessEvent(PointerEvent inputEvent)
        {
            lastValue = inputEvent.AbsoluteDeltaPosition;
            lastDevice = inputEvent.Device;
        }

        protected bool Equals(MouseMovementGesture other)
        {
            // All mouse movement gestures detect the same thing
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MouseMovementGesture)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}