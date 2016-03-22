// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public interface INodeContainer
    {
        /// <summary>
        /// Gets or set the visitor to use to create nodes. Default value is a <see cref="DefaultNodeBuilder"/> constructed with default parameters.
        /// </summary>
        INodeBuilder NodeBuilder { get; set; }

        /// <summary>
        /// Overrides the default factory to use to create <see cref="IGraphNode"/> instances.
        /// </summary>
        /// <param name="nodeFactory">The new factory to use to create <see cref="IGraphNode"/> instances.</param>
        /// <seealso cref="RestoreDefaultFactory"/>
        void OverrideDefaultFactory(NodeFactoryDelegate nodeFactory);

        /// <summary>
        /// Clears any override made to the default factory with <see cref="OverrideDefaultFactory"/>.
        /// This method will restore the default factory of this class.
        /// </summary>
        /// <seealso cref="OverrideDefaultFactory"/>
        void RestoreDefaultFactory();

        /// <summary>
        /// Registers a custom factory to create <see cref="IGraphNode"/> instances. This method returns an
        /// identifier that can be passed to the <see cref="GetOrCreateNode"/> method to create custom nodes when needed.
        /// </summary>
        /// <param name="nodeFactory">The factory to register.</param>
        /// <returns>An identifier to the registered factory.</returns>
        /// <seealso cref="UnregisterFactory"/>
        NodeFactoryId RegisterFactory(NodeFactoryDelegate nodeFactory);

        /// <summary>
        /// Unregisters a previously registered factory to create <see cref="IGraphNode"/> instances.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory to unregister.</param>
        /// <seealso cref="RegisterFactory"/>
        void UnregisterFactory(NodeFactoryId factoryId);

        /// <summary>
        /// Gets the node associated to a data object, if it exists, otherwise creates a new node for the object and its member recursively.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <param name="nodeFactoryId">An identifier to the node factory to use to create nodes.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object.</returns>
        IGraphNode GetOrCreateNode(object rootObject, NodeFactoryId nodeFactoryId = default(NodeFactoryId));

        /// <summary>
        /// Gets the <see cref="IGraphNode"/> associated to a data object, if it exists.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object if available, or <c>null</c> otherwise.</returns>
        /// <remarks>Calling this method will update references of the returned node and its children, recursively.</remarks>
        IGraphNode GetNode(object rootObject);
    }

    /// <summary>
    /// A container used to store nodes and resolve references between them.
    /// </summary>
    public class NodeContainer : INodeContainer
    {
        private readonly Dictionary<Guid, IGraphNode> nodesByGuid = new Dictionary<Guid, IGraphNode>();
        private readonly Dictionary<IGraphNode, NodeFactoryDelegate> factoriesByNode = new Dictionary<IGraphNode, NodeFactoryDelegate>();
        private readonly Dictionary<NodeFactoryId, NodeFactoryDelegate> nodeFactories = new Dictionary<NodeFactoryId, NodeFactoryDelegate>();
        private readonly IGuidContainer guidContainer;
        private readonly object lockObject = new object();
        private readonly ThreadLocal<HashSet<IGraphNode>> processedNodes = new ThreadLocal<HashSet<IGraphNode>>();
        private NodeFactoryDelegate defaultNodeFactory;

        /// <summary>
        /// Creates a new instance of <see cref="NodeContainer"/> class.
        /// </summary>
        public NodeContainer()
            : this(new GuidContainer())
        {
            NodeBuilder = CreateDefaultNodeBuilder();
            OverrideDefaultFactory(DefaultNodeFactory);
        }

        /// <summary>
        /// Creates a new instance of <see cref="NodeContainer"/> class. This constructor allows to provide a custom implementation
        /// of <see cref="IGuidContainer"/> in order to share <see cref="Guid"/> of objects.
        /// </summary>
        /// <param name="guidContainer">A <see cref="IGuidContainer"/> to use to ensure the unicity of guid associated to data objects. Cannot be <c>null</c></param>
        public NodeContainer(IGuidContainer guidContainer)
        {
            if (guidContainer == null) throw new ArgumentNullException(nameof(guidContainer));
            this.guidContainer = guidContainer;
            NodeBuilder = CreateDefaultNodeBuilder();
            OverrideDefaultFactory(DefaultNodeFactory);
        }

        /// <inheritdoc/>
        public INodeBuilder NodeBuilder { get; set; }

        /// <summary>
        /// Gets an enumerable of the registered nodes.
        /// </summary>
        public IEnumerable<IGraphNode> Nodes => nodesByGuid.Values;

        /// <summary>
        /// Gets an enumerable of the registered node guids.
        /// </summary>
        public IEnumerable<Guid> Guids => nodesByGuid.Keys;

        /// <inheritdoc/>
        public void OverrideDefaultFactory(NodeFactoryDelegate nodeFactory)
        {
            lock (lockObject)
            {
                nodeFactories[new NodeFactoryId(Guid.Empty)] = nodeFactory;
                defaultNodeFactory = nodeFactory;
            }
        }

        /// <inheritdoc/>
        public void RestoreDefaultFactory()
        {
            lock (lockObject)
            {
                OverrideDefaultFactory(DefaultNodeFactory);
            }
        }

        /// <inheritdoc/>
        public NodeFactoryId RegisterFactory(NodeFactoryDelegate nodeFactory)
        {
            lock (lockObject)
            {
                var id = new NodeFactoryId(Guid.NewGuid());
                nodeFactories.Add(id, nodeFactory);
                return id;
            }
        }

        /// <inheritdoc/>
        public void UnregisterFactory(NodeFactoryId factoryId)
        {
            lock (lockObject)
            {
                nodeFactories.Remove(factoryId);
            }
        }

        /// <inheritdoc/>
        public IGraphNode GetOrCreateNode(object rootObject, NodeFactoryId nodeFactoryId = default(NodeFactoryId))
        {
            if (rootObject == null)
                return null;

            lock (lockObject)
            {
                if (!processedNodes.IsValueCreated)
                    processedNodes.Value = new HashSet<IGraphNode>();

                NodeFactoryDelegate nodeFactory;
                nodeFactories.TryGetValue(nodeFactoryId, out nodeFactory);
                if (nodeFactory == null)
                    nodeFactory = defaultNodeFactory;
                var node = GetOrCreateNodeInternal(rootObject, nodeFactory);

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
        public void UpdateReferences(IGraphNode node)
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
                guidContainer?.Clear();
                nodesByGuid.Clear();
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
                if (guidContainer == null) throw new InvalidOperationException("This NodeContainer has no GuidContainer and can't retrieve Guid associated to a data object.");
                var guid = guidContainer.GetGuid(rootObject);
                if (guid == Guid.Empty)
                    return null;

                IGraphNode node;
                if (nodesByGuid.TryGetValue(guid, out node))
                {
                    if (node != null && !processedNodes.Value.Contains(node))
                    {
                        UpdateReferencesInternal(node);
                    }
                }
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

                var guid = !rootObject.GetType().IsValueType ? guidContainer.GetOrCreateGuid(rootObject) : Guid.NewGuid();
                result = NodeBuilder.Build(rootObject, guid, nodeFactory);

                if (result != null)
                {
                    // Register the factory used to create this node.
                    factoriesByNode.Add(result, nodeFactory);
                    // Register reference objects
                    nodesByGuid.Add(result.Guid, result);
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
            lock (lockObject)
            {
                if (processedNodes.Value.Contains(node))
                    return;

                processedNodes.Value.Add(node);

                NodeFactoryDelegate nodeFactory;
                factoriesByNode.TryGetValue(node, out nodeFactory);
                if (nodeFactory == null)
                    nodeFactory = defaultNodeFactory;

                // If the node was holding a reference, refresh the reference
                if (node.Content.IsReference)
                {
                    node.Content.Reference.Refresh(node.Content.Value);
                    UpdateOrCreateReferenceTarget(node.Content.Reference, node, nodeFactory);
                }
                else
                {
                    // Otherwise refresh potential references in its children.
                    foreach (var child in node.Children.SelectDeep(x => x.Children).Where(x => x.Content.IsReference))
                    {
                        child.Content.Reference.Refresh(child.Content.Value);
                        UpdateOrCreateReferenceTarget(child.Content.Reference, child, nodeFactory);
                    }
                }
            }
        }

        private void UpdateOrCreateReferenceTarget(IReference reference, IGraphNode node, NodeFactoryDelegate nodeFactory, Stack<object> indices = null)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (node == null) throw new ArgumentNullException(nameof(node));

            var content = (ContentBase)node.Content;

            var referenceEnumerable = reference as ReferenceEnumerable;
            var singleReference = reference as ObjectReference;
            if (referenceEnumerable != null)
            {
                if (indices == null)
                    indices = new Stack<object>();

                foreach (var itemReference in referenceEnumerable)
                {
                    indices.Push(itemReference.Index);
                    UpdateOrCreateReferenceTarget(itemReference, node, nodeFactory, indices);
                    indices.Pop();
                }
            }
            else if (singleReference != null && content.ShouldProcessReference)
            {
                if (singleReference.TargetNode != null && singleReference.TargetNode.Content.Value != reference.ObjectValue)
                {
                    singleReference.Clear();
                }

                if (singleReference.TargetNode == null && reference.ObjectValue != null)
                {
                    // This call will recursively update the references.
                    var target = singleReference.SetTarget(this, nodeFactory);
                    if (target != null)
                    {
                        var structContent = target.Content as BoxedContent;
                        if (structContent != null)
                        {
                            structContent.BoxedStructureOwner = content;
                            structContent.BoxedStructureOwnerIndices = indices?.Reverse().ToArray();
                        }
                    }
                    else
                    {
                        content.ShouldProcessReference = false;
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
