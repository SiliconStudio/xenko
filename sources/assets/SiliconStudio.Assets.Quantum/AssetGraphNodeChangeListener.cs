using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Quantum;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Editor.ViewModel.Quantum
{
    public class AssetGraphNodeChangeListener : GraphNodeChangeListener
    {
        private readonly Dictionary<IContentNode, MemberFlags> previousMemberFlags = new Dictionary<IContentNode, MemberFlags>();

        public AssetGraphNodeChangeListener(IGraphNode rootNode, Func<MemberContent, IGraphNode, bool> shouldRegisterNode)
            : base(rootNode, shouldRegisterNode)
        {
        }

        /// <summary>
        /// Gets the set of registered node. This property is exposed for debug purpose only.
        /// </summary>
        internal HashSet<IGraphNode> DebugRegisteredNodes => RegisteredNodes;

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
            if (e.ChangeType == ContentChangeType.ValueChange || e.ChangeType == ContentChangeType.CollectionRemove)
            {
                var overrideValue = ((AssetNode)e.Content.OwnerNode).GetMemberFlags(e.Index);
                previousMemberFlags[e.Content.OwnerNode] = overrideValue;
            }
            if (e.ChangeType == ContentChangeType.CollectionAdd)
            {
                // If the change is an add, we set the previous flags as Default so the Undo will try to remove the item instead of resetting to the base value
                previousMemberFlags[e.Content.OwnerNode] = MemberFlags.Default;
            }
        }

        private void AssetContentChanged(object sender, ContentChangeEventArgs e)
        {
            var oldMemberFlags = previousMemberFlags[e.Content.OwnerNode];
            previousMemberFlags.Remove(e.Content.OwnerNode);
            var newMemberFlags = MemberFlags.Default;
            if (e.ChangeType == ContentChangeType.ValueChange || e.ChangeType == ContentChangeType.CollectionAdd)
            {
                newMemberFlags = ((AssetNode)e.Content.OwnerNode).GetMemberFlags(e.Index);
            }
            ChangedWithOverride?.Invoke(sender, new AssetContentChangeEventArgs(e, oldMemberFlags, newMemberFlags));
        }
    }
}
