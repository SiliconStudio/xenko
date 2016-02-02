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
                    renderEffect.Effect.ApplyProgram(graphicsDevice);
                }

                renderEffect.Reflection.Binder.Apply(graphicsDevice, ResourceGroupPool, ComputeResourceGroupOffset(renderNodeReference));

                // Bind VAO
                var vertexArrayObject = renderEffect.VertexArrayObject ??
                                        (renderEffect.VertexArrayObject = VertexArrayObject.New(RenderSystem.GraphicsDevice, renderEffect.Effect.InputSignature, drawData.IndexBuffer, drawData.VertexBuffers));
                graphicsDevice.SetVertexArrayObject(vertexArrayObject);

                // Draw
                if (drawData.IndexBuffer == null)
                {
                    graphicsDevice.Draw(drawData.PrimitiveType, drawData.DrawCount, drawData.StartLocation);
                }
                else
                {
                    graphicsDevice.DrawIndexed(drawData.PrimitiveType, drawData.DrawCount, drawData.StartLocation);
                }
            }
        }
    }
}