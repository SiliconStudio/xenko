// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpVulkan;
using SiliconStudio.Xenko.Shaders;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DescriptorSetLayout // TODO VULKAN API: GraphicsResource
    {
        internal struct BindingInfo
        {
            public bool HasImmutableSampler;
        }

        internal readonly SharpVulkan.DescriptorSetLayout NativeLayout;
        internal readonly BindingInfo[] Bindings;

        private DescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
            NativeLayout = CreateNativeDescriptorSetLayout(device, builder, out Bindings);
        }

        internal static unsafe SharpVulkan.DescriptorSetLayout CreateNativeDescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder, out BindingInfo[] bindingInfos)
        {
            var bindings = new DescriptorSetLayoutBinding[builder.Entries.Count];
            bindingInfos = new BindingInfo[builder.Entries.Count];

            int offset = 0;
            for (int i = 0; i < builder.Entries.Count; i++)
            {
                var entry = builder.Entries[i];

                bindings[i] = new DescriptorSetLayoutBinding
                {
                    DescriptorType = VulkanConvertExtensions.ConvertDescriptorType(entry.Class),
                    StageFlags = ShaderStageFlags.All, // TODO VULKAN: Filter?
                    Binding = (uint)i,
                    DescriptorCount = (uint)entry.ArraySize
                };

                if (entry.Class == EffectParameterClass.ShaderResourceView && entry.ImmutableSampler != null)
                {
                    // TODO VULKAN: Handle immutable samplers for DescriptorCount > 1
                    if (entry.ArraySize > 1)
                    {
                        throw new NotImplementedException();
                    }

                    var immutableSampler = entry.ImmutableSampler.NativeSampler;
                    bindings[i].DescriptorType = DescriptorType.CombinedImageSampler;
                    bindings[i].ImmutableSamplers = new IntPtr(&immutableSampler);

                    // Remember this, so we can choose the right DescriptorType in DescriptorSet.SetShaderResourceView
                    bindingInfos[i].HasImmutableSampler = true;
                }

                offset += entry.ArraySize;
            }

            fixed (DescriptorSetLayoutBinding* bindingsPointer = &bindings[0])
            {
                var createInfo = new DescriptorSetLayoutCreateInfo
                {
                    StructureType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint)bindings.Length,
                    Bindings = new IntPtr(bindingsPointer)
                };
                return device.NativeDevice.CreateDescriptorSetLayout(ref createInfo);
            }
        }
    }
}
#endif