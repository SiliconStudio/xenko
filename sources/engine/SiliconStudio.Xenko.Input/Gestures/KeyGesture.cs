// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Represents a single keyboard key
    /// </summary>
    [DataContract]
    [Display("Key")]
    public class KeyGesture : ButtonGestureBase, IInputEventListener<KeyEvent>
    {
        /// <summary>
        /// Key used for this gesture
        /// </summary>
        public Keys Key;

        public KeyGesture()
        {
        }

        public KeyGesture(Keys key)
        {
            Key = key;
        }

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.RepeatCount > 0) return;
            if (inputEvent.Key == Key)
            {
                UpdateButton(inputEvent.State, inputEvent.Device);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}";
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