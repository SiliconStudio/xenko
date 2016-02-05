// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;
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
        public void BindResources(CommandList commandList, DescriptorSet[] descriptorSets)
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
                                commandList.SetConstantBuffer(bindingOperation.Stage, bindingOperation.SlotStart, (Buffer)value.Value);
                                break;
                            }
                        case EffectParameterClass.Sampler:
                            {
                                commandList.SetSamplerState(bindingOperation.Stage, bindingOperation.SlotStart, bindingOperation.ImmutableSampler ?? (SamplerState)value.Value);
                                break;
                            }
                        case EffectParameterClass.ShaderResourceView:
                            {
                                commandList.SetShaderResourceView(bindingOperation.Stage, bindingOperation.SlotStart, (GraphicsResource)value.Value);
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
        // Caches
        private static GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.VertexShader> vertexShaderCache;
        private static GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.PixelShader> pixelShaderCache;
        private static GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.GeometryShader> geometryShaderCache;
        private static GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.HullShader> hullShaderCache;
        private static GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.DomainShader> domainShaderCache;
        private static GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.ComputeShader> computeShaderCache;
        private static GraphicsCache<BlendStateDescription, BlendStateDescription, SharpDX.Direct3D11.BlendState> blendStateCache;
        private static GraphicsCache<RasterizerStateDescription, RasterizerStateDescription, SharpDX.Direct3D11.RasterizerState> rasterizerStateCache;
        private static GraphicsCache<DepthStencilStateDescription, DepthStencilStateDescription, SharpDX.Direct3D11.DepthStencilState> depthStencilStateCache;

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
        private readonly uint sampleMask;
        private SharpDX.Direct3D11.RasterizerState rasterizerState;
        private SharpDX.Direct3D11.DepthStencilState depthStencilState;

        private SharpDX.Direct3D11.InputLayout inputLayout;

        private readonly SharpDX.Direct3D.PrimitiveTopology primitiveTopology;
        // Note: no need to store RTV/DSV formats

        internal PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription) : base(graphicsDevice)
        {
            // First time, build caches
            if (vertexShaderCache == null)
            {
                // Shaders
                vertexShaderCache = new GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.VertexShader>(source => ObjectId.FromBytes(source), source => new SharpDX.Direct3D11.VertexShader(graphicsDevice.NativeDevice, source));
                pixelShaderCache = new GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.PixelShader>(source => ObjectId.FromBytes(source), source => new SharpDX.Direct3D11.PixelShader(graphicsDevice.NativeDevice, source));
                geometryShaderCache = new GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.GeometryShader>(source => ObjectId.FromBytes(source), source => new SharpDX.Direct3D11.GeometryShader(graphicsDevice.NativeDevice, source));
                hullShaderCache = new GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.HullShader>(source => ObjectId.FromBytes(source), source => new SharpDX.Direct3D11.HullShader(graphicsDevice.NativeDevice, source));
                domainShaderCache = new GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.DomainShader>(source => ObjectId.FromBytes(source), source => new SharpDX.Direct3D11.DomainShader(graphicsDevice.NativeDevice, source));
                computeShaderCache = new GraphicsCache<byte[], ObjectId, SharpDX.Direct3D11.ComputeShader>(source => ObjectId.FromBytes(source), source => new SharpDX.Direct3D11.ComputeShader(graphicsDevice.NativeDevice, source));

                // States
                blendStateCache = new GraphicsCache<BlendStateDescription, BlendStateDescription, SharpDX.Direct3D11.BlendState>(source => source, source => new SharpDX.Direct3D11.BlendState(graphicsDevice.NativeDevice, CreateBlendState(source)));
                rasterizerStateCache = new GraphicsCache<RasterizerStateDescription, RasterizerStateDescription, SharpDX.Direct3D11.RasterizerState>(source => source, source => new SharpDX.Direct3D11.RasterizerState(graphicsDevice.NativeDevice, CreateRasterizerState(source)));
                depthStencilStateCache = new GraphicsCache<DepthStencilStateDescription, DepthStencilStateDescription, SharpDX.Direct3D11.DepthStencilState>(source => source, source => new SharpDX.Direct3D11.DepthStencilState(graphicsDevice.NativeDevice, CreateDepthStencilState(source)));
            }

            // Effect
            this.rootSignature = pipelineStateDescription.RootSignature;
            this.effectBytecode = pipelineStateDescription.EffectBytecode;
            CreateShaders();
            if (rootSignature != null && effectBytecode != null)
                ResourceBinder.Compile(graphicsDevice, rootSignature.EffectDescriptorSetReflection, this.effectBytecode);

            // TODO: Cache over Effect|RootSignature to create binding operations

            // States
            blendState = blendStateCache.Instantiate(pipelineStateDescription.BlendState);
            this.sampleMask = pipelineStateDescription.SampleMask;
            rasterizerState = rasterizerStateCache.Instantiate(pipelineStateDescription.RasterizerState);
            depthStencilState = depthStencilStateCache.Instantiate(pipelineStateDescription.DepthStencilState);

            CreateInputLayout(pipelineStateDescription.InputElements);

            primitiveTopology = (SharpDX.Direct3D.PrimitiveTopology)pipelineStateDescription.PrimitiveType;
        }

        internal void Apply(CommandList commandList, PipelineState previousPipeline)
        {
            var nativeDeviceContext = commandList.NativeDeviceContext;

            if (rootSignature != previousPipeline.rootSignature)
            {
                //rootSignature.Apply
            }

            if (effectBytecode != previousPipeline.effectBytecode)
            {
                if (computeShader != null)
                {
                    if (computeShader != previousPipeline.computeShader)
                        nativeDeviceContext.ComputeShader.Set(computeShader);
                }
                else
                {
                    if (vertexShader != previousPipeline.vertexShader)
                        nativeDeviceContext.VertexShader.Set(vertexShader);
                    if (pixelShader != previousPipeline.pixelShader)
                        nativeDeviceContext.PixelShader.Set(pixelShader);
                    if (hullShader != previousPipeline.hullShader)
                        nativeDeviceContext.HullShader.Set(hullShader);
                    if (domainShader != previousPipeline.domainShader)
                        nativeDeviceContext.DomainShader.Set(domainShader);
                    if (geometryShader != previousPipeline.geometryShader)
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

        protected override void DestroyImpl()
        {
            base.DestroyImpl();

            if (vertexShader != null)
                vertexShaderCache.Release(vertexShader);
            if (pixelShader != null)
                pixelShaderCache.Release(pixelShader);
            if (geometryShader != null)
                geometryShaderCache.Release(geometryShader);
            if (hullShader != null)
                hullShaderCache.Release(hullShader);
            if (domainShader != null)
                domainShaderCache.Release(domainShader);
            if (computeShader != null)
                computeShaderCache.Release(computeShader);
        }

        private SharpDX.Direct3D11.BlendStateDescription CreateBlendState(BlendStateDescription description)
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

            return nativeDescription;
        }

        private SharpDX.Direct3D11.RasterizerStateDescription CreateRasterizerState(RasterizerStateDescription description)
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

            return nativeDescription;
        }

        private SharpDX.Direct3D11.DepthStencilStateDescription CreateDepthStencilState(DepthStencilStateDescription description)
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

            return nativeDescription;
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
                        vertexShader = vertexShaderCache.Instantiate(bytecodeRaw);
                        // Note: input signature can be reused when reseting device since it only stores non-GPU data,
                        // so just keep it if it has already been created before.
                        if (inputSignature == null)
                            inputSignature = bytecodeRaw;
                        break;
                    case ShaderStage.Domain:
                        domainShader = domainShaderCache.Instantiate(bytecodeRaw);
                        break;
                    case ShaderStage.Hull:
                        hullShader = hullShaderCache.Instantiate(bytecodeRaw);
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
                            // TODO GRAPHICS REFACTOR better cache
                            geometryShader = new SharpDX.Direct3D11.GeometryShader(GraphicsDevice.NativeDevice, bytecodeRaw, soElements, soStrides.ToArray(), reflection.StreamOutputRasterizedStream);
                        }
                        else
                        {
                            geometryShader = geometryShaderCache.Instantiate(bytecodeRaw);
                        }
                        break;
                    case ShaderStage.Pixel:
                        pixelShader = pixelShaderCache.Instantiate(bytecodeRaw);
                        break;
                    case ShaderStage.Compute:
                        computeShader = computeShaderCache.Instantiate(bytecodeRaw);
                        break;
                }
            }
        }

        // Small helper to cache SharpDX graphics objects
        class GraphicsCache<TSource, TKey, TValue> where TValue : SharpDX.IUnknown
        {
            private object lockObject = new object();

            // Store instantiated objects
            private readonly Dictionary<TKey, TValue> storage = new Dictionary<TKey, TValue>();
            // Used for quick removal
            private readonly Dictionary<TValue, TKey> reverse = new Dictionary<TValue, TKey>();

            private readonly Func<TSource, TKey> computeKey;
            private readonly Func<TSource, TValue> computeValue;

            public GraphicsCache(Func<TSource, TKey> computeKey, Func<TSource, TValue> computeValue)
            {
                this.computeKey = computeKey;
                this.computeValue = computeValue;
            }

            public TValue Instantiate(TSource source)
            {
                lock (lockObject)
                {
                    TValue value;
                    var key = computeKey(source);
                    if (!storage.TryGetValue(key, out value))
                    {
                        value = computeValue(source);
                        storage.Add(key, value);
                    }
                    else
                    {
                        value.AddReference();
                    }

                    return value;
                }
            }

            public void Release(TValue value)
            {
                // Should we remove it from the cache?
                lock (lockObject)
                {
                    int newRefCount = value.Release();
                    if (newRefCount == 0)
                    {
                        reverse.Remove(value);
                        TKey key;
                        if (reverse.TryGetValue(value, out key))
                        {
                            storage.Remove(key);
                        }
                    }
                }
            }
        }
    }
}