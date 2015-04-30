// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Rendering
{
    [DataContract("ImageEffectRendererInput.LayerInput")]
    [Display("Layer Input")]
    public sealed class LayerInputFrameProvider : RenderFrameProviderBase, IImageEffectRendererInput
    {
        public override RenderFrame GetRenderFrame(RenderContext context)
        {
            return context.Tags.GetSafe(SceneGraphicsLayer.CurrentInput);
        }
    }
}