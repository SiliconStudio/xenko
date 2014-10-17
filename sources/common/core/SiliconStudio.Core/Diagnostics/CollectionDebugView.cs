// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICollection = System.Collections.ICollection;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Use this class to provide a debug output in Visual Studio debugger.
    /// </summary>
    public class CollectionDebugView
    {
        private readonly IEnumerable collection;

        public CollectionDebugView(IEnumerable collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                return collection.Cast<object>().ToArray();
            }
        }
    }

    /// <summary>
    /// Use this class to provide a debug output in Visual Studio debugger.
    /// </summary>
    public class CollectionDebugView<T>
    {
        private readonly ICollection<T> collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[this.collection.Count];
                this.collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}