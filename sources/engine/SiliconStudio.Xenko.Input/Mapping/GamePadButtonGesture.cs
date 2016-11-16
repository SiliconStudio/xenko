// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A button gesture generated from a gamepad button press
    /// </summary>
    [DataContract]
    public class GamePadButtonGesture : InputGesture, IButtonGesture, IInputEventListener<GamePadButtonEvent>
    {
        /// <summary>
        /// The gamepad button identifier
        /// </summary>
        public GamePadButton GamePadButton;

        private ButtonState currentState;

        public GamePadButtonGesture()
        {
        }

        public GamePadButtonGesture(GamePadButton button)
        {
            GamePadButton = button;
        }

        [DataMemberIgnore]
        public bool Button => currentState == ButtonState.Down;

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ActionMapping.ControllerIndex)
            {
                if ((inputEvent.Button & GamePadButton) != 0)
                    currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(GamePadButton)}: {GamePadButton}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(GamePadButtonGesture other)
        {
            return GamePadButton == other.GamePadButton;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return GamePadButton.GetHashCode();
        }
    }
}