// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// This interface provides objects and methods to build a nodal view model from a given object.
    /// </summary>
    public interface INodeBuilder
    {
        /// <summary>
        /// Gets the collection of structure types that represents custom primitive types. Primitive structures won't have node created for each of their members.
        /// </summary>
        /// <remarks>Default .NET primitive types, string and enum are always considered to be primitive type.</remarks>
        ICollection<Type> PrimitiveTypes { get; }

        /// <summary>
        /// Gets the collection of available commands to attach to nodes.
        /// </summary>
        ICollection<INodeCommand> AvailableCommands { get; }

        /// <summary>
        /// Gets the enumerable of content that have references created during the last execution of the builder visitor.
        /// </summary>
        IEnumerable<IContent> ReferenceContents { get; }

        /// <summary>
        /// Raised when a node is about to be constructed. The construction can be cancelled by setting <see cref="NodeConstructingArgs.Discard"/> to <c>true</c>.
        /// </summary>
        event EventHandler<NodeConstructingArgs> NodeConstructing;

        /// <summary>
        /// Raised when a node has been constructed. Allows to attach associated data to the node via the <see cref="NodeConstructedArgs.AssociatedData"/> dictionary.
        /// </summary>
        event EventHandler<NodeConstructedArgs> NodeConstructed;

        /// <summary>
        /// Build the node hierarchy corresponding to the given object and fill the <see cref="ReferenceContents"/> list with references created during this process.
        /// </summary>
        /// <param name="obj">The object. Can be <c>null</c>.</param>
        /// <param name="type">The type of the object</param>
        /// <param name="rootGuid">The <see cref="Guid"/> To assign to the root node.</param>
        /// <returns>The root node of the node hierarchy corresponding to the given object.</returns>
        IModelNode Build(object obj, Type type, Guid rootGuid);
    }
}
