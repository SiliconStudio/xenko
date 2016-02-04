// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Skyboxes
{
    public class SkyboxRenderFeature : RootEffectRenderFeature
    {
        private EffectDescriptorSetReference perLightingDescriptorSetSlot;
        private TransformRenderFeature transformRenderFeature = new TransformRenderFeature();

        private ConstantBufferOffsetReference matrixTransform;

        public override bool SupportsRenderObject(RenderObject renderObject)
        {
            return renderObject is RenderSkybox;
        }

        public override void Initialize()
        {
            base.Initialize();

            perLightingDescriptorSetSlot = GetOrCreateEffectDescriptorSetSlot("PerLighting");
            matrixTransform = CreateDrawCBufferOffsetSlot(SpriteBaseKeys.MatrixTransform.Name);

            transformRenderFeature.AttachRootRenderFeature(this);
            transformRenderFeature.Initialize();
        }

        public override void Extract()
        {
            base.Extract();

            transformRenderFeature.Extract();
        }

        public override void PrepareEffectPermutationsImpl()
        {
            var renderEffects = GetData(RenderEffectKey);

            foreach (var renderObject in RenderObjects)
            {
                var staticObjectNode = renderObject.StaticObjectNode;

                for (int i = 0; i < EffectPermutationSlotCount; ++i)
                {
                    var staticEffectObjectNode = staticObjectNode.CreateEffectReference(EffectPermutationSlotCount, i);
                    var renderEffect = renderEffects[staticEffectObjectNode];
                    var renderSkybox = (RenderSkybox)renderObject;

                    // Skip effects not used during this frame
                    if (renderEffect == null || !renderEffect.IsUsedDuringThisFrame(RenderSystem))
                        continue;

                    var parameters = renderSkybox.Background == SkyboxBackground.Irradiance ? renderSkybox.Skybox.DiffuseLightingParameters : renderSkybox.Skybox.Parameters;
                    renderEffect.EffectValidator.ValidateParameter(SkyboxKeys.Shader, parameters.GetResourceSlow(SkyboxKeys.Shader));
                }
            }

            transformRenderFeature.PrepareEffectPermutations();
        }

        public override void Prepare(NextGenRenderContext context)
        {
            base.Prepare(context);

            for (int renderNodeIndex = 0; renderNodeIndex < renderNodes.Count; renderNodeIndex++)
            {
                var renderNodeReference = new RenderNodeReference(renderNodeIndex);
                var renderNode = renderNodes[renderNodeIndex];

                var renderSkybox = (RenderSkybox)renderNode.RenderObject;
                var parameters = renderSkybox.Background == SkyboxBackground.Irradiance ? renderSkybox.Skybox.DiffuseLightingParameters : renderSkybox.Skybox.Parameters;

                var renderEffect = renderNode.RenderEffect;

                if (renderSkybox.Background == SkyboxBackground.Irradiance)
                {
                    // Need to compose keys with "skyboxColor" (sub-buffer?)
                    throw new NotImplementedException();
                }

                var descriptorLayoutBuilder = renderEffect.Reflection.DescriptorReflection.Layouts[perLightingDescriptorSetSlot.Index].Layout;

                if (!parameters.HasLayout)
                {
                    var parameterCollectionLayout = new NextGenParameterCollectionLayout();
                    parameterCollectionLayout.ProcessResources(descriptorLayoutBuilder);

                    // Find material cbuffer
                    var lightingConstantBuffer = renderEffect.Effect.Bytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerLighting");
                    if (lightingConstantBuffer != null)
                    {
                        parameterCollectionLayout.ProcessConstantBuffer(lightingConstantBuffer);
                    }

                    parameters.UpdateLayout(parameterCollectionLayout);

                    renderSkybox.RotationParameter = parameters.GetValueParameter(SkyboxKeys.Rotation);
                    renderSkybox.SkyMatrixParameter = parameters.GetValueParameter(SkyboxKeys.SkyMatrix);

                    // TODO: Cache that
                    renderSkybox.ResourceGroupLayout = ResourceGroupLayout.New(RenderSystem.GraphicsDevice, descriptorLayoutBuilder, renderEffect.Effect.Bytecode, "PerLighting");
                    renderSkybox.Resources = new ResourceGroup();
                }

                // Update SkyMatrix
                var rotation = parameters.Get(renderSkybox.RotationParameter);
                Matrix skyMatrix;
                Matrix.RotationY(MathUtil.DegreesToRadians(rotation), out skyMatrix);
                parameters.Set(renderSkybox.SkyMatrixParameter, ref skyMatrix);

                // Update MatrixTransform
                // TODO: Use default values?
                var matrixTransformOffset = renderNode.RenderEffect.Reflection.PerDrawLayout.GetConstantBufferOffset(this.matrixTransform);
                if (matrixTransformOffset != -1)
                {
                    var mappedCB = renderNode.Resources.ConstantBuffer.Data;
                    unsafe
                    {
                        *(Matrix*)(byte*)mappedCB = Matrix.Identity;
                    }
                }

                var descriptorSetPoolOffset = ComputeResourceGroupOffset(renderNodeReference);
                NextGenParameterCollectionLayoutExtensions.PrepareResourceGroup(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, RenderSystem.BufferPool, renderSkybox.ResourceGroupLayout, BufferPoolAllocationType.UsedMultipleTime, renderSkybox.Resources);
                ResourceGroupPool[descriptorSetPoolOffset + perLightingDescriptorSetSlot.Index] = renderSkybox.Resources;

                var descriptorSet = renderSkybox.Resources.DescriptorSet;

                // Set resource bindings in PerLighting resource set
                for (int resourceSlot = 0; resourceSlot < descriptorLayoutBuilder.ElementCount; ++resourceSlot)
                {
                    descriptorSet.SetValue(resourceSlot, parameters.ResourceValues[resourceSlot]);
                }

                // Process PerLighting cbuffer
                if (renderSkybox.Resources.ConstantBuffer.Size > 0)
                {
                    var mappedCB = renderSkybox.Resources.ConstantBuffer.Data;
                    Utilities.CopyMemory(mappedCB, parameters.DataValues, renderSkybox.Resources.ConstantBuffer.Size);
                }
            }

            transformRenderFeature.Prepare(context);
        }

        public override void Draw(NextGenRenderContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var graphicsDevice = RenderSystem.GraphicsDevice;

            var pipelineState = context.Pipeline.State;

            Effect currentEffect = null;
            var descriptorSets = new DescriptorSet[EffectDescriptorSetSlotCount];

            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                // Get effect
                // TODO: Use real effect slot
                var renderEffect = renderNode.RenderEffect;

                if (currentEffect != renderEffect.Effect)
                {
                    currentEffect = renderEffect.Effect;
                    pipelineState.EffectBytecode = renderEffect.Effect.Bytecode;
                    pipelineState.RootSignature = renderEffect.Reflection.RootSignature;
                }

                // First time, let's compile pipeline state
                // TODO GRAPHICS REFACTOR invalidate if effect is destroyed, or some other cases
                if (renderEffect.PipelineState == null)
                {
                    // Bind VAO
                    pipelineState.InputElements = PrimitiveQuad.VertexDeclaration.CreateInputElements();
                    pipelineState.PrimitiveType = PrimitiveQuad.PrimitiveType;
                    var oldDepthStencilState = pipelineState.DepthStencilState;
                    pipelineState.DepthStencilState = graphicsDevice.DepthStencilStates.None;

                    // TODO GRAPHICS REFACTOR
                    // pipelineState.RenderTargetFormats = 

                    context.Pipeline.Update(graphicsDevice);
                    renderEffect.PipelineState = context.Pipeline.CurrentState;

                    pipelineState.DepthStencilState = oldDepthStencilState;
                }

                graphicsDevice.SetPipelineState(renderEffect.PipelineState);

                var resourceGroupOffset = ComputeResourceGroupOffset(renderNodeReference);
                renderEffect.Reflection.BufferUploader.Apply(graphicsDevice, ResourceGroupPool, resourceGroupOffset);

                // Bind descriptor sets
                for (int i = 0; i < descriptorSets.Length; ++i)
                {
                    var resourceGroup = ResourceGroupPool[resourceGroupOffset++];
                    if (resourceGroup != null)
                        descriptorSets[i] = resourceGroup.DescriptorSet;
                }

                graphicsDevice.SetDescriptorSets(0, descriptorSets);

                graphicsDevice.DrawQuad();
            }
        }
    }
}