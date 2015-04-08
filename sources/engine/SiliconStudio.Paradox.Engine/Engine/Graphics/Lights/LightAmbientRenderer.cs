// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Light renderer for <see cref="LightAmbient"/>.
    /// </summary>
    public class LightAmbientRenderer : LightGroupRendererBase
    {
        private readonly ShaderMixinSource mixin;

        public LightAmbientRenderer()
        {
            mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("LightSimpleAmbient"));
            LightMaxCount = 4;
            IsEnvironmentLight = true;
        }

        public override LightShaderGroup CreateLightShaderGroup(string compositionName, int compositionIndex, int lightMaxCount, ILightShadowMapShaderGroupData shadowGroup)
        {
            return new LightAmbientShaderGroup(compositionName, compositionIndex, mixin);
        }

        private class LightAmbientShaderGroup : LightShaderGroup
        {
            private ParameterKey<Color3> ambientLightKey;
            private Color3 color;

            public LightAmbientShaderGroup(string compositionName, int compositionIndex, ShaderMixinSource mixin)
                : base(mixin, null)
            {
                ambientLightKey = LightSimpleAmbientKeys.AmbientLight.ComposeIndexer(compositionName, compositionIndex);
            }

            protected override void AddLightInternal(LightComponent light)
            {
                color = light.Color;
            }

            protected override void ApplyParametersInternal(ParameterCollection parameters)
            {
                parameters.Set(ambientLightKey, color);
            }
        }
    }
}