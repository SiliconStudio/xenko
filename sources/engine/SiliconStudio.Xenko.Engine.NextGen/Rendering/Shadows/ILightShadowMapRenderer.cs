// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows.NextGen
{
    /// <summary>
    /// Interface to render a shadow map.
    /// </summary>
    // TODO GRAPHICS REFACTOR remove temporary duplicate interface
    public interface ILightShadowMapRenderer : Shadows.ILightShadowMapRenderer
    {
        //LightShadowType GetShadowType(LightShadowMap lightShadowMap);

        //ILightShadowMapShaderGroupData CreateShaderGroupData(string compositionKey, LightShadowType shadowType, int maxLightCount);
        
        void Extract(RenderContext context, ShadowMapRenderer shadowMapRenderer, LightShadowMapTexture lightShadowMap);

        void GetCascadeViewParameters(LightShadowMapTexture shadowMapTexture, int cascadeIndex, out Matrix view, out Matrix projection);
    }
}