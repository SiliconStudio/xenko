// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Runtime.InteropServices;
using SharpVulkan;
using SiliconStudio.Xenko.Shaders;
using Encoding = System.Text.Encoding;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class PipelineState
    {
        internal PipelineLayout NativeLayout;
        internal Pipeline NativePipeline;
        internal RenderPass NativeRenderPass;
        internal PrimitiveTopology PrimitiveTopology;
        internal int[] SrvBindCounts;
        internal int[] SamplerBindCounts;

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
            var stages = CreateShaderStages(pipelineStateDescription);


            var inputAttributes = new VertexInputAttributeDescription[pipelineStateDescription.InputElements.Length];
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

                inputAttributes[inputElementIndex] = new VertexInputAttributeDescription
                {
                    Format = format,
                    Offset = (uint)inputElement.AlignedByteOffset,
                    Binding = (uint)inputElement.InputSlot,
                    Location = (uint)inputElementIndex
                };

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

            fixed (PipelineShaderStageCreateInfo* stagesPointer = &stages[0])
            fixed (VertexInputAttributeDescription* inputAttributesPointer = &inputAttributes[0])
            fixed (VertexInputBindingDescription* inputBindingsPointer = &inputBindings[0])
            fixed (PipelineColorBlendAttachmentState* attachmentsPointer = &colorBlendAttachments[0])
            fixed (DynamicState* dynamicStatesPointer = &dynamicStates[0])
            {
                var vertexInputState = new PipelineVertexInputStateCreateInfo
                {
                    StructureType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexAttributeDescriptionCount = (uint)inputAttributes.Length,
                    VertexAttributeDescriptions = new IntPtr(inputAttributesPointer),
                    VertexBindingDescriptionCount = (uint)inputBindingCount,
                    VertexBindingDescriptions = new IntPtr(inputBindingsPointer),
                };

                var colorBlendState = new PipelineColorBlendStateCreateInfo
                {
                    StructureType = StructureType.PipelineColorBlendStateCreateInfo,
                    AttachmentCount = (uint)renderTargetCount,
                    Attachments = new IntPtr(attachmentsPointer)
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
                    Stages = new IntPtr(stagesPointer),
                    //TessellationState = new IntPtr(&tessellationState),
                    VertexInputState = new IntPtr(&vertexInputState),
                    InputAssemblyState = new IntPtr(&inputAssemblyState),
                    RasterizationState = new IntPtr(&rasterizationState),
                    //MultisampleState = new IntPtr(&multisampleState),
                    DepthStencilState = new IntPtr(&depthStencilState),
                    ColorBlendState = new IntPtr(&colorBlendState),
                    DynamicState = new IntPtr(&dynamicState),
                    ViewportState = IntPtr.Zero, // Dynamic
                    RenderPass = NativeRenderPass,
                    Subpass = 0,
                };
                NativePipeline = graphicsDevice.NativeDevice.CreateGraphicsPipelines(PipelineCache.Null, 1, ref createInfo);
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
            {
                for (int i = 0; i < renderTargetCount; i++)
                {
                    attachments[i] = new AttachmentDescription
                    {
                        Format = VulkanConvertExtensions.ConvertPixelFormat(*(renderTargetFormat + i)),
                        Samples = SampleCountFlags.Sample1,
                        LoadOperation = AttachmentLoadOperation.Load, // TODO VULKAN: Only if any destination blend?
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
                    StoreOperation = AttachmentStoreOperation.DontCare,
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

            fixed (AttachmentDescription* attachmentsPointer = &attachments[0])
            fixed (AttachmentReference* colorAttachmentReferencesPointer = &colorAttachmentReferences[0])
            {
                var subpass = new SubpassDescription
                {
                    PipelineBindPoint = PipelineBindPoint.Graphics,
                    ColorAttachmentCount = (uint)renderTargetCount,
                    ColorAttachments = new IntPtr(colorAttachmentReferencesPointer),
                    DepthStencilAttachment = new IntPtr(&depthAttachmentReference)
                };

                var renderPassCreateInfo = new RenderPassCreateInfo
                {
                    StructureType = StructureType.RenderPassCreateInfo,
                    AttachmentCount = (uint)attachmentCount,
                    Attachments = new IntPtr(attachmentsPointer),
                    SubpassCount = 1,
                    Subpasses = new IntPtr(&subpass)
                };
                NativeRenderPass = GraphicsDevice.NativeDevice.CreateRenderPass(ref renderPassCreateInfo);
            }
        }

        protected internal unsafe override void OnDestroyed()
        {
            GraphicsDevice.NativeDevice.DestroyRenderPass(NativeRenderPass);
            GraphicsDevice.NativeDevice.DestroyPipeline(NativePipeline);

            base.OnDestroyed();
        }

        private unsafe void CreatePipelineLayout(PipelineStateDescription pipelineStateDescription)
        {
            var layouts = pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.Layouts;

            // Create temporary descriptor set layouts
            var nativeLayouts = new SharpVulkan.DescriptorSetLayout[layouts.Count];
            for (int i = 0; i < layouts.Count; i++)
            {
                DescriptorSetLayout.BindingInfo[] bindingInfos;
                nativeLayouts[i] = DescriptorSetLayout.CreateNativeDescriptorSetLayout(GraphicsDevice, layouts[i].Layout, out bindingInfos);
            }

            // Create pipeline layout
            fixed (SharpVulkan.DescriptorSetLayout* nativeDescriptorSetLayoutsPointer = &nativeLayouts[0])
            {
                var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
                {
                    StructureType = StructureType.PipelineLayoutCreateInfo,
                    SetLayoutCount = (uint)nativeLayouts.Length,
                    SetLayouts = new IntPtr(nativeDescriptorSetLayoutsPointer),
                };
                NativeLayout = GraphicsDevice.NativeDevice.CreatePipelineLayout(ref pipelineLayoutCreateInfo);
            }

            // Cleanup temporary layouts
            foreach (var nativeLayout in nativeLayouts)
            {
                GraphicsDevice.NativeDevice.DestroyDescriptorSetLayout(nativeLayout);
            }
        }

        private unsafe PipelineShaderStageCreateInfo[] CreateShaderStages(PipelineStateDescription pipelineStateDescription)
        {
            var stages = pipelineStateDescription.EffectBytecode.Stages;
            var nativeStages = new PipelineShaderStageCreateInfo[stages.Length];

            // GLSL converter always outputs entry point main()
            var entryPoint = Encoding.UTF8.GetBytes("main\0");

            for (int i = 0; i < stages.Length; i++)
            {
                fixed (byte* entryPointPointer = &entryPoint[0])
                fixed (byte* codePointer = &stages[i].Data[0])
                {
                    // Create shader module
                    var moduleCreateInfo = new ShaderModuleCreateInfo
                    {
                        StructureType = StructureType.ShaderModuleCreateInfo,
                        Code = new IntPtr(codePointer),
                        CodeSize = stages[i].Data.Length
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
                DepthClampEnable = false,
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
                DepthBoundsTestEnable = pipelineStateDescription.RasterizerState.DepthClipEnable,
                MinDepthBounds = 0f,
                MaxDepthBounds = 1f,
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

        //private SharpDX.Direct3D12.BlendStateDescription CreateBlendState(BlendStateDescription description)
        //{
        //    var nativeDescription = new SharpDX.Direct3D12.BlendStateDescription();

        //    nativeDescription.AlphaToCoverageEnable = description.AlphaToCoverageEnable;
        //    nativeDescription.IndependentBlendEnable = description.IndependentBlendEnable;
        //    for (int i = 0; i < description.RenderTargets.Length; ++i)
        //    {
        //        nativeDescription.RenderTarget[i].IsBlendEnabled = description.RenderTargets[i].BlendEnable;
        //        nativeDescription.RenderTarget[i].SourceBlend = (BlendOption)description.RenderTargets[i].ColorSourceBlend;
        //        nativeDescription.RenderTarget[i].DestinationBlend = (BlendOption)description.RenderTargets[i].ColorDestinationBlend;
        //        nativeDescription.RenderTarget[i].BlendOperation = (BlendOperation)description.RenderTargets[i].ColorBlendFunction;
        //        nativeDescription.RenderTarget[i].SourceAlphaBlend = (BlendOption)description.RenderTargets[i].AlphaSourceBlend;
        //        nativeDescription.RenderTarget[i].DestinationAlphaBlend = (BlendOption)description.RenderTargets[i].AlphaDestinationBlend;
        //        nativeDescription.RenderTarget[i].AlphaBlendOperation = (BlendOperation)description.RenderTargets[i].AlphaBlendFunction;
        //        nativeDescription.RenderTarget[i].RenderTargetWriteMask = (ColorWriteMaskFlags)description.RenderTargets[i].ColorWriteChannels;
        //    }

        //    return nativeDescription;
        //}

        //private SharpDX.Direct3D12.RasterizerStateDescription CreateRasterizerState(RasterizerStateDescription description)
        //{
        //    SharpDX.Direct3D12.RasterizerStateDescription nativeDescription;

        //    nativeDescription.CullMode = (SharpDX.Direct3D12.CullMode)description.CullMode;
        //    nativeDescription.FillMode = (SharpDX.Direct3D12.FillMode)description.FillMode;
        //    nativeDescription.IsFrontCounterClockwise = description.FrontFaceCounterClockwise;
        //    nativeDescription.DepthBias = description.DepthBias;
        //    nativeDescription.SlopeScaledDepthBias = description.SlopeScaleDepthBias;
        //    nativeDescription.DepthBiasClamp = description.DepthBiasClamp;
        //    nativeDescription.IsDepthClipEnabled = description.DepthClipEnable;
        //    //nativeDescription.IsScissorEnabled = description.ScissorTestEnable;
        //    nativeDescription.IsMultisampleEnabled = description.MultiSampleAntiAlias;
        //    nativeDescription.IsAntialiasedLineEnabled = description.MultiSampleAntiAliasLine;

        //    nativeDescription.ConservativeRaster = ConservativeRasterizationMode.Off;
        //    nativeDescription.ForcedSampleCount = 0;

        //    return nativeDescription;
        //}

        //private SharpDX.Direct3D12.DepthStencilStateDescription CreateDepthStencilState(DepthStencilStateDescription description)
        //{
        //    SharpDX.Direct3D12.DepthStencilStateDescription nativeDescription;

        //    nativeDescription.IsDepthEnabled = description.DepthBufferEnable;
        //    nativeDescription.DepthComparison = (Comparison)description.DepthBufferFunction;
        //    nativeDescription.DepthWriteMask = description.DepthBufferWriteEnable ? SharpDX.Direct3D12.DepthWriteMask.All : SharpDX.Direct3D12.DepthWriteMask.Zero;

        //    nativeDescription.IsStencilEnabled = description.StencilEnable;
        //    nativeDescription.StencilReadMask = description.StencilMask;
        //    nativeDescription.StencilWriteMask = description.StencilWriteMask;

        //    nativeDescription.FrontFace.FailOperation = (SharpDX.Direct3D12.StencilOperation)description.FrontFace.StencilFail;
        //    nativeDescription.FrontFace.PassOperation = (SharpDX.Direct3D12.StencilOperation)description.FrontFace.StencilPass;
        //    nativeDescription.FrontFace.DepthFailOperation = (SharpDX.Direct3D12.StencilOperation)description.FrontFace.StencilDepthBufferFail;
        //    nativeDescription.FrontFace.Comparison = (SharpDX.Direct3D12.Comparison)description.FrontFace.StencilFunction;

        //    nativeDescription.BackFace.FailOperation = (SharpDX.Direct3D12.StencilOperation)description.BackFace.StencilFail;
        //    nativeDescription.BackFace.PassOperation = (SharpDX.Direct3D12.StencilOperation)description.BackFace.StencilPass;
        //    nativeDescription.BackFace.DepthFailOperation = (SharpDX.Direct3D12.StencilOperation)description.BackFace.StencilDepthBufferFail;
        //    nativeDescription.BackFace.Comparison = (SharpDX.Direct3D12.Comparison)description.BackFace.StencilFunction;

        //    return nativeDescription;
        //}
    }
}

#endif