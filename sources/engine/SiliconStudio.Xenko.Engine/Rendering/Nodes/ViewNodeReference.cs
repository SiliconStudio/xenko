// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a <see cref="RenderNode"/>, with attached properties that are kept over time.
    /// </summary>
    partial struct ViewNodeReference
    {
        public EffectViewNodeReference CreateEffectReference(int effectPermutationSlotCount, int effectPermutationSlot)
        {
            return new EffectViewNodeReference(Index * effectPermutationSlotCount + effectPermutationSlot);
        }
    }
}