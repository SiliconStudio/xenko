// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A gesture that responds to movement of a a single mouse mouse axis
    /// </summary>
    [DataContract]
    [Display("Mouse Axis")]
    public class MouseAxisGesture : AxisGestureBase, IInputEventListener<PointerEvent>, IInputEventListener<MouseWheelEvent>
    {
        private float lastValue = 0.0f;
        private IInputDevice lastDevice;

        /// <summary>
        /// The axis that is this gesture detects
        /// </summary>
        public MouseAxis Axis;

        public MouseAxisGesture()
        {
        }

        public MouseAxisGesture(MouseAxis axis)
        {
            Axis = axis;
        }

        public override void PreUpdate(TimeSpan elapsedTime)
        {
            base.PreUpdate(elapsedTime);

            // Reset value every frame
            lastValue = 0.0f;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if(lastDevice != null)
                UpdateAxis(lastValue, lastDevice);
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            switch (Axis)
            {
                case MouseAxis.X:
                    lastValue = inputEvent.AbsoluteDeltaPosition.X;
                    break;
                case MouseAxis.Y:
                    lastValue = inputEvent.AbsoluteDeltaPosition.Y;
                    break;
            }
            lastDevice = inputEvent.Device;
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            if (Axis == MouseAxis.Wheel)
            {
                lastValue = inputEvent.WheelDelta;
            }
            lastDevice = inputEvent.Device;
        }

        public override string ToString()
        {
            return $"{nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}";
        }

        protected bool Equals(MouseAxisGesture other)
        {
            return Axis == other.Axis;
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
            return (int)Axis;
        }
    }
}