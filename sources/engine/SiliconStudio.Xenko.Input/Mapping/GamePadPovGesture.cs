// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A direction or a 0-1 value generated from a gamepad pov controller
    /// </summary>
    [DataContract]
    public class GamePadPovGesture : ScalableInputGesture, IDirectionGesture, IInputEventListener<GamePadPovControllerEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int PovIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        internal int ControllerIndex = 0;

        private Vector2 currentDirection;
        private float currentState;

        public GamePadPovGesture()
        {
        }

        public GamePadPovGesture(int povIndex)
        {
            PovIndex = povIndex;
        }
        
        [DataMemberIgnore]
        public Vector2 Direction => GetScaledOutput(currentDirection, true);

        public void ProcessEvent(GamePadPovControllerEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == PovIndex)
            {
                if (inputEvent.Enabled)
                {
                    currentState = inputEvent.Value;
                    currentDirection = new Vector2((float)Math.Sin(currentState*Math.PI*2), (float)Math.Cos(currentState*Math.PI*2));
                }
                else
                {
                    currentState = 0.0f;
                    currentDirection = Vector2.Zero;
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(PovIndex)}: {PovIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Direction)}: {Direction}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}";
        }

        protected bool Equals(GamePadPovGesture other)
        {
            return PovIndex == other.PovIndex && ControllerIndex == other.ControllerIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadPovGesture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PovIndex*397) ^ ControllerIndex;
            }
        }
    }
}