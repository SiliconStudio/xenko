using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that collects all <see cref="IIdentifiable"/> objects that are visited through nodes that are not representing object references.
    /// </summary>
    public class IdentifiableObjectCollector : AssetGraphVisitorBase
    {
        private readonly Dictionary<Guid, IIdentifiable> result = new Dictionary<Guid, IIdentifiable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifiableObjectCollector"/> class.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        private IdentifiableObjectCollector(AssetPropertyGraph propertyGraph)
            : base(propertyGraph)
        {
        }

        /// <summary>
        /// Collects the <see cref="IIdentifiable"/> objects that are visited through nodes that are not representing object references.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        /// <param name="rootNode">The root object from which to collect. If null, <see cref="AssetPropertyGraph.RootNode"/> will be used.</param>
        /// <returns>A dictionary mapping <see cref="IIdentifiable"/> object by their identifier.</returns>
        [NotNull]
        public static Dictionary<Guid, IIdentifiable> Collect([NotNull] AssetPropertyGraph propertyGraph, [CanBeNull] IGraphNode rootNode = null)
        {
            if (propertyGraph == null) throw new ArgumentNullException(nameof(propertyGraph));
            var visitor = new IdentifiableObjectCollector(propertyGraph);
            visitor.Visit(rootNode ?? propertyGraph.RootNode);
            return visitor.result;
        }

        /// <inheritdoc/>
        protected override void VisitReference(IGraphNode referencer, ObjectReference reference, GraphNodePath targetPath)
        {
            // Remark: VisitReference is invoked only when IsObjectReference returned false, therefore we will visit only 'real' object here, not references to them.
            var value = reference.ObjectValue as IIdentifiable;
            if (value != null)
            {
                result[value.Id] = value;
            }
            base.VisitReference(referencer, reference, targetPath);
        }
    }
}
