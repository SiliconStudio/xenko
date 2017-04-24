// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
