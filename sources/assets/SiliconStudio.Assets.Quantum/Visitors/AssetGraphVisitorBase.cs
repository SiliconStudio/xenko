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
        protected readonly AssetPropertyGraphDefinition PropertyGraphDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetGraphVisitorBase"/> class.
        /// </summary>
        /// <param name="propertyGraphDefinition">The <see cref="AssetPropertyGraphDefinition"/> used to analyze object references.</param>
        public AssetGraphVisitorBase(AssetPropertyGraphDefinition propertyGraphDefinition)
        {
            PropertyGraphDefinition = propertyGraphDefinition;
        }

        /// <inheritdoc/>
        protected override bool ShouldVisitMemberTarget(IMemberNode member)
        {
            return !PropertyGraphDefinition.IsMemberTargetObjectReference(member, member.Retrieve()) && base.ShouldVisitMemberTarget(member);
        }

        /// <inheritdoc/>
        protected override bool ShouldVisitTargetItem(IObjectNode collectionNode, Index index)
        {
            return !PropertyGraphDefinition.IsTargetItemObjectReference(collectionNode, index, collectionNode.Retrieve(index)) && base.ShouldVisitTargetItem(collectionNode, index);
        }
    }
}
