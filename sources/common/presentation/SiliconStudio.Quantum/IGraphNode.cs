using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The <see cref="IGraphNode"/> interface represents a node in a Quantum object graph. This node can represent an object or a member of an object.
    /// </summary>
    public interface IGraphNode
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Guid"/>.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets the command collection.
        /// </summary>
        IReadOnlyCollection<INodeCommand> Commands { get; }

        /// <summary>
        /// Gets the expected type of for the content of this node.
        /// </summary>
        /// <remarks>The actual type of the content can be different, for example it could be a type inheriting from this type.</remarks>
        Type Type { get; }

        /// <summary>
        /// Gets whether this content hold a primitive type value. If so, the node owning this content should have no children and modifying its value should not trigger any node refresh.
        /// </summary>
        /// <remarks>Types registered as primitive types in the <see cref="INodeBuilder"/> used to build this content are taken in account by this property.</remarks>
        bool IsPrimitive { get; }

        /// <summary>
        /// Gets or sets the type descriptor of this content
        /// </summary>
        ITypeDescriptor Descriptor { get; }

        /// <summary>
        /// Gets wheither this content holds a reference or is a direct value.
        /// </summary>
        bool IsReference { get; }

        /// <summary>
        /// Retrieves the value of this content.
        /// </summary>
        object Retrieve();

        /// <summary>
        /// Retrieves the value of one of the item if this content, if it holds a collection.
        /// </summary>
        /// <param name="index">The index to use to retrieve the value.</param>
        object Retrieve(Index index);
    }
}
