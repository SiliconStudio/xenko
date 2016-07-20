// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Materials;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// Should be identical to the cbuffer PerView in ParticleUtilities.xksl
    /// </summary>
    internal struct ParticleUtilitiesPerView
    {
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
        public Matrix ViewProjectionMatrix;

        // .x - Width, .y - Height, .z - Near, .w - Far
        public Vector4 ViewFrustum;
    }

    internal struct RenderAttributesPerNode
    {
        public Buffer VertexBuffer;
        public Buffer IndexBuffer;
        public int VertexBufferOffset;
        public int VertexBufferStride;
        public int VertexBufferSize;
        public int IndexCount;
        public bool Is32BitIndex;
    }

    /// <summary>
    /// Renders <see cref="RenderParticleEmitter"/>.
    /// </summary>
    public class ParticleEmitterRenderFeature : RootEffectRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private RenderPropertyKey<RenderAttributesPerNode> renderParticleNodeKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        private ConstantBufferOffsetReference perViewCBufferOffset;

        // Material alive during this frame
        private readonly Dictionary<ParticleMaterial, ParticleMaterialInfo> allMaterialInfos = new Dictionary<ParticleMaterial, ParticleMaterialInfo>();

        public override Type SupportedRenderObjectType => typeof(RenderParticleEmitter);

        private DescriptorSet[] descriptorSets;

        private ParticleSharedBuffer sharedBuffer = null;

        private Buffer sharedVertexBuffer;
        private int sharedVertexBufferSize;

        private Buffer sharedIndexBuffer;
        private int sharedIndexBufferSize;

        internal class ParticleMaterialInfo : MaterialRenderFeature.MaterialInfoBase
        {
            public readonly ParticleMaterial Material;

            public ParticleMaterialInfo(ParticleMaterial material)
            {
                Material = material;
            }
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            renderEffectKey = RenderEffectKey;

            renderParticleNodeKey = RenderData.CreateRenderKey<RenderAttributesPerNode>();

            // The offset starts with the first element in the buffer
            perViewCBufferOffset = CreateViewCBufferOffsetSlot(ParticleUtilitiesKeys.ViewMatrix.Name);

            perMaterialDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerMaterial");
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            base.Extract();
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutationsImpl(RenderDrawContext context)
        {
            base.PrepareEffectPermutationsImpl(context);

            var renderEffects = RenderData.GetData(renderEffectKey);
            int effectSlotCount = EffectPermutationSlotCount;

            // Update existing materials
            foreach (var material in allMaterialInfos)
            {
                material.Key.Setup(RenderSystem.RenderContextOld);
            }

            foreach (var renderObject in RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;

                var material = renderParticleEmitter.ParticleEmitter.Material;
                var materialInfo = renderParticleEmitter.ParticleMaterialInfo;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    if (materialInfo == null || materialInfo.Material != material)
                    {
                        // First time this material is initialized, let's create associated info
                        if (!allMaterialInfos.TryGetValue(material, out materialInfo))
                        {
                            materialInfo = new ParticleMaterialInfo(material);
                            allMaterialInfos.Add(material, materialInfo);
                        }
                        renderParticleEmitter.ParticleMaterialInfo = materialInfo;

                        // Update new materials
                        material.Setup(RenderSystem.RenderContextOld);
                    }

                    // TODO: Iterate PermuatationParameters automatically?
                    material.ValidateEffect(RenderSystem.RenderContextOld, ref renderEffect.EffectValidator);
                }
            }
        }

        /// <inheritdoc/>
        public override unsafe void Prepare(RenderDrawContext context)
        {
            // Inspect each RenderObject (= ParticleEmitter) to determine if its required vertex buffer size has changed
            foreach (var renderObject in RenderObjects)
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;
                renderParticleEmitter.ParticleEmitter.PrepareForDraw(out renderParticleEmitter.HasVertexBufferChanged,
                    out renderParticleEmitter.VertexSize, out renderParticleEmitter.VertexCount);

                // Handle vertex element changes
                if (renderParticleEmitter.HasVertexBufferChanged)
                {
                    // Create new buffers
                    renderParticleEmitter.ParticleEmitter.VertexBuilder.RecreateBuffers(RenderSystem.GraphicsDevice);
                }

                // TODO: ParticleMaterial should set this up
                var materialInfo = (ParticleMaterialInfo)renderParticleEmitter.ParticleMaterialInfo;
                materialInfo?.Material.Parameters.Set(ParticleBaseKeys.ColorScale, renderParticleEmitter.RenderParticleSystem.ParticleSystemComponent.Color);
            }

            // Calculate the total vertex buffer size required
            int totalVertexBufferSize = 0;
            int highestIndexCount = 0;
            var renderParticleNodeData = RenderData.GetData(renderParticleNodeKey);

            // Reset pipeline states if necessary
            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNode = RenderNodes[renderNodeIndex];
                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                if (renderParticleEmitter.HasVertexBufferChanged)
                {
                    // Reset pipeline state, so input layout is regenerated
                    if (renderNode.RenderEffect != null)
                        renderNode.RenderEffect.PipelineState = null;
                }

                // Write some attributes back which we will need for rendering later
                var vertexBuilder = renderParticleEmitter.ParticleEmitter.VertexBuilder; 
                var newNodeData = new RenderAttributesPerNode
                {
                    VertexBufferOffset = totalVertexBufferSize,
                    VertexBufferSize = renderParticleEmitter.VertexSize * renderParticleEmitter.VertexCount,
                    IndexCount = vertexBuilder.LivingQuads * vertexBuilder.IndicesPerQuad,
                };

                // TODO Change this to a global vertex buffer
                // TODO Assign Buffers and Offset from a shared VertexBuffer
                if (vertexBuilder.ResourceContext != null)
                {
                    newNodeData.VertexBuffer = vertexBuilder.ResourceContext.VertexBuffer.Buffer;
                    newNodeData.VertexBufferStride = vertexBuilder.ResourceContext.VertexBuffer.Stride;
                    newNodeData.IndexBuffer  = vertexBuilder.ResourceContext.IndexBuffer.Buffer;
                    newNodeData.Is32BitIndex = vertexBuilder.ResourceContext.IndexBuffer.Is32Bit;
                }

                renderParticleNodeData[new RenderNodeReference(renderNodeIndex)] = newNodeData;

                totalVertexBufferSize += newNodeData.VertexBufferSize;
                if (newNodeData.IndexCount > highestIndexCount)
                    highestIndexCount = newNodeData.IndexCount;
            }


            // Build particle buffers
            var commandList = context.CommandList;
            // TODO Map buffers here and build the particle data

            // Build the shared vertex buffer
            if (totalVertexBufferSize > 0)
            {
                int newSize = totalVertexBufferSize;

                if (sharedVertexBuffer != null)
                {
                    context.GraphicsContext.Allocator.ReleaseReference(sharedVertexBuffer);
                    sharedVertexBuffer = null;
                }

                sharedVertexBuffer = context.GraphicsContext.Allocator.GetTemporaryBuffer(new BufferDescription(newSize, BufferFlags.VertexBuffer, GraphicsResourceUsage.Dynamic));

                sharedVertexBufferSize = newSize;
            }


            // Build the shared index buffer
            int indexCount = highestIndexCount;
            var indexSize = indexCount * sizeof(short);
            if (indexSize > 0)
            {
                if (sharedIndexBuffer != null)
                {
                    context.GraphicsContext.Allocator.ReleaseReference(sharedIndexBuffer);
                    sharedIndexBuffer = null;
                }

                sharedIndexBuffer = context.GraphicsContext.Allocator.GetTemporaryBuffer(new BufferDescription(indexSize, BufferFlags.IndexBuffer, GraphicsResourceUsage.Dynamic));

                sharedIndexBufferSize = indexSize;

                {
                    var mappedIndices = commandList.MapSubresource(sharedIndexBuffer, 0, MapMode.WriteNoOverwrite, false, 0, sharedIndexBufferSize);
                    var indexPointer = mappedIndices.DataBox.DataPointer;

                    int indexStructSize = sizeof(short);
                    int verticesPerQuad = 4;

                    var k = 0;
                    for (var i = 0; i < indexCount; k += verticesPerQuad)
                    {
                        *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 0);
                        *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 1);
                        *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 2);
                        *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 0);
                        *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 2);
                        *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 3);
                    }

                    commandList.UnmapSubresource(mappedIndices);
                }
            }

            base.Prepare(context);

            // Assign descriptor sets to each render node
            var resourceGroupPool = ResourceGroupPool;
            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RenderNodes[renderNodeIndex];
                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                // Ignore fallback effects
                if (renderNode.RenderEffect.State != RenderEffectState.Normal)
                    continue;

                // Collect materials and create associated MaterialInfo (includes reflection) first time
                // TODO: We assume same material will generate same ResourceGroup (i.e. same resources declared in same order)
                // Need to offer some protection if this invariant is violated (or support it if it can actually happen in real scenario)
                var material = renderParticleEmitter.ParticleEmitter.Material;
                var materialInfo = renderParticleEmitter.ParticleMaterialInfo;
                var materialParameters = material.Parameters;

                if (!MaterialRenderFeature.UpdateMaterial(RenderSystem, context, materialInfo, perMaterialDescriptorSetSlot.Index, renderNode.RenderEffect, materialParameters))
                    continue;

                var descriptorSetPoolOffset = ComputeResourceGroupOffset(renderNodeReference);
                resourceGroupPool[descriptorSetPoolOffset + perMaterialDescriptorSetSlot.Index] = materialInfo.Resources;
            }

            // Per view
            // TODO: Transform sub render feature?
            for (int index = 0; index < RenderSystem.Views.Count; index++)
            {
                var view = RenderSystem.Views[index];
                var viewFeature = view.Features[Index];

                // TODO GRAPHICS REFACTOR: Happens in several places
                Matrix.Multiply(ref view.View, ref view.Projection, out view.ViewProjection);

                // Copy ViewProjection to PerFrame cbuffer
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    var resourceGroup = viewLayout.Entries[view.Index].Resources;
                    var mappedCB = resourceGroup.ConstantBuffer.Data;

                    // PerView constant buffer
                    var perViewOffset = viewLayout.GetConstantBufferOffset(this.perViewCBufferOffset);
                    if (perViewOffset != -1)
                    {
                        var perView = (ParticleUtilitiesPerView*)((byte*)mappedCB + perViewOffset);
                        perView->ViewMatrix     = view.View;
                        perView->ProjectionMatrix = view.Projection;
                        perView->ViewProjectionMatrix = view.ViewProjection;
                        perView->ViewFrustum = new Vector4(view.ViewSize.X, view.ViewSize.Y, view.NearClipPlane, view.FarClipPlane);
                    }
                }
            }

            if (sharedVertexBuffer == null || sharedVertexBufferSize == 0 || totalVertexBufferSize == 0)
                return;

            var mappedVertices = commandList.MapSubresource(sharedVertexBuffer, 0, MapMode.WriteNoOverwrite, false, 0, sharedVertexBufferSize);
            var sharedBufferPtr = mappedVertices.DataBox.DataPointer;
            //var sharedBufferPtr = IntPtr.Zero;

            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNode = RenderNodes[renderNodeIndex];
                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var nodeData = renderParticleNodeData[renderNodeReference];
                if (nodeData.IndexCount <= 0)
                    continue;   // Nothing to draw, nothing to build

                Matrix viewInverse;
                Matrix.Invert(ref renderNode.RenderView.View, out viewInverse); // TODO Build this per view, not per node!!!
                renderParticleEmitter.ParticleEmitter.BuildVertexBuffer(commandList, sharedBufferPtr + nodeData.VertexBufferOffset, ref viewInverse);
            }

            // TODO Unmap buffers here
            commandList.UnmapSubresource(mappedVertices);
        }

        protected override void InvalidateEffectPermutation(RenderObject renderObject, RenderEffect renderEffect)
        {
            var renderParticleEmitter = (RenderParticleEmitter)renderObject;
            var materialInfo = renderParticleEmitter.ParticleMaterialInfo;
            materialInfo.PerMaterialLayout = null;
        }

        /// <inheritdoc/>
        protected override void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            var renderParticleEmitter = (RenderParticleEmitter)renderObject;

            pipelineState.InputElements = renderParticleEmitter.ParticleEmitter.VertexBuilder.VertexDeclaration.CreateInputElements();
            pipelineState.PrimitiveType = PrimitiveType.TriangleList;

            var material = renderParticleEmitter.ParticleMaterialInfo.Material;
            material.SetupPipeline(context, pipelineState);
        }

        /// <inheritdoc/>
        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var commandList = context.CommandList;

            // TODO: PerView data
            Matrix viewInverse;
            Matrix.Invert(ref renderView.View, out viewInverse);
            var renderParticleNodeData = RenderData.GetData(renderParticleNodeKey);

            Array.Resize(ref descriptorSets, EffectDescriptorSetSlotCount);

            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;

                // Get effect
                var renderEffect = GetRenderNode(renderNodeReference).RenderEffect;
                if (renderEffect.Effect == null)
                    continue;

                // Get the extra node data
                var nodeData = renderParticleNodeData[renderNodeReference];
                if (nodeData.IndexCount <= 0)
                    continue;

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

                // Bind VB
                int vertexOffset = nodeData.VertexBufferOffset;
                int indexOffset = 0;
                commandList.SetVertexBuffer(0, sharedVertexBuffer, vertexOffset, nodeData.VertexBufferStride);
                commandList.SetIndexBuffer(sharedIndexBuffer, indexOffset, nodeData.Is32BitIndex);

                var indexBufferPosition = 0;
                commandList.DrawIndexed(nodeData.IndexCount, indexBufferPosition);
            }
        }

        /// <summary>
        /// Use a ResourceContext per GraphicsDevice (DeviceContext)
        /// </summary>
        public class ParticleSharedBuffer : ComponentBase
        {
            private readonly int VertexBufferSize;

            private readonly int IndexBufferSize;

            public Buffer VertexBuffer;

            public Buffer IndexBuffer;

            public ParticleSharedBuffer(GraphicsDevice device, int vertexBufferSize, int indexBufferSize)
            {
                VertexBufferSize = vertexBufferSize;
                IndexBufferSize = indexBufferSize;

                VertexBuffer = Buffer.Vertex.New(device, vertexBufferSize, GraphicsResourceUsage.Dynamic).DisposeBy(this);
                IndexBuffer = Buffer.Index.New(device, indexBufferSize, GraphicsResourceUsage.Dynamic).DisposeBy(this);

                // IndexBuffer = new IndexBufferBinding(indexBuffer, indexStructSize == sizeof(int), indexCount);
                // VertexBuffer = new VertexBufferBinding(vertexBuffer, declaration, VertexCount, vertexSize);
            }

            public unsafe void InitializeIndexBuffer(CommandList commandList, int indexCount)
            {
                var mappedIndices = commandList.MapSubresource(IndexBuffer, 0, MapMode.WriteDiscard, false, 0, IndexBufferSize);
                var indexPointer = mappedIndices.DataBox.DataPointer;

                int indexStructSize = sizeof(short);
                int verticesPerQuad = 4;

                var k = 0;
                for (var i = 0; i < indexCount; k += verticesPerQuad)
                {
                    *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 0);
                    *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 1);
                    *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 2);
                    *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 0);
                    *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 2);
                    *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 3);
                }

                commandList.UnmapSubresource(mappedIndices);
            }
        }
    }
}
