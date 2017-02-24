using System;
using SiliconStudio.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that clear object references to a specific identifiable object.
    /// </summary>
    public class ClearObjectReferenceVisitor : IdentifiableObjectVisitorBase
    {
        private readonly Func<IGraphNode, Index, bool> shouldClearReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearObjectReferenceVisitor"/> class.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        /// <param name="targetId">The identifier of the object for which to clear references.</param>
        /// <param name="shouldClearReference">A method allowing to select which object reference to clear. If null, all object references to the given id will be cleared.</param>
        public ClearObjectReferenceVisitor(AssetPropertyGraph propertyGraph, Guid targetId, Func<IGraphNode, Index, bool> shouldClearReference = null)
            : base(propertyGraph)
        {
            this.shouldClearReference = shouldClearReference;
        }

        /// <inheritdoc/>
        protected override void ProcessIdentifiable(IIdentifiable identifiable, IGraphNode node, Index index)
        {
            if (PropertyGraph.IsObjectReference(node, index))
            {
                if (shouldClearReference?.Invoke(node, index) ?? true)
                {
                    if (index == Index.Empty)
                        ((IMemberNode)node).Update(null);
                    else
                        ((IObjectNode)node).Update(null, index);
                }
            }
        }
    }
}
