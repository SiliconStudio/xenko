// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Assets.Quantum.Visitors;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    /// <summary>
    /// A node visitor that will link nodes of a part of an <see cref="AssetComposite"/> to the root node of the part itself.
    /// </summary>
    public class NodesToOwnerPartVisitor : AssetGraphVisitorBase
    {
        /// <summary>
        /// The identifier of the link in each node.
        /// </summary>
        public const string OwnerPartContentName = "OwnerPart";

        private readonly IAssetObjectNode ownerPartNode;

        public NodesToOwnerPartVisitor(AssetPropertyGraphDefinition propertyGraphDefinition, INodeContainer nodeContainer, object ownerPart)
            : base(propertyGraphDefinition)
        {
            ownerPartNode = (IAssetObjectNode)nodeContainer.GetOrCreateNode(ownerPart);
        }

        protected override void VisitNode(IGraphNode node)
        {
            var assetNode = node as IAssetNode;
            assetNode?.SetContent(OwnerPartContentName, ownerPartNode);

            base.VisitNode(node);
        }
    }
}
