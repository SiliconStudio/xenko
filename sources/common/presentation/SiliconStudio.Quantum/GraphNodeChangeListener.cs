// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    internal class GraphNodeRegistry
    {
        private readonly Dictionary<IGraphNode, List<GraphNodePath>> nodeToPaths = new Dictionary<IGraphNode, List<GraphNodePath>>();
        private readonly Dictionary<GraphNodePath, IGraphNode> pathToNode = new Dictionary<GraphNodePath, IGraphNode>();

        public void RegisterNode(IGraphNode node, GraphNodePath path)
        {
            pathToNode.Add(path, node);
            List<GraphNodePath> paths;
            if (!nodeToPaths.TryGetValue(node, out paths))
            {
                paths = new List<GraphNodePath>();
                nodeToPaths.Add(node, paths);
            }
            else
            {

            }
            paths.Add(path);
        }

        public void UnregisterNode(IGraphNode node, GraphNodePath path)
        {
            pathToNode.Remove(path);
            List<GraphNodePath> paths;
            if (nodeToPaths.TryGetValue(node, out paths))
            {
                paths.Remove(path);
                if (paths.Count == 0)
                    nodeToPaths.Remove(node);
            }
        }

        public IReadOnlyList<GraphNodePath> GetNodePaths(IGraphNode node)
        {
            List<GraphNodePath> paths;
            nodeToPaths.TryGetValue(node, out paths);
            return paths;
        }

        public IGraphNode GetNode(GraphNodePath path)
        {
            IGraphNode node;
            pathToNode.TryGetValue(path, out node);
            return node;
        }
    }

    /// <summary>
    /// An object that tracks the changes in the content of <see cref="IGraphNode"/> referenced by a given root node.
    /// A <see cref="GraphNodeChangeListener"/> will raise events on changes on any node that is either a child, or the
    /// target of a reference from the root node, recursively.
    /// </summary>
    public class GraphNodeChangeListener : IDisposable
    {
        private readonly IGraphNode rootNode;
        private readonly GraphNodeRegistry registeredNodes = new GraphNodeRegistry();
        private readonly Func<IGraphNode, GraphNodePath, bool> shouldRegisterNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeChangeListener"/> class.
        /// </summary>
        /// <param name="rootNode">The root node for which to track referenced node changes.</param>
        /// <param name="shouldRegisterNode">A method that can indicate whether a node of the hierarchy should be registered to the listener.</param>
        public GraphNodeChangeListener(IGraphNode rootNode, Func<IGraphNode, GraphNodePath, bool> shouldRegisterNode)
        {
            this.rootNode = rootNode;
            this.shouldRegisterNode = shouldRegisterNode;
            RegisterAllNodes();
        }

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes and before the <see cref="Changing"/> event is raised.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> PrepareChange;

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed and after the <see cref="Changed"/> event is raised.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> FinalizeChange;

        /// <summary>
        /// Raised before one of the node referenced by the related root node changes.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> Changing;

        /// <summary>
        /// Raised after one of the node referenced by the related root node has changed.
        /// </summary>
        public event EventHandler<GraphContentChangeEventArgs> Changed;

        /// <inheritdoc/>
        public void Dispose()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += UnregisterNode;
            visitor.Visit(rootNode);
        }

        [Obsolete("Use GetPaths to support multiple paths for the same node.")]
        public GraphNodePath GetPath(IContentNode node)
        {
            var paths = GetPaths(node);
            return paths.First();
        }

        public IReadOnlyList<GraphNodePath> GetPaths(IContentNode node)
        {
            var graphNode = node as IGraphNode;
            return graphNode != null ? registeredNodes.GetNodePaths(graphNode) : null;
        }

        protected virtual void RegisterNode(IGraphNode node, GraphNodePath path)
        {
            registeredNodes.RegisterNode(node, path);
            node.Content.PrepareChange += ContentPrepareChange;
            node.Content.FinalizeChange += ContentFinalizeChange;
            node.Content.Changing += ContentChanging;
            node.Content.Changed += ContentChanged;
        }

        protected virtual void UnregisterNode(IGraphNode node, GraphNodePath path)
        {
            registeredNodes.UnregisterNode(node, path);
            node.Content.PrepareChange -= ContentPrepareChange;
            node.Content.FinalizeChange -= ContentFinalizeChange;
            node.Content.Changing -= ContentChanging;
            node.Content.Changed -= ContentChanged;
        }

        private void RegisterAllNodes()
        {
            var visitor = new GraphVisitorBase();
            visitor.Visiting += RegisterNode;
            visitor.ShouldVisit = shouldRegisterNode;
            visitor.Visit(rootNode);
        }

        private void ContentPrepareChange(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content.OwnerNode as IGraphNode;
            var path = GetPath(e.Content.OwnerNode);
            if (node != null)
            {
                var visitor = new GraphVisitorBase();
                visitor.Visiting += UnregisterNode;
                visitor.ShouldVisit = shouldRegisterNode;
                switch (e.ChangeType)
                {
                    case ContentChangeType.ValueChange:
                        // The changed node itself is still valid, we don't want to unregister it
                        visitor.SkipRootNode = true;
                        visitor.Visit(node, path);
                        break;
                    case ContentChangeType.CollectionRemove:
                        if (node.Content.IsReference && e.OldValue != null)
                        {
                            var removedNode = node.Content.Reference.AsEnumerable[e.Index].TargetNode;
                            var removedNodePath = path?.PushIndex(e.Index);
                            if (removedNode != null)
                            {
                                visitor.Visit(removedNode, removedNodePath);
                            }
                        }
                        break;
                }
            }

            PrepareChange?.Invoke(sender, new GraphContentChangeEventArgs(e, path));
        }

        private void ContentFinalizeChange(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content.OwnerNode as IGraphNode;
            var path = GetPath(e.Content.OwnerNode);
            if (node != null)
            {
                var visitor = new GraphVisitorBase();
                visitor.Visiting += RegisterNode;
                visitor.ShouldVisit = shouldRegisterNode;
                switch (e.ChangeType)
                {
                    case ContentChangeType.ValueChange:
                        // The changed node itself is still valid, we don't want to re-register it
                        visitor.SkipRootNode = true;
                        visitor.Visit(node, path);
                        break;
                    case ContentChangeType.CollectionAdd:
                        if (node.Content.IsReference && e.NewValue != null)
                        {
                            var index = e.Index;
                            IGraphNode addedNode;
                            if (!index.IsEmpty)
                            {
                                addedNode = node.Content.Reference.AsEnumerable[e.Index].TargetNode;
                            }
                            else
                            {
                                var reference = node.Content.Reference.AsEnumerable.First(x => x.TargetNode.Content.Retrieve() == e.NewValue);
                                index = reference.Index;
                                addedNode = reference.TargetNode;
                            }

                            if (addedNode != null)
                            {
                                var addedNodePath = path?.PushIndex(index);
                                visitor.Visit(addedNode, addedNodePath);
                            }
                        }
                        break;
                }
            }

            FinalizeChange?.Invoke(sender, new GraphContentChangeEventArgs(e, path));
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            var path = GetPath(e.Content.OwnerNode);
            Changing?.Invoke(sender, new GraphContentChangeEventArgs(e, path));
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            var path = GetPath(e.Content.OwnerNode);
            Changed?.Invoke(sender, new GraphContentChangeEventArgs(e, path));
        }
    }
}
