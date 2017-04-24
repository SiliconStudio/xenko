// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// An interface representing an association between an <see cref="ItemId"/> and the key of a dictionary.
    /// </summary>
    public interface IKeyWithId
    {
        /// <summary>
        /// The <see cref="ItemId"/> associated to the key.
        /// </summary>
        ItemId Id { get; }
        /// <summary>
        /// The key of the dictionary.
        /// </summary>
        object Key { get; }
        /// <summary>
        /// The type of the key.
        /// </summary>
        Type KeyType { get; }
        /// <summary>
        /// Indicates whether this key is considered to be deleted in the dictionary, and kept around for reconcilation purpose.
        /// </summary>
        bool IsDeleted { get; }
    }
}
