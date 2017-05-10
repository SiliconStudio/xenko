// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetGraphNodeLinker : GraphNodeLinker
    {
        private readonly AssetPropertyGraphDefinition propertyGraphDefinition;

        public AssetGraphNodeLinker(AssetPropertyGraphDefinition propertyGraphDefinition)
        {
            this.propertyGraphDefinition = propertyGraphDefinition;
        }

        protected override bool ShouldVisitMemberTarget(IMemberNode member)
        {
            return !propertyGraphDefinition.IsMemberTargetObjectReference(member, member.Retrieve()) && base.ShouldVisitMemberTarget(member);
        }

        protected override bool ShouldVisitTargetItem(IObjectNode collectionNode, Index index)
        {
            return !propertyGraphDefinition.IsTargetItemObjectReference(collectionNode, index, collectionNode.Retrieve(index)) && base.ShouldVisitTargetItem(collectionNode, index);
        }
    }
}
