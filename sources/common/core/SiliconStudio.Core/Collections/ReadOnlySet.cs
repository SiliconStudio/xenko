// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// Represents a strongly-typed, read-only set of element.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public class ReadOnlySet<T> : IReadOnlySet<T>
    {
        private readonly ISet<T> innerSet;

        public ReadOnlySet(ISet<T> innerSet)
        {
            this.innerSet = innerSet;
        }

        public bool Contains(T item)
        {
            return innerSet.Contains(item);
        }

        public int Count => innerSet.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return innerSet.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerSet.GetEnumerator();
        }
    }
}
