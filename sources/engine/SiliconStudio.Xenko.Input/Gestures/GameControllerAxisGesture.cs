// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Represents a game controller axis reading
    /// </summary>
    [DataContract]
    [Display("Controller Axis")]
    public class GameControllerAxisGesture : AxisGestureBase, IInputEventListener<GameControllerAxisEvent>, IGameControllerGesture
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int Index = 0;
        
        private Guid controllerId;

        public GameControllerAxisGesture()
        {
        }

        public GameControllerAxisGesture(int index, Guid controllerId)
        {
            Index = index;
            this.controllerId = controllerId;
        }

        /// <summary>
        /// Id of the controller to watch
        /// </summary>
        public Guid ControllerId
        {
            get { return controllerId; }
            set { controllerId = value; }
        }

        public void ProcessEvent(GameControllerAxisEvent inputEvent)
        {
            if (inputEvent.GameController.Id == controllerId || controllerId == Guid.Empty)
            {
                if (inputEvent.Index == Index)
                    UpdateAxis(inputEvent.Value, inputEvent.Device);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {nameof(controllerId)}: {controllerId}, {base.ToString()}";
        }

        protected bool Equals(GameControllerAxisGesture other)
        {
            return Index == other.Index && controllerId.Equals(other.controllerId);
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
                return (Index * 397) ^ controllerId.GetHashCode();
            }
        }
    }
}