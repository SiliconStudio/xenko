// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Extensions
{
    /// <summary>
    /// An extension class for various types of collection.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Remove an item by swapping it with the last item and removing it from the last position. This function prevents to shift values from the list on removal but does not maintain order.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="item">The item to remove.</param>
        public static void SwapRemove<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index < 0)
                return;

            list.SwapRemoveAt(index);
        }

        /// <summary>
        /// Remove an item by swapping it with the last item and removing it from the last position. This function prevents to shift values from the list on removal but does not maintain order.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="index">Index of the item to remove.</param>
        public static void SwapRemoveAt<T>(this IList<T> list, int index)
        {
            if (index < 0 || index >= list.Count) throw new ArgumentOutOfRangeException("index");

            if (index < list.Count - 1)
            {
                list[index] = list[list.Count - 1];
            }

            list.RemoveAt(list.Count - 1);
        }

        /// <summary>
        /// Gets the item from a list at a specified index. If index is out of the list, returns null.
        /// </summary>
        /// <typeparam name="T">Type of the item in the list</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <returns>The item from a list at a specified index. If index is out of the list, returns null..</returns>
        public static T GetItemOrNull<T>(this IList<T> list, int index) where T : class
        {
            if (list != null && index >= 0 && index < list.Count)
            {
                return list[index];
            }
            return null;
        }
    }
}
