// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Represents a gamepad axis reading
    /// </summary>
    [DataContract]
    public class GamePadAxisGesture : ScalableInputGesture, IAxisGesture, IInputEventListener<GamePadAxisEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int AxisIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        internal int ControllerIndex = 0;

        private float currentState;
        
        public GamePadAxisGesture(int axisIndex)
        {
            AxisIndex = axisIndex;
        }

        [DataMemberIgnore]
        public float Axis => GetScaledOutput(currentState, true);

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == AxisIndex)
                currentState = inputEvent.Value;
        }

        public override string ToString()
        {
            return $"{nameof(AxisIndex)}: {AxisIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}";
        }

        protected bool Equals(GamePadAxisGesture other)
        {
            return AxisIndex == other.AxisIndex && ControllerIndex == other.ControllerIndex;
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
            unchecked
            {
                return (AxisIndex * 397) ^ ControllerIndex;
            }
        }
    }
}