using System;
using SiliconStudio.Quantum.Contents;

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
    /// An interface representing a container for graph nodes.
    /// </summary>
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
        /// <seealso cref="RestoreDefaultNodeFactory"/>
        void OverrideNodeFactory(NodeFactoryDelegate nodeFactory);

        /// <summary>
        /// Clears any override made to the default factory with <see cref="OverrideNodeFactory"/>.
        /// This method will restore the default factory of this class.
        /// </summary>
        /// <seealso cref="OverrideNodeFactory"/>
        void RestoreDefaultNodeFactory();

        /// <summary>
        /// Gets the node associated to a data object, if it exists, otherwise creates a new node for the object and its member recursively.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object.</returns>
        IGraphNode GetOrCreateNode(object rootObject);

        /// <summary>
        /// Gets the <see cref="IGraphNode"/> associated to a data object, if it exists.
        /// </summary>
        /// <param name="rootObject">The data object.</param>
        /// <returns>The <see cref="IGraphNode"/> associated to the given object if available, or <c>null</c> otherwise.</returns>
        /// <remarks>Calling this method will update references of the returned node and its children, recursively.</remarks>
        IGraphNode GetNode(object rootObject);
    }
}
