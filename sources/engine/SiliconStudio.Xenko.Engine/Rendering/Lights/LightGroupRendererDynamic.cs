using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// A light group renderer that can handle a varying number of lights, i.e. point, spots, directional.
    /// </summary>
    public abstract class LightGroupRendererDynamic : LightGroupRendererBase
    {
        private readonly ShadowComparer shadowComparer = new ShadowComparer();

        private readonly Dictionary<LightGroupKey, LightShaderGroupDynamic> lightShaderGroupPool = new Dictionary<LightGroupKey, LightShaderGroupDynamic>();
        private readonly FastList<LightShaderGroupEntry> lightShaderGroups = new FastList<LightShaderGroupEntry>();

        private FastListStruct<LightDynamicEntry> processedLights = new FastListStruct<LightDynamicEntry>(8);

        public abstract LightShaderGroupDynamic CreateLightShaderGroup(RenderDrawContext context, ILightShadowMapShaderGroupData shadowGroup);

        /// <summary>
        /// Next light renderer we can send our unprocessed lights to.
        /// </summary>
        public LightGroupRendererBase NonShadowRenderer { get; set; }

        public override void Initialize(RenderContext context)
        {
            base.Initialize(context);

            NonShadowRenderer?.Initialize(context);
        }

        public override void Reset()
        {
            base.Reset();

            lightShaderGroups.Clear();

            foreach (var lightShaderGroup in lightShaderGroupPool)
            {
                lightShaderGroup.Value.Reset();
            }

            NonShadowRenderer?.Reset();
        }

        public override void SetViews(FastList<RenderView> views)
        {
            base.SetViews(views);

            foreach (var lightShaderGroup in lightShaderGroupPool)
            {
                lightShaderGroup.Value.SetViews(views);
            }

            NonShadowRenderer?.SetViews(views);
        }

        public override void ProcessLights(ProcessLightsParameters parameters)
        {
            if (parameters.LightCollection.Count == 0)
                return;

            ILightShadowMapRenderer currentShadowRenderer = null;
            LightShadowType currentShadowType = 0;

            // Start by filtering/sorting what can be processed
            shadowComparer.ShadowMapTexturesPerLight = parameters.ShadowMapTexturesPerLight;
            parameters.LightCollection.Sort(parameters.LightStart, parameters.LightEnd - parameters.LightStart, shadowComparer);

            for (int index = parameters.LightStart; index <= parameters.LightEnd; index++)
            {
                LightShadowType nextShadowType = 0;
                ILightShadowMapRenderer nextShadowRenderer = null;

                LightShadowMapTexture nextShadowTexture = null;
                LightComponent nextLight = null;
                if (index < parameters.LightEnd)
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
                if (index == parameters.LightEnd || currentShadowType != nextShadowType || currentShadowRenderer != nextShadowRenderer)
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
                            lightShaderGroup.SetViews(parameters.Views);

                            lightShaderGroupPool.Add(lightGroupKey, lightShaderGroup);
                        }

                        // Add view and lights
                        var allowedLightCount = lightShaderGroup.AddView(parameters.ViewIndex, parameters.View, processedLights.Count);
                        for (int i = 0; i < allowedLightCount; ++i)
                        {
                            var light = processedLights[i];
                            lightShaderGroup.AddLight(light.Light, light.ShadowMapTexture);
                        }

                        // TODO: assign extra lights to non-shadow rendering if possible
                        //for (int i = lightCount; i < processedLights.Count; ++i)
                        //    XXX.AddLight(processedLights[i], null);

                        var lightShaderGroupEntry = new LightShaderGroupEntry(lightGroupKey, lightShaderGroup);
                        if (!lightShaderGroups.Contains(lightShaderGroupEntry))
                            lightShaderGroups.Add(lightShaderGroupEntry);

                        processedLights.Clear();
                    }

                    // Start next group
                    currentShadowType = nextShadowType;
                    currentShadowRenderer = nextShadowRenderer;
                }

                if (index < parameters.LightEnd)
                {
                    // Do we need to process non shadowing lights or defer it to something else?
                    if (nextShadowTexture == null && NonShadowRenderer != null)
                    {
                        parameters.LightStart = index;
                        NonShadowRenderer.ProcessLights(parameters);
                        break;
                    }

                    processedLights.Add(new LightDynamicEntry(nextLight, nextShadowTexture));
                }
            }

            processedLights.Clear();
        }

        public override void UpdateShaderPermutationEntry(ForwardLightingRenderFeature.LightShaderPermutationEntry shaderEntry)
        {
            // Sort to make sure we generate the same permutations
            lightShaderGroups.Sort(LightShaderGroupComparer.Default);

            foreach (var lightShaderGroup in lightShaderGroups)
            {
                if (IsEnvironmentLight)
                    shaderEntry.EnvironmentLights.Add(lightShaderGroup.Value);
                else
                    shaderEntry.DirectLightGroups.Add(lightShaderGroup.Value);
            }

            NonShadowRenderer?.UpdateShaderPermutationEntry(shaderEntry);
        }

        class LightShaderGroupComparer : Comparer<LightShaderGroupEntry>
        {
            public new static readonly LightShaderGroupComparer Default = new LightShaderGroupComparer();

            public override int Compare(LightShaderGroupEntry x, LightShaderGroupEntry y)
            {
                var compareRenderer = (x.Key.ShadowRenderer != null).CompareTo(y.Key.ShadowRenderer != null);
                if (compareRenderer != 0)
                    return compareRenderer;

                return ((int)x.Key.ShadowType).CompareTo((int)y.Key.ShadowType);
            }
        }

        private struct LightShaderGroupEntry : IEquatable<LightShaderGroupEntry>
        {
            public readonly LightGroupKey Key;
            public readonly LightShaderGroup Value;

            public LightShaderGroupEntry(LightGroupKey key, LightShaderGroup value)
            {
                Key = key;
                Value = value;
            }

            public bool Equals(LightShaderGroupEntry other)
            {
                return Key.Equals(other.Key) && Value.Equals(other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is LightShaderGroupEntry && Equals((LightShaderGroupEntry)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Key.GetHashCode()*397) ^ Value.GetHashCode();
                }
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

        class ShadowComparer : IComparer<LightComponent>
        {
            public Dictionary<LightComponent, LightShadowMapTexture> ShadowMapTexturesPerLight;

            public int Compare(LightComponent x, LightComponent y)
            {
                LightShadowMapTexture shadowX, shadowY;

                ShadowMapTexturesPerLight.TryGetValue(x, out shadowX);
                ShadowMapTexturesPerLight.TryGetValue(y, out shadowY);

                var shadowTypeX = shadowX?.ShadowType ?? 0;
                var shadowTypeY = shadowY?.ShadowType ?? 0;

                // Decreasing order so that non shadowed lights are last
                return ((int)shadowTypeY).CompareTo((int)shadowTypeX);
            }
        }
    }
}