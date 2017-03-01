using SiliconStudio.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    /// <summary>
    /// A static class allowing to identify and use links between nodes contained in a part of an <see cref="AssetComposite"/> and the root node of the part itself.
    /// </summary>
    public static class NodesToOwnerPartVisitor
    {
        /// <summary>
        /// The identifier of the link in each node.
        /// </summary>
        public const string OwnerPartContentName = "OwnerPart";
    }

    /// <summary>
    /// A node visitor that will link nodes of a part of an <see cref="AssetComposite"/> to the root node of the part itself.
    /// </summary>
    /// <typeparam name="TAssetPartDesign">The type of the design-time object containing the part.</typeparam>
    /// <typeparam name="TAssetPart">The type of the part.</typeparam>
    public class NodesToOwnerPartVisitor<TAssetPartDesign, TAssetPart> : AssetCompositeHierarchyPartVisitor<TAssetPartDesign, TAssetPart>
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
        where TAssetPart : class, IIdentifiable
    {
        private readonly IGraphNode partDesignNode;

        public NodesToOwnerPartVisitor(AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> propertyGraph, INodeContainer nodeContainer, TAssetPartDesign partDesign)
            : base(propertyGraph)
        {
            partDesignNode = nodeContainer.GetOrCreateNode(partDesign);
        }

        protected override void VisitNode(IGraphNode node)
        {
            var assetNode = node as IAssetNode;
            assetNode?.SetContent(NodesToOwnerPartVisitor.OwnerPartContentName, partDesignNode);

            base.VisitNode(node);
        }
    }
}
