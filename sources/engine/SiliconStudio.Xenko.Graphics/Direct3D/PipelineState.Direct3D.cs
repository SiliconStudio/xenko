// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public struct ResourceBinder
    {
        private BindingOperation[][] descriptorSetBindings;

        public void Compile(GraphicsDevice graphicsDevice, EffectDescriptorSetReflection descriptorSetLayouts, EffectBytecode effectBytecode)
        {
            descriptorSetBindings = new BindingOperation[descriptorSetLayouts.Layouts.Count][];
            for (int setIndex = 0; setIndex < descriptorSetLayouts.Layouts.Count; setIndex++)
            {
                var layout = descriptorSetLayouts.Layouts[setIndex].Layout;

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
                            bindingOperations.Add(new BindingOperation
                            {
                                EntryIndex = resourceIndex,
                                Class = resourceBinding.Param.Class,
                                Stage = resourceBinding.Stage,
                                SlotStart = resourceBinding.SlotStart,
                                ImmutableSampler = layoutEntry.ImmutableSampler,
                            });
                        }
                    }
                }

                descriptorSetBindings[setIndex] = bindingOperations.Count > 0 ? bindingOperations.ToArray() : null;
            }
        }
        public void BindResources(GraphicsDevice graphicsDevice, DescriptorSet[] descriptorSets)
        {
            for (int setIndex = 0; setIndex < descriptorSetBindings.Length; setIndex++)
            {
                var bindingOperations = descriptorSetBindings[setIndex];
                if (bindingOperations == null)
                    continue;

                var descriptorSet = descriptorSets[setIndex];

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
                                graphicsDevice.SetSamplerState(bindingOperation.Stage, bindingOperation.SlotStart, bindingOperation.ImmutableSampler ?? (SamplerState)value.Value);
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

        internal struct BindingOperation
        {
            public int EntryIndex;
            public EffectParameterClass Class;
            public ShaderStage Stage;
            public int SlotStart;
            public SamplerState ImmutableSampler;
        }
    }

    public partial class PipelineState
    {
        // Effect
        private RootSignature rootSignature;
        private EffectBytecode effectBytecode;
        internal ResourceBinder ResourceBinder;

        private SharpDX.Direct3D11.VertexShader vertexShader;
        private SharpDX.Direct3D11.GeometryShader geometryShader;
        private SharpDX.Direct3D11.PixelShader pixelShader;
        private SharpDX.Direct3D11.HullShader hullShader;
        private SharpDX.Direct3D11.DomainShader domainShader;
        private SharpDX.Direct3D11.ComputeShader computeShader;
        private byte[] inputSignature;

        private SharpDX.Direct3D11.BlendState blendState;
        public uint sampleMask;
        private SharpDX.Direct3D11.RasterizerState rasterizerState;
        private SharpDX.Direct3D11.DepthStencilState depthStencilState;

        private SharpDX.Direct3D11.InputLayout inputLayout;

        private SharpDX.Direct3D.PrimitiveTopology primitiveTopology;
        // Note: no need to store RTV/DSV formats

        internal PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription) : base(graphicsDevice)
        {
            // Effect
            this.rootSignature = pipelineStateDescription.RootSignature;
            this.effectBytecode = pipelineStateDescription.EffectBytecode;
            CreateShaders();
            if (rootSignature != null && effectBytecode != null)
                ResourceBinder.Compile(graphicsDevice, rootSignature.EffectDescriptorSetReflection, this.effectBytecode);

            // TODO: Cache over Effect|RootSignature to create binding operations

            // States
            CreateBlendState(pipelineStateDescription.BlendState);
            this.sampleMask = pipelineStateDescription.SampleMask;
            CreateRasterizerState(pipelineStateDescription.RasterizerState);
            CreateDepthStencilState(pipelineStateDescription.DepthStencilState);

            CreateInputLayout(pipelineStateDescription.InputElements);

            primitiveTopology = (SharpDX.Direct3D.PrimitiveTopology)pipelineStateDescription.PrimitiveType;
        }

        internal void Apply(GraphicsDevice graphicsDevice, PipelineState previousPipeline)
        {
            var nativeDeviceContext = graphicsDevice.NativeDeviceContext;

            if (rootSignature != previousPipeline.rootSignature)
            {
                //rootSignature.Apply
            }

            if (effectBytecode != previousPipeline.effectBytecode)
            {
                if (computeShader != null)
                {
                    nativeDeviceContext.ComputeShader.Set(computeShader);
                }
                else
                {
                    nativeDeviceContext.VertexShader.Set(vertexShader);
                    nativeDeviceContext.PixelShader.Set(pixelShader);
                    nativeDeviceContext.HullShader.Set(hullShader);
                    nativeDeviceContext.DomainShader.Set(domainShader);
                    nativeDeviceContext.GeometryShader.Set(geometryShader);
                }
            }

            if (blendState != previousPipeline.blendState || sampleMask != previousPipeline.sampleMask)
            {
                nativeDeviceContext.OutputMerger.SetBlendState(blendState, null, sampleMask);
            }

            if (rasterizerState != previousPipeline.rasterizerState)
            {
                nativeDeviceContext.Rasterizer.State = rasterizerState;
            }

            if (depthStencilState != previousPipeline.depthStencilState)
            {
                nativeDeviceContext.OutputMerger.DepthStencilState = depthStencilState;
            }

            if (inputLayout != previousPipeline.inputLayout)
            {
                nativeDeviceContext.InputAssembler.InputLayout = inputLayout;
            }

            if (primitiveTopology != previousPipeline.primitiveTopology)
            {
                nativeDeviceContext.InputAssembler.PrimitiveTopology = primitiveTopology;
            }
        }

        private void CreateBlendState(BlendStateDescription description)
        {
            var nativeDescription = new SharpDX.Direct3D11.BlendStateDescription();

            nativeDescription.AlphaToCoverageEnable = description.AlphaToCoverageEnable;
            nativeDescription.IndependentBlendEnable = description.IndependentBlendEnable;
            for (int i = 0; i < description.RenderTargets.Length; ++i)
            {
                nativeDescription.RenderTarget[i].IsBlendEnabled = description.RenderTargets[i].BlendEnable;
                nativeDescription.RenderTarget[i].SourceBlend = (SharpDX.Direct3D11.BlendOption)description.RenderTargets[i].ColorSourceBlend;
                nativeDescription.RenderTarget[i].DestinationBlend = (SharpDX.Direct3D11.BlendOption)description.RenderTargets[i].ColorDestinationBlend;
                nativeDescription.RenderTarget[i].BlendOperation = (SharpDX.Direct3D11.BlendOperation)description.RenderTargets[i].ColorBlendFunction;
                nativeDescription.RenderTarget[i].SourceAlphaBlend = (SharpDX.Direct3D11.BlendOption)description.RenderTargets[i].AlphaSourceBlend;
                nativeDescription.RenderTarget[i].DestinationAlphaBlend = (SharpDX.Direct3D11.BlendOption)description.RenderTargets[i].AlphaDestinationBlend;
                nativeDescription.RenderTarget[i].AlphaBlendOperation = (SharpDX.Direct3D11.BlendOperation)description.RenderTargets[i].AlphaBlendFunction;
                nativeDescription.RenderTarget[i].RenderTargetWriteMask = (SharpDX.Direct3D11.ColorWriteMaskFlags)description.RenderTargets[i].ColorWriteChannels;
            }

            blendState = new SharpDX.Direct3D11.BlendState(NativeDevice, nativeDescription);
        }

        private void CreateRasterizerState(RasterizerStateDescription description)
        {
            SharpDX.Direct3D11.RasterizerStateDescription nativeDescription;

            nativeDescription.CullMode = (SharpDX.Direct3D11.CullMode)description.CullMode;
            nativeDescription.FillMode = (SharpDX.Direct3D11.FillMode)description.FillMode;
            nativeDescription.IsFrontCounterClockwise = description.FrontFaceCounterClockwise;
            nativeDescription.DepthBias = description.DepthBias;
            nativeDescription.SlopeScaledDepthBias = description.SlopeScaleDepthBias;
            nativeDescription.DepthBiasClamp = description.DepthBiasClamp;
            nativeDescription.IsDepthClipEnabled = description.DepthClipEnable;
            nativeDescription.IsScissorEnabled = description.ScissorTestEnable;
            nativeDescription.IsMultisampleEnabled = description.MultiSampleAntiAlias;
            nativeDescription.IsAntialiasedLineEnabled = description.MultiSampleAntiAliasLine;

            rasterizerState = new SharpDX.Direct3D11.RasterizerState(NativeDevice, nativeDescription);
        }

        private void CreateDepthStencilState(DepthStencilStateDescription description)
        {
            SharpDX.Direct3D11.DepthStencilStateDescription nativeDescription;

            nativeDescription.IsDepthEnabled = description.DepthBufferEnable;
            nativeDescription.DepthComparison = (SharpDX.Direct3D11.Comparison)description.DepthBufferFunction;
            nativeDescription.DepthWriteMask = description.DepthBufferWriteEnable ? SharpDX.Direct3D11.DepthWriteMask.All : SharpDX.Direct3D11.DepthWriteMask.Zero;

            nativeDescription.IsStencilEnabled = description.StencilEnable;
            nativeDescription.StencilReadMask = description.StencilMask;
            nativeDescription.StencilWriteMask = description.StencilWriteMask;

            nativeDescription.FrontFace.FailOperation = (SharpDX.Direct3D11.StencilOperation)description.FrontFace.StencilFail;
            nativeDescription.FrontFace.PassOperation = (SharpDX.Direct3D11.StencilOperation)description.FrontFace.StencilPass;
            nativeDescription.FrontFace.DepthFailOperation = (SharpDX.Direct3D11.StencilOperation)description.FrontFace.StencilDepthBufferFail;
            nativeDescription.FrontFace.Comparison = (SharpDX.Direct3D11.Comparison)description.FrontFace.StencilFunction;

            nativeDescription.BackFace.FailOperation = (SharpDX.Direct3D11.StencilOperation)description.BackFace.StencilFail;
            nativeDescription.BackFace.PassOperation = (SharpDX.Direct3D11.StencilOperation)description.BackFace.StencilPass;
            nativeDescription.BackFace.DepthFailOperation = (SharpDX.Direct3D11.StencilOperation)description.BackFace.StencilDepthBufferFail;
            nativeDescription.BackFace.Comparison = (SharpDX.Direct3D11.Comparison)description.BackFace.StencilFunction;

            depthStencilState = new SharpDX.Direct3D11.DepthStencilState(NativeDevice, nativeDescription);
        }

        private void CreateInputLayout(InputElementDescription[] inputElements)
        {
            if (inputElements == null)
                return;

            var nativeInputElements = new SharpDX.Direct3D11.InputElement[inputElements.Length];
            for (int index = 0; index < inputElements.Length; index++)
            {
                var inputElement = inputElements[index];
                nativeInputElements[index] = new SharpDX.Direct3D11.InputElement
                {
                    Slot = inputElement.InputSlot,
                    SemanticName = inputElement.SemanticName,
                    SemanticIndex = inputElement.SemanticIndex,
                    AlignedByteOffset = inputElement.AlignedByteOffset,
                    Format = (SharpDX.DXGI.Format)inputElement.Format,
                };
            }
            inputLayout = new SharpDX.Direct3D11.InputLayout(NativeDevice, inputSignature, nativeInputElements);
        }

        private void CreateShaders()
        {
            if (effectBytecode == null)
                return;

            foreach (var shaderBytecode in effectBytecode.Stages)
            {
                var bytecodeRaw = shaderBytecode.Data;
                var reflection = effectBytecode.Reflection;

                // TODO CACHE Shaders with a bytecode hash
                switch (shaderBytecode.Stage)
                {
                    case ShaderStage.Vertex:
                        vertexShader = new SharpDX.Direct3D11.VertexShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        // Note: input signature can be reused when reseting device since it only stores non-GPU data,
                        // so just keep it if it has already been created before.
                        if (inputSignature == null)
                            inputSignature = bytecodeRaw;
                        break;
                    case ShaderStage.Domain:
                        domainShader = new SharpDX.Direct3D11.DomainShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                    case ShaderStage.Hull:
                        hullShader = new SharpDX.Direct3D11.HullShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                    case ShaderStage.Geometry:
                        if (reflection.ShaderStreamOutputDeclarations != null && reflection.ShaderStreamOutputDeclarations.Count > 0)
                        {
                            // Calculate the strides
                            var soStrides = new List<int>();
                            foreach (var streamOutputElement in reflection.ShaderStreamOutputDeclarations)
                            {
                                for (int i = soStrides.Count; i < (streamOutputElement.Stream + 1); i++)
                                {
                                    soStrides.Add(0);
                                }

                                soStrides[streamOutputElement.Stream] += streamOutputElement.ComponentCount * sizeof(float);
                            }
                            var soElements = new SharpDX.Direct3D11.StreamOutputElement[0]; // TODO CREATE StreamOutputElement from bytecode.Reflection.ShaderStreamOutputDeclarations
                            geometryShader = new SharpDX.Direct3D11.GeometryShader(GraphicsDevice.NativeDevice, bytecodeRaw, soElements, soStrides.ToArray(), reflection.StreamOutputRasterizedStream);
                        }
                        else
                        {
                            geometryShader = new SharpDX.Direct3D11.GeometryShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        }
                        break;
                    case ShaderStage.Pixel:
                        pixelShader = new SharpDX.Direct3D11.PixelShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                    case ShaderStage.Compute:
                        computeShader = new SharpDX.Direct3D11.ComputeShader(GraphicsDevice.NativeDevice, bytecodeRaw);
                        break;
                }
            }
        }
    }
}