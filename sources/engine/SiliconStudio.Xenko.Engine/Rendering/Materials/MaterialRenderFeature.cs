// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Handles material by permuting shaders and uploading material data.
    /// </summary>
    public class MaterialRenderFeature : SubRenderFeature
    {
        private StaticObjectPropertyKey<RenderEffect> renderEffectKey;
        private StaticObjectPropertyKey<TessellationState> tessellationStateKey;

        private EffectDescriptorSetReference perMaterialDescriptorSetSlot;

        private List<RenderMesh> renderMeshesToGenerateAEN = new List<RenderMesh>();

        // Material instantiated
        private readonly Dictionary<Material, MaterialInfo> allMaterialInfos = new Dictionary<Material, MaterialInfo>();

        public class MaterialInfoBase
        {
            public int LastFrameUsed;

            // Any matching effect
            public ResourceGroupLayout PerMaterialLayout;

            /// <summary>
            /// <c>true</c> if MaterialParameters instance was changed
            /// </summary>
            public bool ParametersChanged;

            public ParameterCollection ParameterCollection = new ParameterCollection();
            public ParameterCollectionLayout ParameterCollectionLayout;
            public ParameterCollection.Copier ParameterCollectionCopier;

            // PerMaterial
            public ResourceGroup Resources = new ResourceGroup();
            public int ResourceCount;
            public EffectConstantBufferDescription ConstantBufferReflection;
        }

        /// <summary>
        /// Custom extra info that we want to store per material.
        /// </summary>
        internal class MaterialInfo : MaterialInfoBase
        {
            public Material Material;

            // Permutation parameters
            public int PermutationCounter; // Dirty counter against material.Parameters.PermutationCounter
            public ParameterCollection MaterialParameters; // Protect against changes of Material.Parameters instance (happens with editor fast reload)
            public CullMode? CullMode;

            public ShaderSource VertexStageSurfaceShaders;
            public ShaderSource VertexStageStreamInitializer;

            public ShaderSource DomainStageSurfaceShaders;
            public ShaderSource DomainStageStreamInitializer;

            public ShaderSource TessellationShader;

            public ShaderSource PixelStageSurfaceShaders;
            public ShaderSource PixelStageStreamInitializer;

            public bool HasNormalMap;

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
            tessellationStateKey = RootRenderFeature.RenderData.CreateStaticObjectKey<TessellationState>();

            perMaterialDescriptorSetSlot = ((RootEffectRenderFeature)RootRenderFeature).GetOrCreateEffectDescriptorSetSlot("PerMaterial");
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            var renderEffects = RootRenderFeature.RenderData.GetData(renderEffectKey);
            var tessellationStates = RootRenderFeature.RenderData.GetData(tessellationStateKey);
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

                var tessellationState = tessellationStates[staticObjectNode];

                // Update draw data if tessellation is active
                if (material.TessellationMethod != XenkoTessellationMethod.None)
                {
                    var tessellationMeshDraw = tessellationState.MeshDraw;

                    if (tessellationState.Method != material.TessellationMethod)
                    {
                        tessellationState.Method = material.TessellationMethod;

                        var oldMeshDraw = renderMesh.ActiveMeshDraw;
                        tessellationMeshDraw = new MeshDraw
                        {
                            VertexBuffers = oldMeshDraw.VertexBuffers,
                            IndexBuffer = oldMeshDraw.IndexBuffer,
                            DrawCount = oldMeshDraw.DrawCount,
                            StartLocation = oldMeshDraw.StartLocation,
                            PrimitiveType = tessellationState.Method.GetPrimitiveType(),
                        };

                        // adapt the primitive type and index buffer to the tessellation used
                        if (tessellationState.Method.PerformsAdjacentEdgeAverage())
                        {
                            renderMeshesToGenerateAEN.Add(renderMesh);
                        }
                        else
                        {
                            // Not using AEN tessellation anymore, dispose AEN indices if they were generated
                            Utilities.Dispose(ref tessellationState.GeneratedIndicesAEN);
                        }
                        tessellationState.MeshDraw = tessellationMeshDraw;

                        // Save back new state
                        tessellationStates[staticObjectNode] = tessellationState;

                        // Reset pipeline states
                        for (int i = 0; i < effectSlotCount; ++i)
                        {
                            var staticEffectObjectNode = staticObjectNode * effectSlotCount + i;
                            var renderEffect = renderEffects[staticEffectObjectNode];

                            if (renderEffect != null)
                                renderEffect.PipelineState = null;
                        }
                    }

                    renderMesh.ActiveMeshDraw = tessellationState.MeshDraw;
                }
                else if (tessellationState.GeneratedIndicesAEN != null)
                {
                    // Not using tessellation anymore, dispose AEN indices if they were generated
                    Utilities.Dispose(ref tessellationState.GeneratedIndicesAEN);
                }

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

                    if (materialInfo.CullMode != material.CullMode)
                    {
                        materialInfo.CullMode = material.CullMode;
                        renderEffect.PipelineState = null;
                    }

                    var isMaterialParametersChanged = materialInfo.MaterialParameters != material.Parameters;
                    if (isMaterialParametersChanged // parameter fast reload?
                        || materialInfo.PermutationCounter != material.Parameters.PermutationCounter)
                    {
                        materialInfo.VertexStageSurfaceShaders = material.Parameters.Get(MaterialKeys.VertexStageSurfaceShaders);
                        materialInfo.VertexStageStreamInitializer = material.Parameters.Get(MaterialKeys.VertexStageStreamInitializer);

                        materialInfo.DomainStageSurfaceShaders = material.Parameters.Get(MaterialKeys.DomainStageSurfaceShaders);
                        materialInfo.DomainStageStreamInitializer = material.Parameters.Get(MaterialKeys.DomainStageStreamInitializer);

                        materialInfo.TessellationShader = material.Parameters.Get(MaterialKeys.TessellationShader);

                        materialInfo.PixelStageSurfaceShaders = material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders);
                        materialInfo.PixelStageStreamInitializer = material.Parameters.Get(MaterialKeys.PixelStageStreamInitializer);
                        materialInfo.HasNormalMap = material.Parameters.Get(MaterialKeys.HasNormalMap);

                        materialInfo.MaterialParameters = material.Parameters;
                        materialInfo.ParametersChanged = isMaterialParametersChanged;
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
                    if (materialInfo.HasNormalMap)
                        renderEffect.EffectValidator.ValidateParameter(MaterialKeys.HasNormalMap, materialInfo.HasNormalMap);
                }
            }
        }

        /// <inheritdoc/>
        public override void Prepare(RenderDrawContext context)
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

                if (!UpdateMaterial(RenderSystem, context, materialInfo, perMaterialDescriptorSetSlot.Index, renderNode.RenderEffect, materialParameters))
                    continue;

                var descriptorSetPoolOffset = ((RootEffectRenderFeature)RootRenderFeature).ComputeResourceGroupOffset(renderNodeReference);
                resourceGroupPool[descriptorSetPoolOffset + perMaterialDescriptorSetSlot.Index] = materialInfo.Resources;
            }
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            if (renderMeshesToGenerateAEN.Count > 0)
            {
                var tessellationStates = RootRenderFeature.RenderData.GetData(tessellationStateKey);

                foreach (var renderMesh in renderMeshesToGenerateAEN)
                {
                    var tessellationState = tessellationStates[renderMesh.StaticObjectNode];
                    if (tessellationState.GeneratedIndicesAEN != null)
                        continue;

                    var tessellationMeshDraw = tessellationState.MeshDraw;

                    var indicesAEN = IndexExtensions.GenerateIndexBufferAEN(tessellationMeshDraw.IndexBuffer, tessellationMeshDraw.VertexBuffers[0], context.CommandList);
                    tessellationState.GeneratedIndicesAEN = Buffer.Index.New(Context.GraphicsDevice, indicesAEN);
                    tessellationMeshDraw.IndexBuffer = new IndexBufferBinding(tessellationState.GeneratedIndicesAEN, true, tessellationMeshDraw.IndexBuffer.Count*12/3);
                    tessellationMeshDraw.DrawCount = 12/3*tessellationMeshDraw.DrawCount;
                }

                renderMeshesToGenerateAEN.Clear();
            }
        }

        public static unsafe bool UpdateMaterial(RenderSystem renderSystem, RenderDrawContext context, MaterialInfoBase materialInfo, int materialSlotIndex, RenderEffect renderEffect, ParameterCollection materialParameters)
        {
            // Check if encountered first time this frame
            if (materialInfo.LastFrameUsed == renderSystem.FrameCounter)
                return true;

            // First time we use the material with a valid effect, let's update layouts
            if (materialInfo.PerMaterialLayout == null || materialInfo.PerMaterialLayout.Hash != renderEffect.Reflection.ResourceGroupDescriptions[materialSlotIndex].Hash)
            {
                var resourceGroupDescription = renderEffect.Reflection.ResourceGroupDescriptions[materialSlotIndex];
                if (resourceGroupDescription.DescriptorSetLayout == null)
                    return false;

                materialInfo.PerMaterialLayout = ResourceGroupLayout.New(renderSystem.GraphicsDevice, resourceGroupDescription, renderEffect.Effect.Bytecode);

                var parameterCollectionLayout = materialInfo.ParameterCollectionLayout = new ParameterCollectionLayout();
                parameterCollectionLayout.ProcessResources(resourceGroupDescription.DescriptorSetLayout);
                materialInfo.ResourceCount = parameterCollectionLayout.ResourceCount;

                // Process material cbuffer (if any)
                if (resourceGroupDescription.ConstantBufferReflection != null)
                {
                    materialInfo.ConstantBufferReflection = resourceGroupDescription.ConstantBufferReflection;
                    parameterCollectionLayout.ProcessConstantBuffer(resourceGroupDescription.ConstantBufferReflection);
                }
                materialInfo.ParametersChanged = true;
            }

            // If the parameters collection instance changed, we need to update it
            if (materialInfo.ParametersChanged)
            {
                materialInfo.ParameterCollection.UpdateLayout(materialInfo.ParameterCollectionLayout);
                materialInfo.ParameterCollectionCopier = new ParameterCollection.Copier(materialInfo.ParameterCollection, materialParameters);
                materialInfo.ParametersChanged = false;
            }

            // Mark this material as used during this frame
            materialInfo.LastFrameUsed = renderSystem.FrameCounter;

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

            return true;
        }

        struct TessellationState : IDisposable
        {
            public XenkoTessellationMethod Method;
            public Buffer GeneratedIndicesAEN;
            public MeshDraw MeshDraw;

            public void Dispose()
            {
                if (GeneratedIndicesAEN != null)
                {
                    GeneratedIndicesAEN.Dispose();
                    GeneratedIndicesAEN = null;
                }
            }
        }
    }
}