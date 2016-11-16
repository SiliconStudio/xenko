// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Represents a gamepad axis reading
    /// </summary>
    [DataContract]
    public class GamePadAxisGesture : InvertibleInputGesture, IAxisGesture, IInputEventListener<GamePadAxisEvent>
    {
        /// <summary>
        /// The gamepad axis identifier
        /// </summary>
        public GamePadAxis GamePadAxis;

        private float currentState;

        public GamePadAxisGesture()
        {
        }

        public GamePadAxisGesture(GamePadAxis axis)
        {
            GamePadAxis = axis;
        }

        [DataMemberIgnore]
        public float Axis => GetScaledOutput(currentState);

        [DataMemberIgnore]
        public bool IsRelative { get; } = true;

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ActionMapping.ControllerIndex)
            {
                if ((inputEvent.Axis & GamePadAxis) != 0)
                    currentState = inputEvent.Value;
            }
        }

        public override string ToString()
        {
            return $"{nameof(GamePadAxis)}: {GamePadAxis}, {nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}, {nameof(IsRelative)}: {IsRelative}";
        }

        protected bool Equals(GamePadAxisGesture other)
        {
            return GamePadAxis == other.GamePadAxis;
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
            return GamePadAxis.GetHashCode();
        }
    }
}