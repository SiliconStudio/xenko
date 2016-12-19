using System;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum
{
    /// <summary>
    /// A <see cref="GraphNodeLinker"/> that can link nodes of an asset to the corresponding nodes in their base.
    /// </summary>
    /// <remarks>This method will invoke <see cref="AssetPropertyGraph.FindTarget(IGraphNode, IGraphNode)"/> when linking, to allow custom links for cases such as <see cref="AssetComposite"/>.</remarks>
    public class AssetToBaseNodeLinker : GraphNodeLinker
    {
        private readonly AssetPropertyGraph propertyGraph;

        public AssetToBaseNodeLinker(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        public Func<MemberContent, IGraphNode, bool> ShouldVisit { get; set; }

        protected override bool ShouldVisitSourceNode(MemberContent memberContent, IGraphNode targetNode)
        {
            return (ShouldVisit?.Invoke(memberContent, targetNode) ?? true) && base.ShouldVisitSourceNode(memberContent, targetNode);
        }

        protected override IGraphNode FindTarget(IGraphNode sourceNode)
        {
            var defaultTarget = base.FindTarget(sourceNode);
            return propertyGraph.FindTarget(sourceNode, defaultTarget);
        }

        protected override ObjectReference FindTargetReference(IGraphNode sourceNode, IGraphNode targetNode, ObjectReference sourceReference)
        {
            if (sourceReference.Index.IsEmpty)
                return targetNode.Content.Reference as ObjectReference;

            // Special case for objects that are identifiable: the object must be linked to the base only if it has the same id
            if (sourceReference.ObjectValue != null)
            {
                if (sourceReference.Index.IsEmpty)
                {
                    return targetNode.Content.Reference.AsObject;
                }

                var sourceAssetNode = (AssetNode)sourceNode;
                if (sourceAssetNode.IsNonIdentifiableCollectionContent)
                    return null;

                // Enumerable reference: we look for an object with the same id
                var targetReference = targetNode.Content.Reference.AsEnumerable;
                var sourceIds = CollectionItemIdHelper.GetCollectionItemIds(sourceNode.Content.Retrieve());
                var targetIds = CollectionItemIdHelper.GetCollectionItemIds(targetNode.Content.Retrieve());
                var itemId = sourceIds[sourceReference.Index.Value];
                var targetKey = targetIds.GetKey(itemId);
                return targetReference.FirstOrDefault(x => Equals(x.Index.Value, targetKey));
            }

            // Not identifiable - default applies
            return base.FindTargetReference(sourceNode, targetNode, sourceReference);
        }
    }
}
