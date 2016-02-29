// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.Materials;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Particles.Rendering
{
    public class ParticleEmitterRenderFeature : RootEffectRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        private ConstantBufferOffsetReference view;

        // Material alive during this frame
        private readonly HashSet<ParticleMaterialInfo> allMaterialInfos = new HashSet<ParticleMaterialInfo>();
        private readonly List<ParticleMaterialInfo> activeMaterialInfos = new List<ParticleMaterialInfo>();

        public override Type SupportedRenderObjectType => typeof(RenderParticleEmitter);

        private class ParticleMaterialInfo
        {
            public readonly ParticleMaterial Material;

            public int LastFrameUsed;

            // Any matching effect
            public ResourceGroupLayout PerMaterialLayout;

            // PerMaterial
            public ResourceGroup Resources;
            public int ResourceCount;
            public ShaderConstantBufferDescription ConstantBufferReflection;

            public RenderEffect RenderEffect;

            public ParticleMaterialInfo(ParticleMaterial material)
            {
                Material = material;
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            renderEffectKey = RenderEffectKey;

            view = CreateViewCBufferOffsetSlot(ParticleBaseKeys.MatrixTransform.Name);

            perMaterialDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerMaterial");
        }

        public override void Extract()
        {
            base.Extract();
        }

        public unsafe override void Prepare(RenderThreadContext context)
        {
            foreach (var renderObject in RenderObjects)
            {
                var renderParticleEmitter = (RenderParticleEmitter)renderObject;
                renderParticleEmitter.ParticleEmitter.PrepareForDraw();

                var materialInfo = (ParticleMaterialInfo)renderParticleEmitter.ParticleEmitter.Material.RenderData;

                // Handle vertex element changes
                if (renderParticleEmitter.ParticleEmitter.VertexBuilder.IsBufferDirty)
                {
                    // Create new buffers
                    renderParticleEmitter.ParticleEmitter.VertexBuilder.RecreateBuffers(RenderSystem.GraphicsDevice);

                    // Reset pipeline state, so input layout is regenerated
                    
                    materialInfo.RenderEffect.PipelineState = null;
                }

                // TODO: ParticleMaterial should set this up
                materialInfo.Material.Parameters.Set(ParticleBaseKeys.ColorScale, renderParticleEmitter.RenderParticleSystem.ParticleSystemComponent.Color);
            }

            base.Prepare(context);

            foreach (var materialInfo in activeMaterialInfos)
            {
                var material = materialInfo.Material;

                material.Setup(RenderSystem.RenderContextOld);

                // First time we use the material, let's update layouts
                if (materialInfo.PerMaterialLayout == null)
                {
                    var renderEffect = materialInfo.RenderEffect;

                    var descriptorLayout = renderEffect.Reflection.DescriptorReflection.GetLayout("PerMaterial");

                    var parameterCollectionLayout = new NextGenParameterCollectionLayout();
                    parameterCollectionLayout.ProcessResources(descriptorLayout);
                    materialInfo.ResourceCount = parameterCollectionLayout.ResourceCount;

                    // Find material cbuffer
                    var materialConstantBuffer = renderEffect.Effect.Bytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerMaterial");

                    // Process cbuffer (if any)
                    if (materialConstantBuffer != null)
                    {
                        materialInfo.ConstantBufferReflection = materialConstantBuffer;
                        parameterCollectionLayout.ProcessConstantBuffer(materialConstantBuffer);
                    }

                    // Update material parameters layout to what is expected by effect
                    material.Parameters.UpdateLayout(parameterCollectionLayout);

                    materialInfo.PerMaterialLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, descriptorLayout, renderEffect.Effect.Bytecode, "PerMaterial");

                    materialInfo.Resources = new ResourceGroup();
                }

                context.ResourceGroupAllocator.PrepareResourceGroup(materialInfo.PerMaterialLayout, BufferPoolAllocationType.UsedMultipleTime, materialInfo.Resources);

                // Set resource bindings in PerMaterial resource set
                for (int resourceSlot = 0; resourceSlot < materialInfo.ResourceCount; ++resourceSlot)
                {
                    materialInfo.Resources.DescriptorSet.SetValue(resourceSlot, material.Parameters.ObjectValues[resourceSlot]);
                }

                // Process PerMaterial cbuffer
                if (materialInfo.ConstantBufferReflection != null)
                {
                    var mappedCB = materialInfo.Resources.ConstantBuffer.Data;
                    fixed (byte* dataValues = material.Parameters.DataValues)
                        Utilities.CopyMemory(mappedCB, (IntPtr)dataValues, materialInfo.Resources.ConstantBuffer.Size);
                }
            }

            // Assign descriptor sets to each render node
            var resourceGroupPool = ResourceGroupPool;
            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RenderNodes[renderNodeIndex];
                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                // Collect materials and create associated MaterialInfo (includes reflection) first time
                // TODO: We assume same material will generate same ResourceGroup (i.e. same resources declared in same order)
                // Need to offer some protection if this invariant is violated (or support it if it can actually happen in real scenario)
                var material = renderParticleEmitter.ParticleEmitter.Material;
                var materialInfo = (ParticleMaterialInfo)material.RenderData;

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
                    var viewProjectionOffset = viewLayout.GetConstantBufferOffset(this.view);
                    if (viewProjectionOffset == -1)
                        continue;

                    var resourceGroup = viewLayout.Entries[view.Index].Resources;
                    var mappedCB = resourceGroup.ConstantBuffer.Data;

                    var perView = (Matrix*)((byte*)mappedCB + viewProjectionOffset);
                    *perView = view.ViewProjection;
                }
            }
        }

        public override void PrepareEffectPermutationsImpl()
        {
            base.PrepareEffectPermutationsImpl();

            var renderEffects = RenderData.GetData(renderEffectKey);
            int effectSlotCount = EffectPermutationSlotCount;

            // Collect materials
            activeMaterialInfos.Clear();

            foreach (var renderObject in RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];
                    var renderParticleEmitter = (RenderParticleEmitter)renderObject;

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    var material = renderParticleEmitter.ParticleEmitter.Material;
                    var materialInfo = (ParticleMaterialInfo)material.RenderData;
                    if (materialInfo == null)
                    {
                        // First time this material is initialized, let's create associated info
                        materialInfo = new ParticleMaterialInfo(material);
                        material.RenderData = materialInfo;
                        allMaterialInfos.Add(materialInfo);

                        materialInfo.RenderEffect = renderEffect;
                    }

                    if (materialInfo.LastFrameUsed != RenderSystem.FrameCounter)
                    {
                        // Add this material to a list of material used during this frame
                        materialInfo.LastFrameUsed = RenderSystem.FrameCounter;
                        activeMaterialInfos.Add(materialInfo);
                    }

                    // TODO: Iterate PermuatationParameters automatically?
                    material.ValidateEffect(RenderSystem.RenderContextOld, ref renderEffect.EffectValidator);
                }
            }
        }

        protected override void InvalidateEffectPermutation(RenderObject renderObject, RenderEffect renderEffect)
        {
            var renderParticleEmitter = (RenderParticleEmitter)renderObject;
            var materialInfo = (ParticleMaterialInfo)renderParticleEmitter.ParticleEmitter.Material.RenderData;
            materialInfo.PerMaterialLayout = null;
        }

        protected override void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            var renderParticleEmitter = (RenderParticleEmitter)renderObject;

            pipelineState.InputElements = renderParticleEmitter.ParticleEmitter.VertexBuilder.VertexDeclaration.CreateInputElements();
            pipelineState.PrimitiveType = PrimitiveType.TriangleList;
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var commandList = context.CommandList;

            // TODO: PerView data
            Matrix viewInverse;
            Matrix.Invert(ref renderView.View, out viewInverse);

            var descriptorSets = new DescriptorSet[EffectDescriptorSetSlotCount];

            for (var index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderParticleEmitter = (RenderParticleEmitter)renderNode.RenderObject;

                // Generate vertices
                // TODO: Just just unmap/barrier here
                renderParticleEmitter.ParticleEmitter.BuildVertexBuffer(context.CommandList, ref viewInverse);

                // Get effect
                var renderEffect = renderNode.RenderEffect;

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