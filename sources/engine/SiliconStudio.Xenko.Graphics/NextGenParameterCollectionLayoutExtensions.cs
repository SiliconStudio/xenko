// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public static class NextGenParameterCollectionLayoutExtensions
    {
        public static void ProcessResources(this NextGenParameterCollectionLayout parameterCollectionLayout, DescriptorSetLayoutBuilder layout)
        {
            foreach (var layoutEntry in layout.Entries)
            {
                parameterCollectionLayout.LayoutParameterKeyInfos.Add(new ParameterKeyInfo(layoutEntry.Key, parameterCollectionLayout.ResourceCount++));
            }
        }

        public static void ProcessConstantBuffer(this NextGenParameterCollectionLayout parameterCollectionLayout, ShaderConstantBufferDescription constantBuffer)
        {
            foreach (var member in constantBuffer.Members)
            {
                parameterCollectionLayout.LayoutParameterKeyInfos.Add(new ParameterKeyInfo(member.Param.Key, parameterCollectionLayout.BufferSize + member.Offset, member.Size));
            }
            parameterCollectionLayout.BufferSize += constantBuffer.Size;
        }
    }

    /// <summary>
    /// Allocator for resource groups.
    /// </summary>
    /// <note>Non thread-safe. You should have one such allocator per thread.</note>
    public class ResourceGroupAllocator : ComponentBase
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly List<DescriptorPool> descriptorPools = new List<DescriptorPool>();
        private readonly List<BufferPool> bufferPools = new List<BufferPool>();

        private readonly List<ResourceGroup> resourceGroupPool = new List<ResourceGroup>();
        private int currentResourceGroupPoolIndex = 0;

        private DescriptorPool currentDescriptorPool;
        private int currentDescriptorPoolIndex = -1;

        private BufferPool currentBufferPool;
        private int currentBufferPoolIndex = -1;

        public ResourceGroupAllocator(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            SetupNextDescriptorPool();
            SetupNextBufferPool();
        }

        public void Reset()
        {
            foreach (var descriptorPool in descriptorPools)
            {
                descriptorPool.Reset();
            }
            foreach (var bufferPool in bufferPools)
            {
                bufferPool.Reset();
            }

            currentResourceGroupPoolIndex = -1;

            currentDescriptorPool = descriptorPools[0];
            currentDescriptorPoolIndex = 0;

            currentBufferPool = bufferPools[0];
            currentBufferPoolIndex = 0;
        }

        public ResourceGroup AllocateResourceGroup()
        {
            ResourceGroup resourceGroup;
            if (++currentResourceGroupPoolIndex >= resourceGroupPool.Count)
            {
                resourceGroupPool.Add(resourceGroup = new ResourceGroup());
            }
            else
            {
                resourceGroup = resourceGroupPool[currentResourceGroupPoolIndex];
            }
            return resourceGroup;
        }

        public void PrepareResourceGroup(ResourceGroupLayout resourceGroupLayout, BufferPoolAllocationType constantBufferAllocationType, ResourceGroup resourceGroup)
        {
            if (resourceGroup == null)
                throw new InvalidOperationException();

            resourceGroup.DescriptorSet = DescriptorSet.New(graphicsDevice, currentDescriptorPool, resourceGroupLayout.DescriptorSetLayout);
            if (!resourceGroup.DescriptorSet.IsValid)
            {
                SetupNextDescriptorPool();
            }

            if (resourceGroupLayout.ConstantBufferSize > 0)
            {
                if (!currentBufferPool.CanAllocate(resourceGroupLayout.ConstantBufferSize))
                {
                    SetupNextBufferPool();
                }

                currentBufferPool.Allocate(graphicsDevice, resourceGroupLayout.ConstantBufferSize, constantBufferAllocationType, ref resourceGroup.ConstantBuffer);
            }
        }

        private void SetupNextBufferPool()
        {
            currentBufferPoolIndex++;
            if (currentBufferPoolIndex >= bufferPools.Count)
            {
                bufferPools.Add(currentBufferPool = BufferPool.New(graphicsDevice, 1024*1024));
            }
            else
            {
                currentBufferPool = bufferPools[currentDescriptorPoolIndex];
            }
        }

        private void SetupNextDescriptorPool()
        {
            currentDescriptorPoolIndex++;
            if (currentDescriptorPoolIndex >= descriptorPools.Count)
            {
                descriptorPools.Add(currentDescriptorPool = DescriptorPool.New(graphicsDevice, new[]
                {
                    new DescriptorTypeCount(EffectParameterClass.ConstantBuffer, 16384),
                    new DescriptorTypeCount(EffectParameterClass.ShaderResourceView, 65536),
                    new DescriptorTypeCount(EffectParameterClass.UnorderedAccessView, 4096),
                }));
            }
            else
            {
                currentDescriptorPool = descriptorPools[currentDescriptorPoolIndex];
            }
        }
    }
}