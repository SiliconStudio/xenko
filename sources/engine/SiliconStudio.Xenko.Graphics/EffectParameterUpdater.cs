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
                resourceGroupAllocator.PrepareResourceGroup(updaterLayout.ResourceGroupLayouts[i], BufferPoolAllocationType.UsedOnce, resourceGroups[i]);
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