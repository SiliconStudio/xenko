// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Represents a key combination, such as Alt+F4, Ctrl+Shift+4, etc.
    /// </summary>
    [DataContract]
    public class KeyCombinationGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<KeyEvent>
    {
        [DataMember] private HashSet<Keys> keys;
        private readonly HashSet<Keys> heldKeys = new HashSet<Keys>();

        public KeyCombinationGesture()
        {
        }

        public KeyCombinationGesture(params Keys[] keys)
        {
            this.keys = new HashSet<Keys>(keys);
        }

        public bool Button => keys != null && heldKeys.Count == keys.Count;
        public float Axis => Button ? 1.0f : 0.0f;

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (keys?.Contains(inputEvent.Key) ?? false)
            {
                if (inputEvent.State == ButtonState.Pressed)
                    heldKeys.Add(inputEvent.Key);
                else
                    heldKeys.Remove(inputEvent.Key);
            }
        }

        public override string ToString()
        {
            return $"Keys: {string.Join(", ", keys)}, Held Keys: {string.Join(", ", heldKeys)}";
        }

        protected bool Equals(KeyCombinationGesture other)
        {
            return Equals(keys, other.keys);
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
            return (keys != null ? keys.GetHashCode() : 0);
        }
    }
}