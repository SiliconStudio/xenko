// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A container used to store nodes and resolve references between them.
    /// </summary>
    public class NodeContainer : INodeContainer
    {
        private readonly object lockObject = new object();
        private readonly ThreadLocal<HashSet<IGraphNode>> processedNodes = new ThreadLocal<HashSet<IGraphNode>>();
        private ConditionalWeakTable<object, IGraphNode> nodesByObject = new ConditionalWeakTable<object, IGraphNode>();
        private NodeFactoryDelegate defaultNodeFactory = DefaultNodeFactory;

        /// <summary>
        /// Creates a new instance of <see cref="NodeContainer"/> class.
        /// </summary>
        public NodeContainer()
        {
            NodeBuilder = CreateDefaultNodeBuilder();
        }

        /// <inheritdoc/>
        public INodeBuilder NodeBuilder { get; set; }

        /// <inheritdoc/>
        public void OverrideNodeFactory(NodeFactoryDelegate nodeFactory)
        {
            lock (lockObject)
            {
                defaultNodeFactory = nodeFactory;
            }
        }

        /// <inheritdoc/>
        public void RestoreDefaultNodeFactory()
        {
            lock (lockObject)
            {
                OverrideNodeFactory(DefaultNodeFactory);
            }
        }

        /// <inheritdoc/>
        public IGraphNode GetOrCreateNode(object rootObject)
        {
            if (rootObject == null)
                return null;

            lock (lockObject)
            {
                if (!processedNodes.IsValueCreated)
                    processedNodes.Value = new HashSet<IGraphNode>();

                var node = GetOrCreateNodeInternal(rootObject, defaultNodeFactory);

                processedNodes.Value.Clear();
                return node;
            }
        }

        /// <inheritdoc/>
        public IGraphNode GetNode(object rootObject)
        {
            lock (lockObject)
            {
                if (!processedNodes.IsValueCreated)
                    processedNodes.Value = new HashSet<IGraphNode>();

                var node = GetNodeInternal(rootObject);

                processedNodes.Value.Clear();
                return node;
            }
        }

        /// <summary>
        /// Refresh all references contained in the given node, creating new nodes for newly referenced objects.
        /// </summary>
        /// <param name="node">The node to update</param>
        internal void UpdateReferences(IGraphNode node)
        {
            lock (lockObject)
            {
                if (!processedNodes.IsValueCreated)
                    processedNodes.Value = new HashSet<IGraphNode>();

                UpdateReferencesInternal(node);

                processedNodes.Value.Clear();
            }
        }

        /// <summary>
        /// Removes all nodes that were previously registered.
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                nodesByObject = new ConditionalWeakTable<object, IGraphNode>();
            }
        }

        /// <summary>
        /// Gets the <see cref="IGraphNode"/> associated to a data object, if it exists. If the NodeContainer has been constructed without <see cref="IGuidContainer"/>, this method will throw an exception.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object if available, or <c>null</c> otherwise.</returns>
        internal IGraphNode GetNodeInternal(object rootObject)
        {
            lock (lockObject)
            {
                if (rootObject == null)
                    return null;

                IGraphNode node;
                nodesByObject.TryGetValue(rootObject, out node);
                return node;
            }
        }

        /// <summary>
        /// Gets the node associated to a data object, if it exists, otherwise creates a new node for the object and its member recursively.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <param name="nodeFactory">The factory to use to create nodes.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object.</returns>
        internal IGraphNode GetOrCreateNodeInternal(object rootObject, NodeFactoryDelegate nodeFactory)
        {
            if (nodeFactory == null) throw new ArgumentNullException(nameof(nodeFactory));

            if (rootObject == null)
                return null;

            lock (lockObject)
            {
                IGraphNode result;
                if (!rootObject.GetType().IsValueType)
                {
                    result = GetNodeInternal(rootObject);
                    if (result != null)
                        return result;
                }

                result = NodeBuilder.Build(rootObject, Guid.NewGuid(), nodeFactory);

                if (result != null)
                {
                    // Register reference objects
                    nodesByObject.Add(rootObject, result);
                    // Create or update nodes of referenced objects
                    UpdateReferencesInternal(result);
                }
                return result;
            }
        }

        /// <summary>
        /// Refresh all references contained in the given node, creating new nodes for newly referenced objects.
        /// </summary>
        /// <param name="node">The node to update</param>
        private void UpdateReferencesInternal(IGraphNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            lock (lockObject)
            {
                if (processedNodes.Value.Contains(node))
                    return;

                processedNodes.Value.Add(node);

                // If the node was holding a reference, refresh the reference
                if (node.Content.IsReference)
                {
                    node.Content.Reference.Refresh(node, this, defaultNodeFactory);
                }
                else
                {
                    // Otherwise refresh potential references in its children.
                    foreach (var child in node.Children)
                    {
                        UpdateReferencesInternal(child);
                    }
                }
            }
        }

        private INodeBuilder CreateDefaultNodeBuilder()
        {
            var nodeBuilder = new DefaultNodeBuilder(this);
            return nodeBuilder;
        }

        private static IGraphNode DefaultNodeFactory(string name, IContent content, Guid guid)
        {
            return new GraphNode(name, content, guid);
        }
    }
}
