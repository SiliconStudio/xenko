// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SiliconStudio.Core.VisualStudio
{
    public static class KeyedCollectionExtensions
    {
        /// <summary>
        /// Adds the specified enumeration of values to this collection.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="collection">The collection to add the value to.</param>
        /// <param name="items">The items to add to the collection.</param>
        public static void AddRange<TKey, TValue>(this KeyedCollection<TKey, TValue> collection, IEnumerable<TValue> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }
        }
    }
}