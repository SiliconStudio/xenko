// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A keyboard event.
    /// </summary>
    public struct KeyEvent : IEquatable<KeyEvent>
    {
        /// <summary>
        /// The key that is being pressed or released.
        /// </summary>
        public readonly Keys Key;

        /// <summary>
        /// The key event type (released or pressed).
        /// </summary>
        public readonly KeyEventType Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEvent"/> struct.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The type.</param>
        public KeyEvent(Keys key, KeyEventType type)
        {
            Key = key;
            Type = type;
        }

        /// <inheritdoc/>
        public bool Equals(KeyEvent other)
        {
            return Key == other.Key && Type == other.Type;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is KeyEvent && Equals((KeyEvent)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Key * 397) ^ (int)Type;
            }
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(KeyEvent left, KeyEvent right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(KeyEvent left, KeyEvent right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Key, Type == KeyEventType.Pressed ? "Pressed" : "Release");
        }
    }
}
