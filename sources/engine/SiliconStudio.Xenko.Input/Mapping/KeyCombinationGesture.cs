// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Represents a key combination, such as Alt+F4, Ctrl+Shift+4, etc.
    /// </summary>
    [DataContract]
    public class KeyCombinationGesture : InputGesture, IButtonGesture, IInputEventListener<KeyEvent>
    {
        /// <summary>
        /// The keys that are checked. When all these keys are pressed, the button evaluates to true.
        /// </summary>
        public List<Keys> Keys;

        private readonly HashSet<Keys> heldKeys = new HashSet<Keys>();

        public KeyCombinationGesture()
        {
        }

        public KeyCombinationGesture(params Keys[] keys)
        {
            this.Keys = new List<Keys>(keys);
        }

        [DataMemberIgnore]
        public bool Button => Keys != null && heldKeys.Count == Keys.Count;

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (!(ActionMapping?.AcceptKeyboard ?? true))
                return;

            if (Keys?.Contains(inputEvent.Key) ?? false)
            {
                if (inputEvent.State == ButtonState.Down)
                    heldKeys.Add(inputEvent.Key);
                else
                    heldKeys.Remove(inputEvent.Key);
            }
        }

        public override string ToString()
        {
            return $"Keys: {string.Join(", ", Keys)}, Held Keys: {string.Join(", ", heldKeys)}";
        }

        protected bool Equals(KeyCombinationGesture other)
        {
            return Equals(Keys, other.Keys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyCombinationGesture)obj);
        }

        public override int GetHashCode()
        {
            return Keys?.GetHashCode() ?? 0;
        }
    }
}