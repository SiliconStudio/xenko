// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A type of action used by <see cref="MemberPath.Apply"/>
    /// </summary>
    public enum MemberPathAction
    {
        /// <summary>
        /// The value is set on the <see cref="MemberPath"/> (field/property setter, or new key for dictionary or index
        /// for collection/array)
        /// </summary>
        ValueSet,

        /// <summary>
        /// Removes a key from the dictionary
        /// </summary>
        DictionaryRemove,

        /// <summary>
        /// Adds a value to the collection.
        /// </summary>
        CollectionAdd,

        /// <summary>
        /// Removes a value from the collection
        /// </summary>
        CollectionRemove,

        /// <summary>
        /// Clears the value.
        /// </summary>
        ValueClear,
    }
}
