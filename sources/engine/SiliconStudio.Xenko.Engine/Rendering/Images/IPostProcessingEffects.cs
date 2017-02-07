// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public interface IPostProcessingEffects
    {
        void Draw(RenderDrawContext drawContext, IList<Texture> inputTargets, Texture inputDepthStencil, Texture outputTarget);
    }
}