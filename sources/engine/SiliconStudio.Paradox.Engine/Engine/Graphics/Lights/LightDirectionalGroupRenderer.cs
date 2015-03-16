// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
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

        private readonly ShaderSource lightShaderDynamic;
        private readonly ShaderSource[] lightShaderFixed;

        public LightDirectionalGroupRenderer()
        {
            // TODO: Handle unroll
            lightShaderDynamic = new ShaderClassSource("LightDirectionalGroup", LightMax);
            lightShaderGroup = new LightShaderGroup();
            lightShaderGroups = new List<LightShaderGroup>() { lightShaderGroup };
            lightDirections = new Vector3[LightMax];
            lightColors = new Color3[LightMax];

            // Precreate fixed lights for profile < 10.0
            lightShaderFixed = new ShaderSource[LightMax];
            for (int i = 1; i < LightMax; i++)
            {
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("LightDirectionalGroup", i));
                mixin.Mixins.Add(new ShaderClassSource("DirectLightGroupFixed", i));
                lightShaderFixed[i] = mixin;
            }
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
                lightDirections[i] = lightComponent.Direction;
                lightColors[i] = light.ComputeColor(lightComponent.Intensity);
            }

            lightShaderGroup.Parameters.Set(DirectLightGroupKeys.LightCount, count);
            lightShaderGroup.Parameters.Set(LightDirectionalGroupKeys.LightDirectionsWS, lightDirections);
            lightShaderGroup.Parameters.Set(LightDirectionalGroupKeys.LightColor, lightColors);

            // Setup the correct shader source depending on the profile
            lightShaderGroup.ShaderSource = context.GraphicsDevice.Features.Profile < GraphicsProfile.Level_10_0
                ? lightShaderFixed[count]
                : lightShaderDynamic;

            return lightShaderGroups;
        }
    }
}