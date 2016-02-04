using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Renders <see cref="RenderMesh"/>.
    /// </summary>
    public class MeshRenderFeature : RootEffectRenderFeature
    {
        public List<SubRenderFeature> RenderFeatures = new List<SubRenderFeature>();

        /// <inheritdoc/>
        public override bool SupportsRenderObject(RenderObject renderObject)
        {
            return renderObject is RenderMesh;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.AttachRootRenderFeature(this);
                renderFeature.Initialize();
            }
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Extract();
            }
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutationsImpl()
        {
            base.PrepareEffectPermutationsImpl();

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareEffectPermutations();
            }
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void Prepare(NextGenRenderContext context)
        {
            base.Prepare(context);

            // Prepare each sub render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Prepare(context);
            }
        }

        /// <inheritdoc/>
        public override void Draw(NextGenRenderContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var graphicsDevice = RenderSystem.GraphicsDevice;

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Draw(context, renderView, renderViewStage, startIndex, endIndex);
            }

            var pipelineState = context.Pipeline.State;
            Effect currentEffect = null;
            var descriptorSets = new DescriptorSet[EffectDescriptorSetSlotCount];

            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderMesh = (RenderMesh)renderNode.RenderObject;
                var drawData = renderMesh.Mesh.Draw;

                // Get effect
                // TODO: Use real effect slot
                var renderEffect = renderNode.RenderEffect;

                if (currentEffect != renderEffect.Effect)
                {
                    currentEffect = renderEffect.Effect;
                    pipelineState.EffectBytecode = renderEffect.Effect.Bytecode;
                    pipelineState.RootSignature = renderEffect.Reflection.RootSignature;
                }

                // Bind VB
                for (int i = 0; i < drawData.VertexBuffers.Length; i++)
                {
                    var vertexBuffer = drawData.VertexBuffers[i];
                    graphicsDevice.SetVertexBuffer(i, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Stride);
                }

                var resourceGroupOffset = ComputeResourceGroupOffset(renderNodeReference);
                // Update cbuffer
                renderEffect.Reflection.BufferUploader.Apply(graphicsDevice, ResourceGroupPool, resourceGroupOffset);

                // Bind descriptor sets
                for (int i = 0; i < descriptorSets.Length; ++i)
                {
                    var resourceGroup = ResourceGroupPool[resourceGroupOffset++];
                    if (resourceGroup != null)
                        descriptorSets[i] = resourceGroup.DescriptorSet;
                }

                graphicsDevice.SetDescriptorSets(0, descriptorSets);

                // First time, let's compile pipeline state
                // TODO GRAPHICS REFACTOR invalidate if effect is destroyed, or some other cases
                if (renderEffect.PipelineState == null)
                {
                    // Bind VAO
                    pipelineState.InputElements = drawData.VertexBuffers.CreateInputElements();
                    pipelineState.PrimitiveType = drawData.PrimitiveType;

                    // TODO GRAPHICS REFACTOR
                    // pipelineState.RenderTargetFormats = 

                    ProcessPipelineState?.Invoke(renderNodeReference, ref renderNode, renderMesh, pipelineState);

                    context.Pipeline.Update(graphicsDevice);
                    renderEffect.PipelineState = context.Pipeline.CurrentState;
                }

                graphicsDevice.SetPipelineState(renderEffect.PipelineState);

                // Draw
                if (drawData.IndexBuffer == null)
                {
                    graphicsDevice.Draw(drawData.PrimitiveType, drawData.DrawCount, drawData.StartLocation);
                }
                else
                {
                    graphicsDevice.SetIndexBuffer(drawData.IndexBuffer.Buffer, drawData.IndexBuffer.Offset, drawData.IndexBuffer.Is32Bit);
                    graphicsDevice.DrawIndexed(drawData.PrimitiveType, drawData.DrawCount, drawData.StartLocation);
                }
            }
        }
    }
}