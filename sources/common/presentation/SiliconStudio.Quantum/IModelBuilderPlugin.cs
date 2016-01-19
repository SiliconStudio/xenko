// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Provides a way to customize <see cref="DefaultContentFactory" />.
    /// </summary>
    public interface IModelBuilderPlugin
    {
        /// <summary>
        /// Processes the specified <see cref="GraphNode"/>.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="graphNode">The model node.</param>
        void Process(INodeBuilder nodeBuilder, GraphNode graphNode);
    }
}