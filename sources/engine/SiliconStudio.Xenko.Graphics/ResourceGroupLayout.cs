using System.Linq;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class ResourceGroupLayout
    {
        public DescriptorSetLayout DescriptorSetLayout;
        public int ConstantBufferSize;
        public ShaderConstantBufferDescription ConstantBufferReflection;
        public ObjectId ConstantBufferHash;

        public static ResourceGroupLayout New(GraphicsDevice graphicsDevice, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder, EffectBytecode effectBytecode, string cbufferName)
        {
            return New<ResourceGroupLayout>(graphicsDevice, descriptorSetLayoutBuilder, effectBytecode, cbufferName);
        }

        public static ResourceGroupLayout New<T>(GraphicsDevice graphicsDevice, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder, EffectBytecode effectBytecode, string cbufferName) where T : ResourceGroupLayout, new()
        {
            var result = new T
            {
                DescriptorSetLayout = DescriptorSetLayout.New(graphicsDevice, descriptorSetLayoutBuilder),
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
}