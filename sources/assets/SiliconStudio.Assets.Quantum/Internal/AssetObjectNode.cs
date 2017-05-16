// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum.Internal
{
    internal class AssetObjectNode : ObjectNode, IAssetObjectNodeInternal
    {
        private AssetObjectNodeExtended ex;

        public AssetObjectNode([NotNull] INodeBuilder nodeBuilder, object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder, value, guid, descriptor, isPrimitive, reference)
        {
            ex = new AssetObjectNodeExtended(this);
            ItemChanged += (sender, e) => ex.OnItemChanged(sender, e);
        }

        public AssetPropertyGraph PropertyGraph => ex.PropertyGraph;

        public IGraphNode BaseNode => ex.BaseNode;

        public new IAssetMemberNode this[string name] => (IAssetMemberNode)base[name];

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public void SetContent(string key, IGraphNode node) => ex.SetContent(key, node);

        public IGraphNode GetContent(string key) => ex.GetContent(key);

        public void ResetOverrideRecursively() => ex.ResetOverrideRecursively(Index.Empty);

        public void ResetOverrideRecursively(Index indexToReset) => ex.ResetOverrideRecursively(indexToReset);

        public void OverrideItem(bool isOverridden, Index index) => ex.OverrideItem(isOverridden, index);

        public void OverrideKey(bool isOverridden, Index index) => ex.OverrideKey(isOverridden, index);

        public void OverrideDeletedItem(bool isOverridden, ItemId deletedId) => ex.OverrideDeletedItem(isOverridden, deletedId);

        public bool IsItemDeleted(ItemId itemId) => ex.IsItemDeleted(itemId);

        public void Restore(object restoredItem, ItemId id) => ex.Restore(restoredItem, id);

        public void Restore(object restoredItem, Index index, ItemId id) => ex.Restore(restoredItem, index, id);

        public void RemoveAndDiscard(object item, Index itemIndex, ItemId id) => ex.RemoveAndDiscard(item, itemIndex, id);

        public OverrideType GetItemOverride(Index index) => ex.GetItemOverride(index);

        public OverrideType GetKeyOverride(Index index) => ex.GetKeyOverride(index);

        public bool IsItemInherited(Index index) => ex.IsItemInherited(index);

        public bool IsKeyInherited(Index index) => ex.IsKeyInherited(index);

        public bool IsItemOverridden(Index index) => ex.IsItemOverridden(index);

        public bool IsItemOverriddenDeleted(ItemId id) => ex.IsItemOverriddenDeleted(id);

        public bool IsKeyOverridden(Index index) => ex.IsKeyOverridden(index);

        public IEnumerable<Index> GetOverriddenItemIndices() => ex.GetOverriddenItemIndices();

        public IEnumerable<Index> GetOverriddenKeyIndices() => ex.GetOverriddenKeyIndices();

        public ItemId IndexToId(Index index) => ex.IndexToId(index);

        public bool TryIndexToId(Index index, out ItemId id) => ex.TryIndexToId(index, out id);

        public bool HasId(ItemId id) => ex.HasId(id);

        public Index IdToIndex(ItemId id) => ex.IdToIndex(id);

        public bool TryIdToIndex(ItemId id, out Index index) => ex.TryIdToIndex(id, out index);

        IAssetObjectNode IAssetObjectNode.IndexedTarget(Index index) => (IAssetObjectNode)IndexedTarget(index);

        void IAssetObjectNodeInternal.DisconnectOverriddenDeletedItem(ItemId deletedId) => ex.DisconnectOverriddenDeletedItem(deletedId);

        void IAssetObjectNodeInternal.NotifyOverrideChanging() => OverrideChanging?.Invoke(this, EventArgs.Empty);

        void IAssetObjectNodeInternal.NotifyOverrideChanged() => OverrideChanged?.Invoke(this, EventArgs.Empty);

        bool IAssetNodeInternal.ResettingOverride { get { return ex.ResettingOverride; } set { ex.ResettingOverride = value; } }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph) => ex.SetPropertyGraph(assetPropertyGraph);

        void IAssetNodeInternal.SetBaseNode(IGraphNode node) => ex.SetBaseContent(node);
    }
}
