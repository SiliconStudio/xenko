// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public abstract class LightGroupRendererBase
    {
        private static readonly Dictionary<Type, int> LightRendererIds = new Dictionary<Type, int>();

        protected LightGroupRendererBase()
        {
            int lightRendererId;
            lock (LightRendererIds)
            {
                if (!LightRendererIds.TryGetValue(GetType(), out lightRendererId))
                {
                    lightRendererId = LightRendererIds.Count + 1;
                    LightRendererIds.Add(GetType(), lightRendererId);
                }
            }

            LightRendererId = (byte)lightRendererId;
        }

        public bool IsEnvironmentLight { get; protected set; }

        public byte LightRendererId { get; private set; }

        public virtual void Initialize(RenderContext context)
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void SetViewCount(int viewCount)
        {
            
        }

        public abstract void ProcessLights(ProcessLightsParameters parameters);

        public struct ProcessLightsParameters
        {
            public RenderDrawContext Context;

            public int ViewIndex;
            public int ViewCount;

            public ForwardLightingRenderFeature.LightShaderPermutationEntry ShaderEntry;

            public LightComponentCollection LightCollection;

            public ShadowMapRenderer ShadowMapRenderer;

            public Dictionary<LightComponent, LightShadowMapTexture> ShadowMapTexturesPerLight;
        }
    }

    public abstract class LightGroupRendererDynamic : LightGroupRendererBase
    {
        private readonly Dictionary<LightGroupKey, LightShaderGroupDynamic> lightShaderGroupPool = new Dictionary<LightGroupKey, LightShaderGroupDynamic>();

        private FastListStruct<LightDynamicEntry> processedLights = new FastListStruct<LightDynamicEntry>(8);

        public abstract LightShaderGroupDynamic CreateLightShaderGroup(RenderDrawContext context, ILightShadowMapShaderGroupData shadowGroup);

        public override void Reset()
        {
            base.Reset();

            foreach (var lightShaderGroup in lightShaderGroupPool)
            {
                lightShaderGroup.Value.Reset();
            }
        }

        public override void SetViewCount(int viewCount)
        {
            base.SetViewCount(viewCount);

            foreach (var lightShaderGroup in lightShaderGroupPool)
            {
                lightShaderGroup.Value.SetViewCount(viewCount);
            }
        }

        public override void ProcessLights(ProcessLightsParameters parameters)
        {
            if (parameters.LightCollection.Count == 0)
                return;

            ILightShadowMapRenderer currentShadowRenderer = null;
            LightShadowType currentShadowType = 0;

            for (int index = 0; index <= parameters.LightCollection.Count; index++)
            {
                LightShadowType nextShadowType = 0;
                ILightShadowMapRenderer nextShadowRenderer = null;

                LightShadowMapTexture nextShadowTexture = null;
                LightComponent nextLight = null;
                if (index < parameters.LightCollection.Count)
                {
                    nextLight = parameters.LightCollection[index];

                    if (parameters.ShadowMapRenderer != null
                        && parameters.ShadowMapTexturesPerLight.TryGetValue(nextLight, out nextShadowTexture)
                        && nextShadowTexture.Atlas != null) // atlas could not be allocated? treat it as a non-shadowed texture
                    {
                        nextShadowType = nextShadowTexture.ShadowType;
                        nextShadowRenderer = nextShadowTexture.Renderer;
                    }
                }

                // Flush current group
                if (index == parameters.LightCollection.Count || currentShadowType != nextShadowType || currentShadowRenderer != nextShadowRenderer)
                {
                    if (processedLights.Count > 0)
                    {
                        var lightGroupKey = new LightGroupKey(currentShadowRenderer, currentShadowType);
                        LightShaderGroupDynamic lightShaderGroup;
                        if (!lightShaderGroupPool.TryGetValue(lightGroupKey, out lightShaderGroup))
                        {
                            ILightShadowMapShaderGroupData shadowGroupData = null;
                            if (currentShadowRenderer != null)
                            {
                                shadowGroupData = currentShadowRenderer.CreateShaderGroupData(currentShadowType);
                            }

                            lightShaderGroup = CreateLightShaderGroup(parameters.Context, shadowGroupData);
                            lightShaderGroup.SetViewCount(parameters.ViewCount);

                            lightShaderGroupPool.Add(lightGroupKey, lightShaderGroup);
                        }

                        // Add view and lights
                        var allowedLightCount = lightShaderGroup.AddView(parameters.ViewIndex, processedLights.Count);
                        for (int i = 0; i < allowedLightCount; ++i)
                        {
                            var light = processedLights[i];
                            lightShaderGroup.AddLight(light.Light, light.ShadowMapTexture);
                        }

                        // TODO: assign extra lights to non-shadow rendering if possible
                        //for (int i = lightCount; i < processedLights.Count; ++i)
                        //    XXX.AddLight(processedLights[i], null);

                        if (IsEnvironmentLight)
                            parameters.ShaderEntry.AddEnvironmentLightGroup(lightShaderGroup);
                        else
                            parameters.ShaderEntry.AddDirectLightGroup(lightShaderGroup);

                        processedLights.Clear();
                    }

                    // Start next group
                    currentShadowType = nextShadowType;
                    currentShadowRenderer = nextShadowRenderer;
                }

                if (index < parameters.LightCollection.Count)
                    processedLights.Add(new LightDynamicEntry(nextLight, nextShadowTexture));
            }
        }

        private struct LightGroupKey : IEquatable<LightGroupKey>
        {
            public readonly ILightShadowMapRenderer ShadowRenderer;

            public readonly LightShadowType ShadowType;

            public LightGroupKey(ILightShadowMapRenderer shadowRenderer, LightShadowType shadowType)
            {
                ShadowRenderer = shadowRenderer;
                ShadowType = shadowType;
            }

            public bool Equals(LightGroupKey other)
            {
                return Equals(ShadowRenderer, other.ShadowRenderer) && ShadowType == other.ShadowType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is LightGroupKey && Equals((LightGroupKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = ShadowRenderer != null ? ShadowRenderer.GetHashCode() : 0;
                    hashCode = (hashCode*397) ^ (int)ShadowType;
                    return hashCode;
                }
            }

            public override string ToString()
            {
                return $"Lights with shadow type [{ShadowType}]";
            }
        }
    }
}