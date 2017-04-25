// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
