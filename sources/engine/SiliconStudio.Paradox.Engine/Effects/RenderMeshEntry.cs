// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Internal structure used to flatten RenderPass and EffectMesh hierarchy.
    /// </summary>
    public struct RenderMeshEntry
    {
        public DelegateHolder<RenderContext> Action;
        public EffectMesh EffectMesh;
    }
}