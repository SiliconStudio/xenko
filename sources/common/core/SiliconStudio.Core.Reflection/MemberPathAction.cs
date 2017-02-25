// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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