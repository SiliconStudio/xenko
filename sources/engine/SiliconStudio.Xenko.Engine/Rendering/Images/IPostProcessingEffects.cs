// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public interface IColorInput : IPostProcessingEffectsInput
    {
        Texture Color { get; }
    }

    public interface INormalsInput : IPostProcessingEffectsInput
    {
        Texture Normals { get; }
    }

    public interface IVelocityInput : IPostProcessingEffectsInput
    {
        Texture Velocity { get; }
    }

    public interface IPostProcessingEffectsInput
    {
    }

    public interface IPostProcessingEffects
    {
        void Collect(RenderContext context);

        void Draw(RenderDrawContext drawContext, IPostProcessingEffectsInput inputTargets, Texture inputDepthStencil, Texture outputTarget);

        bool RequiresVelocityBuffer { get; }

        bool RequiresNormalBuffer { get; }
    }
}