using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace RenderArchitecture
{
    public class ResourceGroupLayout
    {
        public DescriptorSetLayout DescriptorSetLayout;
        public int ConstantBufferSlot;
        public int ConstantBufferSize;
        public ShaderConstantBufferDescription ConstantBufferReflection;
        public ObjectId ConstantBufferHash;

        internal int[] ConstantBufferOffsets;
        internal int[] ResourceIndices;

        public int GetConstantBufferOffset(ConstantBufferOffsetReference offsetReference)
        {
            return ConstantBufferOffsets[offsetReference.Index];
        }

        public static ResourceGroupLayout New(GraphicsDevice graphicsDevice, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder, EffectBytecode effectBytecode, string cbufferName)
        {
            // TODO: This code might need some improvements (waiting to have better visibility on how we define resource groups and descriptor layouts)
            // TODO: For now, assume cbuffer is always in slot 0 (if it exists)
            var constantBufferSlot = 0;

            var result = new ResourceGroupLayout
            {
                DescriptorSetLayout = DescriptorSetLayout.New(graphicsDevice, descriptorSetLayoutBuilder),
                ConstantBufferSlot = constantBufferSlot,
                ConstantBufferReflection = effectBytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == cbufferName),
            };

            if (result.ConstantBufferReflection != null)
            {
                result.ConstantBufferSize = result.ConstantBufferReflection.Size;
                result.ConstantBufferHash = result.ConstantBufferReflection.Hash;
            }

            return result;
        }
    }

    public struct ResourceGroupEntry
    {
        public int LastFrameUsed;
        public ResourceGroup ResourceGroup;

        /// <summary>
        /// Mark resource group as used during this frame.
        /// </summary>
        /// <returns>True if state changed (object was not mark as used during this frame until now), otherwise false.</returns>
        public bool MarkAsUsed(NextGenRenderSystem renderSystem)
        {
            if (LastFrameUsed == renderSystem.FrameCounter)
                return false;

            LastFrameUsed = renderSystem.FrameCounter;
            return true;
        }
    }

    public class FrameResourceGroupLayout : ResourceGroupLayout
    {
        public ResourceGroupEntry Entry;
    }

    public class ViewResourceGroupLayout : ResourceGroupLayout
    {
        public ResourceGroupEntry[] Entries;
    }
}