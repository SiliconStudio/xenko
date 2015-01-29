using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// TODO: Evaluate if it would be possible to split this class with support for different lights instead of a big fat class
    /// TODO: Refactor this class
    /// </summary>
    public abstract class DirectLightRenderProcessorBase
    {
        private readonly System.Collections.Generic.List<KeyValuePair<Type, DirectLightGroupRenderProcessorBase>> processors;
        private readonly LightProcessor lightProcessor;

        private readonly List<ShaderSource> previousShaderSources;
        private readonly List<ShaderSource> shaderSources;
        private readonly Dictionary<ParameterCompositeKey, ParameterKey> compositeKeys;

        protected DirectLightRenderProcessorBase(ModelRenderer modelRenderer)
        {
            if (modelRenderer == null) throw new ArgumentNullException("modelRenderer");
            Enabled = true;
            Services = modelRenderer.Services;
            EntitySystem = Services.GetServiceAs<EntitySystem>();
            lightProcessor = EntitySystem.GetProcessor<LightProcessor>();
            previousShaderSources = new List<ShaderSource>();
            shaderSources = new List<ShaderSource>();
            compositeKeys = new Dictionary<ParameterCompositeKey, ParameterKey>();
            processors = new System.Collections.Generic.List<KeyValuePair<Type, DirectLightGroupRenderProcessorBase>>();

            // Register this render processor
            // TODO: Allow to remove it
            modelRenderer.PreRender.Add(this.PreRender);
        }

        public IServiceRegistry Services { get; private set; }

        public bool Enabled { get; set; }

        private EntitySystem EntitySystem { get;  set; }

        public void RegisterLightGroupProcessor<T>(DirectLightGroupRenderProcessorBase processor)
        {
            if (processor == null) throw new ArgumentNullException("processor");
            processors.Add(new KeyValuePair<Type, DirectLightGroupRenderProcessorBase>(typeof(T), processor));
        }

        /// <summary>
        /// Filter out the inactive lights.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PreRender(RenderContext context)
        {
            var passParameters = context.CurrentPass.Parameters;

            var activeLights = lightProcessor.ActiveLights;
            shaderSources.Clear();

            if (Enabled)
            {
                int lightChangeCount = 0;
                foreach (var lightTypeProcessor in processors)
                {
                    var lightType = lightTypeProcessor.Key;
                    var lightRenderProcessor = lightTypeProcessor.Value;

                    lightChangeCount += PrepareLights(context, passParameters, lightRenderProcessor, lightType, activeLights);
                    // TODO: Handle lights with shadows
                }

                // TODO: Support for RenderLayer (we have to move this per mesh instead), optimize for the case all renderlayers are affected

                // If there is any changes in light groups, then set this in the context
                if (lightChangeCount > 0 || shaderSources.Count != previousShaderSources.Count)
                {
                    passParameters.Set(LightingKeys.DirectLightGroups, shaderSources.ToArray());
                }
            }
            else
            {
                // Disable completely direct light groups
                if (passParameters.Get(LightingKeys.DirectLightGroups) != null)
                {
                    passParameters.Set(LightingKeys.DirectLightGroups, null);
                }
            }

            previousShaderSources.Clear();
            previousShaderSources.AddRange(shaderSources);
            shaderSources.Clear();
        }

        private int PrepareLights(RenderContext context, ParameterCollection passParameters, DirectLightGroupRenderProcessorBase lightGroupProcessor, Type lightType, Dictionary<Type, LightComponentCollection> lightsPerType)
        {
            int changed = 0;
            LightComponentCollection lights;
            if (lightsPerType.TryGetValue(lightType, out lights))
            {
                if (lights.Count > 0)
                {
                    var shaderSource = lightGroupProcessor.PrepareLights(context, lights);
                    var lightGroupIndex = shaderSources.Count;
                    if (lightGroupIndex >= previousShaderSources.Count || !previousShaderSources[lightGroupIndex].Equals(shaderSource))
                    {
                        changed = 1;
                    }

                    // Copy generate parameters for this light group to the current parameter of the pass
                    // TODO: When doing this at mesh level (in case of render layers, we should to this in a different pass)
                    // TODO: This code is duplicated with in ColorTransformGroup, check how to extract a common behavior
                    var sourceParameters = lightGroupProcessor.Parameters;
                    foreach (var parameterValue in sourceParameters)
                    {
                        var key = GetComposedKey(parameterValue.Key, lightGroupIndex);
                        sourceParameters.CopySharedTo(parameterValue.Key, key, passParameters);
                    }

                    // Set the light count
                    // TODO: We could pregenerate/cache DirectLightGroupKeys.LightCount
                    passParameters.Set((ParameterKey<int>)GetComposedKey(DirectLightGroupKeys.LightCount, lightGroupIndex), lights.Count);

                    // Add shaderSource
                    shaderSources.Add(shaderSource);
                }
            }
            return changed;
        }

        private ParameterKey GetComposedKey(ParameterKey key, int lightGroupIndex)
        {
            var compositeKey = new ParameterCompositeKey(key, lightGroupIndex);

            ParameterKey rawCompositeKey;
            if (!compositeKeys.TryGetValue(compositeKey, out rawCompositeKey))
            {
                rawCompositeKey = ParameterKeys.FindByName(string.Format("{0}.directLightGroups[{1}]", key.Name, lightGroupIndex));
                compositeKeys.Add(compositeKey, rawCompositeKey);
            }
            return rawCompositeKey;
        }

        /// <summary>
        /// Clear the light lists.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PostRender(RenderContext context)
        {
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