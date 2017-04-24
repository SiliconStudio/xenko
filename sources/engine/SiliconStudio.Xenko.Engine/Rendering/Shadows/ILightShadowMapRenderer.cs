// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Interface to render shadows
    /// </summary>
    public interface ILightShadowRenderer
    {
        /// <summary>
        /// Reset the state of this instance before calling Render method multiple times for different shadow map textures. See remarks.
        /// </summary>
        /// <remarks>
        /// This method allows the implementation to prepare some internal states before being rendered.
        /// </remarks>
        void Reset(RenderContext context);

        /// <summary>
        /// Test if this renderer can render this kind of light
        /// </summary>
        bool CanRenderLight(IDirectLight light);
    }


    /// <summary>
    /// Interface to render a shadow map.
    /// </summary>
    public interface ILightShadowMapRenderer : ILightShadowRenderer
    {
        RenderStage ShadowCasterRenderStage { get; }

        LightShadowType GetShadowType(LightShadowMap lightShadowMap);

        ILightShadowMapShaderGroupData CreateShaderGroupData(LightShadowType shadowType);

        void Collect(RenderContext context, RenderView sourceView, LightShadowMapTexture lightShadowMap);
        
        void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, LightShadowMapTexture shadowMapTexture);

        LightShadowMapTexture CreateShadowMapTexture(RenderView renderView, LightComponent lightComponent, IDirectLight light, int shadowMapSize);
    }
}
