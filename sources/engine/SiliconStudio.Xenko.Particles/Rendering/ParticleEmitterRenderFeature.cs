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

    /// <summary>
    /// Renders <see cref="RenderParticleEmitter"/>.
    /// </summary>
    public class ParticleEmitterRenderFeature : RootEffectRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        private ConstantBufferOffsetReference perViewCBufferOffset;

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
            // Reset pipeline states if necessary
            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNode = RenderNodes[renderNodeIndex];
                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                if (renderParticleEmitter.ParticleEmitter.VertexBuilder.IsBufferDirty)
                {
                    // Reset pipeline state, so input layout is regenerated
                    if (renderNode.RenderEffect != null)
                        renderNode.RenderEffect.PipelineState = null;
                }
            }

            foreach (var renderObject in RenderObjects)
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;
                renderParticleEmitter.ParticleEmitter.PrepareForDraw();

                var materialInfo = (ParticleMaterialInfo)renderParticleEmitter.ParticleMaterialInfo;

                // Handle vertex element changes
                if (renderParticleEmitter.ParticleEmitter.VertexBuilder.IsBufferDirty)
                {
                    // Create new buffers
                    renderParticleEmitter.ParticleEmitter.VertexBuilder.RecreateBuffers(RenderSystem.GraphicsDevice);
                }

                // TODO: ParticleMaterial should set this up
                materialInfo?.Material.Parameters.Set(ParticleBaseKeys.ColorScale, renderParticleEmitter.RenderParticleSystem.ParticleSystemComponent.Color);
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

            Array.Resize(ref descriptorSets, EffectDescriptorSetSlotCount);

            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;
                if (renderParticleEmitter.ParticleEmitter.VertexBuilder.ResourceContext == null)
                    continue;

                // Generate vertices
                // TODO: Just just unmap/barrier here
                renderParticleEmitter.ParticleEmitter.BuildVertexBuffer(context.CommandList, ref viewInverse);

                // Get effect
                var renderEffect = renderNode.RenderEffect;
                if (renderEffect.Effect == null)
                    continue;

                // TODO GRAPHICS REFACTOR: Extract data
                var particleSystemComponent = renderParticleEmitter.RenderParticleSystem.ParticleSystemComponent;
                var particleSystem = particleSystemComponent.ParticleSystem;
                var vertexBuilder = renderParticleEmitter.ParticleEmitter.VertexBuilder;

                // Bind VB
                var vertexBuffer = vertexBuilder.ResourceContext.VertexBuffer;
                var indexBuffer = vertexBuilder.ResourceContext.IndexBuffer;
                commandList.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Stride);
                commandList.SetIndexBuffer(indexBuffer.Buffer, indexBuffer.Offset, indexBuffer.Is32Bit);

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

                commandList.DrawIndexed(vertexBuilder.LivingQuads * vertexBuilder.IndicesPerQuad, vertexBuilder.ResourceContext.IndexBufferPosition);
            }
        }
    }
}
