// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows.Documents;

using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public abstract class LightShaderGroup
    {
        protected LightShaderGroup(ShaderMixinSource mixin, ILightShadowMapShaderGroupData shadowGroup)
        {
            if (mixin == null) throw new ArgumentNullException("mixin");
            ShaderSource = mixin;
            ShadowGroup = shadowGroup;
        }

        public bool IsEnvironementLightGroup { get; set; }

        public ShaderMixinSource ShaderSource { get; private set; }

        public int Count { get; private set; }

        public ILightShadowMapShaderGroupData ShadowGroup { get; private set; }

        public void Reset()
        {
            Count = 0;
        }

        public void AddLight(LightComponent light, LightShadowMapTexture shadowMapTexture)
        {
            AddLightInternal(light);
            if (ShadowGroup != null)
            {
                ShadowGroup.SetShadowMapShaderData(Count, shadowMapTexture.ShaderData);
            }
            Count++;
        }

        public void ApplyParameters(ParameterCollection parameters)
        {
            ApplyParametersInternal(parameters);
            if (ShadowGroup != null)
            {
                ShadowGroup.ApplyParameters(parameters);
            }
        }

        protected abstract void AddLightInternal(LightComponent light);

        protected abstract void ApplyParametersInternal(ParameterCollection parameters);
    }
}