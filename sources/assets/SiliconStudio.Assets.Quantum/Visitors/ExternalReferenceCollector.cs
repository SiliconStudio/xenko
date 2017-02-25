using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that will collect all object references that target objects that are not included in the visited object.
    /// </summary>
    public class ExternalReferenceCollector : IdentifiableObjectVisitorBase
    {
        private readonly AssetPropertyGraph propertyGraph;

        private readonly HashSet<IIdentifiable> internalReferences = new HashSet<IIdentifiable>();
        private readonly HashSet<IIdentifiable> externalReferences = new HashSet<IIdentifiable>();

        private ExternalReferenceCollector(AssetPropertyGraph propertyGraph)
            : base(propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        /// <summary>
        /// Computes the external references to the given root node.
        /// </summary>
        /// <param name="propertyGraph">The property graph to use to anal</param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static HashSet<IIdentifiable> GetExternalReferences(AssetPropertyGraph propertyGraph, IGraphNode root)
        {
            var visitor = new ExternalReferenceCollector(propertyGraph);
            visitor.Visit(root);
            // An IIdentifiable can have been recorded both as internal and external reference. In this case we still want to clone it so let's remove it from external references
            visitor.externalReferences.ExceptWith(visitor.internalReferences);
            return visitor.externalReferences;
        }

        protected override void ProcessIdentifiable(IIdentifiable identifiable, IGraphNode node, Index index)
        {
            if (propertyGraph.IsObjectReference(node, index, node.Retrieve(index)))
                externalReferences.Add(identifiable);
            else
                internalReferences.Add(identifiable);
        }
    }
}
