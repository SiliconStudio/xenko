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
    public class GameControllerPovGesture : InvertibleInputGesture, IDirectionGesture, IInputEventListener<GameControllerPovControllerEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int PovIndex = -1;

        /// <summary>
        /// If this is true, it will map to the directional pad that is mapped by <see cref="GamePadLayout"/>
        /// </summary>
        public bool MapToLayoutPad = false;

        private Vector2 currentDirection;
        private float currentState;

        public GameControllerPovGesture()
        {
        }

        public GameControllerPovGesture(int povIndex)
        {
            PovIndex = povIndex;
        }
        
        [DataMemberIgnore]
        public Vector2 Direction => GetScaledOutput(currentDirection);

        [DataMemberIgnore]
        public bool IsRelative { get; } = true;

        public void ProcessEvent(GameControllerPovControllerEvent inputEvent)
        {
            if (inputEvent.GameController.Index == ActionMapping.ControllerIndex)
            {
                if (inputEvent.Index == PovIndex || (MapToLayoutPad && inputEvent.Button == GamePadButton.Pad))
                {
                    if (inputEvent.Enabled)
                    {
                        currentState = inputEvent.Value;
                        currentDirection = new Vector2((float)Math.Sin(currentState * Math.PI * 2), (float)Math.Cos(currentState * Math.PI * 2));
                    }
                    else
                    {
                        currentState = 0.0f;
                        currentDirection = Vector2.Zero;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(PovIndex)}: {PovIndex}, {nameof(Direction)}: {Direction}, {nameof(Inverted)}: {Inverted}, {nameof(IsRelative)}: {IsRelative}";
        }

        protected bool Equals(GameControllerPovGesture other)
        {
            return PovIndex == other.PovIndex;
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
            return PovIndex;
        }
    }
}