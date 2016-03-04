// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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