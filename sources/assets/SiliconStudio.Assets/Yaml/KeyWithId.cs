// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A generic structure that implements the <see cref="IKeyWithId"/> interface for keys that are not deleted.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public struct KeyWithId<TKey> : IKeyWithId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyWithId{TKey}"/> structure.
        /// </summary>
        /// <param name="id">The <see cref="ItemId"/> associated to the key.</param>
        /// <param name="key">The key of the dictionary.</param>
        public KeyWithId(ItemId id, TKey key)
        {
            Id = id;
            Key = key;
        }

        /// <summary>
        /// The <see cref="ItemId"/> associated to the key.
        /// </summary>
        public readonly ItemId Id;
        /// <summary>
        /// The key of the dictionary.
        /// </summary>
        public readonly TKey Key;
        /// <inheritdoc/>
        ItemId IKeyWithId.Id => Id;
        /// <inheritdoc/>
        object IKeyWithId.Key => Key;
        /// <inheritdoc/>
        bool IKeyWithId.IsDeleted => false;
        /// <inheritdoc/>
        Type IKeyWithId.KeyType => typeof(TKey);
    }
}
