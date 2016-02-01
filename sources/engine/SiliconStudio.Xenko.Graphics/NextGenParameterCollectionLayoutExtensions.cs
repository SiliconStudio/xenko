// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
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

        public static void PrepareResourceGroup(GraphicsDevice graphicsDevice, DescriptorPool descriptorPool, BufferPool bufferPool, ResourceGroupLayout resourceGroupLayout, BufferPoolAllocationType constantBufferAllocationType, ResourceGroup resourceGroup)
        {
            if (resourceGroup == null)
                throw new InvalidOperationException();

            resourceGroup.DescriptorSet = DescriptorSet.New(graphicsDevice, descriptorPool, resourceGroupLayout.DescriptorSetLayout);

            if (resourceGroupLayout.ConstantBufferSize > 0)
            {
                bufferPool.Allocate(graphicsDevice, resourceGroupLayout.ConstantBufferSize, constantBufferAllocationType, ref resourceGroup.ConstantBuffer);
            }
        }
    }
}