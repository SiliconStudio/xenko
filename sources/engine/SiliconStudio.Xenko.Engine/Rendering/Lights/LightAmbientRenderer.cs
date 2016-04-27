// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Light renderer for <see cref="LightAmbient"/>.
    /// </summary>
    public class LightAmbientRenderer : LightGroupRendererBase
    {
        private readonly ShaderMixinSource mixin;
        private LightAmbientShaderGroup lightShaderGroup;

        public LightAmbientRenderer()
        {
            mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("LightSimpleAmbient"));
            IsEnvironmentLight = true;

            lightShaderGroup = new LightAmbientShaderGroup(mixin);
        }

        public override void Reset()
        {
            base.Reset();

            lightShaderGroup.Reset();
        }

        public override void SetViewCount(int viewCount)
        {
            base.SetViewCount(viewCount);

            // Make sure array is big enough for all render views
            Array.Resize(ref lightShaderGroup.AmbientColor, viewCount);
        }

        public override void ProcessLights(ProcessLightsParameters parameters)
        {
            // Sum contribution from all lights
            var ambientColor = new Color3();
            foreach (var light in parameters.LightCollection)
            {
                ambientColor += light.Color;
            }

            parameters.ShaderEntry.AddEnvironmentLightGroup(lightShaderGroup);

            // Store ambient sum for this view
            lightShaderGroup.AmbientColor[parameters.ViewIndex] = ambientColor;
        }

        private class LightAmbientShaderGroup : LightShaderGroup
        {
            internal Color3[] AmbientColor;

            private ValueParameterKey<Color3> ambientLightKey;
            public LightAmbientShaderGroup(ShaderMixinSource mixin)
                : base(mixin)
            {
            }

            public override void UpdateLayout(string compositionName)
            {
                ambientLightKey = LightSimpleAmbientKeys.AmbientLight.ComposeWith(compositionName);
            }

            public override void ApplyViewParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters)
            {
                base.ApplyViewParameters(context, viewIndex, parameters);

                parameters.Set(ambientLightKey, AmbientColor[viewIndex]);
            }
        }
    }
}