using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetGraphVisitorBase : GraphVisitorBase
    {
        private readonly AssetPropertyGraph propertyGraph;

        public AssetGraphVisitorBase(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        protected override bool ShouldVisitMemberTarget(IMemberNode member)
        {
            return !propertyGraph.IsObjectReference(member, Index.Empty, member.Retrieve()) && base.ShouldVisitMemberTarget(member);
        }

        protected override bool ShouldVisitTargetItem(IObjectNode collectionNode, Index index)
        {
            return !propertyGraph.IsObjectReference(collectionNode, index, collectionNode.Retrieve(index)) && base.ShouldVisitTargetItem(collectionNode, index);
        }
    }
}
