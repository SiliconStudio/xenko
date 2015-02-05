// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// TODO: Refactor this class
    /// </summary>
    public abstract class LightModelRendererBase
    {
        private readonly LightGroupProcessor directLightGroup;
        private readonly LightGroupProcessor environmentLightGroup;

        private readonly LightProcessor lightProcessor;

        protected LightModelRendererBase(ModelRenderer modelRenderer)
        {
            if (modelRenderer == null) throw new ArgumentNullException("modelRenderer");
            Enabled = true;
            Services = modelRenderer.Services;
            EntitySystem = Services.GetServiceAs<EntitySystem>();
            lightProcessor = EntitySystem.GetProcessor<LightProcessor>();

            directLightGroup = new LightGroupProcessor("directLightGroups", LightingKeys.DirectLightGroups);
            environmentLightGroup = new LightGroupProcessor("environmentLights", LightingKeys.EnvironmentLights);

            // Register this render processor
            // TODO: Allow to remove it
            modelRenderer.PreRender.Add(this.PreRender);
        }

        public IServiceRegistry Services { get; private set; }

        public bool Enabled { get; set; }

        private EntitySystem EntitySystem { get;  set; }

        public void RegisterLightGroupProcessor<T>(LightGroupRendererBase processor)
        {
            if (processor == null) throw new ArgumentNullException("processor");
            var group = processor.IsDirectLight ? directLightGroup : environmentLightGroup;
            group.Processors.Add(new KeyValuePair<Type, LightGroupRendererBase>(typeof(T), processor));
        }

        /// <summary>
        /// Filter out the inactive lights.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PreRender(RenderContext context)
        {
            directLightGroup.ProcessLights(context, lightProcessor.ActiveDirectLights, Enabled);
            environmentLightGroup.ProcessLights(context, lightProcessor.ActiveEnvironmentLights, Enabled);
        }

        /// <summary>
        /// Clear the light lists.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PostRender(RenderContext context)
        {
        }

        /// <summary>
        /// Code to process a group of light (either Direct or Environment)
        /// </summary>
        private class LightGroupProcessor
        {
            private readonly Dictionary<ParameterCompositeKey, ParameterKey> compositeKeys;

            public LightGroupProcessor(string compositionName, ParameterKey<ShaderSource[]> shadersKey)
            {
                if (compositionName == null) throw new ArgumentNullException("compositionName");
                if (shadersKey == null) throw new ArgumentNullException("shadersKey");
                this.compositeKeys = new Dictionary<ParameterCompositeKey, ParameterKey>();
                this.compositionName = compositionName;
                this.shadersKey = shadersKey;
                Processors = new List<KeyValuePair<Type, LightGroupRendererBase>>();
                PreviousShaderSources = new List<ShaderSource>();
                CurrentShaderSources = new List<ShaderSource>();
            }

            private readonly string compositionName;

            private readonly ParameterKey<ShaderSource[]> shadersKey;

            public readonly List<KeyValuePair<Type, LightGroupRendererBase>> Processors;

            public readonly List<ShaderSource> PreviousShaderSources;

            public readonly List<ShaderSource> CurrentShaderSources;

            public void ProcessLights(RenderContext context, Dictionary<Type, LightComponentCollection> activeLights, bool enabled)
            {
                var passParameters = context.CurrentPass.Parameters;

                CurrentShaderSources.Clear();

                if (enabled)
                {
                    foreach (var lightTypeProcessor in Processors)
                    {
                        var lightType = lightTypeProcessor.Key;
                        var lightRenderProcessor = lightTypeProcessor.Value;

                        PrepareLights(context, passParameters, lightRenderProcessor, lightType, activeLights);
                    }

                    // TODO: Support for RenderLayer (we have to move this per mesh instead), optimize for the case all renderlayers are affected

                    // If there is any changes in light groups, then set this in the context
                    if (CurrentShaderSources.Count != PreviousShaderSources.Count || !Utilities.Compare(PreviousShaderSources, CurrentShaderSources))
                    {
                        passParameters.Set(shadersKey, CurrentShaderSources.ToArray());
                    }
                }
                else
                {
                    // Disable completely light groups
                    if (passParameters.Get(shadersKey) != null)
                    {
                        passParameters.Set(shadersKey, null);
                    }
                }

                PreviousShaderSources.Clear();
                PreviousShaderSources.AddRange(CurrentShaderSources);
            }

            private ParameterKey GetComposedKey(ParameterKey key, int lightGroupIndex)
            {
                var compositeKey = new ParameterCompositeKey(key, lightGroupIndex);

                ParameterKey rawCompositeKey;
                if (!compositeKeys.TryGetValue(compositeKey, out rawCompositeKey))
                {
                    rawCompositeKey = ParameterKeys.FindByName(string.Format("{0}.{1}[{2}]", key.Name, compositionName, lightGroupIndex));
                    compositeKeys.Add(compositeKey, rawCompositeKey);
                }
                return rawCompositeKey;
            }

            private void PrepareLights(RenderContext context, ParameterCollection passParameters, LightGroupRendererBase lightGroupProcessor, Type lightType, Dictionary<Type, LightComponentCollection> lightsPerType)
            {
                LightComponentCollection lights;
                if (lightsPerType.TryGetValue(lightType, out lights))
                {
                    if (lights.Count > 0)
                    {
                        var lightShaderGroups = lightGroupProcessor.PrepareLights(context, lights);

                        foreach (var lightShaderGroup in lightShaderGroups)
                        {
                            // Copy generate parameters for this light group to the current parameter of the pass
                            // TODO: When doing this at mesh level (in case of render layers, we should to this in a different pass)
                            // TODO: This code is duplicated with in ColorTransformGroup, check how to extract a common behavior
                            var sourceParameters = lightShaderGroup.Parameters;
                            int lightGroupIndex = CurrentShaderSources.Count;
                            foreach (var parameterValue in sourceParameters)
                            {
                                var key = GetComposedKey(parameterValue.Key, lightGroupIndex);
                                sourceParameters.CopySharedTo(parameterValue.Key, key, passParameters);
                            }

                            // Add shaderSource
                            CurrentShaderSources.Add(lightShaderGroup.ShaderSource);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// An internal key to cache {Key,TransformIndex} => CompositeKey
        /// </summary>
        private struct ParameterCompositeKey : IEquatable<ParameterCompositeKey>
        {
            private readonly ParameterKey key;

            private readonly int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterCompositeKey"/> struct.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="transformIndex">Index of the transform.</param>
            public ParameterCompositeKey(ParameterKey key, int transformIndex)
            {
                if (key == null) throw new ArgumentNullException("key");
                this.key = key;
                index = transformIndex;
            }

            public bool Equals(ParameterCompositeKey other)
            {
                return key.Equals(other.key) && index == other.index;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ParameterCompositeKey && Equals((ParameterCompositeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (key.GetHashCode() * 397) ^ index;
                }
            }
        }
    }
}