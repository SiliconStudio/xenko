// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public interface IPostProcessingEffects : ISharedRenderer, IDisposable
    {
        void Collect(RenderContext context);

        void Draw(RenderDrawContext drawContext, RenderOutputValidator outputValidator, Texture[] inputs, Texture inputDepthStencil, Texture outputTarget);

        bool RequiresVelocityBuffer { get; }

        bool RequiresNormalBuffer { get; }

        bool RequiresSpecularRoughnessBuffer { get; }
    }
}
