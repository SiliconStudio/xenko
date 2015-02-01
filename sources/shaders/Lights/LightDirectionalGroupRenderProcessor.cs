// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public class LightDirectionalGroupRenderProcessor : DirectLightGroupRenderProcessorBase
    {
        private const int LightMax = 8;
        private readonly ShaderSource defaultShaderSource;
        private readonly Vector3[] lightDirections;
        private readonly Color3[] lightColors;

        public LightDirectionalGroupRenderProcessor()
        {
            // TODO: Handle unroll
            defaultShaderSource = new ShaderClassSource("LightDirectionalGroup", LightMax);
            lightDirections = new Vector3[LightMax];
            lightColors = new Color3[LightMax];
        }

        public override ShaderSource PrepareLights(RenderContext context, LightComponentCollection lights)
        {
            var count = Math.Min(lights.Count, LightMax);
            for (int i = 0; i < count; i++)
            {
                var light = lights[i];
                var lightDir = LightComponent.DefaultDirection;

                Vector3.TransformNormal(ref lightDir, ref light.Entity.Transformation.WorldMatrix, out lightDirections[i]);
                lightColors[i] = light.ComputeColorWithIntensity();
            }

            Parameters.Set(DirectLightGroupKeys.LightCount, count);
            Parameters.Set(LightDirectionalGroupKeys.LightDirectionsWS, lightDirections);
            Parameters.Set(LightDirectionalGroupKeys.LightColor, lightColors);

            return defaultShaderSource;
        }

        public override void PrepareLightsWithShadows(RenderContext context, LightComponentCollection lights)
        {
        }
    }
}