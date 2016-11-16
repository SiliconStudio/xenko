// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A button gesture generated from a gamepad button press
    /// </summary>
    [DataContract]
    public class GameControllerButtonGesture : InputGesture, IButtonGesture, IInputEventListener<GameControllerButtonEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int ButtonIndex = -1;

        /// <summary>
        /// Additionally the gamepad button to map to if a <see cref="GamePadLayout"/> is used
        /// </summary>
        public GamePadButton GamePadButton;

        private ButtonState currentState;

        public GameControllerButtonGesture()
        {
        }

        public GameControllerButtonGesture(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }

        [DataMemberIgnore]
        public bool Button => currentState == ButtonState.Down;

        public void ProcessEvent(GameControllerButtonEvent inputEvent)
        {
            if (inputEvent.GameController.Index == ActionMapping.ControllerIndex)
            {
                if (inputEvent.Index == ButtonIndex || (inputEvent.Button & GamePadButton) != 0)
                    currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(ButtonIndex)}: {ButtonIndex}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(GameControllerButtonGesture other)
        {
            return ButtonIndex == other.ButtonIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameControllerButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return ButtonIndex;
        }
    }
}