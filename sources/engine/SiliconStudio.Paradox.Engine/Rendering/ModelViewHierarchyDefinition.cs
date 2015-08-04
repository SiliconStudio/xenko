// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Describes hiderarchical nodes in a flattened array.
    /// </summary>
    /// <remarks>
    /// Nodes are ordered so that parents always come first, allowing for hierarchical updates in a simple loop.
    /// </remarks>
    [DataContract]
    public class ModelViewHierarchyDefinition
    {
        /// <summary>
        /// The nodes in this hierarchy.
        /// </summary>
        public ModelNodeDefinition[] Nodes;
    }
}