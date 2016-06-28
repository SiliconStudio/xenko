// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Renders <see cref="RenderMesh"/>.
    /// </summary>
    public class MeshRenderFeature : RootEffectRenderFeature
    {
        public TrackingCollection<SubRenderFeature> RenderFeatures = new TrackingCollection<SubRenderFeature>();

        private DescriptorSet[] descriptorSets;

        /// <inheritdoc/>
        public override Type SupportedRenderObjectType => typeof(RenderMesh);

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderFeatures.CollectionChanged += RenderFeatures_CollectionChanged;

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.AttachRootRenderFeature(this);
                renderFeature.Initialize(Context);
            }
        }

        protected override void Destroy()
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Dispose();
            }

            RenderFeatures.CollectionChanged -= RenderFeatures_CollectionChanged;

            base.Destroy();
        }

        /// <inheritdoc/>
        public override void Collect()
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Collect();
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

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void PrepareEffectPermutationsImpl(RenderDrawContext context)
        {
            // Setup ActiveMeshDraw
            foreach (var objectNodeReference in ObjectNodeReferences)
            {
                var objectNode = GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;

                renderMesh.ActiveMeshDraw = renderMesh.Mesh.Draw;
            }

            base.PrepareEffectPermutationsImpl(context);

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareEffectPermutations(context);
            }
        }

        /// <inheritdoc/>
        public override void Prepare(RenderDrawContext context)
        {
            base.Prepare(context);

            // Prepare each sub render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Prepare(context);
            }
        }

        protected override void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            var renderMesh = (RenderMesh)renderObject;
            var drawData = renderMesh.ActiveMeshDraw;

            pipelineState.InputElements = drawData.VertexBuffers.CreateInputElements();
            pipelineState.PrimitiveType = drawData.PrimitiveType;
        }

        /// <inheritdoc/>
        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var commandList = context.CommandList;

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Draw(context, renderView, renderViewStage, startIndex, endIndex);
            }

            Array.Resize(ref descriptorSets, EffectDescriptorSetSlotCount);

            MeshDraw currentDrawData = null;
            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderMesh = (RenderMesh)renderNode.RenderObject;
                var drawData = renderMesh.ActiveMeshDraw;

                // Get effect
                // TODO: Use real effect slot
                var renderEffect = renderNode.RenderEffect;
                if (renderEffect.Effect == null)
                    continue;

                // Bind VB
                if (currentDrawData != drawData)
                {
                    for (int i = 0; i < drawData.VertexBuffers.Length; i++)
                    {
                        var vertexBuffer = drawData.VertexBuffers[i];
                        commandList.SetVertexBuffer(i, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Stride);
                    }
                    if (drawData.IndexBuffer != null)
                        commandList.SetIndexBuffer(drawData.IndexBuffer.Buffer, drawData.IndexBuffer.Offset, drawData.IndexBuffer.Is32Bit);
                    currentDrawData = drawData;
                }

                var resourceGroupOffset = ComputeResourceGroupOffset(renderNodeReference);
                
                // Update cbuffer
                renderEffect.Reflection.BufferUploader.Apply(context.CommandList, ResourceGroupPool, resourceGroupOffset);

                // Bind descriptor sets
                for (int i = 0; i < descriptorSets.Length; ++i)
                {
                    var resourceGroup = ResourceGroupPool[resourceGroupOffset++];
                    if (resourceGroup != null)
                        descriptorSets[i] = resourceGroup.DescriptorSet;
                }

                commandList.SetPipelineState(renderEffect.PipelineState);
                commandList.SetDescriptorSets(0, descriptorSets);

                // Draw
                if (drawData.IndexBuffer == null)
                {
                    commandList.Draw(drawData.DrawCount, drawData.StartLocation);
                }
                else
                {
                    commandList.DrawIndexed(drawData.DrawCount, drawData.StartLocation);
                }
            }
        }

        private void RenderFeatures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var renderFeature = (SubRenderFeature)e.Item;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    renderFeature.AttachRootRenderFeature(this);
                    renderFeature.Initialize(Context);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    renderFeature.Dispose();
                    break;
            }
        }
    }
}