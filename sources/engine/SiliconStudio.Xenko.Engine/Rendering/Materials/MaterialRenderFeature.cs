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

        // Material instantiated
        private readonly Dictionary<Material, MaterialInfo> allMaterialInfos = new Dictionary<Material, MaterialInfo>();

        /// <summary>
        /// Custom extra info that we want to store per material.
        /// </summary>
        internal class MaterialInfo
        {
            public Material Material;

            public int LastFrameUsed;

            // Any matching effect
            public ResourceGroupLayout PerMaterialLayout;

            public ParameterCollection ParameterCollection = new ParameterCollection();
            public ParameterCollectionLayout ParameterCollectionLayout;
            public ParameterCollection.Copier ParameterCollectionCopier;

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

                var renderMesh = (RenderMesh)renderObject;

                var material = renderMesh.Material;
                var materialInfo = renderMesh.MaterialInfo;

                // Material use first 16 bits
                var materialHashCode = material != null ? (uint)material.GetHashCode() : 0;
                renderObject.StateSortKey = (renderObject.StateSortKey & 0x0000FFFF) | (materialHashCode << 16);

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
                            materialInfo = new MaterialInfo(material);
                            allMaterialInfos.Add(material, materialInfo);
                        }
                        renderMesh.MaterialInfo = materialInfo;
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
                var materialInfo = renderMesh.MaterialInfo;
                var materialParameters = material.Parameters;

                UpdateMaterial(context, materialInfo, renderNode.RenderEffect, materialParameters);

                var descriptorSetPoolOffset = ((RootEffectRenderFeature)RootRenderFeature).ComputeResourceGroupOffset(renderNodeReference);
                resourceGroupPool[descriptorSetPoolOffset + perMaterialDescriptorSetSlot.Index] = materialInfo.Resources;
            }
        }

        private unsafe void UpdateMaterial(RenderThreadContext context, MaterialInfo materialInfo, RenderEffect renderEffect, ParameterCollection materialParameters)
        {
            // Check if encountered first time this frame
            if (materialInfo.LastFrameUsed == RenderSystem.FrameCounter)
                return;

            // First time we use the material with a valid effect, let's update layouts
            if (materialInfo.PerMaterialLayout == null || materialInfo.PerMaterialLayout.Hash != renderEffect.Reflection.ResourceGroupDescriptions[perMaterialDescriptorSetSlot.Index].Hash)
            {
                var resourceGroupDescription = renderEffect.Reflection.ResourceGroupDescriptions[perMaterialDescriptorSetSlot.Index];
                if (resourceGroupDescription.DescriptorSetLayout == null)
                    return;

                materialInfo.PerMaterialLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, resourceGroupDescription, renderEffect.Effect.Bytecode);

                var parameterCollectionLayout = materialInfo.ParameterCollectionLayout = new ParameterCollectionLayout();
                parameterCollectionLayout.ProcessResources(resourceGroupDescription.DescriptorSetLayout);
                materialInfo.ResourceCount = parameterCollectionLayout.ResourceCount;

                // Process material cbuffer (if any)
                if (resourceGroupDescription.ConstantBufferReflection != null)
                {
                    materialInfo.ConstantBufferReflection = resourceGroupDescription.ConstantBufferReflection;
                    parameterCollectionLayout.ProcessConstantBuffer(resourceGroupDescription.ConstantBufferReflection);
                }

                materialInfo.ParameterCollection.UpdateLayout(parameterCollectionLayout);
                materialInfo.ParameterCollectionCopier = new ParameterCollection.Copier(materialInfo.ParameterCollection, materialParameters);
            }

            // Mark this material as used during this frame
            materialInfo.LastFrameUsed = RenderSystem.FrameCounter;

            // Copy back to ParameterCollection
            // TODO GRAPHICS REFACTOR directly copy to resource group?
            materialInfo.ParameterCollectionCopier.Copy();

            // Allocate resource groups
            context.ResourceGroupAllocator.PrepareResourceGroup(materialInfo.PerMaterialLayout, BufferPoolAllocationType.UsedMultipleTime, materialInfo.Resources);

            // Set resource bindings in PerMaterial resource set
            for (int resourceSlot = 0; resourceSlot < materialInfo.ResourceCount; ++resourceSlot)
            {
                materialInfo.Resources.DescriptorSet.SetValue(resourceSlot, materialInfo.ParameterCollection.ObjectValues[resourceSlot]);
            }

            // Process PerMaterial cbuffer
            if (materialInfo.ConstantBufferReflection != null)
            {
                var mappedCB = materialInfo.Resources.ConstantBuffer.Data;
                fixed (byte* dataValues = materialInfo.ParameterCollection.DataValues)
                    Utilities.CopyMemory(mappedCB, (IntPtr)dataValues, materialInfo.Resources.ConstantBuffer.Size);
            }
        }
    }
}