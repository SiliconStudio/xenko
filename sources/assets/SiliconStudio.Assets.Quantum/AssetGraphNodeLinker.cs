using System;
using System.Collections.Generic;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetGraphNodeLinker : GraphNodeLinker
    {
        private readonly AssetPropertyGraph propertyGraph;

        public AssetGraphNodeLinker(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        protected override bool ShouldVisitMemberTarget(IMemberNode member)
        {
            return propertyGraph.IsObjectReference(member, Index.Empty, member.Retrieve()) && base.ShouldVisitMemberTarget(member);
        }

        protected override bool ShouldVisitTargetItem(IObjectNode collectionNode, Index index)
        {
            return propertyGraph.IsObjectReference(collectionNode, index, collectionNode.Retrieve(index)) && base.ShouldVisitTargetItem(collectionNode, index);
        }
    }
}