// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An object that tracks the changes in the content of <see cref="IGraphNode"/> referenced by a given root node.
    /// A <see cref="GraphNodeChangeListener"/> will raise events on changes on any node that is either a child, or the
    /// target of a reference from the root node, recursively.
    /// </summary>
    public class GraphNodeChangeListener : IDisposable
    {
        private readonly IGraphNode rootNode;
        private readonly Dictionary<IGraphNode, GraphNodePath> registeredNodes = new Dictionary<IGraphNode, GraphNodePath>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeChangeListener"/> class.
        /// </summary>
        /// <param name="rootNode">The root node for which to track referenced node changes.</param>
        public GraphNodeChangeListener(IGraphNode rootNode)
        {
            this.rootNode = rootNode;
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
            foreach (var node in rootNode.GetAllChildNodes())
            {
                UnregisterNode(node.Item1);
            }
        }

        public GraphNodePath GetPath(IContentNode node)
        {
            GraphNodePath path;
            var graphNode = node as IGraphNode;
            if (graphNode == null)
                return null;

            registeredNodes.TryGetValue(graphNode, out path);
            return path;
        }

        protected virtual void RegisterNode(IGraphNode node, GraphNodePath path)
        {
            if (registeredNodes.ContainsKey(node))
                throw new InvalidOperationException("Node already registered");

            registeredNodes.Add(node, path);

            node.Content.PrepareChange += ContentPrepareChange;
            node.Content.FinalizeChange += ContentFinalizeChange;
            node.Content.Changing += ContentChanging;
            node.Content.Changed += ContentChanged;
        }

        protected virtual void UnregisterNode(IGraphNode node)
        {
            if (!registeredNodes.ContainsKey(node))
                throw new InvalidOperationException("Node not registered");

            registeredNodes.Remove(node);
            node.Content.PrepareChange -= ContentPrepareChange;
            node.Content.FinalizeChange -= ContentFinalizeChange;
            node.Content.Changing -= ContentChanging;
            node.Content.Changed -= ContentChanged;
        }

        private void RegisterAllNodes()
        {
            foreach (var node in rootNode.GetAllChildNodes(new GraphNodePath(rootNode)))
            {
                RegisterNode(node.Item1, node.Item2);
            }
        }

        private void ContentPrepareChange(object sender, ContentChangeEventArgs e)
        {
            var node = e.Content.OwnerNode as IGraphNode;
            var path = GetPath(e.Content.OwnerNode);
            if (node != null)
            {
                foreach (var child in node.GetAllChildNodes())
                {
                    UnregisterNode(child.Item1);
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
                foreach (var child in node.GetAllChildNodes(path))
                {
                    RegisterNode(child.Item1, child.Item2);
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