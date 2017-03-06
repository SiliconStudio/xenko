using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public interface IAssetObjectNode : IAssetNode, IObjectNode
    {
        // TODO: this should be only here!
        //void ResetOverride(Index indexToReset);

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

        IEnumerable<Index> GetOverriddenItemIndices();

        IEnumerable<Index> GetOverriddenKeyIndices();
    }
}