// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A container used to store nodes and resolve references between them.
    /// </summary>
    public class NodeContainer
    {
        private readonly Dictionary<Guid, IGraphNode> nodesByGuid = new Dictionary<Guid, IGraphNode>();
        private readonly Dictionary<IGraphNode, NodeFactoryDelegate> factoriesByNode = new Dictionary<IGraphNode, NodeFactoryDelegate>();
        private readonly Dictionary<NodeFactoryId, NodeFactoryDelegate> nodeFactories = new Dictionary<NodeFactoryId, NodeFactoryDelegate>();
        private readonly IGuidContainer guidContainer;
        private readonly object lockObject = new object();
        private NodeFactoryDelegate defaultNodeFactory;

        /// <summary>
        /// Create a new instance of <see cref="NodeContainer"/>.
        /// </summary>
        /// <param name="instantiateGuidContainer">Indicate whether to create a <see cref="GuidContainer"/> to store Guid per data object. This can be useful to retrieve an existing nodes for a data object.</param>
        public NodeContainer(bool instantiateGuidContainer = true)
        {
            if (instantiateGuidContainer)
                guidContainer = new GuidContainer();
            NodeBuilder = CreateDefaultNodeBuilder();
            RegisterDefaultFactory((name, content, guid) => new GraphNode(name, content, guid));
        }

        /// <summary>
        /// Create a new instance of <see cref="NodeContainer"/>. This constructor allows to provide a <see cref="IGuidContainer"/>,
        /// in order to share object <see cref="Guid"/> between different <see cref="NodeContainer"/>.
        /// </summary>
        /// <param name="guidContainer">A <see cref="IGuidContainer"/> to use to ensure the unicity of guid associated to data objects. Cannot be <c>null</c></param>
        public NodeContainer(IGuidContainer guidContainer)
        {
            if (guidContainer == null) throw new ArgumentNullException(nameof(guidContainer));
            this.guidContainer = guidContainer;
            NodeBuilder = CreateDefaultNodeBuilder();
            RegisterDefaultFactory((name, content, guid) => new GraphNode(name, content, guid));
        }

        /// <summary>
        /// Gets an enumerable of the registered nodes.
        /// </summary>
        public IEnumerable<IGraphNode> Nodes => nodesByGuid.Values;

        /// <summary>
        /// Gets an enumerable of the registered node guids.
        /// </summary>
        public IEnumerable<Guid> Guids => nodesByGuid.Keys;

        /// <summary>
        /// Gets or set the visitor to use to create nodes. Default value is a <see cref="DefaultNodeBuilder"/> constructed with default parameters.
        /// </summary>
        public INodeBuilder NodeBuilder { get; set; }

        /// <summary>
        /// Registers the default factory to use to create <see cref="IGraphNode"/> instances.
        /// </summary>
        /// <param name="nodeFactory">The factory to register.</param>
        public void RegisterDefaultFactory(NodeFactoryDelegate nodeFactory)
        {
            lock (lockObject)
            {
                nodeFactories[new NodeFactoryId(Guid.Empty)] = nodeFactory;
                defaultNodeFactory = nodeFactory;
            }
        }

        public NodeFactoryId RegisterFactory(NodeFactoryDelegate nodeFactory)
        {
            lock (lockObject)
            {
                var id = new NodeFactoryId(Guid.NewGuid());
                nodeFactories.Add(id, nodeFactory);
                return id;
            }
        }

        /// <summary>
        /// Gets the <see cref="IGraphNode"/> associated to a data object, if it exists. If the NodeContainer has been constructed without <see cref="IGuidContainer"/>, this method will throw an exception.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object if available, or <c>null</c> otherwise.</returns>
        public IGraphNode GetNode(object rootObject)
        {
            lock (lockObject)
            {
                if (guidContainer == null) throw new InvalidOperationException("This NodeContainer has no GuidContainer and can't retrieve Guid associated to a data object.");
                var guid = guidContainer.GetGuid(rootObject);
                return guid == Guid.Empty ? null : GetNode(guid);
            }
        }

        /// <summary>
        /// Gets the <see cref="IGraphNode"/> associated to the given guid, if it exists.
        /// </summary>
        /// <param name="guid">The guid for which to retrieve a <see cref="IGraphNode"/>.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given Guid if available, or <c>null</c> otherwise.</returns>
        public IGraphNode GetNode(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            lock (lockObject)
            {
                IGraphNode result;
                if (nodesByGuid.TryGetValue(guid, out result))
                {
                    if (result != null)
                    {
                        UpdateReferences(result);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> associated to a data object, if it exists. If the NodeContainer has been constructed without <see cref="IGuidContainer"/>, this method will throw an exception.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <returns>The <see cref="Guid"/> associated to the given object if available, or <see cref="Guid.Empty"/> otherwise.</returns>
        public Guid GetGuid(object rootObject)
        {
            lock (lockObject)
            {
                if (guidContainer == null) throw new InvalidOperationException("This NodeContainer has no GuidContainer and can't retrieve Guid associated to a data object.");
                return guidContainer.GetGuid(rootObject);
            }
        }

        /// <summary>
        /// Gets the node associated to a data object, if it exists, otherwise creates a new node for the object and its member recursively.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <param name="nodeFactoryId">An identifier to the node factory to use to create nodes.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object.</returns>
        public IGraphNode GetOrCreateNode(object rootObject, NodeFactoryId nodeFactoryId = default(NodeFactoryId))
        {
            if (rootObject == null)
                return null;

            lock (lockObject)
            {
                NodeFactoryDelegate nodeFactory;
                nodeFactories.TryGetValue(nodeFactoryId, out nodeFactory);
                if (nodeFactory == null)
                    nodeFactory = defaultNodeFactory;
                return GetOrCreateNode(rootObject, nodeFactory);
            }
        }

        /// <summary>
        /// Gets the node associated to a data object, if it exists, otherwise creates a new node for the object and its member recursively.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <param name="nodeFactory">The factory to use to create nodes.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object.</returns>
        internal IGraphNode GetOrCreateNode(object rootObject, NodeFactoryDelegate nodeFactory)
        {
            if (nodeFactory == null) throw new ArgumentNullException(nameof(nodeFactory));

            if (rootObject == null)
                return null;

            lock (lockObject)
            {
                IGraphNode result = null;
                if (guidContainer != null && !rootObject.GetType().IsValueType)
                {
                    result = GetNode(rootObject);
                }

                return result ?? CreateNode(rootObject, nodeFactory);
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
        /// Refresh all references contained in the given node, creating new nodes for newly referenced objects.
        /// </summary>
        /// <param name="node">The node to update</param>
        internal void UpdateReferences(IGraphNode node)
        {
            lock (lockObject)
            {
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

        /// <summary>
        /// Creates a graph node.
        /// </summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="nodeFactory">The factory to use to create nodes.</param>
        /// <returns>A new graph node representing the given root object.</returns>
        private IGraphNode CreateNode(object rootObject, NodeFactoryDelegate nodeFactory)
        {
            if (rootObject == null) throw new ArgumentNullException(nameof(rootObject));

            Guid guid = Guid.NewGuid();

            // Retrieve results
            if (guidContainer != null && !rootObject.GetType().IsValueType)
                guid = guidContainer.GetOrCreateGuid(rootObject);

            var result = (GraphNode)NodeBuilder.Build(rootObject, guid, nodeFactory);

            if (result != null)
            {
                // Register the factory used to create this node.
                factoriesByNode.Add(result, nodeFactory);
                // Register reference objects
                nodesByGuid.Add(result.Guid, result);
                // Create or update nodes of referenced objects
                UpdateReferences(result);
            }

            return result;
        }

        private void UpdateOrCreateReferenceTarget(IReference reference, IGraphNode node, NodeFactoryDelegate nodeFactory, Stack<object> indices = null)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (node == null) throw new ArgumentNullException(nameof(node));

            var content = (ContentBase)node.Content;

            var referenceEnumerable = reference as ReferenceEnumerable;
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
            else
            {
                if (content.ShouldProcessReference)
                {
                    var singleReference = ((ObjectReference)reference);
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
        }

        private INodeBuilder CreateDefaultNodeBuilder()
        {
            var nodeBuilder = new DefaultNodeBuilder(this);
            return nodeBuilder;
        }
    }
}
