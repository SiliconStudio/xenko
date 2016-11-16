// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    [DataContract]
    public class MouseButtonGesture : InputGesture, IButtonGesture, IInputEventListener<MouseButtonEvent>
    {
        /// <summary>
        /// Button used for this gesture
        /// </summary>
        public MouseButton MouseButton;

        private ButtonState currentState = ButtonState.Up;

        public MouseButtonGesture()
        {
        }

        public MouseButtonGesture(MouseButton button)
        {
            MouseButton = button;
        }

        [DataMemberIgnore]
        public bool Button => currentState == ButtonState.Down;

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (!(ActionMapping?.AcceptMouse ?? true))
                return;

            if (inputEvent.Button == MouseButton)
            {
                currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(MouseButton)}: {MouseButton}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(MouseButtonGesture other)
        {
            return MouseButton == other.MouseButton;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MouseButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)MouseButton;
        }
    }
}