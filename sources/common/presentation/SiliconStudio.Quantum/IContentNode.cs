using System;
using System.Collections.Generic;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The <see cref="IContentNode"/> interface represents a node in a Quantum object graph. This node can represent an object or a member of an object.
    /// The value behind the node can be accessed and modified with the <see cref="Content"/> property.
    /// </summary>
    public interface IContentNode
    {
        /// <summary>
        /// Gets or sets the node name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the <see cref="System.Guid"/>.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets the content of the <see cref="IContentNode"/>.
        /// </summary>
        IContent Content { get; }

        /// <summary>
        /// Gets the command collection.
        /// </summary>
        IReadOnlyCollection<INodeCommand> Commands { get; }
    }
}