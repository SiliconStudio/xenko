// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Processors;
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
            var viewMatrix = context.CurrentPass.Parameters.Get(TransformationKeys.View);

            var count = Math.Min(lights.Count, LightMax);
            for (int i = 0; i < count; i++)
            {
                var light = lights[i];
                var lightDir = new Vector3(0, 0, 1);

                Matrix worldView;
                Vector3 direction;
                Matrix.Multiply(ref light.Entity.Transformation.WorldMatrix, ref viewMatrix, out worldView);
                Vector3.TransformNormal(ref lightDir, ref worldView, out direction);

                lightDirections[i] = direction;
                lightColors[i] = light.ComputeColorWithIntensity();
            }

            Parameters.Set(DirectLightGroupKeys.LightCount, count);
            Parameters.Set(LightDirectionalGroupKeys.LightDirectionsVS, lightDirections);
            Parameters.Set(LightDirectionalGroupKeys.LightColor, lightColors);

            return defaultShaderSource;
        }

        public override void PrepareLightsWithShadows(RenderContext context, LightComponentCollection lights)
        {
        }
    }
}