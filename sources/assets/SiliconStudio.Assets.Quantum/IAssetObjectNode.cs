using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public interface IAssetObjectNode : IAssetNode, IObjectNode
    {
        [NotNull]
        new IAssetMemberNode this[string name] { get; }

        new IAssetObjectNode IndexedTarget(Index index);

        void OverrideItem(bool isOverridden, Index index);

        void OverrideKey(bool isOverridden, Index index);

        void OverrideDeletedItem(bool isOverridden, ItemId deletedId);

        bool IsItemDeleted(ItemId itemId);

        bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds);

        void Restore(object restoredItem, ItemId id);

        void Restore(object restoredItem, Index index, ItemId id);

        void RemoveAndDiscard(object item, Index itemIndex, ItemId id);

        OverrideType GetItemOverride(Index index);

        OverrideType GetKeyOverride(Index index);

        bool IsItemInherited(Index index);

        bool IsKeyInherited(Index index);

        bool IsItemOverridden(Index index);

        bool IsItemOverriddenDeleted(ItemId id);

        bool IsKeyOverridden(Index index);

        ItemId IndexToId(Index index);

        bool TryIndexToId(Index index, out ItemId id);

        bool HasId(ItemId id);

        Index IdToIndex(ItemId id);

        bool TryIdToIndex(ItemId id, out Index index);

        /// <summary>
        /// Resets the overrides attached to this node at a specific index and to its descendants, recursively.
        /// </summary>
        /// <param name="indexToReset">The index of the override to reset in this node.</param>
        void ResetOverrideRecursively(Index indexToReset);


        IEnumerable<Index> GetOverriddenItemIndices();

        IEnumerable<Index> GetOverriddenKeyIndices();
    }
}
