// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// Array helper for a particular type, useful to get an empty array.
    /// </summary>
    /// <typeparam name="T">Type of the array element</typeparam>
    public struct ArrayHelper<T>
    {
        /// <summary>
        /// An empty array of the specified <see cref="T"/> element type.
        /// </summary>
        public static readonly T[] Empty = new T[0];
    }
}
