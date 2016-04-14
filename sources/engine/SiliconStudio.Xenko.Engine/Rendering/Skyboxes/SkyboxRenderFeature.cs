// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public class SkyboxRenderFeature : RootEffectRenderFeature
    {
        private EffectDescriptorSetReference perLightingDescriptorSetSlot;
        private TransformRenderFeature transformRenderFeature = new TransformRenderFeature();

        private ConstantBufferOffsetReference matrixTransform;

        /// <inheritdoc/>
        public override Type SupportedRenderObjectType => typeof(RenderSkybox);

        internal class SkyboxInfo
        {
            public Skybox Skybox;

            // Used internally by renderer
            public ResourceGroupLayout ResourceGroupLayout;
            public ResourceGroup Resources = new ResourceGroup();
            public ParameterCollection ParameterCollection = new ParameterCollection();
            public ParameterCollectionLayout ParameterCollectionLayout;
            public ParameterCollection.Copier ParameterCollectionCopier;

            public SkyboxInfo(Skybox skybox)
            {
                Skybox = skybox;
            }
        }

        public SkyboxRenderFeature()
        {
            // Skybox should render after most objects (to take advantage of early z depth test)
            SortKey = 192;
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            perLightingDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerLighting");
            matrixTransform = CreateDrawCBufferOffsetSlot(SpriteBaseKeys.MatrixTransform.Name);

            transformRenderFeature.AttachRootRenderFeature(this);
            transformRenderFeature.Initialize(Context);
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            base.Extract();

            transformRenderFeature.Extract();
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void PrepareEffectPermutationsImpl(RenderDrawContext context)
        {
            var renderEffects = RenderData.GetData(RenderEffectKey);

            foreach (var renderObject in RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < EffectPermutationSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode * EffectPermutationSlotCount + i;
                    var renderEffect = renderEffects[staticEffectObjectNode];
                    var renderSkybox = (RenderSkybox)renderObject;

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    var parameters = renderSkybox.Background == SkyboxBackground.Irradiance ? renderSkybox.Skybox.DiffuseLightingParameters : renderSkybox.Skybox.Parameters;

                    var shader = parameters.Get(SkyboxKeys.Shader);

                    if (shader == null)
                    {
                        renderEffect.EffectValidator.ShouldSkip = true;
                    }
                    renderEffect.EffectValidator.ValidateParameter(SkyboxKeys.Shader, shader);
                }
            }

            transformRenderFeature.PrepareEffectPermutations(context);
        }

        /// <inheritdoc/>
        public override unsafe void Prepare(RenderDrawContext context)
        {
            base.Prepare(context);

            for (int renderNodeIndex = 0; renderNodeIndex < RenderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = RenderNodes[renderNodeIndex];

                var renderSkybox = (RenderSkybox)renderNode.RenderObject;
                var sourceParameters = renderSkybox.Background == SkyboxBackground.Irradiance ? renderSkybox.Skybox.DiffuseLightingParameters : renderSkybox.Skybox.Parameters;

                var skyboxInfo = renderSkybox.SkyboxInfo;
                if (skyboxInfo == null || skyboxInfo.Skybox != renderSkybox.Skybox)
                    skyboxInfo = renderSkybox.SkyboxInfo = new SkyboxInfo(renderSkybox.Skybox);

                var parameters = skyboxInfo.ParameterCollection;

                var renderEffect = renderNode.RenderEffect;
                if (renderEffect.State != RenderEffectState.Normal)
                    continue;

                // TODO GRAPHICS REFACTOR current system is not really safe with multiple renderers (parameters come from Skybox which is shared but ResourceGroupLayout from RenderSkybox is per RenderNode)
                if (skyboxInfo.ResourceGroupLayout == null || skyboxInfo.ResourceGroupLayout.Hash != renderEffect.Reflection.ResourceGroupDescriptions[perLightingDescriptorSetSlot.Index].Hash)
                {
                    var resourceGroupDescription = renderEffect.Reflection.ResourceGroupDescriptions[perLightingDescriptorSetSlot.Index];

                    var parameterCollectionLayout = skyboxInfo.ParameterCollectionLayout = new ParameterCollectionLayout();
                    parameterCollectionLayout.ProcessResources(resourceGroupDescription.DescriptorSetLayout);

                    // Find material cbuffer
                    if (resourceGroupDescription.ConstantBufferReflection != null)
                    {
                        parameterCollectionLayout.ProcessConstantBuffer(resourceGroupDescription.ConstantBufferReflection);
                    }

                    //skyboxInfo.RotationParameter = parameters.GetAccessor(SkyboxKeys.Rotation);
                    //skyboxInfo.SkyMatrixParameter = parameters.GetAccessor(SkyboxKeys.SkyMatrix);

                    // TODO: Cache that
                    skyboxInfo.ResourceGroupLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, resourceGroupDescription, renderEffect.Effect.Bytecode);

                    parameters.UpdateLayout(parameterCollectionLayout);

                    if (renderSkybox.Background == SkyboxBackground.Irradiance)
                    {
                        skyboxInfo.ParameterCollectionCopier = new ParameterCollection.Copier(parameters, sourceParameters, ".skyboxColor");
                    }
                    else
                    {
                        skyboxInfo.ParameterCollectionCopier = new ParameterCollection.Copier(parameters, sourceParameters);
                    }
                }

                skyboxInfo.ParameterCollectionCopier.Copy();

                // Setup the intensity
                parameters.Set(SkyboxKeys.Intensity, renderSkybox.Intensity);

                // Update SkyMatrix
                Matrix skyMatrix;
                Matrix.RotationQuaternion(ref renderSkybox.Rotation, out skyMatrix);
                parameters.Set(SkyboxKeys.SkyMatrix, ref skyMatrix);

                // Update MatrixTransform
                // TODO: Use default values?
                var matrixTransformOffset = renderNode.RenderEffect.Reflection.PerDrawLayout.GetConstantBufferOffset(this.matrixTransform);
                if (matrixTransformOffset != -1)
                {
                    var mappedCB = renderNode.Resources.ConstantBuffer.Data + matrixTransformOffset;
                    Matrix.Translation(0.0f, 0.0f, 1.0f, out *(Matrix*)(byte*)mappedCB);
                }

                var descriptorSetPoolOffset = ComputeResourceGroupOffset(renderNodeReference);
                context.ResourceGroupAllocator.PrepareResourceGroup(skyboxInfo.ResourceGroupLayout, BufferPoolAllocationType.UsedMultipleTime, skyboxInfo.Resources);
                ResourceGroupPool[descriptorSetPoolOffset + perLightingDescriptorSetSlot.Index] = skyboxInfo.Resources;

                var descriptorSet = skyboxInfo.Resources.DescriptorSet;

                // Set resource bindings in PerLighting resource set
                for (int resourceSlot = 0; resourceSlot < parameters.Layout.ResourceCount; ++resourceSlot)
                {
                    descriptorSet.SetValue(resourceSlot, parameters.ObjectValues[resourceSlot]);
                }

                // Process PerLighting cbuffer
                if (skyboxInfo.Resources.ConstantBuffer.Size > 0)
                {
                    var mappedCB = skyboxInfo.Resources.ConstantBuffer.Data;
                    fixed (byte* dataValues = parameters.DataValues)
                        Utilities.CopyMemory(mappedCB, (IntPtr)dataValues, skyboxInfo.Resources.ConstantBuffer.Size);
                }
            }

            transformRenderFeature.Prepare(context);
        }

        protected override void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            // Bind VAO
            pipelineState.InputElements = PrimitiveQuad.VertexDeclaration.CreateInputElements();
            pipelineState.PrimitiveType = PrimitiveQuad.PrimitiveType;

            // Don't clip nor write Z value (we are writing at 1.0f = infinity)
            pipelineState.DepthStencilState = DepthStencilStates.DepthRead;
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var commandList = context.CommandList;

            var descriptorSets = new DescriptorSet[EffectDescriptorSetSlotCount];

            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.SortedRenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                // Get effect
                // TODO: Use real effect slot
                var renderEffect = renderNode.RenderEffect;
                if (renderEffect.Effect == null)
                    continue;

                commandList.SetPipelineState(renderEffect.PipelineState);

                var resourceGroupOffset = ComputeResourceGroupOffset(renderNodeReference);
                renderEffect.Reflection.BufferUploader.Apply(context.CommandList, ResourceGroupPool, resourceGroupOffset);

                // Bind descriptor sets
                for (int i = 0; i < descriptorSets.Length; ++i)
                {
                    var resourceGroup = ResourceGroupPool[resourceGroupOffset++];
                    if (resourceGroup != null)
                        descriptorSets[i] = resourceGroup.DescriptorSet;
                }

                commandList.SetDescriptorSets(0, descriptorSets);

                commandList.DrawQuad();
            }
        }
    }
}