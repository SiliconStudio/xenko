// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that clear object references to a specific identifiable object.
    /// </summary>
    public class ClearObjectReferenceVisitor : IdentifiableObjectVisitorBase
    {
        private readonly HashSet<Guid> targetIds;
        private readonly Func<IGraphNode, Index, bool> shouldClearReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearObjectReferenceVisitor"/> class.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        /// <param name="targetIds">The identifiers of the objects for which to clear references.</param>
        /// <param name="shouldClearReference">A method allowing to select which object reference to clear. If null, all object references to the given id will be cleared.</param>
        public ClearObjectReferenceVisitor([NotNull] AssetPropertyGraph propertyGraph, [NotNull] IEnumerable<Guid> targetIds, [CanBeNull] Func<IGraphNode, Index, bool> shouldClearReference = null)
            : base(propertyGraph)
        {
            if (propertyGraph == null) throw new ArgumentNullException(nameof(propertyGraph));
            if (targetIds == null) throw new ArgumentNullException(nameof(targetIds));
            this.targetIds = new HashSet<Guid>(targetIds);
            this.shouldClearReference = shouldClearReference;
        }

        /// <inheritdoc/>
        protected override void ProcessIdentifiable(IIdentifiable identifiable, IGraphNode node, Index index)
        {
            if (!targetIds.Contains(identifiable.Id))
                return;

            if (PropertyGraph.IsObjectReference(node, index, node.Retrieve(index)))
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
