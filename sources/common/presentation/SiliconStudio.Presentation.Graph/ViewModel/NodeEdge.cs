// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.ObjectModel;
using GraphX;

namespace SiliconStudio.Presentation.Graph.ViewModel
{
    /// <summary>
    /// Base edge used in node-based graphs.
    /// This class must derived from EdgeBase in GraphX.
    /// </summary>
    public class NodeEdge : EdgeBase<NodeVertex>
    {
        /// <summary>
        /// Default constructor. We need to set at least Source and Target properties of the edge.
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="target">Target vertex data</param>
        /// <param name="weight">Optional edge weight</param>
        public NodeEdge(NodeVertex source, NodeVertex target, double weight = 1)
            : base(source, target, weight)
        {
            // nothing
        }

        public object SourceSlot { get; set; }

        public object TargetSlot { get; set; }
    }
}
