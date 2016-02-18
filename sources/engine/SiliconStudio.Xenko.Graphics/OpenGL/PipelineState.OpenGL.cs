// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Shaders;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using PrimitiveTypeGl = OpenTK.Graphics.ES30.PrimitiveType;
#else
using OpenTK.Graphics.OpenGL;
using PrimitiveTypeGl = OpenTK.Graphics.OpenGL.PrimitiveType;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public partial class PipelineState
    {
        // Caches
        private static GraphicsCache<EffectBytecode, EffectBytecode, EffectProgram> effectProgramCache;

        internal readonly BlendState BlendState;
        internal readonly DepthStencilState DepthStencilState;

        internal readonly RasterizerState RasterizerState;

        internal readonly EffectProgram EffectProgram;

        internal readonly PrimitiveTypeGl PrimitiveType;
        internal readonly VertexAttrib[] VertexAttribs;
        internal ResourceBinder ResourceBinder;

        private PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription) : base(graphicsDevice)
        {
            if (effectProgramCache == null)
            {
                effectProgramCache = new GraphicsCache<EffectBytecode, EffectBytecode, EffectProgram>(source => source, source => new EffectProgram(graphicsDevice, source));
            }

            // Store states
            BlendState = new BlendState(pipelineStateDescription.BlendState, pipelineStateDescription.Output.RenderTargetCount > 0);
            RasterizerState = new RasterizerState(pipelineStateDescription.RasterizerState);
            DepthStencilState = new DepthStencilState(pipelineStateDescription.DepthStencilState, pipelineStateDescription.Output.DepthStencilFormat != PixelFormat.None);

            PrimitiveType = pipelineStateDescription.PrimitiveType.ToOpenGL();

            // Compile effect
            var effectBytecode = pipelineStateDescription.EffectBytecode;
            EffectProgram = effectBytecode != null ? effectProgramCache.Instantiate(effectBytecode) : null;

            var rootSignature = pipelineStateDescription.RootSignature;
            if (rootSignature != null && effectBytecode != null)
                ResourceBinder.Compile(graphicsDevice, rootSignature.EffectDescriptorSetReflection, effectBytecode);

            // Vertex attributes
            if (pipelineStateDescription.InputElements != null)
            {
                var vertexAttribs = new List<VertexAttrib>();
                foreach (var inputElement in pipelineStateDescription.InputElements)
                {
                    // Query attribute name from effect
                    var attributeName = "a_" + inputElement.SemanticName + inputElement.SemanticIndex;
                    int attributeIndex;
                    if (!EffectProgram.Attributes.TryGetValue(attributeName, out attributeIndex))
                        continue;

                    var vertexElementFormat = VertexAttrib.ConvertVertexElementFormat(inputElement.Format);
                    vertexAttribs.Add(new VertexAttrib(
                        inputElement.InputSlot,
                        attributeIndex,
                        vertexElementFormat.Size,
                        vertexElementFormat.Type,
                        vertexElementFormat.Normalized,
                        inputElement.AlignedByteOffset));
                }

                VertexAttribs = vertexAttribs.ToArray();
            }
        }

        internal void Apply(CommandList commandList, PipelineState previousPipeline)
        {
            // Apply states
            if (BlendState != previousPipeline.BlendState)
                BlendState.Apply(previousPipeline.BlendState);
            if (RasterizerState != previousPipeline.RasterizerState)
                RasterizerState.Apply();
            if (DepthStencilState != previousPipeline.DepthStencilState)
                DepthStencilState.Apply(0); // TODO GRAPHICS REFACTOR stencil reference support
        }

        protected override void DestroyImpl()
        {
            base.DestroyImpl();

            if (EffectProgram != null)
                effectProgramCache.Release(EffectProgram);
        }

        // Small helper to cache SharpDX graphics objects
        class GraphicsCache<TSource, TKey, TValue>
        {
            private object lockObject = new object();

            // Store instantiated objects
            private readonly Dictionary<TKey, TValue> storage = new Dictionary<TKey, TValue>();
            // Used for quick removal
            private readonly Dictionary<TValue, TKey> reverse = new Dictionary<TValue, TKey>();

            private readonly Dictionary<TValue, int> counter = new Dictionary<TValue, int>();

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
                        int currentCounter;
                        counter.TryGetValue(value, out currentCounter);
                        counter[value] = currentCounter + 1;
                    }

                    return value;
                }
            }

            public void Release(TValue value)
            {
                // Should we remove it from the cache?
                lock (lockObject)
                {
                    var newRefCount = counter[value] - 1;
                    counter[value] = newRefCount--;
                    if (newRefCount == 0)
                    {
                        counter.Remove(value);
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
#endif