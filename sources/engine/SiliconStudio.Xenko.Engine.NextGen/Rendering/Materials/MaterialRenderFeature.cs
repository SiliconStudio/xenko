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
        private StaticEffectObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        // Material alive during this frame
        private HashSet<MaterialInfo> allMaterialInfos = new HashSet<MaterialInfo>();
        private List<MaterialInfo> activeMaterialInfos = new List<MaterialInfo>();

        /// <summary>
        /// Custom extra info that we want to store per material.
        /// </summary>
        internal class MaterialInfo
        {
            public Material Material;

            public int LastFrameUsed;

            // Any matching effect
            public ResourceGroupLayout PerMaterialLayout;

            // PerMaterial
            public ResourceGroup Resources;
            public int ResourceCount;

            public Buffer ConstantBuffer;
            public ShaderConstantBufferDescription ConstantBufferReflection;

            public ResourceParameter<ShaderSource> PixelStageSurfaceShaders;
            public ResourceParameter<ShaderSource> PixelStageStreamInitializer;
            public ResourceParameter<ShaderSource> PixelStageSurfaceFilter;
            public RenderEffect RenderEffect;

            public MaterialInfo(Material material)
            {
                Material = material;
            }
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            renderEffectKey = ((RootEffectRenderFeature)RootRenderFeature).RenderEffectKey;

            perMaterialDescriptorSetSlot = ((RootEffectRenderFeature)RootRenderFeature).GetOrCreateEffectDescriptorSetSlot("PerMaterial");
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutations(NextGenRenderSystem RenderSystem)
        {
            var renderEffects = RootRenderFeature.GetData(renderEffectKey);
            int effectSlotCount = ((RootEffectRenderFeature)RootRenderFeature).EffectPermutationSlotCount;

            // Collect materials
            activeMaterialInfos.Clear();

            foreach (var renderObject in RootRenderFeature.RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < effectSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode.CreateEffectReference(effectSlotCount, i);
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

                        materialInfo.PixelStageSurfaceShaders = material.Parameters.GetResourceParameter(MaterialKeys.PixelStageSurfaceShaders);
                        materialInfo.PixelStageStreamInitializer = material.Parameters.GetResourceParameter(MaterialKeys.PixelStageStreamInitializer);
                        materialInfo.PixelStageSurfaceFilter = material.Parameters.GetResourceParameter(MaterialKeys.PixelStageSurfaceFilter);

                        materialInfo.RenderEffect = renderEffect;
                    }

                    if (materialInfo.LastFrameUsed != RenderSystem.FrameCounter)
                    {
                        // Add this material to a list of material used during this frame
                        materialInfo.LastFrameUsed = RenderSystem.FrameCounter;
                        activeMaterialInfos.Add(materialInfo);
                    }

                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceShaders, material.Parameters.Get(materialInfo.PixelStageSurfaceShaders));
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageStreamInitializer, material.Parameters.Get(materialInfo.PixelStageStreamInitializer));
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceFilter, material.Parameters.Get(materialInfo.PixelStageSurfaceFilter));
                }
            }
        }

        /// <inheritdoc/>
        public override void Prepare()
        {
            foreach (var materialInfo in activeMaterialInfos)
            {
                var material = materialInfo.Material;

                // First time we use the material, let's update layouts
                if (materialInfo.PerMaterialLayout == null)
                {
                    var renderEffect = materialInfo.RenderEffect;
                    var descriptorLayout = renderEffect.Reflection.Binder.DescriptorReflection.GetLayout("PerMaterial");

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
                        materialInfo.Resources.ConstantBufferSize = parameterCollectionLayout.BufferSize;
                    }

                    // Update material parameters layout to what is expected by effect
                    material.Parameters.UpdateLayout(parameterCollectionLayout);

                    materialInfo.PerMaterialLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, renderEffect.Reflection.Binder.DescriptorReflection.GetLayout("PerMaterial"), renderEffect.Effect.Bytecode, "PerMaterial");
                }

                var materialDescriptorSet = DescriptorSet.New(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, materialInfo.PerMaterialLayout.DescriptorSetLayout);
                materialInfo.Resources.DescriptorSet = materialDescriptorSet;

                // Set resource bindings in PerMaterial resource set
                for (int resourceSlot = 0; resourceSlot < materialInfo.ResourceCount; ++resourceSlot)
                {
                    materialDescriptorSet.SetValue(resourceSlot, material.Parameters.ResourceValues[resourceSlot]);
                }

                // Process PerMaterial cbuffer
                if (materialInfo.ConstantBufferReflection != null)
                {
                    var materialConstantBufferOffset = RenderSystem.BufferPool.Allocate(materialInfo.Resources.ConstantBufferSize);

                    // Set constant buffer
                    materialDescriptorSet.SetConstantBuffer(0, RenderSystem.BufferPool.Buffer, materialConstantBufferOffset, materialInfo.Resources.ConstantBufferSize);
                    materialInfo.Resources.ConstantBufferOffset = materialConstantBufferOffset;

                    var mappedCB = RenderSystem.BufferPool.Buffer.Data + materialInfo.Resources.ConstantBufferOffset;
                    Utilities.CopyMemory(mappedCB, material.Parameters.DataValues, materialInfo.Resources.ConstantBufferSize);
                }
            }

            // Assign descriptor sets to each render node
            var descriptorSetPool = ((RootEffectRenderFeature)RootRenderFeature).DescriptorSetPool;
            for (int renderNodeIndex = 0; renderNodeIndex < RootRenderFeature.renderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RootRenderFeature.renderNodes[renderNodeIndex];
                var renderMesh = (RenderMesh)renderNode.RenderObject;

                // Collect materials and create associated MaterialInfo (includes reflection) first time
                // TODO: We assume same material will generate same ResourceGroup (i.e. same resources declared in same order)
                // Need to offer some protection if this invariant is violated (or support it if it can actually happen in real scenario)
                var material = renderMesh.Material.Material;
                var materialInfo = (MaterialInfo)material.RenderData;

                var descriptorSetPoolOffset = ((RootEffectRenderFeature)RootRenderFeature).ComputeDescriptorSetOffset(renderNodeReference);
                descriptorSetPool[descriptorSetPoolOffset + perMaterialDescriptorSetSlot.Index] = materialInfo.Resources.DescriptorSet;
            }
        }
    }
}