// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Represents a game controller axis reading
    /// </summary>
    [DataContract]
    public class GameControllerAxisGesture : AxisGestureBase, IInputEventListener<GameControllerAxisEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int Index = 0;

        public GameControllerAxisGesture()
        {
        }

        public GameControllerAxisGesture(int index)
        {
            Index = index;
        }
        
        public void ProcessEvent(GameControllerAxisEvent inputEvent)
        {
            if (inputEvent.Index == Index)
                UpdateAxis(inputEvent.Value, inputEvent.Device);
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {base.ToString()}";
        }

        protected bool Equals(GameControllerAxisGesture other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameControllerAxisGesture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Index;
            }
        }
    }
}