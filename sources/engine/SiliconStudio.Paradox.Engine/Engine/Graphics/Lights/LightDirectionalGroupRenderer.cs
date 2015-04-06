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

    public struct LightAndShadowsShaderGroup
    {
        public LightAndShadowsShaderGroup(List<LightShaderGroup> lights, List<LightShaderGroup> lightsAndShadows)
        {
            Lights = lights;
            LightsAndShadows = lightsAndShadows;
        }

        public readonly List<LightShaderGroup> Lights;

        public readonly List<LightShaderGroup> LightsAndShadows;
    }

    public interface ILightShaderGenerator
    {
        LightAndShadowsShaderGroup GenerateShaders(RenderContext context, Dictionary<LightComponent, LightShadowMapTexture> shadows);
    }

    public class LightDirectionalGroupRenderer : LightGroupRendererBase
    {
        private static readonly Type[] LightTypes = { typeof(LightDirectional) };
        public LightDirectionalGroupRenderer()
        {
            LightType = 0x1;
            LightMaxCount = 8;
        }

        public override void Initialize(RenderContext context)
        {
            var isLowProfile = context.GraphicsDevice.Features.Profile < GraphicsProfile.Level_10_0;
            LightMaxCount = isLowProfile ? 2 : 8;
            AllocateLightMaxCount = !isLowProfile;
        }

        public override bool AddLight(LightComponent light, ref FastListStruct<LightForwardShaderEntryKey> lightKeys, byte shadowType, byte shadowTextureId)
        {
            if (lightKeys.Count == 0)
            {
                lightKeys.Add(new LightForwardShaderEntryKey(LightType, shadowType.ShadowType, 1, shadowTextureId));
            }
            else
            {
                var current = lightKeys[lightKeys.Count];
                if (current.LightCount + 1 == LightMax)
                {
                    return false;
                }
                if (current.LightType == LightType && current.ShadowType = && current.ShadowTextureId)
                {
                    
                }

            }


        }

        public override ILightShaderGenerator CreateShaderGenerator(LightComponentCollection lightComponents, ILightShadowMapRenderer shadowMapRenderer)
        {
            return new LightDirectionalShaderGenerator(lightComponents, shadowMapRenderer);
        }

        private class LightDirectionalShaderGenerator : ILightShaderGenerator
        {
            private static readonly ShaderClassSource DynamicDirectionalGroupShaderSource = new ShaderClassSource("LightDirectionalGroup", LightMax);
            private static readonly ShaderSource[] FixedDirectionalGroupShaderSources;

            private readonly DirectionalLightShaderGroup allLights;
            private readonly DirectionalLightShaderGroup lightNoShadows;

            private PoolListStruct<DirectionalLightShaderGroup> lightsWithShadowsPool;
            private readonly PoolListStruct<ILightShadowMapShaderGroupData>[] lightsWithShadowsPoolCascades;

            private PoolListStruct<ShaderMixinSource> shadowMixinsPool;

            private LightAndShadowsShaderGroup result;

            private readonly LightComponentCollection lightComponents;

            private readonly ILightShadowMapRenderer shadowMapRenderer;

            private readonly List<KeyValuePair<ShadowKey, DirectionalLightShaderGroup>> shadowMapGroups;

            static LightDirectionalShaderGenerator()
            {
                // Precreate fixed lightComponents for profile < 10.0
                FixedDirectionalGroupShaderSources = new ShaderSource[LightMax];
                for (int i = 1; i < LightMax; i++)
                {
                    var mixin = new ShaderMixinSource();
                    mixin.Mixins.Add(new ShaderClassSource("LightDirectionalGroup", i));
                    mixin.Mixins.Add(new ShaderClassSource("DirectLightGroupFixed", i));
                    FixedDirectionalGroupShaderSources[i] = mixin;
                }
            }

            public LightDirectionalShaderGenerator(LightComponentCollection lightComponents, ILightShadowMapRenderer shadowMapRenderer)
            {
                if (lightComponents == null) throw new ArgumentNullException("lightComponents");
                if (shadowMapRenderer == null) throw new ArgumentNullException("shadowMapRenderer");
                this.lightComponents = lightComponents;
                this.shadowMapRenderer = shadowMapRenderer;
                allLights = new DirectionalLightShaderGroup(LightMax);
                lightNoShadows = new DirectionalLightShaderGroup(LightMax);
                shadowMixinsPool = new PoolListStruct<ShaderMixinSource>(LightMax, CreateLightShadowMixin);
                lightsWithShadowsPool = new PoolListStruct<DirectionalLightShaderGroup>();
                shadowMapGroups = new List<KeyValuePair<ShadowKey, DirectionalLightShaderGroup>>(LightMax);
                lightsWithShadowsPoolCascades = new[]
                {
                    new PoolListStruct<ILightShadowMapShaderGroupData>(LightMax, CreateDirectionalLightShaderGroupCascade1),
                    new PoolListStruct<ILightShadowMapShaderGroupData>(LightMax, CreateDirectionalLightShaderGroupCascade2),
                    new PoolListStruct<ILightShadowMapShaderGroupData>(LightMax, CreateDirectionalLightShaderGroupCascade4),
                };

                result = new LightAndShadowsShaderGroup(new List<LightShaderGroup>(LightMax), new List<LightShaderGroup>(LightMax));
            }

            private ShaderMixinSource CreateLightShadowMixin()
            {
                return new ShaderMixinSource();
            }

            public LightAndShadowsShaderGroup GenerateShaders(RenderContext context, Dictionary<LightComponent, LightShadowMapTexture> shadows)
            {
                var count = Math.Min(lightComponents.Count, LightMax);
                allLights.Count = 0;
                lightNoShadows.Count = 0;

                for (int i = 0; i < lightsWithShadowsPoolCascades.Length; i++)
                {
                    lightsWithShadowsPoolCascades[i].Clear();
                }
                lightsWithShadowsPool.Clear();
                shadowMixinsPool.Clear();
                shadowMapGroups.Clear();

                result.Lights.Clear();
                result.LightsAndShadows.Clear();

                for (int i = 0; i < count; i++)
                {
                    var lightComponent = lightComponents[i];
                    var light = (LightDirectional)lightComponent.Type;

                    var direction = lightComponent.Direction;
                    var color = light.ComputeColor(lightComponent.Intensity);

                    // Add all lightComponents
                    allLights.AddLight(direction, color);

                    DirectionalLightShaderGroup lightShaderGroup = null;

                    LightShadowMapTexture shadowMapTexture;
                    if (shadows.TryGetValue(lightComponent, out shadowMapTexture))
                    {
                        var cascadeCount = shadowMapTexture.CascadeCount;
                        var shadowKey = new ShadowKey(shadowMapTexture.Atlas.Texture, shadowMapTexture.FilterType, cascadeCount, shadowMapTexture.Shadow.Debug);

                        // Find a shadow map group that was allocated before
                        foreach (var shadowKeyAndGroup in shadowMapGroups)
                        {
                            if (shadowKeyAndGroup.Key == shadowKey)
                            {
                                lightShaderGroup = shadowKeyAndGroup.Value;
                                break;
                            }
                        }

                        if (lightShaderGroup == null)
                        {
                            lightShaderGroup = lightsWithShadowsPool.Add();
                            lightShaderGroup.ShadowGroupData = lightsWithShadowsPoolCascades[cascadeCount >> 1].Add();
                            var mixin = shadowMixinsPool.Add();
                            mixin.Mixins.Clear();
                            mixin.Mixins.Add(DynamicDirectionalGroupShaderSource);
                            lightShaderGroup.ShadowGroupData.ApplyMixin(mixin, shadowKey.IsDebug);

                            // We use a special mixin for the shadow maps
                            lightShaderGroup.ShaderSource = mixin;
                        }

                        // Setup the data for this group
                        var shadowGroup = lightShaderGroup.ShadowGroupData;
                        shadowGroup.SetShadowMapShaderData(lightShaderGroup.Count, shadowMapTexture.ShaderData);
                    }
                    else
                    {
                        lightShaderGroup = lightNoShadows;
                    }

                    lightShaderGroup.AddLight(direction, color);
                }

                allLights.ShaderSource = context.GraphicsDevice.Features.Profile < GraphicsProfile.Level_10_0 ? FixedDirectionalGroupShaderSources[count] : DynamicDirectionalGroupShaderSource;
                allLights.UpdateParameters();

                lightNoShadows.ShaderSource = context.GraphicsDevice.Features.Profile < GraphicsProfile.Level_10_0 ? FixedDirectionalGroupShaderSources[lightNoShadows.Count] : DynamicDirectionalGroupShaderSource;
                lightNoShadows.UpdateParameters();

                result.Lights.Add(allLights);
                if (lightNoShadows.Count > 0)
                {
                    result.LightsAndShadows.Add(lightNoShadows);
                }

                if (lightsWithShadowsPool.Count > 0)
                {
                    foreach (var lightWithShadow in lightsWithShadowsPool)
                    {
                        lightWithShadow.UpdateParameters();
                        result.LightsAndShadows.Add(lightWithShadow);
                    }
                }

                return result;
            }
            private ILightShadowMapShaderGroupData CreateDirectionalLightShaderGroupCascade1()
            {
                return shadowMapRenderer.CreateShaderGroupData(1, LightMax);
            }

            private ILightShadowMapShaderGroupData CreateDirectionalLightShaderGroupCascade2()
            {
                return shadowMapRenderer.CreateShaderGroupData(2, LightMax);
            }

            private ILightShadowMapShaderGroupData CreateDirectionalLightShaderGroupCascade4()
            {
                return shadowMapRenderer.CreateShaderGroupData(4, LightMax);
            }

            class DirectionalLightShaderGroup : LightShaderGroup
            {
                public DirectionalLightShaderGroup(int size)
                {
                    LightDirections = new Vector3[size];
                    LightColors = new Color3[size];
                    ShaderSource = null;
                }

                public readonly Vector3[] LightDirections;

                public readonly Color3[] LightColors;

                public int Count;

                public ILightShadowMapShaderGroupData ShadowGroupData;

                public void AddLight(Vector3 direction, Color3 color)
                {
                    LightDirections[Count] = direction;
                    LightColors[Count] = color;
                    Count++;
                }

                public void UpdateParameters()
                {
                    Parameters.Set(DirectLightGroupKeys.LightCount, Count);
                    Parameters.Set(LightDirectionalGroupKeys.LightDirectionsWS, LightDirections);
                    Parameters.Set(LightDirectionalGroupKeys.LightColor, LightColors);

                    if (ShadowGroupData != null)
                    {
                        ShadowGroupData.ApplyParameters(Parameters);
                    }
                }
            }

            struct ShadowKey : IEquatable<ShadowKey>
            {
                public ShadowKey(Texture texture, Type filterType, int cascadeCount, bool isDebug)
                {
                    Texture = texture;
                    FilterType = filterType;
                    CascadeCount = cascadeCount;
                    IsDebug = isDebug;
                }

                public readonly Texture Texture;

                public readonly Type FilterType;

                public readonly int CascadeCount;

                public readonly bool IsDebug;

                public bool Equals(ShadowKey other)
                {
                    return ReferenceEquals(Texture, other.Texture) && FilterType == other.FilterType && CascadeCount == other.CascadeCount && IsDebug.Equals(other.IsDebug);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    return obj is ShadowKey && Equals((ShadowKey)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hashCode = Texture.GetHashCode();
                        hashCode = (hashCode * 397) ^ CascadeCount;
                        hashCode = (hashCode * 397) ^ FilterType.GetHashCode();
                        hashCode = (hashCode * 397) ^ IsDebug.GetHashCode();
                        return hashCode;
                    }
                }

                public static bool operator ==(ShadowKey left, ShadowKey right)
                {
                    return left.Equals(right);
                }

                public static bool operator !=(ShadowKey left, ShadowKey right)
                {
                    return !left.Equals(right);
                }
            }
        }
    }
}