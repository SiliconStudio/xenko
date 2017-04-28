// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A direction or a 0-1 value generated from a gamepad pov controller
    /// </summary>
    [DataContract]
    [Display("Point-of-view Switch")]
    public class PovControllerGesture : DirectionGestureBase, IInputEventListener<PovControllerEvent>, IGameControllerGesture
    {
        /// <summary>
        /// The index of the pov controller to use
        /// </summary>
        public int Index = 0;
        
        /// <summary>
        /// Id of the controller to watch
        /// </summary>
        private Guid controllerId;

        public PovControllerGesture()
        {
        }

        public PovControllerGesture(int index, Guid controllerId)
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

        public void ProcessEvent(PovControllerEvent inputEvent)
        {
            if (inputEvent.GameController.Id == controllerId || controllerId == Guid.Empty)
            {
                if (inputEvent.Index == Index)
                {
                    if (inputEvent.Enabled)
                    {
                        var direction = new Vector2((float)Math.Sin(inputEvent.Value * 2 * Math.PI),
                            (float)Math.Cos(inputEvent.Value * 2 * Math.PI));
                        UpdateDirection(direction, inputEvent.Device);
                    }
                    else
                    {
                        UpdateDirection(Vector2.Zero, inputEvent.Device);
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {nameof(controllerId)}: {controllerId}, {base.ToString()}";
        }

        protected bool Equals(PovControllerGesture other)
        {
            return Index == other.Index && controllerId.Equals(other.controllerId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PovControllerGesture)obj);
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