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
        public int AxisIndex = -1;

        /// <summary>
        /// Additionally the gamepad axis to map to if a <see cref="GamePadLayout"/> is used
        /// </summary>
        public GamePadAxis GamePadAxis;

        /// <summary>
        /// The controller index
        /// </summary>
        internal int ControllerIndex = 0;

        private float currentState;

        public GamePadAxisGesture()
        {
        }

        public GamePadAxisGesture(int axisIndex)
        {
            AxisIndex = axisIndex;
        }

        [DataMemberIgnore]
        public float Axis => GetScaledOutput(currentState);

        [DataMemberIgnore]
        public bool IsRelative { get; } = true;

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex)
            {
                if (inputEvent.Index == AxisIndex)
                    currentState = inputEvent.Value;
                else if ((inputEvent.Axis & GamePadAxis) != 0)
                    currentState = inputEvent.MappedValue;
            }
        }

        public override string ToString()
        {
            return $"{nameof(AxisIndex)}: {AxisIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}, {nameof(IsRelative)}: {IsRelative}";
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