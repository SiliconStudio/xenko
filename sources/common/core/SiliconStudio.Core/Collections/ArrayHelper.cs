// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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