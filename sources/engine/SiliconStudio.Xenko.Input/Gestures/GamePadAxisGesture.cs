// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Represents a gamepad axis reading
    /// </summary>
    [DataContract]
    public class GamePadAxisGesture : AxisGestureBase, IInputEventListener<GamePadAxisEvent>
    {
        /// <summary>
        /// The gamepad axis identifier
        /// </summary>
        public GamePadAxis Axis;

        public GamePadAxisGesture()
        {
        }

        public GamePadAxisGesture(GamePadAxis axis)
        {
            Axis = axis;
        }

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if ((inputEvent.Axis & Axis) != 0)
                UpdateAxis(inputEvent.Value, inputEvent.Device);
        }

        public override string ToString()
        {
            return $"{nameof(Axis)}: {Axis}, {base.ToString()}";
        }

        protected bool Equals(GamePadAxisGesture other)
        {
            return Axis == other.Axis;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadAxisGesture)obj);
        }

        public override int GetHashCode()
        {
            return Axis.GetHashCode();
        }
    }
}