// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.Quantum.View
{
    public class RangedValueTemplateProvider : ObservableNodeTemplateProvider
    {
        public override string Name { get { return "RangedValueTemplateProvider"; } }

        public override bool MatchNode(INodeViewModel node)
        {
            return node.Type.IsNumeric() && node.AssociatedData.ContainsKey("Minimum") && node.AssociatedData.ContainsKey("Maximum")
                   && node.AssociatedData.ContainsKey("SmallStep") && node.AssociatedData.ContainsKey("LargeStep");
        }
    }
}