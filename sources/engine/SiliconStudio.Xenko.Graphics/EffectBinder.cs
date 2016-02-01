using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes how DescriptorSet maps to real resource binding.
    /// This might become a core part of <see cref="Graphics.Effect"/> at some point.
    /// </summary>
    public struct EffectBinder
    {
        public EffectDescriptorSetReflection DescriptorReflection;

        private ResourceGroupBinding[] resourceGroupBindings;

        public void Compile(GraphicsDevice graphicsDevice, EffectBytecode effectBytecode, List<string> effectDescriptorSetSlots)
        {
            // Find resource groups
            // TODO: We should precompute most of that at compile time in BytecodeReflection
            // just waiting for format to be more stable
            var descriptorSetLayouts = new EffectDescriptorSetReflection();
            foreach (var effectDescriptorSetSlot in effectDescriptorSetSlots)
            {
                descriptorSetLayouts.AddLayout(effectDescriptorSetSlot ?? "Globals", InitializeDescriptorSet(effectBytecode, effectDescriptorSetSlot));
            }

            DescriptorReflection = descriptorSetLayouts;

            resourceGroupBindings = new ResourceGroupBinding[DescriptorReflection.Layouts.Count];
            for (int setIndex = 0; setIndex < DescriptorReflection.Layouts.Count; setIndex++)
            {
                var layout = DescriptorReflection.Layouts[setIndex].Layout;

                var resourceGroupBinding = new ResourceGroupBinding();
                var bindingOperations = new List<BindingOperation>();

                for (int resourceIndex = 0; resourceIndex < layout.Entries.Count; resourceIndex++)
                {
                    var layoutEntry = layout.Entries[resourceIndex];

                    // Find it in shader reflection
                    bool bindingFound = false;
                    Buffer preallocatedCBuffer = null;
                    foreach (var resourceBinding in effectBytecode.Reflection.ResourceBindings)
                    {
                        if (resourceBinding.Param.Key == layoutEntry.Key)
                        {
                            if (!bindingFound)
                            {
                                bindingFound = true;

                                // If it's a cbuffer and API without cbuffer offset, we need to preallocate a real cbuffer for emulation
                                if (resourceBinding.Param.Class == EffectParameterClass.ConstantBuffer)
                                {
                                    var constantBuffer = effectBytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Key.Name);
                                    resourceGroupBinding.ConstantBufferSlot = resourceIndex;
                                    resourceGroupBinding.ConstantBufferPreallocated = Buffer.Cosntant.New(graphicsDevice, constantBuffer.Size);

                                }
                            }

                            bindingOperations.Add(new BindingOperation
                            {
                                EntryIndex = resourceIndex,
                                Class = resourceBinding.Param.Class,
                                Stage = resourceBinding.Stage,
                                SlotStart = resourceBinding.SlotStart,
                            });
                        }
                    }
                }

                resourceGroupBinding.ResourceBindingOperations = bindingOperations.Count > 0 ? bindingOperations.ToArray() : null;
                resourceGroupBindings[setIndex] = resourceGroupBinding;
            }
        }

        internal void Apply(GraphicsDevice graphicsDevice, ResourceGroup[] resourceGroups, int resourceGroupsOffset)
        {
            if (resourceGroupBindings.Length == 0)
                return;

            var resourceGroupBinding = Interop.Pin(ref resourceGroupBindings[0]);
            for (int i = 0; i < resourceGroupBindings.Length; i++, resourceGroupBinding = Interop.IncrementPinned(resourceGroupBinding))
            {
                var resourceGroup = resourceGroups[resourceGroupsOffset + i];
                var bindingOperations = resourceGroupBinding.ResourceBindingOperations;
                if (bindingOperations == null)
                    continue;

                // Upload cbuffer (if not done yet)
                if (resourceGroup.ConstantBuffer.Data != IntPtr.Zero)
                {
                    var preallocatedBuffer = resourceGroup.ConstantBuffer.Buffer;
                    bool needUpdate = true;
                    if (preallocatedBuffer == null)
                        preallocatedBuffer = resourceGroupBinding.ConstantBufferPreallocated; // If it's preallocated buffer, we always upload
                    else if (resourceGroup.ConstantBuffer.Uploaded)
                        needUpdate = false; // If it's not preallocated and already uploaded, we can avoid uploading it again
                    else
                        resourceGroup.ConstantBuffer.Uploaded = true; // First time it is uploaded

                    if (needUpdate)
                    {
                        var mappedConstantBuffer = graphicsDevice.MapSubresource(preallocatedBuffer, 0, MapMode.WriteDiscard);
                        Utilities.CopyMemory(mappedConstantBuffer.DataBox.DataPointer, resourceGroup.ConstantBuffer.Data, resourceGroup.ConstantBuffer.Size);
                        graphicsDevice.UnmapSubresource(mappedConstantBuffer);
                    }

                    resourceGroup.DescriptorSet.SetConstantBuffer(resourceGroupBinding.ConstantBufferSlot, preallocatedBuffer, 0, resourceGroup.ConstantBuffer.Size);
                }

                var descriptorSet = resourceGroup.DescriptorSet;
                var bindingOperation = Interop.Pin(ref bindingOperations[0]);
                for (int index = 0; index < bindingOperations.Length; index++, bindingOperation = Interop.IncrementPinned(bindingOperation))
                {
                    var value = descriptorSet.HeapObjects[descriptorSet.DescriptorStartOffset + bindingOperation.EntryIndex];
                    switch (bindingOperation.Class)
                    {
                        case EffectParameterClass.ConstantBuffer:
                        {
                            graphicsDevice.SetConstantBuffer(bindingOperation.Stage, bindingOperation.SlotStart, (Buffer)value.Value);
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
                .GroupBy(x => new { Key = x.Param.Key, Class = x.Param.Class, SlotCount = x.SlotCount })
                .OrderBy(x => x.Key.Class == EffectParameterClass.ConstantBuffer ? 0 : 1))
            {
                // Note: Putting cbuffer first for now
                descriptorSetLayoutBuilder.AddBinding(resourceBinding.Key.Key, resourceBinding.Key.Class, resourceBinding.Key.SlotCount);
            }

            return descriptorSetLayoutBuilder;
        }

        internal struct ResourceGroupBinding
        {
            public BindingOperation[] ResourceBindingOperations;

            // Constant buffer
            public int ConstantBufferSlot;
            public Buffer ConstantBufferPreallocated;
        }

        internal struct BindingOperation
        {
            public int EntryIndex;
            public EffectParameterClass Class;
            public ShaderStage Stage;
            public int SlotStart;
        }
    }
}