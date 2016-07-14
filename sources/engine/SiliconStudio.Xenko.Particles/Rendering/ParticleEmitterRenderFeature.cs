// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Materials;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    /// <summary>
    /// Should be identical to the cbuffer PerView in ParticleUtilities.xksl
    /// </summary>
    struct ParticleUtilitiesPerView
    {
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
        public Matrix ViewProjectionMatrix;

        // .x - Width, .y - Height, .z - Near, .w - Far
        public Vector4 ViewFrustum;
    }

    struct RenderAttributesPerNode
    {
        public int VertexBufferOffset;
        public int IndexCount;
        public VertexBufferBinding VertexBuffer;
        public IndexBufferBinding IndexBuffer;
    }

    /// <summary>
    /// Renders <see cref="RenderParticleEmitter"/>.
    /// </summary>
    public class ParticleEmitterRenderFeature : RootEffectRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        private ConstantBufferOffsetReference perViewCBufferOffset;

        private RenderPropertyKey<RenderAttributesPerNode> renderParticleNodeKey;

        // Material alive during this frame
        private readonly Dictionary<ParticleMaterial, ParticleMaterialInfo> allMaterialInfos = new Dictionary<ParticleMaterial, ParticleMaterialInfo>();

        public override Type SupportedRenderObjectType => typeof(RenderParticleEmitter);

        private DescriptorSet[] descriptorSets;

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

            renderParticleNodeKey = RenderData.CreateRenderKey<RenderAttributesPerNode>();

            renderEffectKey = RenderEffectKey;

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
            foreach (var renderObject in RenderObjects)
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;

                renderParticleEmitter.ParticleEmitter.PrepareForDraw(out renderParticleEmitter.HasVertexBufferChanged, 
                    out renderParticleEmitter.VertexSize, out renderParticleEmitter.VertexCount);

                // Handle vertex element changes
                if (renderParticleEmitter.HasVertexBufferChanged)
                {
                    // BUG - there should be 1 buffer per RenderNode, not per RenderObject
                    // Create new buffers
                    renderParticleEmitter.ParticleEmitter.VertexBuilder.RecreateBuffers(RenderSystem.GraphicsDevice);
                }

                // TODO: ParticleMaterial should set this up
                var materialInfo = (ParticleMaterialInfo)renderParticleEmitter.ParticleMaterialInfo;
                materialInfo?.Material.Parameters.Set(ParticleBaseKeys.ColorScale, renderParticleEmitter.RenderParticleSystem.ParticleSystemComponent.Color);
            }

            // Calculate the total vertex buffer size required
            int totalVertexBufferSize = 0;
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
                var vertexBuilder = renderParticleEmitter.ParticleEmitter.VertexBuilder; // TODO Change this to a global vertex buffer
                renderParticleNodeData[new RenderNodeReference(renderNodeIndex)] = new RenderAttributesPerNode
                {
                    VertexBufferOffset = totalVertexBufferSize,
                    IndexCount = vertexBuilder.LivingQuads * vertexBuilder.IndicesPerQuad,
                    VertexBuffer = vertexBuilder.ResourceContext.VertexBuffer,
                    IndexBuffer = vertexBuilder.ResourceContext.IndexBuffer,
                };

                totalVertexBufferSize += (renderParticleEmitter.VertexSize * renderParticleEmitter.VertexCount);
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



            // TODO Create/reassign vertex buffer based on totalVertexBufferSize
            // BUG - there should be 1 buffer per RenderNode, not per RenderObject
            foreach (var renderObject in RenderObjects)
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;

                if (renderParticleEmitter.HasVertexBufferChanged)
                {
                    renderParticleEmitter.ParticleEmitter.VertexBuilder.RecreateBuffers(RenderSystem.GraphicsDevice);
                }
            }

            // Build particle buffers
            var commandList = context.CommandList;
            // TODO Map buffers here and build the particle data

            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RenderNodes[renderNodeIndex];
                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                var nodeData = renderParticleNodeData[renderNodeReference];

                if (nodeData.IndexCount <= 0)
                    continue;   // Nothing to draw, nothing to build

                Matrix viewInverse;
                Matrix.Invert(ref renderNode.RenderView.View, out viewInverse); // TODO Build this per view, not per node!!!
                renderParticleEmitter.ParticleEmitter.BuildVertexBuffer(commandList, ref viewInverse);
            }

            // TODO Unmap buffers here

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

            var renderParticleNodeData = RenderData.GetData(renderParticleNodeKey);

            Array.Resize(ref descriptorSets, EffectDescriptorSetSlotCount);

            // Draw vertex buffers
            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;

                // Get the effect
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

                // Bind the buffers
                var vertexBuffer = nodeData.VertexBuffer;
                var indexBuffer = nodeData.IndexBuffer;
                commandList.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Stride);  // TODO Offset and stride should go to the nodeData
                commandList.SetIndexBuffer(indexBuffer.Buffer, indexBuffer.Offset, indexBuffer.Is32Bit);        // TODO Offset and stride should go to the nodeData

                var indexBufferPosition = 0;
                commandList.DrawIndexed(nodeData.IndexCount, indexBufferPosition);
            }
        }
    }
}
