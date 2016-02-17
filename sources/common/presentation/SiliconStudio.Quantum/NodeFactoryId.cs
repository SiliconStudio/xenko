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
    /// An identifier object for node factories.
    /// </summary>
    public struct NodeFactoryId : IEquatable<NodeFactoryId>
    {
        private readonly Guid id;

        internal NodeFactoryId(Guid id)
        {
            this.id = id;
        }

        /// <inheritdoc/>
        public bool Equals(NodeFactoryId other)
        {
            return id.Equals(other.id);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => id.GetHashCode();
    }
}