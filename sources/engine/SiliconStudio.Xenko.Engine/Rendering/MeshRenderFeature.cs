// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Threading;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Renders <see cref="RenderMesh"/>.
    /// </summary>
    public class MeshRenderFeature : RootEffectRenderFeature
    {
        /// <summary>
        /// Lists of sub render features that can be applied on <see cref="RenderMesh"/>.
        /// </summary>
        [DataMember]
        [Category]
        [MemberCollection(CanReorderItems = true, NotNullItems = true)]
        public TrackingCollection<SubRenderFeature> RenderFeatures = new TrackingCollection<SubRenderFeature>();

        private readonly ThreadLocal<DescriptorSet[]> descriptorSets = new ThreadLocal<DescriptorSet[]>();

        // stream absence compensation buffer
        private Buffer zeroBuffer;

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

            // initialize the global stream-compensatory buffer
            var bufferDataSource = new byte[PixelFormat.R32G32B32A32_Float.SizeInBytes()];
            var white = new float[4];
            for (int i = 0; i < 4; ++i)
                white[i] = 1.0f;
            System.Buffer.BlockCopy(white, 0, bufferDataSource, 0, 16);
            unsafe
            {
                fixed (byte* zeroBufferFloatPointer = bufferDataSource)
                    zeroBuffer = Buffer.Vertex.New(Context.GraphicsDevice, new DataPointer(zeroBufferFloatPointer, bufferDataSource.Length));
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
            Dispatcher.ForEach(ObjectNodeReferences, objectNodeReference =>
            {
                var objectNode = GetObjectNode(objectNodeReference);
                var renderMesh = (RenderMesh)objectNode.RenderObject;

                renderMesh.ActiveMeshDraw = renderMesh.Mesh.Draw;
            });

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

            pipelineState.InputElements = PrepareInputElements(pipelineState, drawData);
            pipelineState.PrimitiveType = drawData.PrimitiveType;

            // Prepare each sub render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.ProcessPipelineState(context, renderNodeReference, ref renderNode, renderObject, pipelineState);
            }
        }

        /// <inheritdoc/>
        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage)
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Draw(context, renderView, renderViewStage);
            }
        }

        /// <inheritdoc/>
        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var commandList = context.CommandList;

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Draw(context, renderView, renderViewStage, startIndex, endIndex);
            }

            // TODO: stackalloc?
            var descriptorSetsLocal = descriptorSets.Value;
            if (descriptorSetsLocal == null || descriptorSetsLocal.Length < EffectDescriptorSetSlotCount)
            {
                descriptorSetsLocal = descriptorSets.Value = new DescriptorSet[EffectDescriptorSetSlotCount];
            }
            
            MeshDraw currentDrawData = null;
            int zeroBufferLastSlot = -1;
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
                    // Here we bind a safety stream made up of one element, to accomodate for effects
                    // which input's layouts requires a stream that wasn't present in the mesh's original buffer.
                    if (zeroBufferLastSlot != drawData.VertexBuffers.Length)
                    {
                        // use a 0 stride to be able to cover any draw size, but with just a buffer of size 1.
                        commandList.SetVertexBuffer(drawData.VertexBuffers.Length, zeroBuffer, 0, 0);
                        zeroBufferLastSlot = drawData.VertexBuffers.Length;
                    }
                    if (drawData.IndexBuffer != null)
                        commandList.SetIndexBuffer(drawData.IndexBuffer.Buffer, drawData.IndexBuffer.Offset, drawData.IndexBuffer.Is32Bit);
                    currentDrawData = drawData;
                }

                var resourceGroupOffset = ComputeResourceGroupOffset(renderNodeReference);
                
                // Update cbuffer
                renderEffect.Reflection.BufferUploader.Apply(context.CommandList, ResourceGroupPool, resourceGroupOffset);

                // Bind descriptor sets
                for (int i = 0; i < descriptorSetsLocal.Length; ++i)
                {
                    var resourceGroup = ResourceGroupPool[resourceGroupOffset++];
                    if (resourceGroup != null)
                        descriptorSetsLocal[i] = resourceGroup.DescriptorSet;
                }

                commandList.SetPipelineState(renderEffect.PipelineState);
                commandList.SetDescriptorSets(0, descriptorSetsLocal);

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

        /// <inheritdoc/>
        public override void Flush(RenderDrawContext context)
        {
            base.Flush(context);

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Flush(context);
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

        private InputElementDescription[] PrepareInputElements(PipelineStateDescription pipelineState, MeshDraw drawData)
        {
            // access the input layout excpected by the shader
            var layout = pipelineState.EffectBytecode.Reflection.InputParameterDescription;

            // prepare available elements from the present streams
            var availableInputElements = drawData.VertexBuffers.CreateInputElements(); // TODO: defend against systematic recreation? **

            // the number of elements is going to be the count of elements expected by the input layout
            var elements = new InputElementDescription[layout.Count];  // TODO ** (here too)

            for (int i = 0; i < layout.Count; ++i)
            {
                var requiredInput = layout[i];
                int elementIndex = FindElementBySemantic(requiredInput.SemanticName, requiredInput.SemanticIndex, availableInputElements);
                if (elementIndex != -1)
                    elements[i] = availableInputElements[elementIndex];
                else
                {
                    // return a default element
                    elements[i] = new InputElementDescription
                    {
                        AlignedByteOffset = 0,
                        Format = PixelFormat.R32G32B32A32_Float,
                        InputSlot = drawData.VertexBuffers.Length,
                        InputSlotClass = InputClassification.Vertex,
                        InstanceDataStepRate = 0,
                        SemanticIndex = requiredInput.SemanticIndex,
                        SemanticName = requiredInput.SemanticName
                    };
                }
            }
            return elements;
            //return elements.Where(ied => ied.InputSlot == 0).ToArray();
        }

        private int FindElementBySemantic(string requiredInputSemanticName, int requiredSemanticIndex, InputElementDescription[] availableInputElements)
        {
            int foundDescIndex = -1;
            for (int i = 0; i < availableInputElements.Length; ++i)
                if (requiredInputSemanticName == availableInputElements[i].SemanticName && requiredSemanticIndex == availableInputElements[i].SemanticIndex)
                    foundDescIndex = i;
            return foundDescIndex;
        }
    }
}
