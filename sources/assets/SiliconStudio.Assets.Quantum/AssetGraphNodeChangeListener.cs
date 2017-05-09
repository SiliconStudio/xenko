// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetGraphNodeChangeListener : GraphNodeChangeListener
    {
        public AssetGraphNodeChangeListener(IGraphNode rootNode, [NotNull] AssetPropertyGraphDefinition propertyGraphDefinition)
            : base(rootNode, member => !propertyGraphDefinition.IsMemberTargetObjectReference(member, member.Retrieve()), (node, index) => !propertyGraphDefinition.IsTargetItemObjectReference(node, index, node.Retrieve(index)))
        {
        }
    }
}
