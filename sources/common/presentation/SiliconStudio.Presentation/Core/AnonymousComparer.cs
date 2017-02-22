// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Core
{
    /// <summary>
    /// This class allows implementation of <see cref="IComparer{T}"/> using an anonymous function.
    /// </summary>
    /// <typeparam name="T">The type of object this comparer can compare.</typeparam>
    public class AnonymousComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> compare;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousComparer{T}"/> class.
        /// </summary>
        /// <param name="compare">The comparison function to use for this comparer.</param>
        public AnonymousComparer([NotNull] Func<T, T, int> compare)
        {
            if (compare == null) throw new ArgumentNullException(nameof(compare));
            this.compare = compare;
        }

        /// <inheritdoc/>
        public int Compare(T x, T y)
        {
            return compare(x, y);
        }
    }
}