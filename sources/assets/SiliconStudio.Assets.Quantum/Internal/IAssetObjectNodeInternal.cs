// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Internal
{
    /// <summary>
    /// An interface exposing internal methods of <see cref="IAssetObjectNode"/>
    /// </summary>
    internal interface IAssetObjectNodeInternal : IAssetObjectNode, IAssetNodeInternal
    {
        OverrideType GetItemOverride(Index index);

        OverrideType GetKeyOverride(Index index);

        /// <summary>
        /// Removes the given <see cref="ItemId"/> from the list of overridden deleted items in the underlying <see cref="CollectionItemIdentifiers"/>, but keep
        /// track of it if this node is requested whether this id is overridden-deleted.
        /// </summary>
        /// <param name="deletedId">The id to disconnect.</param>
        /// <remarks>The purpose of this method is to unmark as deleted the given id, but keep track of it for undo-redo.</remarks>
        void DisconnectOverriddenDeletedItem(ItemId deletedId);

        void NotifyOverrideChanging();

        void NotifyOverrideChanged();
    }
}
