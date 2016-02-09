// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Collections.Generic;
using SiliconStudio.Core;

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
        internal readonly BlendState BlendState;
        internal readonly DepthStencilState DepthStencilState;

        internal readonly RasterizerState RasterizerState;

        internal readonly EffectProgram EffectProgram;

        internal readonly PrimitiveTypeGl PrimitiveType;
        internal readonly VertexAttrib[] VertexAttribs;
        internal ResourceBinder ResourceBinder;

        private PipelineState(GraphicsDevice graphicsDevice, PipelineStateDescription pipelineStateDescription) : base(graphicsDevice)
        {
            // Store states
            BlendState = new BlendState(pipelineStateDescription.BlendState, pipelineStateDescription.Output.RenderTargetCount > 0);
            RasterizerState = new RasterizerState(pipelineStateDescription.RasterizerState);
            DepthStencilState = new DepthStencilState(pipelineStateDescription.DepthStencilState, pipelineStateDescription.Output.DepthStencilFormat != PixelFormat.None);

            PrimitiveType = pipelineStateDescription.PrimitiveType.ToOpenGL();

            // Compile effect
            var effectBytecode = pipelineStateDescription.EffectBytecode;
            EffectProgram = effectBytecode != null ? new EffectProgram(graphicsDevice, effectBytecode) : null;

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
    }
}
#endif