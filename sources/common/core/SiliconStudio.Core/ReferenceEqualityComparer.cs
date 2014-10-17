// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core
{
    /// <summary>
    /// A Comparator to use <see cref="object.ReferenceEquals"/> method.
    /// </summary>
    /// <typeparam name="T">Type of the comparer</typeparam>
    public class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
    {
        private static IEqualityComparer<T> defaultComparer;

        /// <summary>
        /// Gets the default.
        /// </summary>
        public new static IEqualityComparer<T> Default
        {
            get { return defaultComparer ?? (defaultComparer = new ReferenceEqualityComparer<T>()); }
        }

        #region IEqualityComparer<T> Members

        /// <inheritdoc/>
        public override bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        /// <inheritdoc/>
        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

        #endregion
    }

}
