// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
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
        private FastListStruct<ConstantBufferInfo> constantBuffers = new FastListStruct<ConstantBufferInfo>(2);
        private int constantBufferTotalSize;

        // Descriptor sets
        private ResourceGroupLayout[] resourceGroupLayouts;
        private ResourceGroup[] resourceGroups;

        // Store current effect
        private Effect effect;
        private bool effectDirty = true;

        // Describes how to update resource bindings
        private EffectBinder binder;

        public DynamicEffectInstance(string effectName)
        {
            this.effectName = effectName;
        }

        public NextGenParameterCollection Parameters { get; } = new NextGenParameterCollection();

        protected override void Destroy()
        {
            base.Destroy();

            Parameters.Dispose();
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

        public void UpdateEffect(GraphicsDevice graphicsDevice, EffectSystem effectSystem)
        {
            if (effectDirty)
            {
                effectDirty = false;

                // TODO: Free previous descriptor sets and layouts?

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

                // Process constant buffers
                var parameterCollectionLayout = new NextGenParameterCollectionLayout();
                for (int layoutIndex = 0; layoutIndex < binder.DescriptorReflection.Layouts.Count; layoutIndex++)
                {
                    var layout = binder.DescriptorReflection.Layouts[layoutIndex].Layout;

                    parameterCollectionLayout.ProcessResources(layout);

                    for (int entryIndex = 0; entryIndex < layout.Entries.Count; ++entryIndex)
                    {
                        var layoutEntry = layout.Entries[entryIndex];
                        if (layoutEntry.Class == EffectParameterClass.ConstantBuffer)
                        {
                            var constantBuffer = effect.Bytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Key.Name);
                            constantBuffers.Add(new ConstantBufferInfo { DescriptorSet = layoutIndex, BindingSlot = entryIndex, DataOffset = parameterCollectionLayout.BufferSize, Description = constantBuffer });

                            parameterCollectionLayout.ProcessConstantBuffer(constantBuffer);
                        }
                    }
                }

                resourceGroups = new ResourceGroup[binder.DescriptorReflection.Layouts.Count];
                resourceGroupLayouts = new ResourceGroupLayout[binder.DescriptorReflection.Layouts.Count];
                for (int i = 0; i < binder.DescriptorReflection.Layouts.Count; ++i)
                {
                    var name = binder.DescriptorReflection.Layouts[i].Name;
                    var layout = binder.DescriptorReflection.Layouts[i].Layout;
                    resourceGroupLayouts[i] = ResourceGroupLayout.New(graphicsDevice, layout, effect.Bytecode, name);
                    resourceGroups[i] = new ResourceGroup();
                }

                // Update parameters layout to match what this effect expect
                Parameters.UpdateLayout(parameterCollectionLayout);
                constantBufferTotalSize = parameterCollectionLayout.BufferSize;
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
            for (int i = 0; i < resourceGroups.Length; ++i)
            {
                RootEffectRenderFeature.PrepareResourceGroup(null, resourceGroupLayouts[i], BufferPoolAllocationType.UsedOnce, resourceGroups[i]);
            }

            // Set resources
            if (Parameters.ResourceValues != null)
            {
                var descriptorStartSlot = 0;
                for (int layoutIndex = 0; layoutIndex < binder.DescriptorReflection.Layouts.Count; layoutIndex++)
                {
                    var descriptorSet = resourceGroups[layoutIndex].DescriptorSet;
                    var layout = binder.DescriptorReflection.Layouts[layoutIndex].Layout;

                    for (int resourceSlot = 0; resourceSlot < layout.ElementCount; ++resourceSlot)
                    {
                        descriptorSet.SetValue(resourceSlot, Parameters.ResourceValues[descriptorStartSlot + resourceSlot]);
                    }

                    descriptorStartSlot += layout.ElementCount;
                }
            }

            // Copy cbuffer data
            if (Parameters.DataValues != IntPtr.Zero && constantBuffers.Count > 0)
            {
                var bufferPool = BufferPool.New(graphicsDevice, constantBufferTotalSize);
                Utilities.CopyMemory(bufferPool.Data, Parameters.DataValues, constantBufferTotalSize);
                foreach (var constantBuffer in constantBuffers)
                {
                    var descriptorSet = resourceGroups[constantBuffer.DescriptorSet].ConstantBuffer.Data;
                    throw new NotImplementedException();
                    //descriptorSet.SetConstantBuffer(constantBuffer.BindingSlot, bufferPool.Buffer, constantBuffer.DataOffset, constantBuffer.Description.Size);
                }
            }

            // Apply
            //binder.Apply(graphicsDevice, descriptorSets, 0);
            throw new NotImplementedException();
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