using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace RenderArchitecture
{
    /// <summary>
    /// Describes how DescriptorSet maps to real resource binding.
    /// This might become a core part of <see cref="Effect"/> at some point.
    /// </summary>
    public struct EffectBinder
    {
        public EffectDescriptorSetReflection DescriptorReflection;

        private BindingOperation[][] bindingOperationSets;

        public void Compile(GraphicsDevice graphicsDevice, EffectBytecode effectBytecode, List<string> effectDescriptorSetSlots)
        {
            // Find resource groups
            // TODO: We should precompute most of that at compile time in BytecodeReflection
            // just waiting for format to be more stable
            var descriptorSetLayouts = new EffectDescriptorSetReflection();
            foreach (var effectDescriptorSetSlot in effectDescriptorSetSlots)
            {
                descriptorSetLayouts.AddLayout(effectDescriptorSetSlot, InitializeDescriptorSet(effectBytecode, effectDescriptorSetSlot));
            }

            DescriptorReflection = descriptorSetLayouts;

            bindingOperationSets = new BindingOperation[DescriptorReflection.Layouts.Count][];
            for (int setIndex = 0; setIndex < DescriptorReflection.Layouts.Count; setIndex++)
            {
                var layout = DescriptorReflection.Layouts[setIndex].Layout;

                var bindingOperations = new List<BindingOperation>();

                for (int resourceIndex = 0; resourceIndex < layout.Entries.Count; resourceIndex++)
                {
                    var layoutEntry = layout.Entries[resourceIndex];

                    // Find it in shader reflection
                    bool bindingFound = false;
                    Buffer preallocatedCBuffer = null;
                    foreach (var resourceBinding in effectBytecode.Reflection.ResourceBindings)
                    {
                        if (resourceBinding.Param.KeyName == layoutEntry.Name)
                        {
                            if (!bindingFound)
                            {
                                bindingFound = true;

                                // If it's a cbuffer and API without cbuffer offset, we need to preallocate a real cbuffer for emulation
                                if (resourceBinding.Param.Class == EffectParameterClass.ConstantBuffer)
                                {
                                    var constantBuffer = effectBytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Name);
                                    preallocatedCBuffer = Buffer.Cosntant.New(graphicsDevice, constantBuffer.Size);
                                }
                            }

                            bindingOperations.Add(new BindingOperation
                            {
                                EntryIndex = resourceIndex,
                                Class = resourceBinding.Param.Class,
                                Stage = resourceBinding.Stage,
                                SlotStart = resourceBinding.SlotStart,
                                PreallocatedCBuffer = preallocatedCBuffer,
                            });
                        }
                    }
                }

                bindingOperationSets[setIndex] = bindingOperations.ToArray();
            }
        }

        internal void Apply(GraphicsDevice graphicsDevice, DescriptorSet[] descriptorSets, int descriptorSetOffset)
        {
            for (int i = 0; i < bindingOperationSets.Length; i++)
            {
                var bindingOperations = this.bindingOperationSets[i];
                var descriptorSet = descriptorSets[descriptorSetOffset + i];

                for (int index = 0; index < bindingOperations.Length; index++)
                {
                    var bindingOperation = bindingOperations[index];

                    var value = descriptorSet.HeapObjects[descriptorSet.DescriptorStartOffset + bindingOperation.EntryIndex];
                    switch (bindingOperation.Class)
                    {
                        case EffectParameterClass.ConstantBuffer:
                        {
                            // Update cbuffer
                            var constantBuffer2 = (ConstantBuffer2)value.Value;
                            var mappedConstantBuffer = graphicsDevice.MapSubresource(bindingOperation.PreallocatedCBuffer, 0, MapMode.WriteDiscard);
                            var sourceData = constantBuffer2.Data + value.Offset;
                            Utilities.CopyMemory(mappedConstantBuffer.DataBox.DataPointer, sourceData, value.Size);
                            graphicsDevice.UnmapSubresource(mappedConstantBuffer);

                            graphicsDevice.SetConstantBuffer(bindingOperation.Stage, bindingOperation.SlotStart, bindingOperation.PreallocatedCBuffer);
                            break;
                        }
                        case EffectParameterClass.Sampler:
                        {
                            graphicsDevice.SetSamplerState(bindingOperation.Stage, bindingOperation.SlotStart, (SamplerState)value.Value);
                            break;
                        }
                        case EffectParameterClass.ShaderResourceView:
                        {
                            graphicsDevice.SetShaderResourceView(bindingOperation.Stage, bindingOperation.SlotStart, (GraphicsResource)value.Value);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private static DescriptorSetLayoutBuilder InitializeDescriptorSet(EffectBytecode effectBytecode, string descriptorSetName)
        {
            var descriptorSetLayoutBuilder = new DescriptorSetLayoutBuilder();
            foreach (var resourceBinding in effectBytecode.Reflection.ResourceBindings
                .Where(x => x.Param.ResourceGroup == descriptorSetName)
                .GroupBy(x => new { Name = x.Param.KeyName, Class = x.Param.Class, SlotCount = x.SlotCount })
                .OrderBy(x => x.Key.Class == EffectParameterClass.ConstantBuffer ? 0 : 1))
            {
                // Note: Putting cbuffer first for now
                descriptorSetLayoutBuilder.AddBinding(resourceBinding.Key.Name, resourceBinding.Key.Class, resourceBinding.Key.SlotCount);
            }

            return descriptorSetLayoutBuilder;
        }

        internal struct BindingOperation
        {
            public int EntryIndex;
            public EffectParameterClass Class;
            public ShaderStage Stage;
            public int SlotStart;
            public Buffer PreallocatedCBuffer;
        }
    }
}