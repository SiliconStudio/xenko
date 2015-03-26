// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Shadows
{

    public interface ILightShadowRenderer
    {
    }

    /// <summary>
    /// Interface to render a shadow map.
    /// </summary>
    public interface ILightShadowMapRenderer : ILightShadowRenderer
    {
        void Render(RenderContext context, ShadowMapRenderer shadowMapRenderer, ref LightShadowMapTexture lightShadowMap);
    }
}