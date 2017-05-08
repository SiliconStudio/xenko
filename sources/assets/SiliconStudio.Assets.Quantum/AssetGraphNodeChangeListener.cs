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
        public AssetGraphNodeChangeListener(IGraphNode rootNode, [NotNull] AssetPropertyGraph propertyGraph)
            : base(rootNode, member => !propertyGraph.Definition.IsMemberTargetObjectReference(member, member.Retrieve()), (node, index) => !propertyGraph.Definition.IsTargetItemObjectReference(node, index, node.Retrieve(index)))
        {
        }
    }
}
