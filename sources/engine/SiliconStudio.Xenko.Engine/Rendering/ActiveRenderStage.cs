// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Rendering
{
    public struct ActiveRenderStage
    {
        public bool Active => EffectSelector != null;

        public EffectSelector EffectSelector;

        public ActiveRenderStage(string effectName)
        {
            EffectSelector = new EffectSelector(effectName);
        }
    }
}
