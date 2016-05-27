// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using SharpVulkan;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DescriptorPool
    {
        //internal DescriptorHeap SrvHeap;
        //internal DescriptorHeap SamplerHeap;

        private readonly DescriptorTypeCount[] counts;
        internal int Offset;
        internal int Count;

        internal SharpVulkan.DescriptorPool NativeDescriptorPool;

        public void Reset()
        {
            Offset = 0;
            GraphicsDevice.NativeDevice.ResetDescriptorPool(NativeDescriptorPool, DescriptorPoolResetFlags.None);
        }

        private DescriptorPool(GraphicsDevice graphicsDevice, DescriptorTypeCount[] counts) : base(graphicsDevice)
        {
            this.counts = counts;
            Recreate();
        }

        private unsafe void Recreate()
        {
            // For now, we put everything together so let's compute total count
            Count = counts.Length;

            var poolSizes = new[]
            {
                new DescriptorPoolSize { Type = DescriptorType.UniformBuffer, DescriptorCount = 1 << 15 },
                new DescriptorPoolSize { Type = DescriptorType.Sampler, DescriptorCount = 1 << 12 },
                new DescriptorPoolSize { Type = DescriptorType.SampledImage, DescriptorCount = 1 << 15 },
                new DescriptorPoolSize { Type = DescriptorType.UniformTexelBuffer, DescriptorCount = 1 << 10 },
            };

            //var poolSizes = new DescriptorPoolSize[Count];
            //for (int i = 0; i < Count; i++)
            //{
            //    poolSizes[i] = new DescriptorPoolSize
            //    {
            //        Type = VulkanConvertExtensions.ConvertDescriptorType(counts[i].Type),
            //        DescriptorCount = (uint)counts[i].Count
            //    };
            //}

            fixed (DescriptorPoolSize* poolSizesPointer = &poolSizes[0])
            {
                var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
                {
                    StructureType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = (uint)Count,
                    PoolSizes = new IntPtr(poolSizesPointer),
                    MaxSets = 16384, // TODO VULKAN API: Expose
                };

                NativeDescriptorPool = GraphicsDevice.NativeDevice.CreateDescriptorPool(ref descriptorPoolCreateInfo);
            }
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            Recreate();
            return true;
        }

        /// <inheritdoc/>
        protected internal override unsafe void OnDestroyed()
        {
            // TODO VULKAN: Defer?
            GraphicsDevice.NativeDevice.DestroyDescriptorPool(NativeDescriptorPool);
            
            base.OnDestroyed();
        }
    }
}
#endif