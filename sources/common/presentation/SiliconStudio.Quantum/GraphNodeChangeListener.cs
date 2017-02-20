// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An object that tracks the changes in the content of <see cref="IGraphNode"/> referenced by a given root node.
    /// A <see cref="GraphNodeChangeListener"/> will raise events on changes on any node that is either a child, or the
    /// target of a reference from the root node, recursively.
    /// </summary>
    public class GraphNodeChangeListener : INotifyContentValueChange, INotifyItemChange, IDisposable
    {
        private readonly IGraphNode rootNode;
        private readonly Func<IMemberNode, bool> shouldRegisterMemberTarget;
        private readonly Func<IGraphNode, Index, bool> shouldRegisterItemTarget;
        protected readonly HashSet<IGraphNode> RegisteredNodes = new HashSet<IGraphNode>();

        public GraphNodeChangeListener(IGraphNode rootNode, Func<IMemberNode, bool> shouldRegisterMemberTarget = null, Func<IGraphNode, Index, bool> shouldRegisterItemTarget = null)
        {
            this.rootNode = rootNode;
            this.shouldRegisterMemberTarget = shouldRegisterMemberTarget;
            this.shouldRegisterItemTarget = shouldRegisterItemTarget;
            RegisterAllNodes();
        }

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes.
        /// </summary>
        public event EventHandler<MemberNodeChangeEventArgs> Changing;

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed.
        /// </summary>
        public event EventHandler<MemberNodeChangeEventArgs> Changed;

        public event EventHandler<ItemChangeEventArgs> ItemChanging;

        public event EventHandler<ItemChangeEventArgs> ItemChanged;

        /// <inheritdoc/>
        public void Dispose()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => UnregisterNode(node);
            visitor.Visit(rootNode);
        }

        protected virtual bool RegisterNode(IGraphNode node)
        {
            // A node can be registered multiple times when it is referenced via multiple paths
            if (RegisteredNodes.Add(node))
            {
                ((IGraphNodeInternal)node).PrepareChange += ContentPrepareChange;
                ((IGraphNodeInternal)node).FinalizeChange += ContentFinalizeChange;
                var memberNode = node as IMemberNode;
                if (memberNode != null)
                {
                    memberNode.Changing += ContentChanging;
                    memberNode.Changed += ContentChanged;
                }
                var objectNode = node as IObjectNode;
                if (objectNode != null)
                {
                    objectNode.ItemChanging += OnItemChanging;
                    objectNode.ItemChanged += OnItemChanged;
                }
                return true;
            }

            return false;
        }

        protected virtual bool UnregisterNode(IGraphNode node)
        {
            if (RegisteredNodes.Remove(node))
            {
                ((IGraphNodeInternal)node).PrepareChange -= ContentPrepareChange;
                ((IGraphNodeInternal)node).FinalizeChange -= ContentFinalizeChange;
                var memberNode = node as IMemberNode;
                if (memberNode != null)
                {
                    memberNode.Changing -= ContentChanging;
                    memberNode.Changed -= ContentChanged;
                }
                var objectNode = node as IObjectNode;
                if (objectNode != null)
                {
                    objectNode.ItemChanging += OnItemChanging;
                    objectNode.ItemChanged += OnItemChanged;
                }
                return true;
            }
            return false;
        }

        private void RegisterAllNodes()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => RegisterNode(node);
            visitor.ShouldVisitMemberTargetNode =  shouldRegisterMemberTarget;
            visitor.ShouldVisitTargetItemNode = shouldRegisterItemTarget;
            visitor.Visit(rootNode);
        }

        private void ContentPrepareChange(object sender, INodeChangeEventArgs e)
        {
            var node = e.Node;
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node1, path) => UnregisterNode(node1);
            visitor.ShouldVisitMemberTargetNode = shouldRegisterMemberTarget;
            visitor.ShouldVisitTargetItemNode = shouldRegisterItemTarget;
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                case ContentChangeType.CollectionUpdate:
                    // The changed node itself is still valid, we don't want to unregister it
                    visitor.SkipRootNode = true;
                    visitor.Visit(node);
                    // TODO: In case of CollectionUpdate we could probably visit only the target node of the corresponding index
                    break;
                case ContentChangeType.CollectionRemove:
                    if (node.IsReference && e.OldValue != null)
                    {
                        var removedNode = (node as IObjectNode)?.ItemReferences[e.Index].TargetNode;
                        if (removedNode != null)
                        {
                            // TODO: review this
                            visitor.Visit(removedNode, node as MemberNode);
                        }
                    }
                    break;
            }
        }

        private void ContentFinalizeChange(object sender, INodeChangeEventArgs e)
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += (node, path) => RegisterNode(node);
            visitor.ShouldVisitMemberTargetNode = shouldRegisterMemberTarget;
            visitor.ShouldVisitTargetItemNode = shouldRegisterItemTarget;
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                case ContentChangeType.CollectionUpdate:
                    // The changed node itself is still valid, we don't want to re-register it
                    visitor.SkipRootNode = true;
                    visitor.Visit(e.Node);
                    // TODO: In case of CollectionUpdate we could probably visit only the target node of the corresponding index
                    break;
                case ContentChangeType.CollectionAdd:
                    if (e.Node.IsReference && e.NewValue != null)
                    {
                        IGraphNode addedNode;
                        Index index;
                        if (!e.Index.IsEmpty)
                        {
                            index = e.Index;
                            addedNode = (e.Node as IObjectNode)?.ItemReferences[e.Index].TargetNode;
                        }
                        else
                        {
                            // TODO: review this
                            var reference = (e.Node as IObjectNode)?.ItemReferences.First(x => x.TargetNode.Retrieve() == e.NewValue);
                            index = reference.Index;
                            addedNode = reference.TargetNode;
                        }

                        if (addedNode != null)
                        {
                            var path = new GraphNodePath(e.Node).PushIndex(index);
                            visitor.Visit(addedNode, e.Node as MemberNode, path);
                        }
                    }
                    break;
            }
        }

        private void ContentChanging(object sender, MemberNodeChangeEventArgs e)
        {
            Changing?.Invoke(sender, e);
        }

        private void ContentChanged(object sender, MemberNodeChangeEventArgs e)
        {
            Changed?.Invoke(sender, e);
        }

        private void OnItemChanging(object sender, ItemChangeEventArgs e)
        {
            ItemChanging?.Invoke(sender, e);
        }

        private void OnItemChanged(object sender, ItemChangeEventArgs e)
        {
            ItemChanged?.Invoke(sender, e);
        }
    }
}
