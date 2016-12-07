// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    public interface ILightShadowRenderer
    {
        /// <summary>
        /// Reset the state of this instance before calling Render method multiple times for different shadow map textures. See remarks.
        /// </summary>
        /// <remarks>
        /// This method allows the implementation to prepare some internal states before being rendered.
        /// </remarks>
        void Reset();
    }


    /// <summary>
    /// Interface to render a shadow map.
    /// </summary>
    public interface ILightShadowMapRenderer : ILightShadowRenderer
    {
        LightShadowType GetShadowType(LightShadowMap lightShadowMap);

        ILightShadowMapShaderGroupData CreateShaderGroupData(LightShadowType shadowType);

        /// <summary>
        /// Test if this renderer can render this kind of light
        /// </summary>
        bool CanRenderLight(IDirectLight light);

        void Collect(RenderContext context, LightShadowMapTexture lightShadowMap);

        void CreateRenderViews(LightShadowMapTexture lightShadowMap, VisibilityGroup visibilityGroup);

        void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, LightShadowMapTexture shadowMapTexture);

        LightShadowMapTexture CreateTexture(LightComponent lightComponent, IDirectLight light, int shadowMapSize);
    }
}