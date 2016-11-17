// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A direction or a 0-1 value generated from a gamepad pov controller
    /// </summary>
    [DataContract]
    public class GameControllerPovGesture : DirectionGestureBase, IInputEventListener<PovControllerEvent>
    {
        /// <summary>
        /// The index of the pov controller to use
        /// </summary>
        public int Index = 0;
        
        public GameControllerPovGesture()
        {
        }

        public GameControllerPovGesture(int index)
        {
            Index = index;
        }

        public void ProcessEvent(PovControllerEvent inputEvent)
        {
            if (inputEvent.Index == Index)
            {
                if (inputEvent.Enabled)
                {
                    var direction = new Vector2((float)Math.Sin(inputEvent.Value * Math.PI * 2), 
                        (float)Math.Cos(inputEvent.Value * Math.PI * 2));
                    UpdateDirection(direction, inputEvent.Device);
                }
                else
                {
                    UpdateDirection(Vector2.Zero, inputEvent.Device);
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}, {base.ToString()}";
        }

        protected bool Equals(GameControllerPovGesture other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameControllerPovGesture)obj);
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }
}