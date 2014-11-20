// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Renderers
{
    /// <summary>
    /// Extensions for forward lighting on <see cref="ModelRenderer"/>.
    /// </summary>
    public static class LightForwardModelRendererExtensions
    {
        /// <summary>
        /// Adds support for forward lighting.
        /// </summary>
        /// <param name="modelRenderer">The model renderer.</param>
        /// <returns>ModelRenderer.</returns>
        public static ModelRenderer AddLightForwardSupport(this ModelRenderer modelRenderer)
        {
            var renderer = new LightForwardModelRenderer(modelRenderer.Services);
            modelRenderer.PreRender.Add(renderer.PreRender);
            modelRenderer.PostRender.Add(renderer.PostRender);
            modelRenderer.PreEffectUpdate.Add(renderer.PreEffectUpdate);
            modelRenderer.PostEffectUpdate.Add(renderer.PostEffectUpdate);
            return modelRenderer;
        }
    }
}