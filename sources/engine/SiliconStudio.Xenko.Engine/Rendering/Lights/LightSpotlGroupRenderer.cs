// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
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
            internal readonly ValueParameterKey<Vector3> DirectionsKey;
            internal readonly ValueParameterKey<Vector3> PositionsKey;
            internal readonly ValueParameterKey<Vector3> AngleOffsetAndInvSquareRadiusKey;
            internal readonly ValueParameterKey<Color3> ColorsKey;

            public SpotLightShaderGroup(ShaderMixinSource mixin, string compositionName, ILightShadowMapShaderGroupData shadowGroupData)
                : base(mixin, compositionName, shadowGroupData)
            {
                CountKey = DirectLightGroupKeys.LightCount.ComposeWith(compositionName);
                DirectionsKey = LightSpotGroupKeys.LightDirectionsWS.ComposeWith(compositionName);
                PositionsKey = LightSpotGroupKeys.LightPositionWS.ComposeWith(compositionName);
                AngleOffsetAndInvSquareRadiusKey = LightSpotGroupKeys.LightAngleOffsetAndInvSquareRadius.ComposeWith(compositionName); 
                ColorsKey = LightSpotGroupKeys.LightColor.ComposeWith(compositionName);
            }

            protected override SpotLightShaderGroupData CreateData()
            {
                return new SpotLightShaderGroupData(this, ShadowGroup);
            }
        }

        class SpotLightShaderGroupData : LightShaderGroupData
        {
            private readonly ValueParameterKey<int> countKey;
            private readonly ValueParameterKey<Vector3> directionsKey;
            private readonly ValueParameterKey<Color3> colorsKey;
            internal readonly ValueParameterKey<Vector3> positionsKey;
            internal readonly ValueParameterKey<Vector3> angleOffsetAndInvSquareRadiusKey;
            private readonly Vector3[] lightDirections;
            private readonly Vector3[] lightPositions;
            private readonly Vector3[] lightAngleOffsetAndInvSquareRadius;
            private readonly Color3[] lightColors;

            public SpotLightShaderGroupData(SpotLightShaderGroup group, ILightShadowMapShaderGroupData shadowGroupData)
                : base(shadowGroupData)
            {
                countKey = group.CountKey;
                directionsKey = group.DirectionsKey;
                colorsKey = group.ColorsKey;
                positionsKey = group.PositionsKey;
                angleOffsetAndInvSquareRadiusKey = group.AngleOffsetAndInvSquareRadiusKey;

                lightDirections = new Vector3[StaticLightMaxCount];
                lightColors = new Color3[StaticLightMaxCount];
                lightPositions = new Vector3[StaticLightMaxCount];
                lightAngleOffsetAndInvSquareRadius = new Vector3[StaticLightMaxCount];
            }

            protected override void AddLightInternal(LightComponent light)
            {
                var spotLight = (LightSpot)light.Type;
                lightDirections[Count] = light.Direction;
                lightColors[Count] = light.Color;
                lightPositions[Count] = light.Position;
                lightAngleOffsetAndInvSquareRadius[Count] = new Vector3(spotLight.LightAngleScale, spotLight.LightAngleOffset, spotLight.InvSquareRange);
            }

            protected override void ApplyParametersInternal(ParameterCollection parameters)
            {
                parameters.Set(countKey, Count);
                parameters.Set(directionsKey, Count, ref lightDirections[0]);
                parameters.Set(colorsKey, Count, ref lightColors[0]);
                parameters.Set(positionsKey, Count, ref lightPositions[0]);
                parameters.Set(angleOffsetAndInvSquareRadiusKey, Count, ref lightAngleOffsetAndInvSquareRadius[0]);
            }
        }
    }
}