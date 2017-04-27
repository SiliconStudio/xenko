// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Use this class to provide a debug output in Visual Studio debugger.
    /// </summary>
    public class CollectionDebugView
    {
        private readonly IEnumerable collection;

        public CollectionDebugView([NotNull] IEnumerable collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            this.collection = collection;
        }

        [NotNull]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items => collection.Cast<object>().ToArray();
    }

    /// <summary>
    /// Use this class to provide a debug output in Visual Studio debugger.
    /// </summary>
    public class CollectionDebugView<T>
    {
        private readonly ICollection<T> collection;

        public CollectionDebugView([NotNull] ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            this.collection = collection;
        }

        [NotNull]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var array = new T[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}
