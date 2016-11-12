using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetGraphNodeChangeListener : GraphNodeChangeListener
    {
        private readonly Dictionary<IContentNode, OverrideType> previousOverrides = new Dictionary<IContentNode, OverrideType>();

        public AssetGraphNodeChangeListener(IGraphNode rootNode, Func<MemberContent, IGraphNode, bool> shouldRegisterNode)
            : base(rootNode, shouldRegisterNode)
        {
        }

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed.
        /// </summary>
        public event EventHandler<AssetContentChangeEventArgs> ChangedWithOverride;

        /// <summary>
        /// Registers the given node to the listener.
        /// </summary>
        /// <param name="node">The node to register.</param>
        /// <returns><c>True</c> if the node has been registered, <c>False</c> if it was already registered before.</returns>
        /// <remarks>This method returns a <c>bool</c> because a node can be registered multiple times via different paths.</remarks>
        protected override bool RegisterNode(IGraphNode node)
        {
            if (base.RegisterNode(node))
            {
                ((AssetNode)node).Content.Changing += AssetContentChanging;
                ((AssetNode)node).Content.Changed += AssetContentChanged;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unregisters the given node from the listener.
        /// </summary>
        /// <param name="node">The node to unregister.</param>
        /// <returns><c>True</c> if the node has been unregistered, <c>False</c> if it was not registered.</returns>
        /// <remarks>This method returns a <c>bool</c> because a node can be unregistered multiple times via different paths.</remarks>
        protected override bool UnregisterNode(IGraphNode node)
        {
            if (base.RegisterNode(node))
            {
                ((AssetNode)node).Content.Changing -= AssetContentChanging;
                ((AssetNode)node).Content.Changed -= AssetContentChanged;
                return true;
            }
            return false;
        }

        private void AssetContentChanging(object sender, ContentChangeEventArgs e)
        {
            var overrideValue = OverrideType.Base;
            var node = (AssetNode)e.Content.OwnerNode;
            if (e.ChangeType == ContentChangeType.ValueChange || e.ChangeType == ContentChangeType.CollectionRemove)
            {
                if (e.Index == Index.Empty)
                {
                    overrideValue = node.GetContentOverride();
                }
                else if (!node.IsNonIdentifiableCollectionContent)
                {
                    overrideValue = node.GetItemOverride(e.Index);
                }
            }
            if (e.ChangeType == ContentChangeType.CollectionAdd && !node.IsNonIdentifiableCollectionContent)
            {
                // If the change is an add, we set the previous override as New so the Undo will try to remove the item instead of resetting to the base value
                previousOverrides[e.Content.OwnerNode] = OverrideType.New;
            }
            previousOverrides[e.Content.OwnerNode] = overrideValue;
        }

        private void AssetContentChanged(object sender, ContentChangeEventArgs e)
        {
            var previousOverride = previousOverrides[e.Content.OwnerNode];
            previousOverrides.Remove(e.Content.OwnerNode);

            var overrideValue = OverrideType.Base;
            if (e.ChangeType == ContentChangeType.ValueChange || e.ChangeType == ContentChangeType.CollectionAdd)
            {
                var node = (AssetNode)e.Content.OwnerNode;
                if (e.Index == Index.Empty)
                {
                    overrideValue = node.GetContentOverride();
                }
                else if (!node.IsNonIdentifiableCollectionContent)
                {
                    overrideValue = node.GetItemOverride(e.Index);
                }
            }

            ChangedWithOverride?.Invoke(sender, new AssetContentChangeEventArgs(e, previousOverride, overrideValue));
        }
    }
}
