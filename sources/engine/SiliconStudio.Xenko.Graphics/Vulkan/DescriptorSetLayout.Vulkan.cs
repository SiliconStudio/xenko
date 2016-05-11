// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Shaders;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DescriptorSetLayout
    {
        internal SharpVulkan.DescriptorSetLayout NativeLayout;
        internal Sampler[] ImmutableSamplers;

        private DescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder) : base(device)
        {
            NativeLayout = CreateNativeDescriptorSetLayout(device, builder.Entries, out ImmutableSamplers);
        }

        internal static unsafe SharpVulkan.DescriptorSetLayout CreateNativeDescriptorSetLayout(GraphicsDevice device, IList<DescriptorSetLayoutBuilder.Entry> entries, out Sampler[] immutableSamplers)
        {
            var bindings = new DescriptorSetLayoutBinding[entries.Count];
            immutableSamplers = new Sampler[entries.Count];

            fixed (Sampler* immutableSamplersPointer = &immutableSamplers[0])
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];

                    bindings[i] = new DescriptorSetLayoutBinding
                    {
                        DescriptorType = VulkanConvertExtensions.ConvertDescriptorType(entry.Class),
                        StageFlags = ShaderStageFlags.All, // TODO VULKAN: Filter?
                        Binding = (uint)i,
                        DescriptorCount = (uint)entry.ArraySize
                    };

                    if (entry.ImmutableSampler != null)
                    {
                        // TODO VULKAN: Handle immutable samplers for DescriptorCount > 1
                        if (entry.ArraySize > 1)
                        {
                            throw new NotImplementedException();
                        }

                        // Remember this, so we can choose the right DescriptorType in DescriptorSet.SetShaderResourceView
                        immutableSamplers[i] = entry.ImmutableSampler.NativeSampler;
                        //bindings[i].DescriptorType = DescriptorType.CombinedImageSampler;
                        bindings[i].ImmutableSamplers = new IntPtr(immutableSamplersPointer + i);
                    }
                }

                var createInfo = new DescriptorSetLayoutCreateInfo
                {
                    StructureType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint)bindings.Length,
                    Bindings = bindings.Length > 0 ? new IntPtr(Interop.Fixed(bindings)) : IntPtr.Zero
                };
                return device.NativeDevice.CreateDescriptorSetLayout(ref createInfo);
            }
        }

        protected internal override void OnDestroyed()
        {
            base.OnDestroyed();
            DestroyImpl();
        }

        protected override unsafe void DestroyImpl()
        {
            GraphicsDevice.NativeDevice.DestroyDescriptorSetLayout(NativeLayout);
            NativeLayout = SharpVulkan.DescriptorSetLayout.Null;

            base.DestroyImpl();
        }
    }
}
#endif