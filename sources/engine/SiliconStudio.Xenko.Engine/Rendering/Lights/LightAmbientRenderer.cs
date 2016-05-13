// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;
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
        private LightAmbientShaderGroup lightShaderGroup = new LightAmbientShaderGroup();

        public LightAmbientRenderer()
        {
            IsEnvironmentLight = true;
        }

        public override void Reset()
        {
            base.Reset();

            lightShaderGroup.Reset();
        }

        public override void SetViews(FastList<RenderView> views)
        {
            base.SetViews(views);

            // Make sure array is big enough for all render views
            Array.Resize(ref lightShaderGroup.AmbientColor, views.Count);
        }

        public override void ProcessLights(ProcessLightsParameters parameters)
        {
            // Sum contribution from all lights
            var ambientColor = new Color3();
            for (int index = parameters.LightStart; index < parameters.LightEnd; index++)
            {
                var light = parameters.LightCollection[index];
                ambientColor += light.Color;
            }

            // Store ambient sum for this view
            lightShaderGroup.AmbientColor[parameters.ViewIndex] = ambientColor;
        }

        public override void UpdateShaderPermutationEntry(ForwardLightingRenderFeature.LightShaderPermutationEntry shaderEntry)
        {
            shaderEntry.EnvironmentLights.Add(lightShaderGroup);
        }

        private class LightAmbientShaderGroup : LightShaderGroup
        {
            internal Color3[] AmbientColor;

            private ValueParameterKey<Color3> ambientLightKey;
            public LightAmbientShaderGroup()
                : base(new ShaderClassSource("LightSimpleAmbient"))
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