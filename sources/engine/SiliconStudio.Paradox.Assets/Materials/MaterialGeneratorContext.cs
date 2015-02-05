// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public enum MaterialShaderStage
    {
        Vertex,

        Pixel
    }

    // TODO REWRITE AND COMMENT THIS CLASS
    public class MaterialGeneratorContext : ShaderGeneratorContextBase
    {
        private readonly Dictionary<string, ShaderSource> registeredStreamBlend = new Dictionary<string, ShaderSource>();
        private int shadingModelCount;
        private MaterialBlendOverrides currentOverrides;

        private readonly List<KeyValuePair<Type, ShaderSource>> vertexInputStreamModifiers = new List<KeyValuePair<Type, ShaderSource>>();

        public MaterialGeneratorContext()
            : this(null)
        {
        }

        public MaterialGeneratorContext(Package package)
            : base(package)
        {
            currentOverrides = new MaterialBlendOverrides();
        }

        public HashSet<string> Streams
        {
            get
            {
                return Current.Streams;
            }
        }

        private MaterialBlendLayerNode Current { get; set; }

        private MaterialShadingModelCollection CurrentShadingModel { get; set; }


        public bool IsVertexStage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether materials will be optimized (textures blended together, generate optimized shader permutations, etc...).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [materials are optimized]; otherwise, <c>false</c>.
        /// </value>
        public bool OptimizeMaterials { get; set; }

        public void AddVertexStreamModifier<T>(ShaderSource shaderSource)
        {
            if (shaderSource == null)
            {
                return;
            }

            var typeT = typeof(T);
            if (vertexInputStreamModifiers.All(modifiers => modifiers.Key != typeT))
            {
                vertexInputStreamModifiers.Add(new KeyValuePair<Type, ShaderSource>(typeT, shaderSource));
            }
        }

        public void PushLayer(MaterialBlendOverrides overrides)
        {
            var newLayer = new MaterialBlendLayerNode(this, Current, overrides);
            if (Current != null)
            {
                Current.Children.Add(newLayer);
            }
            Current = newLayer;

            // Update overrides by squashing them using multiplication
            currentOverrides = new MaterialBlendOverrides();
            var current = Current;
            while (current != null && current.Overrides != null)
            {
                // TODO: We are doing this bottom-up. We might have to do this top-down in case overrides multiplcation is not commutative
                currentOverrides *= current.Overrides;
                current = current.Parent;
            }
        }

        public MaterialBlendOverrides CurrentOverrides
        {
            get
            {
                return currentOverrides;
            }
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

            // ----------------------------------------------
            // Vertex surface shaders
            // ----------------------------------------------
            if (Current.Parent != null)
            {
                // Copy vertex surface shaders to parent
                foreach (var shaderSource in Current.VertexStageSurfaceShaders)
                {
                    Current.Parent.VertexStageSurfaceShaders.Add(shaderSource);
                }
            }

            // ----------------------------------------------
            // Pixel surface shaders
            // ----------------------------------------------
            // If last shading model or current shading model different from next shading model
            if (CurrentShadingModel != null && (!sameShadingModel || Current.Parent == null))
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
                    currentOrParentLayer.PixelStageSurfaceShaders.Add(shaderSource);
                }
            }
            else if (Current.Parent != null)
            {
                // Copy pixel surface shaders to parent
                foreach (var shaderSource in Current.PixelStageSurfaceShaders)
                {
                    Current.Parent.PixelStageSurfaceShaders.Add(shaderSource);
                }
            }

            // In case of the root material and for vertex stream, add all vertex stream modifiers just at the end
            if (Current.Parent == null)
            {
                foreach (var shaderSource in vertexInputStreamModifiers.Select(modifiers => modifiers.Value))
                {
                    Current.VertexStageSurfaceShaders.Add(shaderSource);
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
            return GetTextureKey(computeTexture.TextureReference, keyResolved, baseKeys.DefaultTextureValue);
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

        public ShaderSource GenerateMixin(MaterialShaderStage stage)
        {
            return Current.GenerateMixin(stage);
        }

        public void UseStream(string stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            Current.Streams.Add(stream);
        }

        public ShaderSource GetStreamBlendShaderSource(string stream)
        {
            ShaderSource shaderSource;
            registeredStreamBlend.TryGetValue(stream, out shaderSource);
            return shaderSource ?? new ShaderClassSource("MaterialStreamLinearBlend", stream);
        }

        public void UseStreamWithCustomBlend(string stream, ShaderSource blendStream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            UseStream(stream);
            registeredStreamBlend[stream] = blendStream;
        }

        public void SetStream(MaterialShaderStage stage, string stream, IComputeNode computeNode, ParameterKey<Texture> defaultTexturingKey, ParameterKey defaultValueKey, Color? defaultTextureValue = null)
        {
            Current.SetStream(stage, stream, computeNode, defaultTexturingKey, defaultValueKey, defaultTextureValue);
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

            private readonly MaterialBlendOverrides overrides;

            public MaterialBlendLayerNode(MaterialGeneratorContext context, MaterialBlendLayerNode parentNode, MaterialBlendOverrides overrides)
            {
                this.context = context;
                this.parentNode = parentNode;
                this.overrides = overrides;
                VertexStageSurfaceShaders = new List<ShaderSource>();
                PixelStageSurfaceShaders = new List<ShaderSource>();
                Children = new List<MaterialBlendLayerNode>();
                Streams = new HashSet<string>();
                ShadingModels = new MaterialShadingModelCollection();
            }

            public MaterialBlendOverrides Overrides
            {
                get
                {
                    return overrides;
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

            public List<ShaderSource> VertexStageSurfaceShaders { get; private set; }

            public List<ShaderSource> PixelStageSurfaceShaders { get; private set; }

            public HashSet<string> Streams { get; private set; }

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

            private void SetStream(MaterialShaderStage stage, string stream, MaterialStreamType streamType, ShaderSource classSource)
            {
                // Blend stream isnot part of the stream used
                if (stream != MaterialBlendLayer.BlendStream)
                {
                    Streams.Add(stream);
                }

                string channel;
                switch (streamType)
                {
                    case MaterialStreamType.Float:
                        channel = "r";
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
                return stage == MaterialShaderStage.Pixel ? PixelStageSurfaceShaders : VertexStageSurfaceShaders;
            }

            public ShaderSource GenerateMixin(MaterialShaderStage stage)
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
                Streams.Clear();
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
