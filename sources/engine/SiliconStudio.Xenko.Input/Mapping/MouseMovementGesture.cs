// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    [DataContract]
    public class MouseMovementGesture : InputGesture, IAxisGesture, IDirectionGesture, IInputEventListener<PointerEvent>, IInputEventListener<MouseWheelEvent>
    {
        /// <summary>
        /// Axis that is used for this gesture
        /// </summary>
        public MouseAxis MouseAxis;

        private float currentDelta;
        private Vector2 currentDirection;

        public MouseMovementGesture()
        {
        }

        public MouseMovementGesture(MouseAxis axis)
        {
            MouseAxis = axis;
        }

        public float Axis => Inverted ? -currentDelta : currentDelta;
        public Vector2 Direction => currentDirection;
        public bool Inverted { get; set; } = false;

        public override void Reset()
        {
            // Reset delta
            currentDelta = 0;
            currentDirection = Vector2.Zero;
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            switch (MouseAxis)
            {
                case MouseAxis.X:
                    currentDelta = inputEvent.DeltaPosition.X;
                    break;
                case MouseAxis.Y:
                    currentDelta = inputEvent.DeltaPosition.Y;
                    break;
            }
            currentDirection = inputEvent.DeltaPosition;
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
            return $"{nameof(MouseAxis)}: {MouseAxis}, {nameof(Axis)}: {Axis}, {nameof(Direction)}: {Direction}, {nameof(Inverted)}: {Inverted}";
        }

        protected bool Equals(MouseMovementGesture other)
        {
            return MouseAxis == other.MouseAxis;
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
            return (int)MouseAxis;
        }
    }
}