// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Represents a key combination, such as Alt+F4, Ctrl+Shift+4, etc.
    /// </summary>
    [DataContract]
    [Display("Key Combination")]
    public class KeyCombinationGesture : ButtonGestureBase, IInputEventListener<KeyEvent>
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
            Keys = new List<Keys>(keys);
        }
        
        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.RepeatCount > 0) return;
            if (Keys?.Contains(inputEvent.Key) ?? false)
            {
                if (inputEvent.State == ButtonState.Down)
                {
                    heldKeys.Add(inputEvent.Key);
                    if (heldKeys.Count == Keys.Count) // Is held after this
                    {
                        UpdateButton(ButtonState.Down, inputEvent.Device);
                    }
                }
                else
                {
                    if (heldKeys.Count == Keys.Count) // Was held before this
                    {
                        UpdateButton(ButtonState.Up, inputEvent.Device);
                    }
                    heldKeys.Remove(inputEvent.Key);
                }
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