// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public class LightDirectionalGroupRenderer : LightGroupRendererBase
    {
        private const int LightMax = 8;

        private readonly LightShaderGroup lightShaderGroup;
        private readonly List<LightShaderGroup> lightShaderGroups;
        private readonly ShaderSource[] defaultShaderSource;
        private readonly Vector3[] lightDirections;
        private readonly Color3[] lightColors;

        public LightDirectionalGroupRenderer()
        {
            // TODO: Handle unroll
            lightShaderGroup  = new LightShaderGroup(new ShaderClassSource("LightDirectionalGroup", LightMax));
            lightShaderGroups = new List<LightShaderGroup>() { lightShaderGroup };
            lightDirections = new Vector3[LightMax];
            lightColors = new Color3[LightMax];
        }

        public override bool IsDirectLight
        {
            get
            {
                return true;
            }
        }

        public override List<LightShaderGroup> PrepareLights(RenderContext context, LightComponentCollection lights)
        {
            var count = Math.Min(lights.Count, LightMax);
            for (int i = 0; i < count; i++)
            {
                var lightComponent = lights[i];
                var light = (LightDirectional)lightComponent.Type;
                var lightDir = LightComponent.DefaultDirection;

                Vector3.TransformNormal(ref lightDir, ref lightComponent.Entity.Transform.WorldMatrix, out lightDirections[i]);
                lightColors[i] = light.ComputeColor(lightComponent.Intensity);
            }

            lightShaderGroup.Parameters.Set(DirectLightGroupKeys.LightCount, count);
            lightShaderGroup.Parameters.Set(LightDirectionalGroupKeys.LightDirectionsWS, lightDirections);
            lightShaderGroup.Parameters.Set(LightDirectionalGroupKeys.LightColor, lightColors);

            return lightShaderGroups;
        }
    }
}