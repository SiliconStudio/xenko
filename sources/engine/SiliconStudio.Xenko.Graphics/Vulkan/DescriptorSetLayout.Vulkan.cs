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
        internal readonly SharpVulkan.DescriptorSetLayout NativeLayout;

        private DescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
            NativeLayout = CreateNativeDescriptorSetLayout(device, builder);
        }

        internal static unsafe SharpVulkan.DescriptorSetLayout CreateNativeDescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
            var bindings = new DescriptorSetLayoutBinding[builder.Entries.Count];

            for (int i = 0; i < builder.Entries.Count; i++)
            {
                var entry = builder.Entries[i];
                var immutableSampler = entry.ImmutableSampler != null ? entry.ImmutableSampler.NativeSampler : Sampler.Null;

                bindings[i] = new DescriptorSetLayoutBinding
                {
                    DescriptorType = VulkanConvertExtensions.ConvertDescriptorType(entry.Class),
                    StageFlags = ShaderStageFlags.All, // TODO VULKAN: Filter
                    Binding = (uint)i,
                    DescriptorCount = (uint)entry.ArraySize,
                    ImmutableSamplers = new IntPtr(&immutableSampler)
                };
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