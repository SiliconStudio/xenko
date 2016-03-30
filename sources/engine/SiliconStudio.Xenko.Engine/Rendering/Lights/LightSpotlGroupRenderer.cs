// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public struct SpotLightData
    {
        public Vector3 PositionWS;
        private float padding0;
        public Vector3 DirectionWS;
        private float padding1;
        public Vector3 AngleOffsetAndInvSquareRadius;
        private float padding2;
        public Color3 Color;
        private float padding3;
    }

    public class LightSpotGroupRenderer : LightGroupRendererBase
    {
        private const int StaticLightMaxCount = 8;

        private static readonly ShaderClassSource DynamicDirectionalGroupShaderSource = new ShaderClassSource("LightSpotGroup", StaticLightMaxCount);

        public LightSpotGroupRenderer()
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
                mixin.Mixins.Add(new ShaderClassSource("LightSpotGroup", lightMaxCount));
                mixin.Mixins.Add(new ShaderClassSource("DirectLightGroupFixed", lightMaxCount));
            }

            if (shadowGroup != null)
            {
                shadowGroup.ApplyShader(mixin);
            }

            return new SpotLightShaderGroup(mixin, compositionName, shadowGroup);
        }

        class SpotLightShaderGroup : LightShaderGroupAndDataPool<SpotLightShaderGroupData>
        {
            internal readonly ValueParameterKey<int> CountKey;
            internal readonly ValueParameterKey<SpotLightData> LightsKey;
            internal readonly ValueParameterKey<Vector3> PositionsKey;
            internal readonly ValueParameterKey<Vector3> AngleOffsetAndInvSquareRadiusKey;
            internal readonly ValueParameterKey<Color3> ColorsKey;

            public SpotLightShaderGroup(ShaderMixinSource mixin, string compositionName, ILightShadowMapShaderGroupData shadowGroupData)
                : base(mixin, compositionName, shadowGroupData)
            {
                CountKey = DirectLightGroupKeys.LightCount.ComposeWith(compositionName);
                LightsKey = LightSpotGroupKeys.Lights.ComposeWith(compositionName);
            }

            protected override SpotLightShaderGroupData CreateData()
            {
                return new SpotLightShaderGroupData(this, ShadowGroup);
            }
        }

        class SpotLightShaderGroupData : LightShaderGroupData
        {
            private readonly ValueParameterKey<int> countKey;
            private readonly ValueParameterKey<SpotLightData> lightsKey;
            private readonly SpotLightData[] lights;

            public SpotLightShaderGroupData(SpotLightShaderGroup group, ILightShadowMapShaderGroupData shadowGroupData)
                : base(shadowGroupData)
            {
                countKey = group.CountKey;
                lightsKey = group.LightsKey;

                lights = new SpotLightData[StaticLightMaxCount];
            }

            protected override void AddLightInternal(LightComponent light)
            {
                var spotLight = (LightSpot)light.Type;
                lights[Count] = new SpotLightData
                {
                    PositionWS = light.Position,
                    DirectionWS = light.Direction,
                    AngleOffsetAndInvSquareRadius = new Vector3(spotLight.LightAngleScale, spotLight.LightAngleOffset, spotLight.InvSquareRange),
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