// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.Quantum.View
{
    public class AnimationFrameTemplateProvider : ObservableNodeTemplateProvider
    {
        public override string Name { get { return "AnimationFrameTemplateProvider"; } }

        public override bool MatchNode(IObservableNode node)
        {
            return node.Type.IsNumeric() && (node.Name.Equals("StartAnimationFrame") || node.Name.Equals("EndAnimationFrame"));
        }
    }
}