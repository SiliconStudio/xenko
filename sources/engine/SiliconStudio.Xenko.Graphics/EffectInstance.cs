// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A dynamic effect instance, including its values and resources.
    /// </summary>
    public class EffectInstance : DisposeBase
    {
        // Parameter keys for shader values
        private int constantBufferTotalSize;

        // Descriptor sets
        private ResourceGroupLayout[] resourceGroupLayouts;
        private ResourceGroup[] resourceGroups;

        // Store current effect
        protected Effect effect;
        protected bool effectDirty = true;

        // Describes how to update resource bindings
        private EffectBinder binder;

        public EffectInstance(Effect effect)
        {
            this.effect = effect;
        }

        public Effect Effect => effect;

        public NextGenParameterCollection Parameters { get; } = new NextGenParameterCollection();

        protected override void Destroy()
        {
            base.Destroy();

            Parameters.Dispose();
        }

        public void UpdateEffect(GraphicsDevice graphicsDevice)
        {
            if (effectDirty)
            {
                effectDirty = false;

                ChooseEffect(graphicsDevice);

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

        protected virtual void ChooseEffect(GraphicsDevice graphicsDevice)
        {
        }

        public void Apply(GraphicsDevice graphicsDevice)
        {
            UpdateEffect(graphicsDevice);

            effect.ApplyProgram(graphicsDevice);

            // Bind resources
            // TODO: What descriptor pool should we use?
            var descriptorPool = DescriptorPool.New(graphicsDevice, new[]
            {
                new DescriptorTypeCount(EffectParameterClass.ConstantBuffer, 256),
            });

            var bufferPool = BufferPool.New(graphicsDevice, constantBufferTotalSize);

            // Instantiate descriptor sets
            for (int i = 0; i < resourceGroups.Length; ++i)
            {
                NextGenParameterCollectionLayoutExtensions.PrepareResourceGroup(graphicsDevice, descriptorPool, bufferPool, resourceGroupLayouts[i], BufferPoolAllocationType.UsedOnce, resourceGroups[i]);
            }

            // Set resources
            if (Parameters.ResourceValues != null)
            {
                var descriptorStartSlot = 0;
                for (int layoutIndex = 0; layoutIndex < binder.DescriptorReflection.Layouts.Count; layoutIndex++)
                {
                    var resourceGroup = resourceGroups[layoutIndex];
                    var descriptorSet = resourceGroup.DescriptorSet;
                    var layout = binder.DescriptorReflection.Layouts[layoutIndex].Layout;

                    for (int resourceSlot = 0; resourceSlot < layout.ElementCount; ++resourceSlot)
                    {
                        descriptorSet.SetValue(resourceSlot, Parameters.ResourceValues[descriptorStartSlot + resourceSlot]);
                    }

                    descriptorStartSlot += layout.ElementCount;

                    if (resourceGroup.ConstantBuffer.Size > 0)
                    {
                        Utilities.CopyMemory(resourceGroup.ConstantBuffer.Data, Parameters.DataValues, resourceGroup.ConstantBuffer.Size);
                    }
                }
            }

            // Apply
            binder.Apply(graphicsDevice, resourceGroups, 0);
        }


        struct ConstantBufferInfo
        {
            public int DescriptorSet;
            public int BindingSlot;

            /// <summary>
            /// Offset in <see cref="EffectInstance.dataValues"/>.
            /// </summary>
            public int DataOffset;
            public ShaderConstantBufferDescription Description;
        }
    }
}