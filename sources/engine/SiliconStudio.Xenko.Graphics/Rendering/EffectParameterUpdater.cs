// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Helper class to update several <see cref="ResourceGroup"/> from a <see cref="ParameterCollection"/>.
    /// </summary>
    public struct EffectParameterUpdater
    {
        private EffectParameterUpdaterLayout updaterLayout;
        private ResourceGroup[] resourceGroups;

        public ResourceGroup[] ResourceGroups => resourceGroups;

        public EffectParameterUpdater(EffectParameterUpdaterLayout updaterLayout, ParameterCollection parameters) : this()
        {
            this.updaterLayout = updaterLayout;

            this.resourceGroups = new ResourceGroup[updaterLayout.Layouts.Length];
            for (int i = 0; i < resourceGroups.Length; ++i)
                resourceGroups[i] = new ResourceGroup();

            parameters.UpdateLayout(updaterLayout.ParameterCollectionLayout);
        }

        public unsafe void Update(GraphicsDevice graphicsDevice, ResourceGroupAllocator resourceGroupAllocator, ParameterCollection parameters)
        {
            // Instantiate descriptor sets
            for (int i = 0; i < resourceGroups.Length; ++i)
            {
                var resourceGroupLayout = updaterLayout.ResourceGroupLayouts[i];
                if (resourceGroupLayout != null)
                    resourceGroupAllocator.PrepareResourceGroup(resourceGroupLayout, BufferPoolAllocationType.UsedOnce, resourceGroups[i]);
            }

            // Set resources
            var layouts = updaterLayout.Layouts;
            var descriptorStartSlot = 0;
            var bufferStartOffset = 0;
            for (int layoutIndex = 0; layoutIndex < layouts.Length; layoutIndex++)
            {
                var resourceGroup = resourceGroups[layoutIndex];
                var descriptorSet = resourceGroup.DescriptorSet;
                var layout = layouts[layoutIndex];
                if (layout == null)
                    continue;

                if (parameters.ObjectValues != null)
                {
                    for (int resourceSlot = 0; resourceSlot < layout.ElementCount; ++resourceSlot)
                    {
                        descriptorSet.SetValue(resourceSlot, parameters.ObjectValues[descriptorStartSlot + resourceSlot]);
                    }
                }

                descriptorStartSlot += layout.ElementCount;

                if (parameters.DataValues != null && resourceGroup.ConstantBuffer.Size > 0)
                {
                    fixed (byte* dataValues = parameters.DataValues)
                        Utilities.CopyMemory(resourceGroup.ConstantBuffer.Data, (IntPtr)dataValues + bufferStartOffset, resourceGroup.ConstantBuffer.Size);
                    bufferStartOffset += resourceGroup.ConstantBuffer.Size;
                }
            }
        }
    }
}