// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public interface IColorTarget : IRenderTarget
    {
        Texture Color { get; set; }
    }

    public interface INormalsTarget : IRenderTarget
    {
        Texture Normals { get; set; }
    }

    public interface IVelocityTarget : IRenderTarget
    {
        Texture Velocity { get; set;  }
    }

    public interface IRenderTarget
    {
        Texture[] AllTargets { get; }

        int NumberOfTargets { get; }
    }

    public interface IPostProcessingEffects
    {
        void Collect(RenderContext context);

        void Draw(RenderDrawContext drawContext, IRenderTarget renderTargetsComposition, Texture inputDepthStencil, Texture outputTarget);

        bool RequiresVelocityBuffer { get; }

        bool RequiresNormalBuffer { get; }
    }
}