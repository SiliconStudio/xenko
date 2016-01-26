// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A dynamic effect instance, including its values and resources.
    /// </summary>
    public class DynamicEffectInstance : DisposeBase
    {
        private readonly string effectName;

        // Parameter keys used for effect permutation
        private KeyValuePair<ParameterKey, object>[] effectParameterKeys;

        // Parameter keys for shader values
        private FastListStruct<ValueParameterKeyInfo> valueParameterKeyInfos = new FastListStruct<ValueParameterKeyInfo>(4);
        private FastListStruct<ResourceParameterKeyInfo> resourceParameterKeyInfos = new FastListStruct<ResourceParameterKeyInfo>(4);
        private FastListStruct<ConstantBufferInfo> constantBuffers = new FastListStruct<ConstantBufferInfo>(2);
        private int constantBufferTotalSize;

        // Constants and resources
        // TODO: Currently stored in unmanaged array so we can get a pointer that can be updated from outside
        //   However, maybe ref locals would make this not needed anymore?
        private IntPtr dataValues;
        private int dataValuesSize;
        private object[] resourceValues;

        // Descriptor sets
        private DescriptorSetLayout[] descriptorSetLayouts;
        private DescriptorSet[] descriptorSets;

        // Store current effect
        private Effect effect;
        private bool effectDirty = true;

        // Describes how to update resource bindings
        private EffectBinder binder;

        public DynamicEffectInstance(string effectName)
        {
            this.effectName = effectName;
        }

        protected override void Destroy()
        {
            base.Destroy();

            Marshal.FreeHGlobal(dataValues);
            dataValues = IntPtr.Zero;
        }

        /// <summary>
        /// Sets a value that will impact effect permutation (used in .xkfx file).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterKey"></param>
        /// <param name="value"></param>
        public void SetPermutationValue<T>(ParameterKey<T> parameterKey, T value)
        {
            // Look for existing entries
            if (effectParameterKeys != null)
            {
                for (int i = 0; i < effectParameterKeys.Length; ++i)
                {
                    if (effectParameterKeys[i].Key == parameterKey)
                    {
                        if (effectParameterKeys[i].Value != (object) value)
                        {
                            effectParameterKeys[i] = new KeyValuePair<ParameterKey, object>(parameterKey, value);
                            effectDirty = true;
                        }
                        return;
                    }
                }
            }

            // It's a new key, let's add it
            Array.Resize(ref effectParameterKeys, (effectParameterKeys?.Length ?? 0) + 1);
            effectParameterKeys[effectParameterKeys.Length - 1] = new KeyValuePair<ParameterKey, object>(parameterKey, value);
            effectDirty = true;
        }

        public ResourceParameter<T> GetResourceParameter<T>(ParameterKey<T> parameterKey) where T : class
        {
            // Find existing first
            foreach (var parameterKeyOffset in resourceParameterKeyInfos)
            {
                if (parameterKeyOffset.ParameterKey == parameterKey)
                {
                    return new ResourceParameter<T>(parameterKeyOffset.BindingSlot);
                }
            }

            // Find things existing in reflection
            var @class = EffectParameterClass.Object;
            var descriptorSet = -1;
            var bindingSlot = -1;
            if (effect != null)
            {
                for (int layoutIndex = 0; layoutIndex < binder.DescriptorReflection.Layouts.Count; layoutIndex++)
                {
                    var layout = binder.DescriptorReflection.Layouts[layoutIndex];
                    for (int entryIndex = 0; entryIndex < layout.Layout.Entries.Count; ++entryIndex)
                    {
                        if (layout.Layout.Entries[entryIndex].Name == parameterKey.Name)
                        {
                            @class = layout.Layout.Entries[entryIndex].Class;
                            descriptorSet = layoutIndex;
                            bindingSlot = entryIndex;
                        }
                    }
                }
            }

            // Create info entry
            Array.Resize(ref resourceValues, resourceParameterKeyInfos.Count + 1);
            resourceParameterKeyInfos.Add(new ResourceParameterKeyInfo(parameterKey, @class, descriptorSet, bindingSlot));
            return new ResourceParameter<T>(resourceParameterKeyInfos.Count - 1);
        }

        // TODO: Temporary, until we remove arrays from ParameterKey
        [Obsolete]
        public ValueParameter<T> GetValueParameterArray<T>(ParameterKey<T[]> parameterKey, int elementCount = 1) where T : struct
        {
            // Find existing first
            foreach (var parameterKeyOffset in valueParameterKeyInfos)
            {
                if (parameterKeyOffset.ParameterKey == parameterKey)
                {
                    return new ValueParameter<T>(parameterKeyOffset.Offset);
                }
            }

            // Find things existing in reflection
            var offset = -1;
            if (effect != null)
            {
                foreach (var constantBuffer in effect.Bytecode.Reflection.ConstantBuffers)
                {
                    foreach (var member in constantBuffer.Members)
                    {
                        if (member.Param.Key == parameterKey)
                        {
                            offset = member.Offset;
                        }
                    }
                }
            }

            // Compute size
            var elementSize = parameterKey.Size;
            var totalSize = elementSize;
            if (elementCount > 1)
                totalSize += (elementSize + 15) / 16 * 16 * (elementCount - 1);

            // Create offset entry
            var result = new ValueParameter<T>(valueParameterKeyInfos.Count);
            valueParameterKeyInfos.Add(new ValueParameterKeyInfo(parameterKey, offset != -1 ? offset : dataValuesSize, totalSize));

            // Otherwise, we append at the end; resize array to accomodate new data
            if (offset == -1)
            {
                dataValuesSize += totalSize;
                dataValues = dataValues != IntPtr.Zero
                    ? Marshal.ReAllocHGlobal(dataValues, (IntPtr)dataValuesSize)
                    : Marshal.AllocHGlobal((IntPtr)dataValuesSize);

                // Initialize default value
                if (parameterKey.DefaultValueMetadataT?.DefaultValue != null)
                {
                    SetValues(result, parameterKey.DefaultValueMetadataT.DefaultValue);
                }
            }

            return result;
        }

        public ValueParameter<T> GetValueParameter<T>(ParameterKey<T> parameterKey, int elementCount = 1) where T : struct
        {
            // Find existing first
            foreach (var parameterKeyOffset in valueParameterKeyInfos)
            {
                if (parameterKeyOffset.ParameterKey == parameterKey)
                {
                    return new ValueParameter<T>(parameterKeyOffset.Offset);
                }
            }

            // Find things existing in reflection
            var offset = -1;
            if (effect != null)
            {
                foreach (var constantBuffer in effect.Bytecode.Reflection.ConstantBuffers)
                {
                    foreach (var member in constantBuffer.Members)
                    {
                        if (member.Param.Key == parameterKey)
                        {
                            offset = member.Offset;
                        }
                    }
                }
            }

            // Compute size
            var elementSize = parameterKey.Size;
            var totalSize = elementSize;
            if (elementCount > 1)
                totalSize += (elementSize + 15) / 16 * 16 * (elementCount - 1);

            // Create offset entry
            var result = new ValueParameter<T>(valueParameterKeyInfos.Count);
            valueParameterKeyInfos.Add(new ValueParameterKeyInfo(parameterKey, offset != -1 ? offset : dataValuesSize, totalSize));

            // Otherwise, we append at the end; resize array to accomodate new data
            if (offset == -1)
            {
                dataValuesSize += totalSize;
                dataValues = dataValues != IntPtr.Zero
                    ? Marshal.ReAllocHGlobal(dataValues, (IntPtr)dataValuesSize)
                    : Marshal.AllocHGlobal((IntPtr)dataValuesSize);


                // Initialize default value
                if (parameterKey.DefaultValueMetadataT != null)
                {
                    SetValue(result, parameterKey.DefaultValueMetadataT.DefaultValue);
                }
            }

            return result;
        }

        public void SetValues<T>(ValueParameter<T> parameter, T[] values) where T : struct
        {
            var data = GetValuePointer(parameter);

            // Align to float4
            var stride = (Utilities.SizeOf<T>() + 15) / 16 * 16;
            for (int i = 0; i < values.Length; ++i)
            {
                Utilities.Write(data, ref values[i]);
                data += stride;
            }
        }

        public IntPtr GetValuePointer<T>(ValueParameter<T> parameter) where T : struct
        {
            return dataValues + valueParameterKeyInfos[parameter.Index].Offset;
        }

        public void SetValue<T>(ValueParameter<T> parameter, T value) where T : struct
        {
            Utilities.Write(dataValues + valueParameterKeyInfos[parameter.Index].Offset, ref value);
        }

        public void SetValue<T>(ValueParameter<T> parameter, ref T value) where T : struct
        {
            Utilities.Write(dataValues + valueParameterKeyInfos[parameter.Index].Offset, ref value);
        }

        public void SetValue<T>(ResourceParameter<T> parameter, T value) where T : class
        {
            resourceValues[parameter.Index] = value;
        }

        public void UpdateEffect(GraphicsDevice graphicsDevice, EffectSystem effectSystem)
        {
            if (effectDirty)
            {
                effectDirty = false;

                // Looks like the effect changed, it needs a recompilation
                var compilerParameters = new CompilerParameters();
                if (effectParameterKeys != null)
                {
                    foreach (var effectParameterKey in effectParameterKeys)
                    {
                        compilerParameters.SetObject(effectParameterKey.Key, effectParameterKey.Value);
                    }
                }

                effect = effectSystem.LoadEffect(effectName, compilerParameters).WaitForResult();

                // Update reflection and rearrange buffers/resources
                var layouts = effect.Bytecode.Reflection.ResourceBindings.Select(x => x.Param.ResourceGroup).Distinct().ToList();
                binder.Compile(graphicsDevice, effect.Bytecode, layouts);

                // TODO: Free previous descriptor sets and layouts?

                descriptorSets = new DescriptorSet[binder.DescriptorReflection.Layouts.Count];
                descriptorSetLayouts = new DescriptorSetLayout[binder.DescriptorReflection.Layouts.Count];
                for (int i = 0; i < binder.DescriptorReflection.Layouts.Count; ++i)
                {
                    var layout = binder.DescriptorReflection.Layouts[i];
                    descriptorSetLayouts[i] = DescriptorSetLayout.New(graphicsDevice, layout.Layout);
                }

                // Do a first pass to measure constant buffer size
                constantBuffers.Clear();
                var bufferSize = 0;
                var newValueParameterKeyInfos = new FastListStruct<ValueParameterKeyInfo>(Math.Max(1, valueParameterKeyInfos.Count));
                var newResourceParameterKeyInfos = new FastListStruct<ResourceParameterKeyInfo>(Math.Max(1, resourceParameterKeyInfos.Count));

                // Process constant buffers
                for (int layoutIndex = 0; layoutIndex < binder.DescriptorReflection.Layouts.Count; layoutIndex++)
                {
                    var layout = binder.DescriptorReflection.Layouts[layoutIndex];
                    for (int entryIndex = 0; entryIndex < layout.Layout.Entries.Count; ++entryIndex)
                    {
                        var layoutEntry = layout.Layout.Entries[entryIndex];
                        if (layoutEntry.Class == EffectParameterClass.ConstantBuffer)
                        {
                            var constantBuffer = effect.Bytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Name);
                            constantBuffers.Add(new ConstantBufferInfo { DescriptorSet = layoutIndex, BindingSlot = entryIndex, DataOffset = bufferSize, Description = constantBuffer });
                            bufferSize += constantBuffer.Size;
                        }
                    }
                }

                constantBufferTotalSize = bufferSize;

                // Update resource bindings
                foreach (var resourceParameterKeyInfo in resourceParameterKeyInfos)
                {
                    for (int layoutIndex = 0; layoutIndex < binder.DescriptorReflection.Layouts.Count; layoutIndex++)
                    {
                        var layout = binder.DescriptorReflection.Layouts[layoutIndex];
                        for (int entryIndex = 0; entryIndex < layout.Layout.Entries.Count; ++entryIndex)
                        {
                            if (layout.Layout.Entries[entryIndex].Name == resourceParameterKeyInfo.ParameterKey.Name)
                            {
                                newResourceParameterKeyInfos.Add(new ResourceParameterKeyInfo(resourceParameterKeyInfo.ParameterKey, layout.Layout.Entries[entryIndex].Class, layoutIndex, entryIndex));
                                goto memberFound;
                            }
                        }
                    }

                    // Not found, let's add it without binding info
                    newResourceParameterKeyInfos.Add(new ResourceParameterKeyInfo(resourceParameterKeyInfo.ParameterKey, EffectParameterClass.Object, -1, -1));

                memberFound:
                    ;
                }

                // Find new offsets for data
                foreach (var dataParameterKeyInfo in valueParameterKeyInfos)
                {
                    // Look for it in reflection
                    foreach (var constantBuffer in effect.Bytecode.Reflection.ConstantBuffers)
                    {
                        foreach (var member in constantBuffer.Members)
                        {
                            if (member.Param.Key == dataParameterKeyInfo.ParameterKey)
                            {
                                newValueParameterKeyInfos.Add(new ValueParameterKeyInfo(dataParameterKeyInfo.ParameterKey, member.Offset, member.Size));
                                goto memberFound;
                            }
                        }
                    }

                    // Not found, let's add it (packed at the end)
                    newValueParameterKeyInfos.Add(new ValueParameterKeyInfo(dataParameterKeyInfo.ParameterKey, bufferSize, dataParameterKeyInfo.Size));
                    bufferSize += dataParameterKeyInfo.ParameterKey.Size;

                memberFound:
                    ;
                }

                var newData = Marshal.AllocHGlobal(bufferSize);

                // Update default values
                foreach (var constantBuffer in constantBuffers)
                {
                    foreach (var member in constantBuffer.Description.Members)
                    {
                        var defaultValueMetadata = member.Param.Key?.DefaultValueMetadata;
                        if (defaultValueMetadata != null)
                        {
                            defaultValueMetadata.WriteBuffer(newData + constantBuffer.DataOffset + member.Offset, 16);
                        }
                    }
                }

                // Second pass to copy existing data at new offsets/slots
                for (int i = 0; i < valueParameterKeyInfos.Count; ++i)
                {
                    var oldOffset = valueParameterKeyInfos[i].Offset;
                    var newOffset = newValueParameterKeyInfos[i].Offset;

                    Utilities.CopyMemory(newData + newOffset, dataValues + oldOffset, valueParameterKeyInfos[i].Size);
                }

                valueParameterKeyInfos = newValueParameterKeyInfos;
                resourceParameterKeyInfos = newResourceParameterKeyInfos;

                Marshal.FreeHGlobal(dataValues);
                dataValues = newData;
            }
        }

        public void Apply(GraphicsDevice graphicsDevice)
        {
            effect.ApplyProgram(graphicsDevice);

            // Bind resources
            // TODO: What descriptor pool should we use?
            var descriptorPool = DescriptorPool.New(graphicsDevice, new[]
            {
                new DescriptorTypeCount(EffectParameterClass.ConstantBuffer, 256),
            });

            // Instantiate descriptor sets
            for (int i = 0; i < descriptorSets.Length; ++i)
            {
                descriptorSets[i] = DescriptorSet.New(graphicsDevice, descriptorPool, descriptorSetLayouts[i]);
            }

            // Copy cbuffer data
            if (dataValues != IntPtr.Zero && constantBuffers.Count > 0)
            {
                var bufferPool = BufferPool.New(graphicsDevice, constantBufferTotalSize);
                Utilities.CopyMemory(bufferPool.Buffer.Data, dataValues, constantBufferTotalSize);
                foreach (var constantBuffer in constantBuffers)
                {
                    var descriptorSet = descriptorSets[constantBuffer.DescriptorSet];
                    descriptorSet.SetConstantBuffer(constantBuffer.BindingSlot, bufferPool.Buffer, constantBuffer.DataOffset, constantBuffer.Description.Size);
                }
            }

            // Set other resources
            for (int index = 0; index < resourceParameterKeyInfos.Count; ++index)
            {
                var resourceParameterKeyInfo = resourceParameterKeyInfos[index];
                if (resourceParameterKeyInfo.DescriptorSet == -1)
                    continue;

                var descriptorSet = descriptorSets[resourceParameterKeyInfo.DescriptorSet];
                switch (resourceParameterKeyInfo.Class)
                {
                    case EffectParameterClass.Sampler:
                        descriptorSet.SetSamplerState(resourceParameterKeyInfo.BindingSlot, (SamplerState)resourceValues[index]);
                        break;
                    case EffectParameterClass.ShaderResourceView:
                        descriptorSet.SetShaderResourceView(resourceParameterKeyInfo.BindingSlot, (GraphicsResource)resourceValues[index]);
                        break;
                    case EffectParameterClass.UnorderedAccessView:
                        descriptorSet.SetUnorderedAccessView(resourceParameterKeyInfo.BindingSlot, (GraphicsResource)resourceValues[index]);
                        break;
                }
            }

            // Apply
            binder.Apply(graphicsDevice, descriptorSets, 0);
        }

        struct ValueParameterKeyInfo
        {
            public ParameterKey ParameterKey;
            public int Offset;
            public int Size;

            public ValueParameterKeyInfo(ParameterKey parameterKey, int offset, int size)
            {
                ParameterKey = parameterKey;
                Offset = offset;
                Size = size;
            }
        }

        struct ResourceParameterKeyInfo
        {
            public ParameterKey ParameterKey;
            public EffectParameterClass Class;
            public int DescriptorSet;
            public int BindingSlot;

            public ResourceParameterKeyInfo(ParameterKey parameterKey, EffectParameterClass @class, int descriptorSet, int bindingSlot)
            {
                ParameterKey = parameterKey;
                Class = @class;
                DescriptorSet = descriptorSet;
                BindingSlot = bindingSlot;
            }
        }

        struct ConstantBufferInfo
        {
            public int DescriptorSet;
            public int BindingSlot;

            /// <summary>
            /// Offset in <see cref="DynamicEffectInstance.dataValues"/>.
            /// </summary>
            public int DataOffset;
            public ShaderConstantBufferDescription Description;
        }
    }

    public class ImageEffectShader : ImageEffect
    {
        private readonly DynamicEffectInstance dynamicEffectInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectShader" /> class.
        /// </summary>
        public ImageEffectShader(string effectName = null)
        {
            dynamicEffectInstance = new DynamicEffectInstance(effectName);
        }

        protected override void DrawCore(RenderContext context)
        {
            dynamicEffectInstance.UpdateEffect(GraphicsDevice, EffectSystem);
            dynamicEffectInstance.Apply(GraphicsDevice);
        }
    }
}