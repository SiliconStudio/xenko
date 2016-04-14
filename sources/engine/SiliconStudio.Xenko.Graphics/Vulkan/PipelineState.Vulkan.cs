// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;
using System.Linq;
using SharpVulkan;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Shaders;
using Encoding = System.Text.Encoding;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class PipelineState
    {
        internal SharpVulkan.DescriptorSetLayout[] NativeDescriptorSetLayouts;
        internal PipelineLayout NativeLayout;
        internal Pipeline NativePipeline;
        internal RenderPass NativeRenderPass;
        internal int[] ResourceGroupMapping;
        internal int ResourceGroupCount;
        internal Sampler[] ImmutableSamplers;

        // State exposed by the CommandList
        private static readonly DynamicState[] dynamicStates =
        {
            DynamicState.Viewport,
            DynamicState.Scissor,
            DynamicState.BlendConstants,
            DynamicState.StencilReference,
        };

        internal unsafe PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription) : base(graphicsDevice)
        {
            if (pipelineStateDescription.RootSignature == null)
                return;

            CreateRenderPass(pipelineStateDescription);

            CreatePipelineLayout(pipelineStateDescription);

            // Create shader stages
            Dictionary<int, string> inputAttributeNames;
            var stages = CreateShaderStages(pipelineStateDescription, out inputAttributeNames);

            var inputAttributes = new VertexInputAttributeDescription[pipelineStateDescription.InputElements.Length];
            int inputAttributeCount = 0;
            var inputBindings = new VertexInputBindingDescription[inputAttributes.Length];
            int inputBindingCount = 0;

            for (int inputElementIndex = 0; inputElementIndex < inputAttributes.Length; inputElementIndex++)
            {
                var inputElement = pipelineStateDescription.InputElements[inputElementIndex];
                var slotIndex = inputElement.InputSlot;

                if (inputElement.InstanceDataStepRate > 1)
                {
                    throw new NotImplementedException();
                }

                Format format;
                int size;
                bool isCompressed;
                VulkanConvertExtensions.ConvertPixelFormat(inputElement.Format, out format, out size, out isCompressed);

                var location = inputAttributeNames.FirstOrDefault(x => x.Value == inputElement.SemanticName && inputElement.SemanticIndex == 0 || x.Value == inputElement.SemanticName + inputElement.SemanticIndex);
                if (location.Value != null)
                {
                    inputAttributes[inputAttributeCount++] = new VertexInputAttributeDescription
                    {
                        Format = format,
                        Offset = (uint)inputElement.AlignedByteOffset,
                        Binding = (uint)inputElement.InputSlot,
                        Location = (uint)location.Key
                    };
                }

                inputBindings[slotIndex].Binding = (uint)slotIndex;
                inputBindings[slotIndex].InputRate = inputElement.InputSlotClass == InputClassification.Vertex ? VertexInputRate.Vertex : VertexInputRate.Instance;

                // TODO VULKAN: This is currently an argument to Draw() overloads.
                if (inputBindings[slotIndex].Stride < inputElement.AlignedByteOffset + size)
                    inputBindings[slotIndex].Stride = (uint)(inputElement.AlignedByteOffset + size);

                if (inputElement.InputSlot >= inputBindingCount)
                    inputBindingCount = inputElement.InputSlot + 1;
            }

            var inputAssemblyState = new PipelineInputAssemblyStateCreateInfo
            {
                StructureType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = VulkanConvertExtensions.ConvertPrimitiveType(pipelineStateDescription.PrimitiveType),
                PrimitiveRestartEnable = true,
            };

            // TODO VULKAN: Tessellation and multisampling
            var multisampleState = new PipelineMultisampleStateCreateInfo();
            var tessellationState = new PipelineTessellationStateCreateInfo();

            var rasterizationState = CreateRasterizationState(pipelineStateDescription.RasterizerState);

            var depthStencilState = CreateDepthStencilState(pipelineStateDescription);

            var description = pipelineStateDescription.BlendState;

            var renderTargetCount = pipelineStateDescription.Output.RenderTargetCount;
            var colorBlendAttachments = new PipelineColorBlendAttachmentState[renderTargetCount];

            var renderTargetBlendState = &description.RenderTarget0;
            for (int i = 0; i < renderTargetCount; i++)
            {
                colorBlendAttachments[i] = new PipelineColorBlendAttachmentState
                {
                    BlendEnable = renderTargetBlendState->BlendEnable,
                    AlphaBlendOperation = VulkanConvertExtensions.ConvertBlendFunction(renderTargetBlendState->AlphaBlendFunction),
                    ColorBlendOperation = VulkanConvertExtensions.ConvertBlendFunction(renderTargetBlendState->ColorBlendFunction),
                    DestinationAlphaBlendFactor = VulkanConvertExtensions.ConvertBlend(renderTargetBlendState->AlphaDestinationBlend),
                    DestinationColorBlendFactor = VulkanConvertExtensions.ConvertBlend(renderTargetBlendState->ColorDestinationBlend),
                    SourceAlphaBlendFactor = VulkanConvertExtensions.ConvertBlend(renderTargetBlendState->AlphaSourceBlend),
                    SourceColorBlendFactor = VulkanConvertExtensions.ConvertBlend(renderTargetBlendState->ColorSourceBlend),
                    ColorWriteMask = VulkanConvertExtensions.ConvertColorWriteChannels(renderTargetBlendState->ColorWriteChannels),
                };

                if (description.IndependentBlendEnable)
                    renderTargetBlendState++;
            }

            var viewportState = new PipelineViewportStateCreateInfo
            {
                StructureType = StructureType.PipelineViewportStateCreateInfo,
                ScissorCount = 1,
                ViewportCount = 1,
            };

            fixed (DynamicState* dynamicStatesPointer = &dynamicStates[0])
            {
                var vertexInputState = new PipelineVertexInputStateCreateInfo
                {
                    StructureType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexAttributeDescriptionCount = (uint)inputAttributeCount,
                    VertexAttributeDescriptions = inputAttributes.Length > 0 ? new IntPtr(Interop.Fixed(inputAttributes)) : IntPtr.Zero,
                    VertexBindingDescriptionCount = (uint)inputBindingCount,
                    VertexBindingDescriptions = inputBindings.Length > 0 ? new IntPtr(Interop.Fixed(inputBindings)) : IntPtr.Zero,
                };

                var colorBlendState = new PipelineColorBlendStateCreateInfo
                {
                    StructureType = StructureType.PipelineColorBlendStateCreateInfo,
                    AttachmentCount = (uint)renderTargetCount,
                    Attachments = colorBlendAttachments.Length > 0 ? new IntPtr(Interop.Fixed(colorBlendAttachments)) : IntPtr.Zero,
                };

                var dynamicState = new PipelineDynamicStateCreateInfo
                {
                    StructureType = StructureType.PipelineDynamicStateCreateInfo,
                    DynamicStateCount = (uint)dynamicStates.Length,
                    DynamicStates = new IntPtr(dynamicStatesPointer)
                };

                var createInfo = new GraphicsPipelineCreateInfo
                {
                    StructureType = StructureType.GraphicsPipelineCreateInfo,
                    Layout = NativeLayout,
                    StageCount = (uint)stages.Length,
                    Stages = stages.Length > 0 ? new IntPtr(Interop.Fixed(stages)) : IntPtr.Zero,
                    //TessellationState = new IntPtr(&tessellationState),
                    VertexInputState = new IntPtr(&vertexInputState),
                    InputAssemblyState = new IntPtr(&inputAssemblyState),
                    RasterizationState = new IntPtr(&rasterizationState),
                    //MultisampleState = new IntPtr(&multisampleState),
                    DepthStencilState = new IntPtr(&depthStencilState),
                    ColorBlendState = new IntPtr(&colorBlendState),
                    DynamicState = new IntPtr(&dynamicState),
                    ViewportState = new IntPtr(&viewportState),
                    RenderPass = NativeRenderPass,
                    Subpass = 0,
                };
                NativePipeline = graphicsDevice.NativeDevice.CreateGraphicsPipelines(PipelineCache.Null, 1, &createInfo);
            }

            // Cleanup shader modules
            foreach (var stage in stages)
            {
                GraphicsDevice.NativeDevice.DestroyShaderModule(stage.Module);
            }
        }

        private unsafe void CreateRenderPass(PipelineStateDescription pipelineStateDescription)
        {
            bool hasDepthStencilAttachment = pipelineStateDescription.Output.DepthStencilFormat != PixelFormat.None;

            var renderTargetCount = pipelineStateDescription.Output.RenderTargetCount;

            var attachmentCount = renderTargetCount;
            if (hasDepthStencilAttachment)
                attachmentCount++;

            var attachments = new AttachmentDescription[attachmentCount];
            var colorAttachmentReferences = new AttachmentReference[renderTargetCount];

            fixed (PixelFormat* renderTargetFormat = &pipelineStateDescription.Output.RenderTargetFormat0)
            fixed (BlendStateRenderTargetDescription* blendDescription = &pipelineStateDescription.BlendState.RenderTarget0)
            {
                for (int i = 0; i < renderTargetCount; i++)
                {
                    var currentBlendDesc = pipelineStateDescription.BlendState.IndependentBlendEnable ? (blendDescription + i) : blendDescription;

                    attachments[i] = new AttachmentDescription
                    {
                        Format = VulkanConvertExtensions.ConvertPixelFormat(*(renderTargetFormat + i)),
                        Samples = SampleCountFlags.Sample1,
                        LoadOperation = currentBlendDesc->BlendEnable ? AttachmentLoadOperation.Load : AttachmentLoadOperation.DontCare, // TODO VULKAN: Only if any destination blend?
                        StoreOperation = AttachmentStoreOperation.Store,
                        StencilLoadOperation = AttachmentLoadOperation.DontCare,
                        StencilStoreOperation = AttachmentStoreOperation.DontCare,
                        InitialLayout = ImageLayout.ColorAttachmentOptimal,
                        FinalLayout = ImageLayout.ColorAttachmentOptimal,
                    };

                    colorAttachmentReferences[i] = new AttachmentReference
                    {
                        Attachment = (uint)i,
                        Layout = ImageLayout.ColorAttachmentOptimal,
                    };
                }
            }

            if (hasDepthStencilAttachment)
            {
                attachments[attachmentCount - 1] = new AttachmentDescription
                {
                    Format = VulkanConvertExtensions.ConvertPixelFormat(pipelineStateDescription.Output.DepthStencilFormat),
                    Samples = SampleCountFlags.Sample1,
                    LoadOperation = AttachmentLoadOperation.Load, // TODO VULKAN: Only if depth read enabled?
                    StoreOperation = AttachmentStoreOperation.Store, // TODO VULKAN: Only if depth write enabled?
                    StencilLoadOperation = AttachmentLoadOperation.DontCare, // TODO VULKAN: Handle stencil
                    StencilStoreOperation = AttachmentStoreOperation.DontCare,
                    InitialLayout = ImageLayout.DepthStencilAttachmentOptimal,
                    FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
                };
            }

            var depthAttachmentReference = new AttachmentReference
            {
                Attachment = (uint)attachments.Length - 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal,
            };

            var subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = (uint)renderTargetCount,
                ColorAttachments = colorAttachmentReferences.Length > 0 ? new IntPtr(Interop.Fixed(colorAttachmentReferences)) : IntPtr.Zero,
                DepthStencilAttachment = hasDepthStencilAttachment ? new IntPtr(&depthAttachmentReference) : IntPtr.Zero,
            };

            var renderPassCreateInfo = new RenderPassCreateInfo
            {
                StructureType = StructureType.RenderPassCreateInfo,
                AttachmentCount = (uint)attachmentCount,
                Attachments = attachments.Length > 0 ? new IntPtr(Interop.Fixed(attachments)) : IntPtr.Zero,
                SubpassCount = 1,
                Subpasses = new IntPtr(&subpass)
            };
            NativeRenderPass = GraphicsDevice.NativeDevice.CreateRenderPass(ref renderPassCreateInfo);
        }

        protected internal unsafe override void OnDestroyed()
        {
            GraphicsDevice.NativeDevice.DestroyRenderPass(NativeRenderPass);
            GraphicsDevice.NativeDevice.DestroyPipeline(NativePipeline);
            GraphicsDevice.NativeDevice.DestroyPipelineLayout(NativeLayout);

            foreach (var nativeDescriptorSetLayout in NativeDescriptorSetLayouts)
            {
                GraphicsDevice.NativeDevice.DestroyDescriptorSetLayout(nativeDescriptorSetLayout);
            }

            base.OnDestroyed();
        }

        internal struct DescriptorSetInfo
        {
            public int Index;
            public int BindingOffset;
            public int BindingCount;
        }

        internal DescriptorSetInfo[] DescriptorSetMapping;

        private unsafe void CreatePipelineLayout(PipelineStateDescription pipelineStateDescription)
        {
            // Remap descriptor set indices to those in the shader. This ordering generated by the ShaderCompiler
            var resourceGroups = pipelineStateDescription.EffectBytecode.Reflection.ResourceBindings.Select(x => x.Param.ResourceGroup ?? "Globals").Distinct().ToList();
            ResourceGroupCount = resourceGroups.Count;

            var layouts = pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.Layouts;
            DescriptorSetMapping = new DescriptorSetInfo[layouts.Count];
            for (int i = 0; i < DescriptorSetMapping.Length; i++)
            {
                DescriptorSetMapping[i].Index = -1;
            }

            NativeDescriptorSetLayouts = new SharpVulkan.DescriptorSetLayout[1];
            var layoutEntries = new List<DescriptorSetLayoutBuilder.Entry>();
            for (int i = 0; i < resourceGroups.Count; i++)
            {
                var resourceGroupName = resourceGroups[i] == "Globals" ? pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.DefaultSetSlot : resourceGroups[i];
                var layoutIndex = resourceGroups[i] == null ? 0 : layouts.FindIndex(x => x.Name == resourceGroupName);
                if (layoutIndex != -1)
                {
                    DescriptorSetMapping[layoutIndex] = new DescriptorSetInfo
                    {
                        Index = 0,
                        BindingOffset = layoutEntries.Count,
                        BindingCount = layouts[layoutIndex].Layout.Entries.Count
                    };

                    layoutEntries.AddRange(layouts[layoutIndex].Layout.Entries);
                }
            }

            NativeDescriptorSetLayouts[0] = DescriptorSetLayout.CreateNativeDescriptorSetLayout(GraphicsDevice, layoutEntries, out ImmutableSamplers);

            // Create pipeline layout
            var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                StructureType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)NativeDescriptorSetLayouts.Length,
                SetLayouts = NativeDescriptorSetLayouts.Length > 0 ? new IntPtr(Interop.Fixed(NativeDescriptorSetLayouts)) : IntPtr.Zero,
            };
            NativeLayout = GraphicsDevice.NativeDevice.CreatePipelineLayout(ref pipelineLayoutCreateInfo);
        }

        private unsafe PipelineShaderStageCreateInfo[] CreateShaderStages(PipelineStateDescription pipelineStateDescription, out Dictionary<int, string> inputAttributeNames)
        {
            var stages = pipelineStateDescription.EffectBytecode.Stages;
            var nativeStages = new PipelineShaderStageCreateInfo[stages.Length];

            inputAttributeNames = null;

            // GLSL converter always outputs entry point main()
            var entryPoint = Encoding.UTF8.GetBytes("main\0");

            for (int i = 0; i < stages.Length; i++)
            {
                var shaderBytecode = BinarySerialization.Read<ShaderInputBytecode>(stages[i].Data);
                if (stages[i].Stage == ShaderStage.Vertex)
                    inputAttributeNames = shaderBytecode.InputAttributeNames;

                fixed (byte* entryPointPointer = &entryPoint[0])
                fixed (byte* codePointer = &shaderBytecode.Data[0])
                {
                    // Create shader module
                    var moduleCreateInfo = new ShaderModuleCreateInfo
                    {
                        StructureType = StructureType.ShaderModuleCreateInfo,
                        Code = new IntPtr(codePointer),
                        CodeSize = shaderBytecode.Data.Length
                    };

                    // Create stage
                    nativeStages[i] = new PipelineShaderStageCreateInfo
                    {
                        StructureType = StructureType.PipelineShaderStageCreateInfo,
                        Stage = VulkanConvertExtensions.Convert(stages[i].Stage),
                        Name = new IntPtr(entryPointPointer),
                        Module = GraphicsDevice.NativeDevice.CreateShaderModule(ref moduleCreateInfo)
                    };
                }
            };

            return nativeStages;
        }

        private PipelineRasterizationStateCreateInfo CreateRasterizationState(RasterizerStateDescription description)
        {
            return new PipelineRasterizationStateCreateInfo
            {
                StructureType = StructureType.PipelineRasterizationStateCreateInfo,
                CullMode = VulkanConvertExtensions.ConvertCullMode(description.CullMode),
                FrontFace = description.FrontFaceCounterClockwise ? FrontFace.CounterClockwise : FrontFace.Clockwise,
                PolygonMode = VulkanConvertExtensions.ConvertFillMode(description.FillMode),
                DepthBiasEnable = true, // TODO VULKAN
                DepthBiasConstantFactor = description.DepthBias,
                DepthBiasSlopeFactor = description.SlopeScaleDepthBias,
                DepthBiasClamp = description.DepthBiasClamp,
                LineWidth = 1.0f,
                DepthClampEnable = !description.DepthClipEnable,
                RasterizerDiscardEnable = false,
            };
        }

        private PipelineDepthStencilStateCreateInfo CreateDepthStencilState(PipelineStateDescription pipelineStateDescription)
        {
            var description = pipelineStateDescription.DepthStencilState;

            return new PipelineDepthStencilStateCreateInfo
            {
                StructureType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = description.DepthBufferEnable,
                StencilTestEnable = description.StencilEnable,
                DepthWriteEnable = description.DepthBufferWriteEnable,

                MinDepthBounds = 0.0f,
                MaxDepthBounds = 1.0f,
                DepthCompareOperation = VulkanConvertExtensions.ConvertComparisonFunction(description.DepthBufferFunction),
                Front = new StencilOperationState
                {
                    CompareOperation = VulkanConvertExtensions.ConvertComparisonFunction(description.FrontFace.StencilFunction),
                    DepthFailOperation = VulkanConvertExtensions.ConvertStencilOperation(description.FrontFace.StencilDepthBufferFail),
                    FailOperation = VulkanConvertExtensions.ConvertStencilOperation(description.FrontFace.StencilFail),
                    PassOperation = VulkanConvertExtensions.ConvertStencilOperation(description.FrontFace.StencilPass),
                    CompareMask = description.StencilMask,
                    WriteMask = description.StencilWriteMask
                },
                Back = new StencilOperationState
                {
                    CompareOperation = VulkanConvertExtensions.ConvertComparisonFunction(description.BackFace.StencilFunction),
                    DepthFailOperation = VulkanConvertExtensions.ConvertStencilOperation(description.BackFace.StencilDepthBufferFail),
                    FailOperation = VulkanConvertExtensions.ConvertStencilOperation(description.BackFace.StencilFail),
                    PassOperation = VulkanConvertExtensions.ConvertStencilOperation(description.BackFace.StencilPass),
                    CompareMask = description.StencilMask,
                    WriteMask = description.StencilWriteMask
                }
            };
        }
    }
}

#endif