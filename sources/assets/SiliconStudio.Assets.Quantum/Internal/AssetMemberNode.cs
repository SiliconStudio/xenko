using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum
{
    internal class AssetMemberNode : MemberNode, IAssetMemberNode, IAssetNodeInternal
    {
        private AssetPropertyGraph propertyGraph;
        private readonly Dictionary<string, IGraphNode> contents = new Dictionary<string, IGraphNode>();

        private OverrideType contentOverride;

        public AssetMemberNode(INodeBuilder nodeBuilder, Guid guid, IObjectNode parent, IMemberDescriptor memberDescriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder, guid, parent, memberDescriptor, isPrimitive, reference)
        {
            Changed += ContentChanged;
            IsNonIdentifiableCollectionContent = MemberDescriptor.GetCustomAttributes<NonIdentifiableCollectionItemsAttribute>(true)?.Any() ?? false;
            CanOverride = MemberDescriptor.GetCustomAttributes<NonOverridableAttribute>(true)?.Any() != true;
        }

        public bool IsNonIdentifiableCollectionContent { get; }

        public bool CanOverride { get; }

        internal bool ResettingOverride { get; set; }

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public AssetPropertyGraph PropertyGraph { get { return propertyGraph; } internal set { if (value == null) throw new ArgumentNullException(nameof(value)); propertyGraph = value; } }

        public IGraphNode BaseNode { get; private set; }

        public new IAssetObjectNode Parent => (IAssetObjectNode)base.Parent;

        public new IAssetObjectNode Target => (IAssetObjectNode)base.Target;

        public void SetContent(string key, IGraphNode node)
        {
            contents[key] = node;
        }

        public IGraphNode GetContent(string key)
        {
            IGraphNode node;
            contents.TryGetValue(key, out node);
            return node;
        }

        public void OverrideContent(bool isOverridden)
        {
            if (CanOverride)
            {
                OverrideChanging?.Invoke(this, EventArgs.Empty);
                contentOverride = isOverridden ? OverrideType.New : OverrideType.Base;
                OverrideChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public void ResetOverride(Index indexToReset)
        {
            OverrideContent(false);
            PropertyGraph.ResetOverride(this, indexToReset);
        }

        private void ContentChanged(object sender, MemberNodeChangeEventArgs e)
        {
            // Make sure that we have item ids everywhere we're supposed to.
            AssetCollectionItemIdHelper.GenerateMissingItemIds(e.Member.Retrieve());

            var node = (AssetMemberNode)e.Member;
            if (node.IsNonIdentifiableCollectionContent)
                return;

            // Don't update override if propagation from base is disabled.
            if (PropertyGraph?.Container == null || PropertyGraph?.Container?.PropagateChangesFromBase == false)
                return;

            // Mark it as New if it does not come from the base
            if (BaseNode != null && !PropertyGraph.UpdatingPropertyFromBase && !ResettingOverride)
            {
                OverrideContent(!ResettingOverride);
            }
        }

        internal void SetContentOverride(OverrideType overrideType)
        {
            if (CanOverride)
            {
                contentOverride = overrideType;
            }
        }

        public OverrideType GetContentOverride()
        {
            return contentOverride;
        }

        public bool IsContentOverridden()
        {
            return (contentOverride & OverrideType.New) == OverrideType.New;
        }

        public bool IsContentInherited()
        {
            return BaseNode != null && !IsContentOverridden();
        }

        bool IAssetNodeInternal.ResettingOverride { get; set; }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        void IAssetNodeInternal.SetBaseNode(IGraphNode node)
        {
            BaseNode = node;
        }
    }
}
