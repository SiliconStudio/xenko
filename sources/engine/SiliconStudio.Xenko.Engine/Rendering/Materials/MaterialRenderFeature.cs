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

            public PermutationParameter<ShaderSource> PixelStageSurfaceShaders;
            public PermutationParameter<ShaderSource> PixelStageStreamInitializer;
            public PermutationParameter<ShaderSource> PixelStageSurfaceFilter;

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

                    var material = renderMesh.Material.Material;
                    var materialInfo = (MaterialInfo)material.RenderData;
                    if (materialInfo == null)
                    {
                        // First time this material is initialized, let's create associated info
                        materialInfo = new MaterialInfo(material);
                        material.RenderData = materialInfo;
                        allMaterialInfos.Add(materialInfo);

                        materialInfo.PixelStageSurfaceShaders = material.Parameters.GetAccessor(MaterialKeys.PixelStageSurfaceShaders);
                        materialInfo.PixelStageStreamInitializer = material.Parameters.GetAccessor(MaterialKeys.PixelStageStreamInitializer);
                        materialInfo.PixelStageSurfaceFilter = material.Parameters.GetAccessor(MaterialKeys.PixelStageSurfaceFilter);
                    }

                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceShaders, material.Parameters.Get(materialInfo.PixelStageSurfaceShaders));
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageStreamInitializer, material.Parameters.Get(materialInfo.PixelStageStreamInitializer));
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceFilter, material.Parameters.Get(materialInfo.PixelStageSurfaceFilter));
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
                var material = renderMesh.Material.Material;
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