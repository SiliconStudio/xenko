using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A render feature that will bind materials.
    /// </summary>
    public class MaterialRenderFeature : SubRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        // Material alive during this frame
        private readonly HashSet<MaterialInfo> allMaterialInfos = new HashSet<MaterialInfo>();

        /// <summary>
        /// Custom extra info that we want to store per material.
        /// </summary>
        internal class MaterialInfo
        {
            public Material Material;

            public int LastFrameUsed;
            public NextGenRenderSystem LastRenderSystemUsed;

            // Any matching effect
            public ResourceGroupLayout PerMaterialLayout;

            // PerMaterial
            public ResourceGroup Resources = new ResourceGroup();
            public int ResourceCount;
            public ShaderConstantBufferDescription ConstantBufferReflection;

            // Permutation parameters
            public int PermutationCounter; // Dirty counter against material.Parameters.PermutationCounter

            public ShaderSource VertexStageSurfaceShaders;
            public ShaderSource VertexStageStreamInitializer;

            public ShaderSource DomainStageSurfaceShaders;
            public ShaderSource DomainStageStreamInitializer;

            public ShaderSource TessellationShader;

            public ShaderSource PixelStageSurfaceShaders;
            public ShaderSource PixelStageStreamInitializer;

            public MaterialInfo(Material material)
            {
                Material = material;
            }
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;

            perMaterialDescriptorSetSlot = ((RootEffectRenderFeature)RootRenderFeature).GetOrCreateEffectDescriptorSetSlot("PerMaterial");
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutations()
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];
                    var renderMesh = (RenderMesh)renderObject;

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    var material = renderMesh.Material;
                    var materialInfo = (MaterialInfo)material.RenderData;
                    if (materialInfo == null)
                    {
                        // First time this material is initialized, let's create associated info
                        materialInfo = new MaterialInfo(material);
                        material.RenderData = materialInfo;
                        allMaterialInfos.Add(materialInfo);
                    }

                    if (materialInfo.PermutationCounter != material.Parameters.PermutationCounter)
                    {
                        materialInfo.VertexStageSurfaceShaders = material.Parameters.Get(MaterialKeys.VertexStageSurfaceShaders);
                        materialInfo.VertexStageStreamInitializer = material.Parameters.Get(MaterialKeys.VertexStageStreamInitializer);

                        materialInfo.DomainStageSurfaceShaders = material.Parameters.Get(MaterialKeys.DomainStageSurfaceShaders);
                        materialInfo.DomainStageStreamInitializer = material.Parameters.Get(MaterialKeys.DomainStageStreamInitializer);

                        materialInfo.TessellationShader = material.Parameters.Get(MaterialKeys.TessellationShader);

                        materialInfo.PixelStageSurfaceShaders = material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders);
                        materialInfo.PixelStageStreamInitializer = material.Parameters.Get(MaterialKeys.PixelStageStreamInitializer);

                        materialInfo.PermutationCounter = material.Parameters.PermutationCounter;
                    }

                    // VS
                    if (materialInfo.VertexStageSurfaceShaders != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.VertexStageSurfaceShaders, materialInfo.VertexStageSurfaceShaders);
                    if (materialInfo.VertexStageStreamInitializer != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.VertexStageStreamInitializer, materialInfo.VertexStageStreamInitializer);

                    // DS
                    if (materialInfo.DomainStageSurfaceShaders != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.DomainStageSurfaceShaders, materialInfo.DomainStageSurfaceShaders);
                    if (materialInfo.DomainStageStreamInitializer != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.DomainStageStreamInitializer, materialInfo.DomainStageStreamInitializer);

                    // Tessellation
                    if (materialInfo.TessellationShader != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.TessellationShader, materialInfo.TessellationShader);

                    // PS
                    if (materialInfo.PixelStageSurfaceShaders != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceShaders, materialInfo.PixelStageSurfaceShaders);
                    if (materialInfo.PixelStageStreamInitializer != null)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageStreamInitializer, materialInfo.PixelStageStreamInitializer);
                }
            }
        }

        /// <inheritdoc/>
        public override void Prepare(RenderThreadContext context)
        {
            // Assign descriptor sets to each render node
            var resourceGroupPool = ((RootEffectRenderFeature)RootRenderFeature).ResourceGroupPool;
            for (int renderNodeIndex = 0; renderNodeIndex < RootRenderFeature.RenderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RootRenderFeature.RenderNodes[renderNodeIndex];
                var renderMesh = (RenderMesh)renderNode.RenderObject;

                // Ignore fallback effects
                if (renderNode.RenderEffect.State != RenderEffectState.Normal)
                    continue;

                // Collect materials and create associated MaterialInfo (includes reflection) first time
                // TODO: We assume same material will generate same ResourceGroup (i.e. same resources declared in same order)
                // Need to offer some protection if this invariant is violated (or support it if it can actually happen in real scenario)
                var material = renderMesh.Material;
                var materialInfo = (MaterialInfo)material.RenderData;
                var materialParameters = material.Parameters;

                UpdateMaterial(context, materialInfo, renderNode.RenderEffect, materialParameters);

                var descriptorSetPoolOffset = ((RootEffectRenderFeature)RootRenderFeature).ComputeResourceGroupOffset(renderNodeReference);
                resourceGroupPool[descriptorSetPoolOffset + perMaterialDescriptorSetSlot.Index] = materialInfo.Resources;
            }
        }

        private unsafe void UpdateMaterial(RenderThreadContext context, MaterialInfo materialInfo, RenderEffect renderEffect, NextGenParameterCollection materialParameters)
        {
            // Check if encountered first time this frame
            if (materialInfo.LastFrameUsed == RenderSystem.FrameCounter
                && materialInfo.LastRenderSystemUsed == RenderSystem)
                return;

            // Mark this material as used during this frame
            materialInfo.LastRenderSystemUsed = RenderSystem;
            materialInfo.LastFrameUsed = RenderSystem.FrameCounter;

            // First time we use the material with a valid effect, let's update layouts
            var descriptorLayout = renderEffect.Reflection.DescriptorReflection.Layouts[perMaterialDescriptorSetSlot.Index].Layout;
            if (materialInfo.PerMaterialLayout == null || materialInfo.PerMaterialLayout.Hash != descriptorLayout.Hash)
            {
                materialInfo.PerMaterialLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, descriptorLayout, renderEffect.Effect.Bytecode, "PerMaterial");

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
                materialParameters.UpdateLayout(parameterCollectionLayout);
            }

            // Allocate resource groups
            context.ResourceGroupAllocator.PrepareResourceGroup(materialInfo.PerMaterialLayout, BufferPoolAllocationType.UsedMultipleTime, materialInfo.Resources);

            // Set resource bindings in PerMaterial resource set
            for (int resourceSlot = 0; resourceSlot < materialInfo.ResourceCount; ++resourceSlot)
            {
                materialInfo.Resources.DescriptorSet.SetValue(resourceSlot, materialParameters.ObjectValues[resourceSlot]);
            }

            // Process PerMaterial cbuffer
            if (materialInfo.ConstantBufferReflection != null)
            {
                var mappedCB = materialInfo.Resources.ConstantBuffer.Data;
                fixed (byte* dataValues = materialParameters.DataValues)
                    Utilities.CopyMemory(mappedCB, (IntPtr)dataValues, materialInfo.Resources.ConstantBuffer.Size);
            }
        }
    }
}