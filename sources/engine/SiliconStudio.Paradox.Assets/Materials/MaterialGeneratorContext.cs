// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialGeneratorContext
    {
        private int idCounter;

        private readonly Dictionary<ParameterKey, int> parameterKeyIndices = new Dictionary<ParameterKey, int>();
        private readonly Dictionary<SamplerStateDescription, ParameterKey<SamplerState>> declaredSamplerStates;
        private MaterialBlendLayerNode currentLayerNode;

        private int shadingModelCount;

        public delegate MaterialAsset FindMaterialDelegate(AssetReference<MaterialAsset> materialReference);

        public MaterialGeneratorContext()
        {
            Parameters = new ParameterCollection();
            declaredSamplerStates = new Dictionary<SamplerStateDescription, ParameterKey<SamplerState>>();
        }

        public HashSet<string> Streams
        {
            get
            {
                return Current.Streams;
            }
        }

        public ParameterCollection Parameters { get; private set; }

        private MaterialBlendLayerNode Current
        {
            get
            {
                return currentLayerNode;
            }
        }

        private MaterialShadingModelCollection CurrentShadingModel { get; set; }


        public FindMaterialDelegate FindMaterial { get; set; }

        public void PushLayer()
        {
            var newLayer = new MaterialBlendLayerNode(this, currentLayerNode);
            if (currentLayerNode != null)
            {
                currentLayerNode.Children.Add(newLayer);
            }
            currentLayerNode = newLayer;
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

            var parentLayer = currentLayerNode.Parent ?? currentLayerNode;

            // If last shading model or current shading model different from next shading model
            if (CurrentShadingModel != null && (!sameShadingModel || currentLayerNode.Parent == null))
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

                foreach (var shaderSource in shadingSources)
                {
                    parentLayer.SurfaceShaders.Add(shaderSource);
                }
            }
            else if (parentLayer != null)
            {
                foreach (var shaderSource in currentLayerNode.SurfaceShaders)
                {
                    parentLayer.SurfaceShaders.Add(shaderSource);
                }
            }

            if (currentLayerNode.Parent != null && !sameShadingModel)
            {
                CurrentShadingModel = Current.ShadingModels;
            }

            if (currentLayerNode.Parent != null)
            {
                currentLayerNode = currentLayerNode.Parent;
            }
        }

        public void ResetSurfaceShaders()
        {
            Current.SurfaceShaders.Clear();
        }

        public void AddSurfaceShader(ShaderSource shaderSource)
        {
            if (shaderSource == null) throw new ArgumentNullException("shaderSource");
            Current.SurfaceShaders.Add(shaderSource);
        }

        public LoggerResult Log;

        public int NextId()
        {
            return idCounter++;
        }

        public ParameterKey GetParameterKey(ParameterKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            var baseKey = key;
            int parameterKeyIndex;
            parameterKeyIndices.TryGetValue(baseKey, out parameterKeyIndex);

            key = parameterKeyIndex == 0 ? baseKey : baseKey.ComposeWith(parameterKeyIndex.ToString(CultureInfo.InvariantCulture));

            parameterKeyIndex++;
            parameterKeyIndices[baseKey] = parameterKeyIndex;
            return key;
        }

        // TODO: move this method to an extension method
        public ParameterKey<Texture> GetTextureKey(MaterialTextureComputeColor textureComputeColor, MaterialComputeColorKeys baseKeys)
        {
            var textureKey = (ParameterKey<Texture>)GetParameterKey(textureComputeColor.Key ?? baseKeys.TextureBaseKey ?? MaterialKeys.GenericTexture);
            var textureReference = textureComputeColor.TextureReference;
            var texture = AttachedReferenceManager.CreateSerializableVersion<Texture>(textureReference.Id, textureReference.Location);
            Parameters.Set(textureKey, texture);
            return textureKey;
        }

        public ParameterKey<SamplerState> GetSamplerKey(ComputeColorParameterSampler sampler)
        {
            if (sampler == null) throw new ArgumentNullException("sampler");

            var samplerStateDesc = new SamplerStateDescription(sampler.Filtering, sampler.AddressModeU)
            {
                AddressV = sampler.AddressModeV,
                AddressW = TextureAddressMode.Wrap
            };

            ParameterKey<SamplerState> key;

            if (!declaredSamplerStates.TryGetValue(samplerStateDesc, out key))
            {
                key = MaterialKeys.Sampler.ComposeWith(declaredSamplerStates.Count.ToString(CultureInfo.InvariantCulture));
                declaredSamplerStates.Add(samplerStateDesc, key);
            }

            var samplerState = new SamplerState(samplerStateDesc);
            Parameters.Set(key, samplerState);
            return key;
        }

        public void Visit(IMaterialFeature feature)
        {
            if (feature != null)
            {
                feature.Visit(this);
            }
        }

        public ShaderSource GenerateMixin()
        {
            return Current.GenerateMixin();
        }

        public void UseStream(string stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            Current.Streams.Add(stream);
        }

        public void SetStream(string stream, MaterialComputeColor computeColor, ParameterKey<Texture> defaultTexturingKey, ParameterKey defaultValueKey)
        {
            Current.SetStream(stream, computeColor, defaultTexturingKey, defaultValueKey);
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
                SurfaceShaders = new List<ShaderSource>();
                Children = new List<MaterialBlendLayerNode>();
                Streams = new HashSet<string>();
                ShadingModels = new MaterialShadingModelCollection();
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

            public List<ShaderSource> SurfaceShaders { get; private set; }

            public HashSet<string> Streams { get; private set; }

            public List<MaterialBlendLayerNode> Children { get; private set; }

            public MaterialShadingModelCollection ShadingModels { get; set; }

            public void SetStream(string stream, MaterialComputeColor computeColor, ParameterKey<Texture> defaultTexturingKey, ParameterKey defaultValueKey)
            {
                if (defaultTexturingKey == null) throw new ArgumentNullException("defaultKey");
                if (computeColor == null)
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

                var classSource = computeColor.GenerateShaderSource(context, new MaterialComputeColorKeys(defaultTexturingKey, defaultValueKey));
                SetStream(stream, streamType, classSource);
            }

            private void SetStream(string stream, MaterialStreamType streamType, ShaderSource classSource)
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
                    default:
                        throw new NotSupportedException("StreamType [{0}] is not supported".ToFormat(streamType));
                }

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceSetStreamFromComputeColor", stream, channel));
                mixin.AddComposition("computeColorSource", classSource);

                SurfaceShaders.Add(mixin);
            }

            public ShaderSource GenerateMixin()
            {
                if (SurfaceShaders.Count == 0)
                {
                    return null;
                }

                ShaderSource result;
                // If there is only a single op, don't generate a mixin
                if (SurfaceShaders.Count == 1)
                {
                    result = SurfaceShaders[0];
                }
                else
                {
                    var mixin = new ShaderMixinSource();
                    result = mixin;
                    mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceArray"));

                    // Squash all operations into MaterialLayerArray
                    foreach (var operation in SurfaceShaders)
                    {
                        mixin.AddCompositionToArray("layers", operation);
                    }
                }

                SurfaceShaders.Clear();
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
