// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Assets;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    // TODO REWRITE AND COMMENT THIS CLASS
    public class MaterialGeneratorContext : ShaderGeneratorContextBase
    {
        private readonly Dictionary<string, ShaderSource> registeredStreamBlend = new Dictionary<string, ShaderSource>();
        private int shadingModelCount;
        private MaterialOverrides currentOverrides;

        private readonly Dictionary<KeyValuePair<MaterialShaderStage, Type>, ShaderSource> inputStreamModifiers = new Dictionary<KeyValuePair<MaterialShaderStage, Type>, ShaderSource>();

        public delegate void MaterialGeneratorCallback(MaterialShaderStage stage, MaterialGeneratorContext context);
        private readonly Dictionary<MaterialShaderStage, List<MaterialGeneratorCallback>> finalCallbacks = new Dictionary<MaterialShaderStage, List<MaterialGeneratorCallback>>();

        private readonly Material material;

        private Stack<MaterialOverrides> overridesStack = new Stack<MaterialOverrides>();

        private Stack<IMaterialDescriptor> materialStack = new Stack<IMaterialDescriptor>();

        public MaterialGeneratorContext(Material material = null)
        {
            this.material = material ?? new Material();
            Parameters = this.material.Parameters;

            currentOverrides = new MaterialOverrides();

            foreach (MaterialShaderStage stage in Enum.GetValues(typeof(MaterialShaderStage)))
            {
                finalCallbacks[stage] = new List<MaterialGeneratorCallback>();
            }

            // By default return the asset
            FindAsset = asset => ((Material)asset).Descriptor;
            GetAssetFriendlyName = asset => ((Material)asset).Descriptor != null ? ((Material)asset).Descriptor.MaterialId.ToString() : string.Empty;
        }

        public Dictionary<MaterialShaderStage, HashSet<string>> Streams
        {
            get
            {
                return Current.Streams;
            }
        }

        public Material Material
        {
            get
            {
                return material;
            }
        }

        public ColorSpace ColorSpace { get; set; }

        private MaterialBlendLayerNode Current { get; set; }

        private MaterialShadingModelCollection CurrentShadingModel { get; set; }

        public bool IsNotPixelStage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether materials will be optimized (textures blended together, generate optimized shader permutations, etc...).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [materials are optimized]; otherwise, <c>false</c>.
        /// </value>
        public bool OptimizeMaterials { get; set; }

        public void AddFinalCallback(MaterialShaderStage stage, MaterialGeneratorCallback callback)
        {
            finalCallbacks[stage].Add(callback);
        }

        public void SetStreamFinalModifier<T>(MaterialShaderStage stage, ShaderSource shaderSource)
        {
            if (shaderSource == null)
                return;

            var typeT = typeof(T);
            inputStreamModifiers[new KeyValuePair<MaterialShaderStage, Type>(stage, typeT)] = shaderSource;
        }

        public ShaderSource GetStreamFinalModifier<T>(MaterialShaderStage stage)
        {
            ShaderSource shaderSource = null;
            inputStreamModifiers.TryGetValue(new KeyValuePair<MaterialShaderStage, Type>(stage, typeof(T)), out shaderSource);
            return shaderSource;
        }

        public void PushOverrides(MaterialOverrides overrides)
        {
            if (overrides == null) throw new ArgumentNullException("overrides");
            overridesStack.Push(overrides);
            UpdateOverrides();
        }

        public void PopOverrides()
        {
            overridesStack.Pop();
            UpdateOverrides();
        }

        private void UpdateOverrides()
        {
            // Update overrides by squashing them using multiplication
            currentOverrides = new MaterialOverrides();
            foreach (var current in overridesStack)
            {
                currentOverrides *= current;
            }
        }

        public void PushLayer()
        {
            var newLayer = new MaterialBlendLayerNode(this, Current);
            if (Current != null)
            {
                Current.Children.Add(newLayer);
            }
            Current = newLayer;
        }

        public MaterialOverrides CurrentOverrides
        {
            get
            {
                return currentOverrides;
            }
        }

        /// <summary>
        /// Push a material for processing.
        /// </summary>
        /// <param name="materialDescriptor">The material descriptor.</param>
        /// <param name="materialName">Friendly name of the material.</param>
        /// <returns><c>true</c> if the material is valid and can be visited, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">materialDescriptor</exception>
        public bool PushMaterial(IMaterialDescriptor materialDescriptor, string materialName)
        {
            if (materialDescriptor == null) throw new ArgumentNullException("materialDescriptor");
            bool hasErrors = false;
            foreach (var previousMaterial in materialStack)
            {
                if (ReferenceEquals(previousMaterial, materialDescriptor) || previousMaterial.MaterialId == materialDescriptor.MaterialId)
                {
                    Log.Error("The material [{0}] cannot be used recursively.", materialName);
                    hasErrors = true;
                }
            }

            if (!hasErrors)
            {
                materialStack.Push(materialDescriptor);
            }

            return !hasErrors;
        }

        public IMaterialDescriptor PopMaterial()
        {
            if (materialStack.Count == 0)
            {
                throw new InvalidOperationException("Cannot PopMaterial more than PushMaterial");
            }
            return materialStack.Pop();
        }

        public void PopLayer()
        {
            // If current shading model is not set, 
            if (CurrentShadingModel == null && Current.ShadingModels.Count > 0)
            {
                CurrentShadingModel = Current.ShadingModels;
            }

            var sameShadingModel = Current.ShadingModels.Equals(CurrentShadingModel);
            if (!sameShadingModel)
            {
                shadingModelCount++;
            }
            
            var shouldBlendShadingModels = CurrentShadingModel != null && (!sameShadingModel || Current.Parent == null); // current shading model different from next shading model

            // --------------------------------------------------------------------
            // Copy the shading surfaces and the stream initializer to the parent.
            // --------------------------------------------------------------------
            if (Current.Parent != null)
            {
                foreach (MaterialShaderStage stage in Enum.GetValues(typeof(MaterialShaderStage)))
                {
                    // the Initializers
                    Current.Parent.StreamInitializers[stage].AddRange(Current.StreamInitializers[stage]);

                    // skip pixel shader if shading model need to be blended
                    if (stage == MaterialShaderStage.Pixel && shouldBlendShadingModels)
                        continue;

                    // the surface shaders
                    Current.Parent.SurfaceShaders[stage].AddRange(Current.SurfaceShaders[stage]);
                }
            }

            // -------------------------------------------------
            // Blend shading models between layers if necessary
            // -------------------------------------------------
            if (shouldBlendShadingModels)
            {
                var shadingSources = CurrentShadingModel.Generate(this);

                // If we are in a multi-shading-blending, only blend shading models after 1st shading model
                if (shadingModelCount > 1)
                {
                    var shaderBlendingSource = new ShaderMixinSource();
                    shaderBlendingSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceBlendShading"));

                    foreach (var shaderSource in shadingSources)
                    {
                        shaderBlendingSource.AddCompositionToArray("layers", shaderSource);
                    }

                    shadingSources = new List<ShaderSource>() { shaderBlendingSource };
                }

                var currentOrParentLayer = Current.Parent ?? Current;
                foreach (var shaderSource in shadingSources)
                {
                    currentOrParentLayer.SurfaceShaders[MaterialShaderStage.Pixel].Add(shaderSource);
                }
            }

            // In case of the root material, add all stream modifiers just at the end and call final callbacks
            if (Current.Parent == null)
            {
                foreach (var modifierKey in inputStreamModifiers.Keys)
                {
                    Current.SurfaceShaders[modifierKey.Key].Add(inputStreamModifiers[modifierKey]);
                }

                // Clear final callback
                foreach (var callbackKeyPair in finalCallbacks)
                {
                    var stage = callbackKeyPair.Key;
                    var callbacks = callbackKeyPair.Value;
                    foreach (var callback in callbacks)
                    {
                        callback(stage, this);
                    }
                    callbacks.Clear();
                }
            }

            // ----------------------------------------------
            // Pop to Parent and set Current
            // ----------------------------------------------
            if (Current.Parent != null && !sameShadingModel)
            {
                CurrentShadingModel = Current.ShadingModels;
            }

            if (Current.Parent != null)
            {
                Current = Current.Parent;
            }
        }

        public void ResetSurfaceShaders(MaterialShaderStage stage)
        {
            Current.GetSurfaceShaders(stage).Clear();
        }

        public void AddSurfaceShader(MaterialShaderStage stage, ShaderSource shaderSource)
        {
            if (shaderSource == null) throw new ArgumentNullException("shaderSource");
            Current.GetSurfaceShaders(stage).Add(shaderSource);
        }

        // TODO: move this method to an extension method
        public ParameterKey<Texture> GetTextureKey(ComputeTextureBase computeTexture, MaterialComputeColorKeys baseKeys)
        {
            var keyResolved = (ParameterKey<Texture>)(computeTexture.Key ?? baseKeys.TextureBaseKey ?? MaterialKeys.GenericTexture);
            return GetTextureKey(computeTexture.Texture, keyResolved, baseKeys.DefaultTextureValue);
        }

        public ParameterKey<SamplerState> GetSamplerKey(ComputeColorParameterSampler sampler)
        {
            if (sampler == null) throw new ArgumentNullException("sampler");

            var samplerStateDesc = new SamplerStateDescription(sampler.Filtering, sampler.AddressModeU)
            {
                AddressV = sampler.AddressModeV,
                AddressW = TextureAddressMode.Wrap
            };
            return GetSamplerKey(samplerStateDesc);
        }

        public void Visit(IMaterialFeature feature)
        {
            if (feature != null)
            {
                feature.Visit(this);
            }
        }

        public bool HasSurfaceShaders(MaterialShaderStage stage)
        {
            return Current.GetSurfaceShaders(stage).Count > 0;
        }

        public ShaderSource GenerateSurfaceShader(MaterialShaderStage stage)
        {
            return Current.GenerateSurfaceShader(stage);
        }

        public ShaderSource GenerateStreamInitializer(MaterialShaderStage stage)
        {
            return Current.GenerateStreamInilizer(stage);
        }

        public void UseStream(MaterialShaderStage stage, string stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            Current.Streams[stage].Add(stream);
        }

        public ShaderSource GetStreamBlendShaderSource(string stream)
        {
            ShaderSource shaderSource;
            registeredStreamBlend.TryGetValue(stream, out shaderSource);
            return shaderSource ?? new ShaderClassSource("MaterialStreamLinearBlend", stream);
        }

        public void UseStreamWithCustomBlend(MaterialShaderStage stage, string stream, ShaderSource blendStream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            UseStream(stage, stream);
            registeredStreamBlend[stream] = blendStream;
        }

        public void AddStreamInitializer(MaterialShaderStage stage, string streamInitilizerSource)
        {
            Current.StreamInitializers[stage].Add(streamInitilizerSource);
        }

        public void SetStream(MaterialShaderStage stage, string stream, IComputeNode computeNode, ParameterKey<Texture> defaultTexturingKey, ParameterKey defaultValueKey, Color? defaultTextureValue = null)
        {
            Current.SetStream(stage, stream, computeNode, defaultTexturingKey, defaultValueKey, defaultTextureValue);
        }

        public void SetStream(MaterialShaderStage stage, string stream, MaterialStreamType streamType, ShaderSource shaderSource)
        {
            Current.SetStream(stage, stream, streamType, shaderSource);
        }

        public void SetStream(string stream, IComputeNode computeNode, ParameterKey<Texture> defaultTexturingKey, ParameterKey defaultValueKey, Color? defaultTextureValue = null)
        {
            SetStream(MaterialShaderStage.Pixel, stream, computeNode, defaultTexturingKey, defaultValueKey, defaultTextureValue);
        }

        public void AddShading<T>(T shadingModel, ShaderSource shaderSource) where T : class, IMaterialShadingModelFeature
        {
            Current.ShadingModels.Add(shadingModel, shaderSource);
        }

        private class MaterialBlendLayerNode
        {
            private readonly MaterialGeneratorContext context;

            private readonly MaterialBlendLayerNode parentNode;

            public MaterialBlendLayerNode(MaterialGeneratorContext context, MaterialBlendLayerNode parentNode)
            {
                this.context = context;
                this.parentNode = parentNode;

                Children = new List<MaterialBlendLayerNode>();
                ShadingModels = new MaterialShadingModelCollection();

                foreach (MaterialShaderStage stage in Enum.GetValues(typeof(MaterialShaderStage)))
                {
                    SurfaceShaders[stage] = new List<ShaderSource>();
                    StreamInitializers[stage] = new List<string>();
                    Streams[stage] = new HashSet<string>();
                }
            }

            public MaterialGeneratorContext Context
            {
                get
                {
                    return context;
                }
            }

            public MaterialBlendLayerNode Parent
            {
                get
                {
                    return parentNode;
                }
            }

            public readonly Dictionary<MaterialShaderStage, List<ShaderSource>> SurfaceShaders = new Dictionary<MaterialShaderStage, List<ShaderSource>>();
            public readonly Dictionary<MaterialShaderStage, List<string>> StreamInitializers = new Dictionary<MaterialShaderStage, List<string>>();
            public readonly Dictionary<MaterialShaderStage, HashSet<string>> Streams = new Dictionary<MaterialShaderStage, HashSet<string>>();

            public List<MaterialBlendLayerNode> Children { get; private set; }

            public MaterialShadingModelCollection ShadingModels { get; set; }

            public void SetStream(MaterialShaderStage stage, string stream, IComputeNode computeNode, ParameterKey<Texture> defaultTexturingKey, ParameterKey defaultValueKey, Color? defaultTextureValue)
            {
                if (defaultValueKey == null) throw new ArgumentNullException("defaultKey");
                if (computeNode == null)
                {
                    return;
                }

                var streamType = MaterialStreamType.Float;
                if (defaultValueKey.PropertyType == typeof(Vector4) || defaultValueKey.PropertyType == typeof(Color4))
                {
                    streamType = MaterialStreamType.Float4;
                }
                else if (defaultValueKey.PropertyType == typeof(Vector3) || defaultValueKey.PropertyType == typeof(Color3))
                {
                    streamType = MaterialStreamType.Float3;
                }
                else if (defaultValueKey.PropertyType == typeof(Vector2) || defaultValueKey.PropertyType == typeof(Half2))
                {
                    streamType = MaterialStreamType.Float2;
                }
                else if (defaultValueKey.PropertyType == typeof(float))
                {
                    streamType = MaterialStreamType.Float;
                }
                else
                {
                    throw new NotSupportedException("ParameterKey type [{0}] is not supported by SetStream".ToFormat(defaultValueKey.PropertyType));
                }

                var classSource = computeNode.GenerateShaderSource(context, new MaterialComputeColorKeys(defaultTexturingKey, defaultValueKey, defaultTextureValue));
                SetStream(stage, stream, streamType, classSource);
            }

            public void SetStream(MaterialShaderStage stage, string stream, MaterialStreamType streamType, ShaderSource classSource)
            {
                // Blend stream isnot part of the stream used
                if (stream != MaterialBlendLayer.BlendStream)
                {
                    Streams[stage].Add(stream);
                }

                string channel;
                switch (streamType)
                {
                    case MaterialStreamType.Float:
                        channel = "r";
                        break;
                    case MaterialStreamType.Float2:
                        channel = "rg";
                        break;
                    case MaterialStreamType.Float3:
                        channel = "rgb";
                        break;
                    case MaterialStreamType.Float4:
                        channel = "rgba";
                        break;
                    default:
                        throw new NotSupportedException("StreamType [{0}] is not supported".ToFormat(streamType));
                }

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceSetStreamFromComputeColor", stream, channel));
                mixin.AddComposition("computeColorSource", classSource);

                GetSurfaceShaders(stage).Add(mixin);
            }

            public List<ShaderSource> GetSurfaceShaders(MaterialShaderStage stage)
            {
                return SurfaceShaders[stage];
            }

            public ShaderSource GenerateStreamInilizer(MaterialShaderStage stage)
            {
                var mixin = new ShaderMixinSource();

                // the basic streams contained by every materials
                mixin.Mixins.Add(new ShaderClassSource("MaterialStream"));

                // the streams coming from the material layers
                foreach (var streamInitializer in StreamInitializers[stage])
                {
                    mixin.Mixins.Add(streamInitializer);
                }
                StreamInitializers[stage].Clear();

                // the streams specific to a stage
                if(stage == MaterialShaderStage.Pixel)
                    mixin.Mixins.Add("MaterialPixelShadingStream");

                return mixin;
            }

            public ShaderSource GenerateSurfaceShader(MaterialShaderStage stage)
            {
                var surfaceShaders = GetSurfaceShaders(stage);

                if (surfaceShaders.Count == 0)
                {
                    return null;
                }

                ShaderSource result;
                // If there is only a single op, don't generate a mixin
                if (surfaceShaders.Count == 1)
                {
                    result = surfaceShaders[0];
                }
                else
                {
                    var mixin = new ShaderMixinSource();
                    result = mixin;
                    mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceArray"));

                    // Squash all operations into MaterialLayerArray
                    foreach (var operation in surfaceShaders)
                    {
                        mixin.AddCompositionToArray("layers", operation);
                    }
                }

                surfaceShaders.Clear();
                Streams[stage].Clear();
                return result;
            }
        }

        private sealed class MaterialShadingModelCollection : Dictionary<Type, KeyValuePair<IMaterialShadingModelFeature, ShaderSource>>
        {
            public void Add<T>(T shadingModel, ShaderSource shaderSource) where T : class, IMaterialShadingModelFeature
            {
                if (shadingModel == null)
                {
                    return;
                }

                this[shadingModel.GetType()] = new KeyValuePair<IMaterialShadingModelFeature, ShaderSource>(shadingModel, shaderSource);
            }

            public bool Equals(MaterialShadingModelCollection node)
            {
                if (node == null || ReferenceEquals(node, this))
                {
                    return true;
                }

                if (Count != node.Count)
                {
                    return false;
                }
                if (Count == 0 || node.Count == 0)
                {
                    return true;
                }

                foreach (var shadingModelKeyPair in this)
                {
                    KeyValuePair<IMaterialShadingModelFeature, ShaderSource> shadingModelAgainst;
                    if (!node.TryGetValue(shadingModelKeyPair.Key, out shadingModelAgainst))
                    {
                        return false;
                    }
                    if (!shadingModelKeyPair.Value.Key.Equals(shadingModelAgainst.Key))
                    {
                        return false;
                    }
                }

                return true;
            }

            public IEnumerable<ShaderSource> Generate(MaterialGeneratorContext context)
            {
                // Generate MaterialLayer Shading that is light dependent
                ShaderMixinSource mixinSource = null;
                foreach (var shadingModelKeyPair in Values)
                {
                    var shadingModel = shadingModelKeyPair.Key;
                    if (shadingModel.IsLightDependent)
                    {
                        if (mixinSource == null)
                        {
                            mixinSource = new ShaderMixinSource();
                            mixinSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceLightingAndShading"));
                        }
                        mixinSource.AddCompositionToArray("surfaces", shadingModelKeyPair.Value);
                    }
                }
                if (mixinSource != null)
                {
                    yield return mixinSource;
                }

                // Generate shading light independent
                foreach (var shadingSource in Values.Where(keyPair => !keyPair.Key.IsLightDependent).Select(keyPair => keyPair.Value))
                {
                    yield return shadingSource;
                }
            }
        }
    }
}
