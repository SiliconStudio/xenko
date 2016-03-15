// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public class LightDirectionalGroupRenderer : LightGroupRendererBase
    {
        private const int StaticLightMaxCount = 8;

        private static readonly ShaderClassSource DynamicDirectionalGroupShaderSource = new ShaderClassSource("LightDirectionalGroup", StaticLightMaxCount);

        public LightDirectionalGroupRenderer()
        {
            LightMaxCount = StaticLightMaxCount;
            CanHaveShadows = true;
        }

        public override void Initialize(RenderContext context)
        {
            var isLowProfile = context.GraphicsDevice.Features.RequestedProfile < GraphicsProfile.Level_10_0;
            LightMaxCount = isLowProfile ? 2 : StaticLightMaxCount;
            AllocateLightMaxCount = !isLowProfile;
        }

        public override LightShaderGroup CreateLightShaderGroup(string compositionName, int lightMaxCount, ILightShadowMapShaderGroupData shadowGroup)
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

            if (shadowGroup != null)
            {
                shadowGroup.ApplyShader(mixin);
            }

            return new DirectionalLightShaderGroup(mixin, compositionName, shadowGroup);
        }

        class DirectionalLightShaderGroup : LightShaderGroupAndDataPool<DirectionalLightShaderGroupData>
        {
            internal readonly ValueParameterKey<int> CountKey;
            internal readonly ValueParameterKey<Vector3> DirectionsKey;
            internal readonly ValueParameterKey<Color3> ColorsKey;

            public DirectionalLightShaderGroup(ShaderMixinSource mixin, string compositionName, ILightShadowMapShaderGroupData shadowGroupData)
                : base(mixin, compositionName, shadowGroupData)
            {
                CountKey = DirectLightGroupKeys.LightCount.ComposeWith(compositionName);
                DirectionsKey = LightDirectionalGroupKeys.LightDirectionsWS.ComposeWith(compositionName);
                ColorsKey = LightDirectionalGroupKeys.LightColor.ComposeWith(compositionName);
            }

            protected override DirectionalLightShaderGroupData CreateData()
            {
                return new DirectionalLightShaderGroupData(this, ShadowGroup);
            }
        }

        class DirectionalLightShaderGroupData : LightShaderGroupData
        {
            private readonly ValueParameterKey<int> countKey;
            private readonly ValueParameterKey<Vector3> directionsKey;
            private readonly ValueParameterKey<Color3> colorsKey;
            private readonly Vector3[] lightDirections;
            private readonly Color3[] lightColors;

            public DirectionalLightShaderGroupData(DirectionalLightShaderGroup group, ILightShadowMapShaderGroupData shadowGroupData)
                : base(shadowGroupData)
            {
                countKey = group.CountKey;
                directionsKey = group.DirectionsKey;
                colorsKey = group.ColorsKey;

                lightDirections = new Vector3[StaticLightMaxCount];
                lightColors = new Color3[StaticLightMaxCount];
            }

            protected override void AddLightInternal(LightComponent light)
            {
                lightDirections[Count] = light.Direction;
                lightColors[Count] = light.Color;
            }

            protected override void ApplyParametersInternal(ParameterCollection parameters)
            {
                parameters.Set(countKey, Count);
                parameters.Set(directionsKey, Count, ref lightDirections[0]);
                parameters.Set(colorsKey, Count, ref lightColors[0]);
            }
        }
    }
}