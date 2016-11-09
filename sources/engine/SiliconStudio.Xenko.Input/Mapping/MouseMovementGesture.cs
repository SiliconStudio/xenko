// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A gesture that responds to mouse movement
    /// </summary>
    [DataContract]
    public class MouseMovementGesture : ScalableInputGesture,  IDirectionGesture, IInputEventListener<PointerEvent>
    {
        private Vector2 currentDirection;
        
        [DataMemberIgnore]
        public Vector2 Direction => GetScaledOutput(currentDirection);

        public override void Reset()
        {
            // Reset delta
            currentDirection = Vector2.Zero;
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            currentDirection = inputEvent.DeltaPosition;
        }

        public override string ToString()
        {
            return $"{nameof(Direction)}: {Direction}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}";
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