using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An object representing a single accessor of the value of a node, or one if its item.
    /// </summary>
    public struct NodeAccessor
    {
        /// <summary>
        /// The node of the accessor.
        /// </summary>
        public readonly IGraphNode Node;
        /// <summary>
        /// The index of the accessor.
        /// </summary>
        public readonly Index Index;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeAccessor"/> structure.
        /// </summary>
        /// <param name="node">The target node of this accessor.</param>
        /// <param name="index">The index of the target item if this accessor target an item. <see cref="Quantum.Index.Empty"/> otherwise.</param>
        public NodeAccessor([NotNull] IGraphNode node, Index index)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            Node = node;
            Index = index;
        }

        /// <summary>
        /// Retrieves the value backed by this accessor.
        /// </summary>
        /// <returns>The value backed by this accessor.</returns>
        public object RetrieveValue() => Node.Retrieve(Index);
    }
}