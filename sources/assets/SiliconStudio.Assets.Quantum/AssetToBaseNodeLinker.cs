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
    /// <remarks>This method will invoke <see cref="AssetPropertyNodeGraph.FindTarget(IGraphNode, IGraphNode)"/> when linking, to allow custom links for cases such as <see cref="AssetComposite"/>.</remarks>
    public class AssetToBaseNodeLinker : GraphNodeLinker
    {
        private readonly AssetPropertyNodeGraph propertyNodeGraph;

        public AssetToBaseNodeLinker(AssetPropertyNodeGraph propertyNodeGraph)
        {
            this.propertyNodeGraph = propertyNodeGraph;
        }

        public Func<MemberContent, IGraphNode, bool> ShouldVisit { get; set; }

        protected override bool ShouldVisitSourceNode(MemberContent memberContent, IGraphNode targetNode)
        {
            return (ShouldVisit?.Invoke(memberContent, targetNode) ?? true) && base.ShouldVisitSourceNode(memberContent, targetNode);
        }

        protected override IGraphNode FindTarget(IGraphNode sourceNode)
        {
            var defaultTarget = base.FindTarget(sourceNode);
            return propertyNodeGraph.FindTarget(sourceNode, defaultTarget);
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

                // Enumerable reference: we look for an object with the same id
                var targetReference = targetNode.Content.Reference.AsEnumerable;
                var sourceIds = CollectionItemIdHelper.GetCollectionItemIds(sourceNode.Content.Retrieve());
                var targetIds = CollectionItemIdHelper.GetCollectionItemIds(targetNode.Content.Retrieve());
                var itemId = sourceIds.GetId(sourceReference.Index.Value);
                var targetKey = targetIds.GetKey(itemId);
                return targetReference.FirstOrDefault(x => Equals(x.Index.Value, targetKey));
            }

            // Not identifiable - default applies
            return base.FindTargetReference(sourceNode, targetNode, sourceReference);
        }

        protected ObjectReference FindTargetReference2(IGraphNode sourceNode, IGraphNode targetNode, ObjectReference sourceReference)
        {
            if (sourceReference.Index.IsEmpty)
                return targetNode.Content.Reference as ObjectReference;

            // Special case for objects that are identifiable: the object must be linked to the base only if it has the same id
            if (sourceReference.ObjectValue != null && IdentifiableHelper.IsIdentifiable(sourceReference.ObjectValue.GetType()))
            {
                var sourceId = IdentifiableHelper.GetId(sourceReference.ObjectValue);
                if (sourceReference.Index.IsEmpty)
                {
                    // Object reference: we check if the object reference of the target has the same id.
                    var targetReference = targetNode.Content.Reference.AsObject;
                    if (targetReference?.ObjectValue != null && IdentifiableHelper.GetId(targetReference.ObjectValue) == sourceId)
                        return targetReference;
                }
                else
                {
                    // Enumerable reference: we look for an object with the same id
                    var targetReference = targetNode.Content.Reference.AsEnumerable;
                    return targetReference.FirstOrDefault(x => x?.ObjectValue != null && IdentifiableHelper.GetId(x.ObjectValue) == sourceId);
                }
                return null;
            }

            // Not identifiable - default applies
            return base.FindTargetReference(sourceNode, targetNode, sourceReference);
        }
    }
}
