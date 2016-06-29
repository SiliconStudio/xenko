// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public struct ResourceGroupDescription
    {
        public readonly DescriptorSetLayoutBuilder DescriptorSetLayout;
        public readonly EffectConstantBufferDescription ConstantBufferReflection;
        public readonly ObjectId Hash;

        public ResourceGroupDescription(DescriptorSetLayoutBuilder descriptorSetLayout, EffectConstantBufferDescription constantBufferReflection) : this()
        {
            DescriptorSetLayout = descriptorSetLayout;
            ConstantBufferReflection = constantBufferReflection;

            // We combine both hash for DescriptorSet and cbuffer itself (if it exists)
            Hash = descriptorSetLayout.Hash;
            if (constantBufferReflection != null)
                ObjectId.Combine(ref Hash, ref constantBufferReflection.Hash, out Hash);
        }
    }

    public class ResourceGroupLayout
    {
        public DescriptorSetLayoutBuilder DescriptorSetLayoutBuilder;
        public DescriptorSetLayout DescriptorSetLayout;
        public int ConstantBufferSize;
        public EffectConstantBufferDescription ConstantBufferReflection;
        public ObjectId Hash;
        public ObjectId ConstantBufferHash;

        public static ResourceGroupLayout New(GraphicsDevice graphicsDevice, ResourceGroupDescription resourceGroupDescription, EffectBytecode effectBytecode)
        {
            return New<ResourceGroupLayout>(graphicsDevice, resourceGroupDescription, effectBytecode);
        }

        public static ResourceGroupLayout New<T>(GraphicsDevice graphicsDevice, ResourceGroupDescription resourceGroupDescription, EffectBytecode effectBytecode) where T : ResourceGroupLayout, new()
        {
            var result = new T
            {
                DescriptorSetLayoutBuilder = resourceGroupDescription.DescriptorSetLayout,
                DescriptorSetLayout = DescriptorSetLayout.New(graphicsDevice, resourceGroupDescription.DescriptorSetLayout),
                ConstantBufferReflection = resourceGroupDescription.ConstantBufferReflection,
                Hash = resourceGroupDescription.Hash,
            };

            if (result.ConstantBufferReflection != null)
            {
                result.ConstantBufferSize = result.ConstantBufferReflection.Size;
                result.ConstantBufferHash = result.ConstantBufferReflection.Hash;
            }

            return result;
        }
    }
}