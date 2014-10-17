// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Quantum.Legacy
{
    class ListEqualityComparer<T> : EqualityComparer<IList<T>>
    {
        private readonly IEqualityComparer<T> itemComparer;

        public ListEqualityComparer()
        {
            this.itemComparer = EqualityComparer<T>.Default;
        }

        public ListEqualityComparer(IEqualityComparer<T> itemComparer)
        {
            this.itemComparer = itemComparer;
        }

        public override bool Equals(IList<T> x, IList<T> y)
        {
            return ArrayExtensions.ArraysEqual(x, y, itemComparer);
        }

        public override int GetHashCode(IList<T> obj)
        {
            return obj.ComputeHash(itemComparer);
        }
    }
}