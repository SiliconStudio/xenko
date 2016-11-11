// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A gesture that responds to a single mouse mouse axis movement
    /// </summary>
    [DataContract]
    public class MouseAxisGesture : ScalableInputGesture, IAxisGesture, IInputEventListener<PointerEvent>, IInputEventListener<MouseWheelEvent>
    {
        /// <summary>
        /// Axis that is used for this gesture
        /// </summary>
        public MouseAxis MouseAxis;

        private float currentDelta;

        public MouseAxisGesture()
        {
        }

        public MouseAxisGesture(MouseAxis axis)
        {
            MouseAxis = axis;
        }

        [DataMemberIgnore]
        public float Axis => GetScaledOutput(currentDelta, false);

        public override void Reset(TimeSpan elapsedTime)
        {
            base.Reset(elapsedTime);

            // Reset delta
            currentDelta = 0;
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            if (Action.IgnoreMouseWhenNotLocked)
            {
                var mouse = inputEvent.Pointer as IMouseDevice;
                if (mouse != null && !mouse.IsMousePositionLocked)
                {
                    currentDelta = 0.0f;
                    return;
                }
            }
            
            switch (MouseAxis)
            {
                case MouseAxis.X:
                    currentDelta = inputEvent.AbsoluteDeltaPosition.X;
                    break;
                case MouseAxis.Y:
                    currentDelta = inputEvent.AbsoluteDeltaPosition.Y;
                    break;
            }
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            if (MouseAxis == MouseAxis.Wheel)
            {
                currentDelta = inputEvent.WheelDelta;
            }
        }

        public override string ToString()
        {
            return $"{nameof(MouseAxis)}: {MouseAxis}, {nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}";
        }

        protected bool Equals(MouseAxisGesture other)
        {
            return MouseAxis == other.MouseAxis;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MouseAxisGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)MouseAxis;
        }
    }
}