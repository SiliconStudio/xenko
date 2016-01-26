using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace RenderArchitecture
{
    /// <summary>
    /// A render feature that will bind materials.
    /// </summary>
    public class MaterialRenderFeature : SubRenderFeature
    {
        private StaticEffectObjectPropertyKey<RenderEffect> renderEffectKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        // TODO: Temporarily internal static until we support automatic custom DescriptorSet
        internal static Dictionary<Material, MaterialInfo> materials = new Dictionary<Material, MaterialInfo>();

        /// <summary>
        /// Custom extra info that we want to store per material.
        /// </summary>
        internal class MaterialInfo
        {
            // Any matching effect
            public ResourceGroupLayout PerMaterialLayout;

            // PerMaterial
            public ResourceGroup Resources;

            public Buffer ConstantBuffer;
            public ShaderConstantBufferDescription ConstantBufferReflection;

            public KeyValuePair<int, ParameterKey>[] ResourceBindingKeys;

            public MaterialInfo()
            {
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
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceShaders, material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders));
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageStreamInitializer, material.Parameters.Get(MaterialKeys.PixelStageStreamInitializer));
                    renderEffect.EffectValidator.ValidateParameter(MaterialKeys.PixelStageSurfaceFilter, material.Parameters.Get(MaterialKeys.PixelStageSurfaceFilter));
                }
            }
        }

        /// <inheritdoc/>
        public override void Prepare()
        {
            // Collect materials
            materials.Clear();
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
                MaterialInfo materialInfo;
                if (!materials.TryGetValue(material, out materialInfo))
                {
                    materialInfo = new MaterialInfo();

                    // Get effect
                    // TODO: Use real effect slot
                    var renderEffect = renderNode.RenderEffect;

                    // Find material cbuffer
                    var materialConstantBuffer = renderEffect.Effect.Bytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerMaterial");

                    // Process cbuffer (if any)
                    if (materialConstantBuffer != null)
                    {
                        materialInfo.ConstantBufferReflection = materialConstantBuffer;
                        materialInfo.Resources.ConstantBufferSize = materialConstantBuffer.Size;
                    }

                    // Find resources
                    // TODO: Scanning everything, but not good since:
                    //  - We might miss stuff not present existing in shader but not in Material yet
                    //  - Quite slow (but well, should be rare enough for now
                    //  - Need to define a scope for resources in shader to easily detect material-related ones

                    var descriptorLayout = renderEffect.Reflection.Binder.DescriptorReflection.GetLayout("PerMaterial");
                    var resourceBindingSlots = new List<KeyValuePair<int, ParameterKey>>();
                    foreach (var materialParameter in material.Parameters)
                    {
                        // Check if material parameter actually exist in shader
                        for (int index = 0; index < descriptorLayout.Entries.Count; index++)
                        {
                            var layoutEntry = descriptorLayout.Entries[index];
                            if (layoutEntry.Name == materialParameter.Key.Name)
                            {
                                resourceBindingSlots.Add(new KeyValuePair<int, ParameterKey>(index, materialParameter.Key));
                                break;
                            }
                        }
                    }

                    materialInfo.ResourceBindingKeys = resourceBindingSlots.ToArray();
                    materialInfo.PerMaterialLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, renderEffect.Reflection.Binder.DescriptorReflection.GetLayout("PerMaterial"), renderEffect.Effect.Bytecode, "PerMaterial");

                    var materialDescriptorSet = DescriptorSet.New(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, materialInfo.PerMaterialLayout.DescriptorSetLayout);
                    materialInfo.Resources.DescriptorSet = materialDescriptorSet;

                    materials.Add(material, materialInfo);
                }

                var descriptorSetPoolOffset = ((RootEffectRenderFeature)RootRenderFeature).ComputeDescriptorSetOffset(renderNodeReference);
                descriptorSetPool[descriptorSetPoolOffset + perMaterialDescriptorSetSlot.Index] = materialInfo.Resources.DescriptorSet;
            }

            // Process each material
            foreach (var material in materials)
            {
                var materialDescriptorSet = material.Value.Resources.DescriptorSet;

                // Set resource bindings in PerMaterial resource set
                for (int index = 0; index < material.Value.ResourceBindingKeys.Length; index++)
                {
                    var resourceBindingKey = material.Value.ResourceBindingKeys[index];
                    var resourceValue = material.Key.Parameters.GetObject(resourceBindingKey.Value);
                    materialDescriptorSet.SetValue(resourceBindingKey.Key, resourceValue);
                }

                // Process PerMaterial cbuffer
                if (material.Value.ConstantBufferReflection != null)
                {
                    var materialConstantBufferOffset = RenderSystem.BufferPool.Allocate(material.Value.Resources.ConstantBufferSize);

                    // Set constant buffer
                    materialDescriptorSet.SetConstantBuffer(0, RenderSystem.BufferPool.Buffer, materialConstantBufferOffset, material.Value.Resources.ConstantBufferSize);
                    material.Value.Resources.ConstantBufferOffset = materialConstantBufferOffset;

                    var mappedCB = RenderSystem.BufferPool.Buffer.Data + material.Value.Resources.ConstantBufferOffset;

                    // Iterate over cbuffer members to update and pull them from material Parameters
                    // TODO: we should cache reflection offsets, but currently waiting for Material to have a more efficient internal structure
                    //        without ParameterCollection so that it is just a few simple copies without ParamterKey lookup
                    foreach (var constantBufferMember in material.Value.ConstantBufferReflection.Members)
                    {
                        var internalValue = material.Key.Parameters.GetInternalValue(constantBufferMember.Param.Key);
                        if (internalValue != null)
                        {
                            internalValue.ReadFrom(mappedCB + constantBufferMember.Offset, 0, constantBufferMember.Size);
                        }
                    }
                }
            }
        }
    }
}