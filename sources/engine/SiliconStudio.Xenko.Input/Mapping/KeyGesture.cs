// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Represents a single keyboard key
    /// </summary>
    [DataContract]
    public class KeyGesture : InputGesture, IButtonGesture, IInputEventListener<KeyEvent>
    {
        /// <summary>
        /// Key used for this gesture
        /// </summary>
        public Keys Key;

        private ButtonState currentState = ButtonState.Up;

        public KeyGesture()
        {
        }

        public KeyGesture(Keys key)
        {
            this.Key = key;
        }

        [DataMemberIgnore]
        public bool Button => currentState == ButtonState.Down;

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (!(ActionMapping?.AcceptKeyboard ?? true))
                return;

            if (inputEvent.Key == Key)
            {
                currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(KeyGesture other)
        {
            return Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)Key;
        }
    }
}