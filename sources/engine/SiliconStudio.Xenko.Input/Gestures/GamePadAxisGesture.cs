// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Represents a gamepad axis reading
    /// </summary>
    [DataContract]
    [Display("Gamepad Axis")]
    public class GamePadAxisGesture : AxisGestureBase, IInputEventListener<GamePadAxisEvent>, IGamePadGesture
    {
        /// <summary>
        /// The gamepad axis identifier
        /// </summary>
        public GamePadAxis Axis;

        /// <summary>
        /// The index of the gamepad to watch
        /// </summary>
        public int GamePadIndex { get; set; }

        public GamePadAxisGesture()
        {
        }

        public GamePadAxisGesture(GamePadAxis axis, int gamePadIndex)
        {
            Axis = axis;
            GamePadIndex = gamePadIndex;
        }
        
        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == GamePadIndex)
            {
                if ((inputEvent.Axis & Axis) != 0)
                    UpdateAxis(inputEvent.Value, inputEvent.Device);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Axis)}: {Axis}, {nameof(GamePadIndex)}: {GamePadIndex}, {base.ToString()}";
        }

        protected bool Equals(GamePadAxisGesture other)
        {
            return Axis == other.Axis && GamePadIndex == other.GamePadIndex;
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
                return ((int)Axis * 397) ^ GamePadIndex;
            }
        }
    }
}