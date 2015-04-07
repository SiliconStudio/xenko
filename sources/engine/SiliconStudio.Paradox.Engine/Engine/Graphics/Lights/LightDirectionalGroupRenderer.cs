// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public class LightDirectionalGroupRenderer : LightGroupRendererBase
    {
        private const int StaticLightMaxCount = 8;

        private static readonly ShaderClassSource DynamicDirectionalGroupShaderSource = new ShaderClassSource("LightDirectionalGroup", StaticLightMaxCount);

        public LightDirectionalGroupRenderer()
        {
            LightMaxCount = StaticLightMaxCount;
        }

        public override void Initialize(RenderContext context)
        {
            var isLowProfile = context.GraphicsDevice.Features.Profile < GraphicsProfile.Level_10_0;
            LightMaxCount = isLowProfile ? 2 : StaticLightMaxCount;
            AllocateLightMaxCount = !isLowProfile;
        }

        public override LightShaderGroup CreateLightShaderGroup(string compositionName, int compositionIndex, int lightMaxCount, ILightShadowMapShaderGroupData shadowGroup)
        {
            var mixin = new ShaderMixinSource();
            if (AllocateLightMaxCount)
            {
                mixin.Mixins.Add(DynamicDirectionalGroupShaderSource);
            }
            else
            {
                mixin.Mixins.Add(new ShaderClassSource("LightDirectionalGroup", lightMaxCount));
                mixin.Mixins.Add(new ShaderClassSource("DirectLightGroupFixed", lightMaxCount));
            }

            return new DirectionalLightShaderGroup(mixin, compositionName, compositionIndex, AllocateLightMaxCount ? LightMaxCount : lightMaxCount, shadowGroup);
        }

        class DirectionalLightShaderGroup : LightShaderGroup
        {
            private readonly ParameterKey<int> countKey;
            private readonly ParameterKey<Vector3[]> directionsKey;
            private readonly ParameterKey<Color3[]> colorsKey;
            private readonly Vector3[] lightDirections;
            private readonly Color3[] lightColors;

            public DirectionalLightShaderGroup(ShaderMixinSource mixin, string compositionName, int compositionIndex, int size, ILightShadowMapShaderGroupData shadowGroupData)
                : base(mixin, shadowGroupData)
            {
                countKey = DirectLightGroupKeys.LightCount.ComposeIndexer(compositionName, compositionIndex);
                directionsKey = LightDirectionalGroupKeys.LightDirectionsWS.ComposeIndexer(compositionName, compositionIndex);
                colorsKey = LightDirectionalGroupKeys.LightColor.ComposeIndexer(compositionName, compositionIndex);

                lightDirections = new Vector3[size];
                lightColors = new Color3[size];
            }

            protected override void AddLightInternal(LightComponent light)
            {
                lightDirections[Count] = light.Direction;
                lightColors[Count] = light.Color;
            }

            protected override void ApplyParametersInternal(ParameterCollection parameters)
            {
                parameters.Set(countKey, Count);
                parameters.Set(directionsKey, lightDirections);
                parameters.Set(colorsKey, lightColors);
            }
        }
    }
}