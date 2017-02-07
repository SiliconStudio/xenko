// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    [DataContract("BasePostProcessingEffects")]
    public abstract class BasePostProcessingEffects : ImageEffect
    {
        public abstract void Draw(RenderDrawContext drawContext, List<Texture> inputTargets, Texture inputDepthStencil, Texture outputTarget);
    }
}