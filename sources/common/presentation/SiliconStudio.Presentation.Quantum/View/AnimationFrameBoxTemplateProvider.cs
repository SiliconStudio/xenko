// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Presentation.Quantum.View
{
    public class AnimationFrameBoxTemplateProvider : NodeViewModelTemplateProvider
    {
        public override string Name { get { return "AnimationFrameBoxTemplateProvider"; } }

        public override bool MatchNode(INodeViewModel node)
        {
            return (node.Name.Equals("StartAnimationTimeBox") || node.Name.Equals("EndAnimationTimeBox"));
        }
    }
}
