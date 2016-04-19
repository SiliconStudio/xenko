// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public struct PointLightData
    {
        public Vector3 PositionWS;
        public float InvSquareRadius;
        public Color3 Color;
        private float padding0;
    }

    public class LightPointGroupRenderer : LightGroupRendererBase
    {
        private const int StaticLightMaxCount = 8;

        private static readonly ShaderClassSource DynamicDirectionalGroupShaderSource = new ShaderClassSource("LightPointGroup", StaticLightMaxCount);

        public LightPointGroupRenderer()
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
                if (lightMaxCount == 0) lightMaxCount = 1; //todo verify this.. this is just an hot fix 
                mixin.Mixins.Add(new ShaderClassSource("LightPointGroup", lightMaxCount));
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
            internal readonly ValueParameterKey<PointLightData> LightsKey;

            public SpotLightShaderGroup(ShaderMixinSource mixin, string compositionName, ILightShadowMapShaderGroupData shadowGroupData)
                : base(mixin, compositionName, shadowGroupData)
            {
                CountKey = DirectLightGroupKeys.LightCount.ComposeWith(compositionName);
                LightsKey = LightPointGroupKeys.Lights.ComposeWith(compositionName);
            }

            protected override SpotLightShaderGroupData CreateData()
            {
                return new SpotLightShaderGroupData(this, ShadowGroup);
            }
        }

        class SpotLightShaderGroupData : LightShaderGroupData
        {
            private readonly ValueParameterKey<int> countKey;
            private readonly ValueParameterKey<PointLightData> lightsKey;
            private readonly PointLightData[] lights;

            public SpotLightShaderGroupData(SpotLightShaderGroup group, ILightShadowMapShaderGroupData shadowGroupData)
                : base(shadowGroupData)
            {
                countKey = group.CountKey;
                lightsKey = group.LightsKey;

                lights = new PointLightData[StaticLightMaxCount];
            }

            protected override void AddLightInternal(LightComponent light)
            {
                var pointLight = (LightPoint)light.Type;
                lights[Count] = new PointLightData
                {
                    PositionWS = light.Position,
                    InvSquareRadius = pointLight.InvSquareRadius,
                    Color = light.Color,
                };
            }

            protected override void ApplyParametersInternal(RenderDrawContext context, ParameterCollection parameters)
            {
                parameters.Set(countKey, Count);
                parameters.Set(lightsKey, Count, ref lights[0]);
            }
        }
    }
}