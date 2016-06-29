// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class PipelineState
    {
        internal SharpDX.Direct3D12.PipelineState CompiledState;
        internal SharpDX.Direct3D12.RootSignature RootSignature;
        internal PrimitiveTopology PrimitiveTopology;
        internal int[] SrvBindCounts;
        internal int[] SamplerBindCounts;

        internal unsafe PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription) : base(graphicsDevice)
        {
            if (pipelineStateDescription.RootSignature != null)
            {
                var effectReflection = pipelineStateDescription.EffectBytecode.Reflection;

                var rootSignatureParameters = new List<RootParameter>();
                var immutableSamplers = new List<StaticSamplerDescription>();
                SrvBindCounts = new int[pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.Layouts.Count];
                SamplerBindCounts = new int[pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.Layouts.Count];
                for (int layoutIndex = 0; layoutIndex < pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.Layouts.Count; layoutIndex++)
                {
                    var layout = pipelineStateDescription.RootSignature.EffectDescriptorSetReflection.Layouts[layoutIndex];
                    if (layout.Layout == null)
                        continue;

                    // TODO D3D12 for now, we don't control register so we simply generate one resource table per shader stage and per descriptor set layout
                    //            we should switch to a model where we make sure VS/PS don't overlap for common descriptors so that they can be shared
                    var srvDescriptorRangesVS = new List<DescriptorRange>();
                    var srvDescriptorRangesPS = new List<DescriptorRange>();
                    var samplerDescriptorRangesVS = new List<DescriptorRange>();
                    var samplerDescriptorRangesPS = new List<DescriptorRange>();

                    int descriptorSrvOffset = 0;
                    int descriptorSamplerOffset = 0;
                    foreach (var item in layout.Layout.Entries)
                    {
                        var isSampler = item.Class == EffectParameterClass.Sampler;

                        // Find matching resource bindings
                        foreach (var binding in effectReflection.ResourceBindings)
                        {
                            if (binding.Stage == ShaderStage.None || binding.KeyInfo.Key != item.Key)
                                continue;

                            List<DescriptorRange> descriptorRanges;
                            switch (binding.Stage)
                            {
                                case ShaderStage.Vertex:
                                    descriptorRanges = isSampler ? samplerDescriptorRangesVS : srvDescriptorRangesVS;
                                    break;
                                case ShaderStage.Pixel:
                                    descriptorRanges = isSampler ? samplerDescriptorRangesPS : srvDescriptorRangesPS;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }

                            if (isSampler)
                            {
                                if (item.ImmutableSampler != null)
                                {
                                    immutableSamplers.Add(new StaticSamplerDescription((ShaderVisibility)binding.Stage, binding.SlotStart, 0)
                                    {
                                        // TODO D3D12 ImmutableSampler should only be a state description instead of a GPU object?
                                        Filter = (Filter)item.ImmutableSampler.Description.Filter,
                                        ComparisonFunc = (Comparison)item.ImmutableSampler.Description.CompareFunction,
                                        BorderColor = ColorHelper.ConvertStatic(item.ImmutableSampler.Description.BorderColor),
                                        AddressU = (SharpDX.Direct3D12.TextureAddressMode)item.ImmutableSampler.Description.AddressU,
                                        AddressV = (SharpDX.Direct3D12.TextureAddressMode)item.ImmutableSampler.Description.AddressV,
                                        AddressW = (SharpDX.Direct3D12.TextureAddressMode)item.ImmutableSampler.Description.AddressW,
                                        MinLOD = item.ImmutableSampler.Description.MinMipLevel,
                                        MaxLOD = item.ImmutableSampler.Description.MaxMipLevel,
                                        MipLODBias = item.ImmutableSampler.Description.MipMapLevelOfDetailBias,
                                        MaxAnisotropy = item.ImmutableSampler.Description.MaxAnisotropy,
                                    });
                                }
                                else
                                {
                                    // Add descriptor range
                                    descriptorRanges.Add(new DescriptorRange(DescriptorRangeType.Sampler, item.ArraySize, binding.SlotStart, 0, descriptorSamplerOffset));
                                }
                            }
                            else
                            {
                                DescriptorRangeType descriptorRangeType;
                                switch (binding.Class)
                                {
                                    case EffectParameterClass.ConstantBuffer:
                                        descriptorRangeType = DescriptorRangeType.ConstantBufferView;
                                        break;
                                    case EffectParameterClass.ShaderResourceView:
                                        descriptorRangeType = DescriptorRangeType.ShaderResourceView;
                                        break;
                                    case EffectParameterClass.UnorderedAccessView:
                                        descriptorRangeType = DescriptorRangeType.UnorderedAccessView;
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }

                                // Add descriptor range
                                descriptorRanges.Add(new DescriptorRange(descriptorRangeType, item.ArraySize, binding.SlotStart, 0, descriptorSrvOffset));
                            }
                        }

                        // Move to next element (mirror what is done in DescriptorSetLayout)
                        if (isSampler)
                        {
                            if (item.ImmutableSampler == null)
                                descriptorSamplerOffset += item.ArraySize;
                        }
                        else
                        {
                            descriptorSrvOffset += item.ArraySize;
                        }
                    }
                    if (srvDescriptorRangesVS.Count > 0)
                    {
                        rootSignatureParameters.Add(new RootParameter(ShaderVisibility.Vertex, srvDescriptorRangesVS.ToArray()));
                        SrvBindCounts[layoutIndex]++;
                    }
                    if (srvDescriptorRangesPS.Count > 0)
                    {
                        rootSignatureParameters.Add(new RootParameter(ShaderVisibility.Pixel, srvDescriptorRangesPS.ToArray()));
                        SrvBindCounts[layoutIndex]++;
                    }
                    if (samplerDescriptorRangesVS.Count > 0)
                    {
                        rootSignatureParameters.Add(new RootParameter(ShaderVisibility.Vertex, samplerDescriptorRangesVS.ToArray()));
                        SamplerBindCounts[layoutIndex]++;
                    }
                    if (samplerDescriptorRangesPS.Count > 0)
                    {
                        rootSignatureParameters.Add(new RootParameter(ShaderVisibility.Pixel, samplerDescriptorRangesPS.ToArray()));
                        SamplerBindCounts[layoutIndex]++;
                    }
                }
                var rootSignatureDesc = new RootSignatureDescription(RootSignatureFlags.AllowInputAssemblerInputLayout, rootSignatureParameters.ToArray(), immutableSamplers.ToArray());

                var rootSignature = NativeDevice.CreateRootSignature(0, rootSignatureDesc.Serialize());

                var inputElements = new InputElement[pipelineStateDescription.InputElements.Length];
                for (int i = 0; i < inputElements.Length; ++i)
                {
                    var inputElement = pipelineStateDescription.InputElements[i];
                    inputElements[i] = new InputElement
                    {
                        Format = (SharpDX.DXGI.Format)inputElement.Format,
                        AlignedByteOffset = inputElement.AlignedByteOffset,
                        SemanticName = inputElement.SemanticName,
                        SemanticIndex = inputElement.SemanticIndex,
                        Slot = inputElement.InputSlot,
                        Classification = (SharpDX.Direct3D12.InputClassification)inputElement.InputSlotClass,
                        InstanceDataStepRate = inputElement.InstanceDataStepRate,
                    };
                }

                PrimitiveTopologyType primitiveTopologyType;
                switch (pipelineStateDescription.PrimitiveType)
                {
                    case PrimitiveType.Undefined:
                        throw new ArgumentOutOfRangeException();
                    case PrimitiveType.PointList:
                        primitiveTopologyType = PrimitiveTopologyType.Point;
                        break;
                    case PrimitiveType.LineList:
                    case PrimitiveType.LineStrip:
                    case PrimitiveType.LineListWithAdjacency:
                    case PrimitiveType.LineStripWithAdjacency:
                        primitiveTopologyType = PrimitiveTopologyType.Line;
                        break;
                    case PrimitiveType.TriangleList:
                    case PrimitiveType.TriangleStrip:
                    case PrimitiveType.TriangleListWithAdjacency:
                    case PrimitiveType.TriangleStripWithAdjacency:
                        primitiveTopologyType = PrimitiveTopologyType.Triangle;
                        break;
                    default:
                        if (pipelineStateDescription.PrimitiveType >= PrimitiveType.PatchList && pipelineStateDescription.PrimitiveType < PrimitiveType.PatchList + 32)
                            primitiveTopologyType = PrimitiveTopologyType.Patch;
                        else
                            throw new ArgumentOutOfRangeException("pipelineStateDescription.PrimitiveType");
                        break;
                }

                var nativePipelineStateDescription = new GraphicsPipelineStateDescription
                {
                    InputLayout = new InputLayoutDescription(inputElements),
                    RootSignature = rootSignature,
                    RasterizerState = CreateRasterizerState(pipelineStateDescription.RasterizerState),
                    BlendState = CreateBlendState(pipelineStateDescription.BlendState),
                    SampleMask = (int)pipelineStateDescription.SampleMask,
                    DepthStencilFormat = (SharpDX.DXGI.Format)pipelineStateDescription.Output.DepthStencilFormat,
                    DepthStencilState = CreateDepthStencilState(pipelineStateDescription.DepthStencilState),
                    RenderTargetCount = pipelineStateDescription.Output.RenderTargetCount,
                    // TODO D3D12 hardcoded
                    StreamOutput = new StreamOutputDescription(),
                    PrimitiveTopologyType = primitiveTopologyType,
                    // TODO D3D12 hardcoded
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                };

                fixed (PixelFormat* renderTargetFormats = &pipelineStateDescription.Output.RenderTargetFormat0)
                {
                    for (int i = 0; i < pipelineStateDescription.Output.RenderTargetCount; ++i)
                        nativePipelineStateDescription.RenderTargetFormats[i] = (SharpDX.DXGI.Format)renderTargetFormats[i];
                }

                foreach (var stage in pipelineStateDescription.EffectBytecode.Stages)
                {
                    switch (stage.Stage)
                    {
                        case ShaderStage.Vertex:
                            nativePipelineStateDescription.VertexShader = stage.Data;
                            break;
                        case ShaderStage.Hull:
                            nativePipelineStateDescription.HullShader = stage.Data;
                            break;
                        case ShaderStage.Domain:
                            nativePipelineStateDescription.DomainShader = stage.Data;
                            break;
                        case ShaderStage.Geometry:
                            nativePipelineStateDescription.GeometryShader = stage.Data;
                            break;
                        case ShaderStage.Pixel:
                            nativePipelineStateDescription.PixelShader = stage.Data;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                CompiledState = NativeDevice.CreateGraphicsPipelineState(nativePipelineStateDescription);
                RootSignature = rootSignature;
                PrimitiveTopology = (PrimitiveTopology)pipelineStateDescription.PrimitiveType;
            }
        }

        protected internal override void OnDestroyed()
        {
            RootSignature?.Dispose();
            RootSignature = null;

            CompiledState?.Dispose();
            CompiledState = null;

            base.OnDestroyed();
        }

        private unsafe SharpDX.Direct3D12.BlendStateDescription CreateBlendState(BlendStateDescription description)
        {
            var nativeDescription = new SharpDX.Direct3D12.BlendStateDescription();

            nativeDescription.AlphaToCoverageEnable = description.AlphaToCoverageEnable;
            nativeDescription.IndependentBlendEnable = description.IndependentBlendEnable;

            var renderTargets = &description.RenderTarget0;
            for (int i = 0; i < 8; ++i)
            {
                nativeDescription.RenderTarget[i].IsBlendEnabled = renderTargets[i].BlendEnable;
                nativeDescription.RenderTarget[i].SourceBlend = (BlendOption)renderTargets[i].ColorSourceBlend;
                nativeDescription.RenderTarget[i].DestinationBlend = (BlendOption)renderTargets[i].ColorDestinationBlend;
                nativeDescription.RenderTarget[i].BlendOperation = (BlendOperation)renderTargets[i].ColorBlendFunction;
                nativeDescription.RenderTarget[i].SourceAlphaBlend = (BlendOption)renderTargets[i].AlphaSourceBlend;
                nativeDescription.RenderTarget[i].DestinationAlphaBlend = (BlendOption)renderTargets[i].AlphaDestinationBlend;
                nativeDescription.RenderTarget[i].AlphaBlendOperation = (BlendOperation)renderTargets[i].AlphaBlendFunction;
                nativeDescription.RenderTarget[i].RenderTargetWriteMask = (ColorWriteMaskFlags)renderTargets[i].ColorWriteChannels;
            }

            return nativeDescription;
        }

        private SharpDX.Direct3D12.RasterizerStateDescription CreateRasterizerState(RasterizerStateDescription description)
        {
            SharpDX.Direct3D12.RasterizerStateDescription nativeDescription;

            nativeDescription.CullMode = (SharpDX.Direct3D12.CullMode)description.CullMode;
            nativeDescription.FillMode = (SharpDX.Direct3D12.FillMode)description.FillMode;
            nativeDescription.IsFrontCounterClockwise = description.FrontFaceCounterClockwise;
            nativeDescription.DepthBias = description.DepthBias;
            nativeDescription.SlopeScaledDepthBias = description.SlopeScaleDepthBias;
            nativeDescription.DepthBiasClamp = description.DepthBiasClamp;
            nativeDescription.IsDepthClipEnabled = description.DepthClipEnable;
            //nativeDescription.IsScissorEnabled = description.ScissorTestEnable;
            nativeDescription.IsMultisampleEnabled = description.MultiSampleLevel >= MSAALevel.None;
            nativeDescription.IsAntialiasedLineEnabled = description.MultiSampleAntiAliasLine;

            nativeDescription.ConservativeRaster = ConservativeRasterizationMode.Off;
            nativeDescription.ForcedSampleCount = 0;

            return nativeDescription;
        }

        private SharpDX.Direct3D12.DepthStencilStateDescription CreateDepthStencilState(DepthStencilStateDescription description)
        {
            SharpDX.Direct3D12.DepthStencilStateDescription nativeDescription;

            nativeDescription.IsDepthEnabled = description.DepthBufferEnable;
            nativeDescription.DepthComparison = (Comparison)description.DepthBufferFunction;
            nativeDescription.DepthWriteMask = description.DepthBufferWriteEnable ? SharpDX.Direct3D12.DepthWriteMask.All : SharpDX.Direct3D12.DepthWriteMask.Zero;

            nativeDescription.IsStencilEnabled = description.StencilEnable;
            nativeDescription.StencilReadMask = description.StencilMask;
            nativeDescription.StencilWriteMask = description.StencilWriteMask;

            nativeDescription.FrontFace.FailOperation = (SharpDX.Direct3D12.StencilOperation)description.FrontFace.StencilFail;
            nativeDescription.FrontFace.PassOperation = (SharpDX.Direct3D12.StencilOperation)description.FrontFace.StencilPass;
            nativeDescription.FrontFace.DepthFailOperation = (SharpDX.Direct3D12.StencilOperation)description.FrontFace.StencilDepthBufferFail;
            nativeDescription.FrontFace.Comparison = (SharpDX.Direct3D12.Comparison)description.FrontFace.StencilFunction;

            nativeDescription.BackFace.FailOperation = (SharpDX.Direct3D12.StencilOperation)description.BackFace.StencilFail;
            nativeDescription.BackFace.PassOperation = (SharpDX.Direct3D12.StencilOperation)description.BackFace.StencilPass;
            nativeDescription.BackFace.DepthFailOperation = (SharpDX.Direct3D12.StencilOperation)description.BackFace.StencilDepthBufferFail;
            nativeDescription.BackFace.Comparison = (SharpDX.Direct3D12.Comparison)description.BackFace.StencilFunction;

            return nativeDescription;
        }
    }
}

#endif