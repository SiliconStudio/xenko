// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// An implementation of <see cref="GraphVisitorBase"/> that will stop visiting deeper each time it reaches a node representing an object reference.
    /// </summary>
    /// <remarks>This visitor requires a <see cref="AssetPropertyGraph"/> to analyze if a node represents an object reference.</remarks>
    public class AssetGraphVisitorBase : GraphVisitorBase
    {
        protected readonly AssetPropertyGraph PropertyGraph;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetGraphVisitorBase"/> class.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        public AssetGraphVisitorBase(AssetPropertyGraph propertyGraph)
        {
            PropertyGraph = propertyGraph;
        }

        /// <inheritdoc/>
        protected override bool ShouldVisitMemberTarget(IMemberNode member)
        {
            return !PropertyGraph.Definition.IsObjectReference(member, Index.Empty, member.Retrieve()) && base.ShouldVisitMemberTarget(member);
        }

        /// <inheritdoc/>
        protected override bool ShouldVisitTargetItem(IObjectNode collectionNode, Index index)
        {
            return !PropertyGraph.Definition.IsObjectReference(collectionNode, index, collectionNode.Retrieve(index)) && base.ShouldVisitTargetItem(collectionNode, index);
        }
    }
}
