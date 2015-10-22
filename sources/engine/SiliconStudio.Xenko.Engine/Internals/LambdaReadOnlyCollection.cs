// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Xenko.Internals
{
    class LambdaReadOnlyCollection<TSource, T> : IReadOnlyList<T>
    {
        private IReadOnlyList<TSource> source;
        private Func<TSource, T> selector;

        public LambdaReadOnlyCollection(IReadOnlyList<TSource> source, Func<TSource, T> selector)
        {
            this.source = source;
            this.selector = selector;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return source.Select(x => selector(x)).GetEnumerator();
        }

        /// <inheritdoc/>
        public int Count { get { return source.Count; } }

        /// <inheritdoc/>
        public T this[int index]
        {
            get { return selector(source[index]); }
        }
    }
}