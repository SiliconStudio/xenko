// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class MaterialShaderGeneratorContext
    {
        private int idCounter;

        private readonly Dictionary<ParameterKey<Texture>, int> textureKeyIndices = new Dictionary<ParameterKey<Texture>, int>();
        private readonly Dictionary<SamplerStateDescription, ParameterKey<SamplerState>> declaredSamplerStates;

        public MaterialShaderGeneratorContext()
        {
            Stack = new Stack<StackOperations>();
            Parameters = new ParameterCollectionData();
            declaredSamplerStates = new Dictionary<SamplerStateDescription, ParameterKey<SamplerState>>();
            PushStack();
            RootStack = Stack.Peek();
        }

        public StackOperations RootStack { get; private set; }

        public ParameterCollectionData Parameters { get; private set; }

        public MaterialAsset FindMaterial(AssetReference<MaterialAsset> materialReference)
        {
            throw new NotImplementedException();
        }

        public Stack<StackOperations> Stack { get; private set; } 

        public StackOperations PushStack()
        {
            var stack = new StackOperations(this);
            Stack.Push(stack);
            return stack;
        }

        public StackOperations CurrentStack
        {
            get
            {
                return Stack.Peek();
            }
        }

        public void PopStack()
        {
            Stack.Pop();
        }

        public bool ExploreGenerics = false;

        public LoggerResult Log;

        public int NextId()
        {
            return idCounter++;
        }

        public ParameterKey<Texture> GetTextureKey(MaterialTextureComputeColor textureComputeColor, ParameterKey<Texture> baseKey)
        {
            ParameterKey<Texture> key = null;
            ContentReference keyReference = null;

            if (textureComputeColor.Key != null)
            {
                key = textureComputeColor.Key;
            }
            else
            {
                baseKey = baseKey ?? MaterialKeys.Texture;
                int textureKeyIndex;
                textureKeyIndices.TryGetValue(baseKey, out textureKeyIndex);

                key = textureKeyIndex == 0 ? baseKey : baseKey.ComposeWith(textureKeyIndex.ToString(CultureInfo.InvariantCulture));

                textureKeyIndex++;
                textureKeyIndices[baseKey] = textureKeyIndex;
                
                if (textureComputeColor.TextureReference != null)
                {
                    keyReference = new ContentReference<Texture>(textureComputeColor.TextureReference.Id, textureComputeColor.TextureReference.Location);
                }
            }

            Parameters.Set(key, keyReference);
            return key;
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

            var samplerState = new FakeSamplerState(samplerStateDesc);
            Parameters.Set(key, ContentReference.Create((SamplerState)samplerState));
            return key;
        }

        public class StackOperations
        {
            private readonly MaterialShaderGeneratorContext context;

            public StackOperations(MaterialShaderGeneratorContext context)
            {
                this.context = context;
                Operations = new List<ShaderSource>();
                Streams = new HashSet<string>();
            }

            public List<ShaderSource> Operations { get; private set; }

            public HashSet<string> Streams { get; private set; }

            // TODO: Not used anymore. Reset streams directly from MaterialStreams.ResetStreams
            private void ResetStream<T>(string stream, T value, bool force = false) where T : struct
            {
                if (!Streams.Contains(stream) || force)
                {
                    object objValue = value;
                    string channel = null;
                    string valueStr = null;

                    if (value is float)
                    {
                        channel = "r";
                        valueStr = string.Format(CultureInfo.InvariantCulture, "float4({0}, 0, 0, 0)", value);
                    }
                    else if (value is Vector2)
                    {
                        channel = "rg";
                        var vector2 = (Vector2)objValue;
                        valueStr = string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, 0, 0)", vector2.X, vector2.Y);
                    }
                    else if (value is Vector3)
                    {
                        channel = "rgb";
                        var vector3 = (Vector3)objValue;
                        valueStr = string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, 0)", vector3.X, vector3.Y, vector3.Z);
                    }
                    else if (value is Color3)
                    {
                        channel = "rgb";
                        var color3 = (Color3)objValue;
                        valueStr = string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, 0)", color3.R, color3.G, color3.B);
                    }
                    else if (value is Vector4)
                    {
                        channel = "rgba";
                        var vector4 = (Vector4)objValue;
                        valueStr = string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", vector4.X, vector4.Y, vector4.Z, vector4.W);
                    }
                    else if (value is Color4)
                    {
                        channel = "rgba";
                        var color4 = (Color4)objValue;
                        valueStr = string.Format(CultureInfo.InvariantCulture, "float4({0}, {1}, {2}, {3})", color4.R, color4.G, color4.B, color4.A);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("value", "Type [{0}] is not supported as a value for ResetStream".ToFormat(typeof(T)));
                    }

                    var mixin = new ShaderMixinSource();
                    mixin.Mixins.Add(new ShaderClassSource("MaterialLayerSetStreamFromComputeColor", stream, channel));
                    mixin.AddComposition("computeColorSource", new ShaderClassSource("ComputeColorFixed", valueStr));

                    Operations.Add(mixin);
                }
                Streams.Add(stream);
            }

            public void SetStream(string stream, MaterialStreamType streamType, ShaderSource classSource)
            {
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
                mixin.Mixins.Add(new ShaderClassSource("MaterialLayerSetStreamFromComputeColor", stream, channel));
                mixin.AddComposition("computeColorSource", classSource);

                Operations.Add(mixin);
            }

            public ShaderSource GenerateMixin()
            {
                if (Operations.Count == 0)
                {
                    return null;
                }

                ShaderSource result;
                // If there is only a single op, don't generate a mixin
                if (Operations.Count == 1)
                {
                    result = Operations[0];
                }
                else
                {
                    var mixin = new ShaderMixinSource();
                    result = mixin;
                    mixin.Mixins.Add(new ShaderClassSource("MaterialLayerArray"));

                    // Squash all operations into MaterialLayerArray
                    foreach (var operation in Operations)
                    {
                        mixin.AddCompositionToArray("layers", operation);
                    }
                }

                Operations.Clear();
                Streams.Clear();
                return result;
            }
        }
    }
}
