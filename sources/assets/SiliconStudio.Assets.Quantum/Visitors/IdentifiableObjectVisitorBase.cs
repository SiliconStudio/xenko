// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that allows specific handling of all <see cref="IIdentifiable"/> instances visited, whether they are object references of not.
    /// </summary>
    public abstract class IdentifiableObjectVisitorBase : AssetGraphVisitorBase
    {
        /// <summary>
        /// Initializes a new instance of hte <see cref="IdentifiableObjectVisitorBase"/> class.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        protected IdentifiableObjectVisitorBase(AssetPropertyGraph propertyGraph)
            : base(propertyGraph)
        {
        }

        /// <inheritdoc/>
        protected override void VisitMemberTarget(IMemberNode node)
        {
            CheckAndProcessIdentifiable(node, Index.Empty);
            base.VisitMemberTarget(node);
        }

        /// <inheritdoc/>
        protected override void VisitItemTargets(IObjectNode node)
        {
            node.ItemReferences?.ForEach(x => CheckAndProcessIdentifiable(node, x.Index));
            base.VisitItemTargets(node);
        }

        /// <summary>
        /// Processes the <see cref="IIdentifiable"/> instance.
        /// </summary>
        /// <param name="identifiable">The identifiable instance to process.</param>
        /// <param name="node">The node containing the identifiable instance.</param>
        /// <param name="index">The index at which the identifiable instance can be reached in the node, or <see cref="Index.Empty"/> if the instance is not indexed.</param>
        protected abstract void ProcessIdentifiable([NotNull] IIdentifiable identifiable, IGraphNode node, Index index);

        private void CheckAndProcessIdentifiable(IGraphNode node, Index index)
        {
            var identifiable = node?.Retrieve(index) as IIdentifiable;
            if (identifiable == null)
                return;

            ProcessIdentifiable(identifiable, node, index);
        }
    }
}
