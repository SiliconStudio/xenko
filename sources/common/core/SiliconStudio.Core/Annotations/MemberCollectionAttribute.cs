// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attributes provides additional information on a member collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class MemberCollectionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether this collection is read-only. If <c>true</c>, applications using this collection
        /// should not allow to add or remove items.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets whether the items of this collection can be reordered. If <c>true</c>, applications using
        /// this collection should provide users a way to reorder items.
        /// </summary>
        public bool CanReorderItems { get; set; }

        /// <summary>
        /// Gets or sets whether the items of this collection can be null. If <c>true</c>, applications using
        /// this collection should prevent user to add null items to the collection.
        /// </summary>
        public bool NotNullItems { get; set; }
    }
}
