using System;
using System.Collections.Generic;

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

        public NodeAccessor(IGraphNode node, Index index)
        {
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