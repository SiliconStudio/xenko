// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Graphics
{
    [ContentSerializer(typeof(DataContentSerializer<Effect>))]
    [DataSerializer(typeof(EffectSerializer))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Effect>), Profile = "Asset")]
    public class Effect : ComponentBase
    {
        private GraphicsDevice graphicsDeviceDefault;
        private EffectProgram program;
        private EffectReflection reflection;
        private EffectInputSignature inputSignature;

        private EffectBytecode bytecode;

        internal Effect()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Effect"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="bytecode">The bytecode.</param>
        /// <param name="usedParameters">The parameters used to create this shader (from a xkfx).</param>
        /// <exception cref="System.ArgumentNullException">
        /// device
        /// or
        /// bytecode
        /// </exception>
        public Effect(GraphicsDevice device, EffectBytecode bytecode)
        {
            InitializeFrom(device, bytecode);
        }

        internal void InitializeFrom(GraphicsDevice device, EffectBytecode bytecode)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (bytecode == null) throw new ArgumentNullException("bytecode");

            this.graphicsDeviceDefault = device;
            this.bytecode = bytecode;
            Initialize();
        }

        /// <summary>
        /// Gets the input signature of this effect.
        /// </summary>
        /// <value>The input signature.</value>
        public EffectInputSignature InputSignature
        {
            get
            {
                return inputSignature;
            }
        }

        /// <summary>
        /// Gets the bytecode.
        /// </summary>
        /// <value>The bytecode.</value>
        public EffectBytecode Bytecode
        {
            get
            {
                return bytecode;
            }
        }

        public void ApplyProgram(GraphicsDevice graphicsDevice)
        {
            PrepareApply(graphicsDevice);
        }

        public bool HasParameter(ParameterKey parameterKey)
        {
            // Check resources
            for (int i = 0; i < reflection.ResourceBindings.Count; i++)
            {
                var key = reflection.ResourceBindings[i].Param.Key;
                if (key == parameterKey)
                    return true;
            }

            // Check cbuffer
            foreach (var constantBuffer in reflection.ConstantBuffers)
            {
                var constantBufferMembers = constantBuffer.Members;

                for (int i = 0; i < constantBufferMembers.Length; ++i)
                {
                    var key = constantBufferMembers[i].Param.Key;
                    if (key == parameterKey)
                        return true;
                }
            }

            return false;
        }

        private void PrepareApply(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");

            program.Apply(graphicsDevice);
            graphicsDevice.CurrentEffect = this;
            graphicsDevice.ApplyPlatformSpecificParams(this);
        }

        private void Initialize()
        {
            program = EffectProgram.New(graphicsDeviceDefault, bytecode);
            reflection = program.Reflection;

            PrepareReflection(reflection);
            inputSignature = program.InputSignature;
            LoadDefaultParameters();
        }

        private static void PrepareReflection(EffectReflection reflection)
        {
            // prepare resource bindings used internally
            for (int i = 0; i < reflection.ResourceBindings.Count; i++)
            {
                var resourceBinding = reflection.ResourceBindings[i];
                UpdateResourceBindingKey(ref resourceBinding);
                reflection.ResourceBindings[i] = resourceBinding;
            }
            foreach (var constantBuffer in reflection.ConstantBuffers)
            {
                var constantBufferMembers = constantBuffer.Members;

                for (int i = 0; i < constantBufferMembers.Length; ++i)
                {
                    // Update binding key
                    UpdateValueBindingKey(ref constantBufferMembers[i]);
                }
            }

            UpdateConstantBufferHashes(reflection);
        }

        private void LoadDefaultParameters()
        {
            // Create parameter bindings
            for (int i = 0; i < reflection.ResourceBindings.Count; i++)
            {
                // Update binding key
                var key = reflection.ResourceBindings[i].Param.Key;

                if (reflection.ResourceBindings[i].Param.Class == EffectParameterClass.Sampler)
                {
                    var samplerBinding = reflection.SamplerStates.FirstOrDefault(x => x.KeyName == reflection.ResourceBindings[i].Param.KeyName);
                    if (samplerBinding != null)
                    {
                        samplerBinding.Key = key;
                        var samplerDescription = samplerBinding.Description;
                        // TODO GRAPHICS REFACTOR
                        //defaultParameters.Set((ParameterKey<SamplerState>)key, SamplerState.New(graphicsDeviceDefault, samplerDescription));
                    }
                }
            }

            // Create constant buffers from descriptions (previously generated from shader reflection)
            foreach (var constantBuffer in reflection.ConstantBuffers)
            {
                // TODO GRAPHICS REFACTOR (check if necessary)
                // Handle ConstantBuffer. Share the same key ParameterConstantBuffer with all the stages
                //var parameterConstantBuffer = new ParameterConstantBuffer(graphicsDeviceDefault, constantBuffer.Name, constantBuffer);
                //var constantBufferKey = ParameterKeys.New<Buffer>(constantBuffer.Name);
                //shaderParameters.RegisterParameter(constantBufferKey, false);

                //for (int i = 0; i < resourceBindings.Length; i++)
                //{
                //    if (resourceBindings[i].Description.Param.Class == EffectParameterClass.ConstantBuffer && resourceBindings[i].Description.Param.Key.Name == constantBuffer.Name)
                //    {
                //        resourceBindings[i].Description.Param.Key = constantBufferKey;
                //    }
                //}
            }
        }

        private static void UpdateResourceBindingKey(ref EffectParameterResourceData binding)
        {
            var keyName = binding.Param.KeyName;

            switch (binding.Param.Class)
            {
                case EffectParameterClass.Sampler:
                    binding.Param.Key = FindOrCreateResourceKey<SamplerState>(keyName);
                    break;
                case EffectParameterClass.ConstantBuffer:
                case EffectParameterClass.TextureBuffer:
                case EffectParameterClass.ShaderResourceView:
                case EffectParameterClass.UnorderedAccessView:
                    switch (binding.Param.Type)
                    {
                        case EffectParameterType.Buffer:
                        case EffectParameterType.ConstantBuffer:
                        case EffectParameterType.TextureBuffer:
                        case EffectParameterType.AppendStructuredBuffer:
                        case EffectParameterType.ByteAddressBuffer:
                        case EffectParameterType.ConsumeStructuredBuffer:
                        case EffectParameterType.StructuredBuffer:
                        case EffectParameterType.RWBuffer:
                        case EffectParameterType.RWStructuredBuffer:
                        case EffectParameterType.RWByteAddressBuffer:
                            binding.Param.Key = FindOrCreateResourceKey<Buffer>(keyName);
                            break;
                        case EffectParameterType.Texture:
                        case EffectParameterType.Texture1D:
                        case EffectParameterType.Texture1DArray:
                        case EffectParameterType.RWTexture1D:
                        case EffectParameterType.RWTexture1DArray:
                        case EffectParameterType.Texture2D:
                        case EffectParameterType.Texture2DArray:
                        case EffectParameterType.Texture2DMultisampled:
                        case EffectParameterType.Texture2DMultisampledArray:
                        case EffectParameterType.RWTexture2D:
                        case EffectParameterType.RWTexture2DArray:
                        case EffectParameterType.TextureCube:
                        case EffectParameterType.TextureCubeArray:
                        case EffectParameterType.RWTexture3D:
                        case EffectParameterType.Texture3D:
                            binding.Param.Key = FindOrCreateResourceKey<Texture>(keyName);
                            break;
                    }
                    break;
            }

            if (binding.Param.Key == null)
            {
                throw new InvalidOperationException(string.Format("Unable to find/generate key [{0}] with unsupported type [{1}/{2}]", binding.Param.KeyName, binding.Param.Class, binding.Param.Type));
            }
        }

        private static void UpdateValueBindingKey(ref EffectParameterValueData binding)
        {
            switch (binding.Param.Class)
            {
                case EffectParameterClass.Scalar:
                    switch (binding.Param.Type)
                    {
                        case EffectParameterType.Bool:
                            binding.Param.Key = FindOrCreateValueKey<bool>(binding);
                            break;
                        case EffectParameterType.Int:
                            binding.Param.Key = FindOrCreateValueKey<int>(binding);
                            break;
                        case EffectParameterType.UInt:
                            binding.Param.Key = FindOrCreateValueKey<uint>(binding);
                            break;
                        case EffectParameterType.Float:
                            binding.Param.Key = FindOrCreateValueKey<float>(binding);
                            break;
                    }
                    break;
                case EffectParameterClass.Color:
                    {
                        var componentCount = binding.RowCount != 1 ? binding.RowCount : binding.ColumnCount;
                        switch (binding.Param.Type)
                        {
                            case EffectParameterType.Float:
                                binding.Param.Key = componentCount == 4
                                                        ? FindOrCreateValueKey<Color4>(binding)
                                                        : (componentCount == 3 ? (ParameterKey)FindOrCreateValueKey<Color3>(binding) : null);
                                break;
                        }
                    }
                    break;
                case EffectParameterClass.Vector:
                    {
                        var componentCount = binding.RowCount != 1 ? binding.RowCount : binding.ColumnCount;
                        switch (binding.Param.Type)
                        {
                            case EffectParameterType.Bool:
                            case EffectParameterType.Int:
                                binding.Param.Key = componentCount == 4 ? (ParameterKey)FindOrCreateValueKey<Int4>(binding) : (componentCount == 3 ? FindOrCreateValueKey<Int3>(binding) : null);
                                break;
                            case EffectParameterType.UInt:
                                binding.Param.Key = componentCount == 4 ? FindOrCreateValueKey<UInt4>(binding) : null;
                                break;
                            case EffectParameterType.Float:
                                binding.Param.Key = componentCount == 4
                                                        ? FindOrCreateValueKey<Vector4>(binding)
                                                        : (componentCount == 3 ? (ParameterKey)FindOrCreateValueKey<Vector3>(binding) : (componentCount == 2 ? FindOrCreateValueKey<Vector2>(binding) : null));
                                break;
                        }
                    }
                    break;
                case EffectParameterClass.MatrixRows:
                case EffectParameterClass.MatrixColumns:
                    binding.Param.Key = FindOrCreateValueKey<Matrix>(binding);
                    break;
                case EffectParameterClass.Struct:
                    binding.Param.Key = ParameterKeys.FindByName(binding.Param.KeyName);
                    break;
            }

            if (binding.Param.Key == null)
            {
                throw new InvalidOperationException(string.Format("Unable to find/generate key [{0}] with unsupported type [{1}/{2}]", binding.Param.KeyName, binding.Param.Class, binding.Param.Type));
            }

        }

        private static ParameterKey FindOrCreateResourceKey<T>(string name)
        {
            return ParameterKeys.FindByName(name) ?? ParameterKeys.New<T>(name);
        }

        private static ParameterKey FindOrCreateValueKey<T>(EffectParameterValueData binding) where T : struct
        {
            var name = binding.Param.KeyName;
            return ParameterKeys.FindByName(name) ?? (binding.Count > 1 ? (ParameterKey)ParameterKeys.New<T[]>(name) : ParameterKeys.New<T>(name));
        }

        private static void UpdateConstantBufferHashes(EffectReflection reflection)
        {
            // Update Constant buffers description
            foreach (var constantBuffer in reflection.ConstantBuffers)
            {
                // We will generate a unique hash that depends on cbuffer layout (to easily detect if they differ when binding a new effect)
                // TODO: currently done at runtime, but it should better be done at compile time
                var hashBuilder = new ObjectIdBuilder();
                hashBuilder.Write(constantBuffer.Name);
                hashBuilder.Write(constantBuffer.Size);

                for (int i = 0; i < constantBuffer.Members.Length; ++i)
                {
                    var member = constantBuffer.Members[i];
                    constantBuffer.Members[i] = member;

                    hashBuilder.Write(member.Param.RawName);
                    hashBuilder.Write(member.SourceOffset);
                    hashBuilder.Write(member.SourceOffset);
                    hashBuilder.Write(member.Offset);
                    hashBuilder.Write(member.Count);
                    hashBuilder.Write(member.Size);
                    hashBuilder.Write(member.RowCount);
                    hashBuilder.Write(member.ColumnCount);
                }

                // Update the hash
                constantBuffer.Hash = hashBuilder.ComputeHash();
            }
        }
    }
}