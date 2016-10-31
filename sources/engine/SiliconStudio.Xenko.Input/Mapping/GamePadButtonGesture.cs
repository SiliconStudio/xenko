// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A button gesture generated from a gamepad button press
    /// </summary>
    [DataContract]
    public class GamePadButtonGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<GamePadButtonEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int ButtonIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        public int ControllerIndex = 0;

        private ButtonState currentState;

        public GamePadButtonGesture()
        {
        }

        public GamePadButtonGesture(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }

        public float Axis => Button ? 1.0f : 0.0f;
        public bool Button => currentState == ButtonState.Pressed;

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == ButtonIndex)
            {
                currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(ButtonIndex)}: {ButtonIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(GamePadButtonGesture other)
        {
            return ButtonIndex == other.ButtonIndex && ControllerIndex == other.ControllerIndex;
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
            unchecked
            {
                return (ButtonIndex*397) ^ ControllerIndex;
            }
        }
    }
}