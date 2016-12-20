// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Provides basic functionality for shadow map shader groups with a single shader source and a filter based on the <see cref="LightShadowType"/>
    /// </summary>
    public abstract class LightShadowMapShaderGroupDataBase : ILightShadowMapShaderGroupData
    {
        public LightShadowMapShaderGroupDataBase(LightShadowType shadowType)
        {
            ShadowType = shadowType;
        }

        public LightShadowType ShadowType { get; private set; }

        public ShaderMixinSource ShadowShader { get; private set; }

        public virtual void ApplyShader(ShaderMixinSource mixin)
        {
            mixin.CloneFrom(ShadowShader);
        }

        public virtual void UpdateLightCount(int lightLastCount, int lightCurrentCount)
        {
            ShadowShader = new ShaderMixinSource();
            ShadowShader.Mixins.Add(CreateShaderSource(lightCurrentCount));

            // Add filter for current shadow type
            switch (ShadowType & LightShadowType.FilterMask)
            {
                case LightShadowType.PCF3x3:
                    ShadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerDraw.Lighting", 3));
                    break;
                case LightShadowType.PCF5x5:
                    ShadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerDraw.Lighting", 5));
                    break;
                case LightShadowType.PCF7x7:
                    ShadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterPcf", "PerDraw.Lighting", 7));
                    break;
                default:
                    ShadowShader.Mixins.Add(new ShaderClassSource("ShadowMapFilterDefault", "PerDraw.Lighting"));
                    break;
            }
        }

        public virtual void UpdateLayout(string compositionName)
        {
        }

        public virtual void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights)
        {
        }

        public virtual void ApplyDrawParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights, ref BoundingBoxExt boundingBox)
        {
        }

        /// <summary>
        /// Creates the shader source that performs shadowing
        /// </summary>
        /// <returns></returns>
        public abstract ShaderClassSource CreateShaderSource(int lightCurrentCount);
    }
}