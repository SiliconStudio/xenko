// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A key used to attach/retrieve property values from a <see cref="ShadowObject"/>
    /// </summary>
    /// <remarks>
    /// This key allow to associate two pseudo-keys together.
    /// </remarks>
    public struct ShadowObjectPropertyKey : IEquatable<ShadowObjectPropertyKey>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShadowObjectPropertyKey"/>
        /// </summary>
        /// <param name="item1">The first part of this key. Cannot be null</param>
        public ShadowObjectPropertyKey(object item1) : this()
        {
            if (item1 == null) throw new ArgumentNullException(nameof(item1));
            Item1 = item1;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShadowObjectPropertyKey"/>
        /// </summary>
        /// <param name="item1">The first part of this key. Cannot be null</param>
        /// <param name="item2">The second part of this key. Can be null</param>
        public ShadowObjectPropertyKey(object item1, object item2)
        {
            if (item1 == null) throw new ArgumentNullException(nameof(item1));
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// First part of this key.
        /// </summary>
        public readonly object Item1;

        /// <summary>
        /// Second part of this key.
        /// </summary>
        public readonly object Item2;

        public bool Equals(ShadowObjectPropertyKey other)
        {
            return Equals(Item1, other.Item1) && Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ShadowObjectPropertyKey && Equals((ShadowObjectPropertyKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Item1 != null ? Item1.GetHashCode() : 0) * 397) ^ (Item2 != null ? Item2.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ShadowObjectPropertyKey left, ShadowObjectPropertyKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShadowObjectPropertyKey left, ShadowObjectPropertyKey right)
        {
            return !left.Equals(right);
        }
    }
}