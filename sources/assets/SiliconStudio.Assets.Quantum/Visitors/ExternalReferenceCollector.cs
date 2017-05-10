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
    /// A visitor that will collect all object references that target objects that are not included in the visited object.
    /// </summary>
    public class ExternalReferenceCollector : IdentifiableObjectVisitorBase
    {
        private readonly AssetPropertyGraphDefinition propertyGraphDefinition;

        private readonly HashSet<IIdentifiable> internalReferences = new HashSet<IIdentifiable>();
        private readonly HashSet<IIdentifiable> externalReferences = new HashSet<IIdentifiable>();

        private ExternalReferenceCollector(AssetPropertyGraphDefinition propertyGraphDefinition)
            : base(propertyGraphDefinition)
        {
            this.propertyGraphDefinition = propertyGraphDefinition;
        }

        /// <summary>
        /// Computes the external references to the given root node.
        /// </summary>
        /// <param name="propertyGraphDefinition">The property graph definition to use to analyze the graph.</param>
        /// <param name="root">The root node to analyze.</param>
        /// <returns>A set containing all external references to identifiable objects.</returns>
        public static HashSet<IIdentifiable> GetExternalReferences(AssetPropertyGraphDefinition propertyGraphDefinition, IGraphNode root)
        {
            var visitor = new ExternalReferenceCollector(propertyGraphDefinition);
            visitor.Visit(root);
            // An IIdentifiable can have been recorded both as internal and external reference. In this case we still want to clone it so let's remove it from external references
            visitor.externalReferences.ExceptWith(visitor.internalReferences);
            return visitor.externalReferences;
        }

        protected override void ProcessIdentifiableMembers(IIdentifiable identifiable, IMemberNode member)
        {
            if (propertyGraphDefinition.IsMemberTargetObjectReference(member, identifiable))
                externalReferences.Add(identifiable);
            else
                internalReferences.Add(identifiable);
        }

        protected override void ProcessIdentifiableItems(IIdentifiable identifiable, IObjectNode collection, Index index)
        {
            if (propertyGraphDefinition.IsTargetItemObjectReference(collection, index, identifiable))
                externalReferences.Add(identifiable);
            else
                internalReferences.Add(identifiable);
        }
    }
}
