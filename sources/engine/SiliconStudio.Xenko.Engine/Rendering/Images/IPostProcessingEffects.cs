// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public interface IColorTarget : IRenderTarget
    {
        Texture Color { get; set; }
    }

    public interface INormalTarget : IRenderTarget
    {
        Texture Normal { get; set; }
    }

    public interface IVelocityTarget : IRenderTarget
    {
        Texture Velocity { get; set;  }
    }

    public interface IMultipleRenderViews : IRenderTarget
    {
        int Count { get; set; }

        int Index { get; set; }
    }

    public interface IRenderTarget
    {
        Texture[] AllTargets { get; }

        int NumberOfTargets { get; }
    }

    public interface IPostProcessingEffects : ISharedRenderer, IDisposable
    {
        void Collect(RenderContext context);

        void Draw(RenderDrawContext drawContext, IRenderTarget renderTargetsComposition, Texture inputDepthStencil, Texture outputTarget);

        bool RequiresVelocityBuffer { get; }

        bool RequiresNormalBuffer { get; }
    }
}