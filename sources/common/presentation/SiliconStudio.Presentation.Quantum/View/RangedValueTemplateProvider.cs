// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.ViewModels;

namespace SiliconStudio.Presentation.Quantum.View
{
    public class RangedValueTemplateProvider : NodeViewModelTemplateProvider
    {
        public override string Name => "RangedValueTemplateProvider";

        public override bool MatchNode(NodeViewModel node)
        {
            return node.Type.IsNumeric() && node.AssociatedData.ContainsKey("Minimum") && node.AssociatedData.ContainsKey("Maximum")
                   && node.AssociatedData.ContainsKey("SmallStep") && node.AssociatedData.ContainsKey("LargeStep");
        }
    }
}
