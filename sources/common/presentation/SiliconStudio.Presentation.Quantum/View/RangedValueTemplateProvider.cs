// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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