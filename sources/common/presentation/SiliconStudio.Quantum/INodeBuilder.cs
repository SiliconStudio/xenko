// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A delegate representing a factory used to create a graph node from a content and its related information.
    /// </summary>
    /// <param name="name">The name of the node to create.</param>
    /// <param name="content">The content for which to create a node.</param>
    /// <param name="guid">The unique identifier of the node to create.</param>
    /// <returns>A new instance of <see cref="IGraphNode"/> containing the given content.</returns>
    public delegate IGraphNode NodeFactoryDelegate(string name, IContent content, Guid guid);

    /// <summary>
    /// This interface provides objects and methods to build a nodal view model from a given object.
    /// </summary>
    public interface INodeBuilder
    {
        /// <summary>
        /// Gets the instance of <see cref="NodeContainer"/> associated to this node builder.
        /// </summary>
        NodeContainer NodeContainer { get; }

        /// <summary>
        /// Gets the collection of structure types that represents custom primitive types. Primitive structures won't have node created for each of their members.
        /// </summary>
        /// <remarks>Default .NET primitive types, string and enum are always considered to be primitive type.</remarks>
        ICollection<Type> PrimitiveTypes { get; }

        /// <summary>
        /// Gets the type descriptor factory.
        /// </summary>
        /// <value>The type descriptor factory.</value>
        ITypeDescriptorFactory TypeDescriptorFactory { get; }

        /// <summary>
        /// Gets or sets the factory that will create instances of <see cref="IContent"/> for nodes.
        /// </summary>
        IContentFactory ContentFactory { get; set; }

        /// <summary>
        /// Gets the collection of available commands to attach to nodes.
        /// </summary>
        ICollection<INodeCommand> AvailableCommands { get; }

        /// <summary>
        /// Raised when a node is about to be constructed. The construction can be cancelled by setting <see cref="NodeConstructingArgs.Discard"/> to <c>true</c>.
        /// </summary>
        event EventHandler<NodeConstructingArgs> NodeConstructing;

        /// <summary>
        /// Raised when a node has been constructed.
        /// </summary>
        event EventHandler<NodeConstructedArgs> NodeConstructed;

        /// <summary>
        /// Build the node hierarchy corresponding to the given object.
        /// </summary>
        /// <param name="obj">The object. Can be <c>null</c>.</param>
        /// <param name="rootGuid">The <see cref="Guid"/> To assign to the root node.</param>
        /// <param name="nodeFactory">The factory that creates node for each content.</param>
        /// <returns>The root node of the node hierarchy corresponding to the given object.</returns>
        IGraphNode Build(object obj, Guid rootGuid, NodeFactoryDelegate nodeFactory);

        /// <summary>
        /// Creates a reference for the specified type/value node.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IReference CreateReferenceForNode(Type type, object value);
    }
}
