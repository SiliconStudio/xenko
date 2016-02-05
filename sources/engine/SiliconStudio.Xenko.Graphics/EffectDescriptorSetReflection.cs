using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public class EffectDescriptorSetReflection
    {
        public static EffectDescriptorSetReflection New(GraphicsDevice graphicsDevice, EffectBytecode effectBytecode, List<string> effectDescriptorSetSlots)
        {
            // Find resource groups
            // TODO: We should precompute most of that at compile time in BytecodeReflection
            // just waiting for format to be more stable
            var descriptorSetLayouts = new EffectDescriptorSetReflection();
            foreach (var effectDescriptorSetSlot in effectDescriptorSetSlots)
            {
                // Find all resources related to this slot name
                var descriptorSetLayoutBuilder = new DescriptorSetLayoutBuilder();
                foreach (var resourceBinding in effectBytecode.Reflection.ResourceBindings
                    .Where(x => x.Param.ResourceGroup == effectDescriptorSetSlot || (effectDescriptorSetSlot == "Globals" && x.Param.ResourceGroup == null))
                    .GroupBy(x => new { Key = x.Param.Key, Class = x.Param.Class, SlotCount = x.SlotCount })
                    .OrderBy(x => x.Key.Class == EffectParameterClass.ConstantBuffer ? 0 : 1)) // Note: Putting cbuffer first for now
                {
                    SamplerState samplerState = null;
                    if (resourceBinding.Key.Class == EffectParameterClass.Sampler)
                    {
                        var matchingSamplerState = effectBytecode.Reflection.SamplerStates.FirstOrDefault(x => x.Key == resourceBinding.Key.Key);
                        if (matchingSamplerState != null)
                            samplerState = SamplerState.New(graphicsDevice, matchingSamplerState.Description);
                    }
                    descriptorSetLayoutBuilder.AddBinding(resourceBinding.Key.Key, resourceBinding.Key.Class, resourceBinding.Key.SlotCount, samplerState);
                }

                descriptorSetLayouts.AddLayout(effectDescriptorSetSlot, descriptorSetLayoutBuilder);
            }

            return descriptorSetLayouts;
        }

        internal List<LayoutEntry> Layouts { get; } = new List<LayoutEntry>();

        public DescriptorSetLayoutBuilder GetLayout(string name)
        {
            foreach (var entry in Layouts)
            {
                if (entry.Name == name)
                    return entry.Layout;
            }

            return null;
        }

        public int GetLayoutIndex(string name)
        {
            for (int index = 0; index < Layouts.Count; index++)
            {
                if (Layouts[index].Name == name)
                    return index;
            }

            return -1;
        }

        public void AddLayout(string descriptorSetName, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder)
        {
            Layouts.Add(new LayoutEntry(descriptorSetName, descriptorSetLayoutBuilder));
        }

        internal struct LayoutEntry
        {
            public string Name;
            public DescriptorSetLayoutBuilder Layout;

            public LayoutEntry(string name, DescriptorSetLayoutBuilder layout)
            {
                Name = name;
                Layout = layout;
            }
        }
    }
}