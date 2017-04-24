// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections;
using System.Reflection;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Processor for a single node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="nodeProcessorContext">The node processor context.</param>
    /// <returns>The node transformed</returns>
    public delegate Node NodeProcessor(Node node, ref NodeProcessorContext nodeProcessorContext);

    /// <summary>
    /// Processor for a list of node.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="nodeProcessorContext">The node processor context.</param>
    public delegate void NodeListProcessor(IList list, ref NodeProcessorContext nodeProcessorContext);

    /// <summary>
    /// Node explorer.
    /// </summary>
    public struct NodeProcessorContext
    {
        /// <summary>
        /// Gets or sets the node processor.
        /// </summary>
        public NodeProcessor NodeProcessor;

        /// <summary>
        /// Gets or sets the list processor.
        /// </summary>
        public NodeListProcessor ListProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeProcessorContext"/> class.
        /// </summary>
        /// <param name="nodeProcessor">The node processor.</param>
        /// <param name="listProcessor">The list processor.</param>
        public NodeProcessorContext(NodeProcessor nodeProcessor, NodeListProcessor listProcessor)
        {
            NodeProcessor = nodeProcessor;
            ListProcessor = listProcessor;
        }
    }
}
