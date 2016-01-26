// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace RenderArchitecture
{
    /// <summary>
    /// Represents a <see cref="RenderNode"/>, with attached properties that are kept over time.
    /// </summary>
    partial struct StaticObjectNodeReference
    {
        public StaticEffectObjectNodeReference CreateEffectReference(int effectPermutationSlotCount, int effectPermutationSlot)
        {
            return new StaticEffectObjectNodeReference(Index * effectPermutationSlotCount + effectPermutationSlot);
        }
    }
}