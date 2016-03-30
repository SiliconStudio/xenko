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
    public struct DirectionalLightData
    {
        public Vector3 DirectionWS;
        private float padding0;
        public Color3 Color;
        private float padding1;
    }

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
            internal readonly ValueParameterKey<DirectionalLightData> LightsKey;

            public DirectionalLightShaderGroup(ShaderMixinSource mixin, string compositionName, ILightShadowMapShaderGroupData shadowGroupData)
                : base(mixin, compositionName, shadowGroupData)
            {
                CountKey = DirectLightGroupKeys.LightCount.ComposeWith(compositionName);
                LightsKey = LightDirectionalGroupKeys.Lights.ComposeWith(compositionName);
            }

            protected override DirectionalLightShaderGroupData CreateData()
            {
                return new DirectionalLightShaderGroupData(this, ShadowGroup);
            }
        }

        class DirectionalLightShaderGroupData : LightShaderGroupData
        {
            private readonly ValueParameterKey<int> countKey;
            private readonly ValueParameterKey<DirectionalLightData> lightsKey;
            private readonly ValueParameterKey<Color3> colorsKey;
            private readonly DirectionalLightData[] lights;

            public DirectionalLightShaderGroupData(DirectionalLightShaderGroup group, ILightShadowMapShaderGroupData shadowGroupData)
                : base(shadowGroupData)
            {
                countKey = group.CountKey;
                lightsKey = group.LightsKey;

                lights = new DirectionalLightData[StaticLightMaxCount];
            }

            protected override void AddLightInternal(LightComponent light)
            {
                lights[Count] = new DirectionalLightData
                {
                    DirectionWS = light.Direction,
                    Color = light.Color,
                };
            }

            protected override void ApplyParametersInternal(ParameterCollection parameters)
            {
                parameters.Set(countKey, Count);
                parameters.Set(lightsKey, Count, ref lights[0]);
            }
        }
    }
}