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

            transformRenderFeature.PrepareEffectPermutations(RenderSystem);
        }

        public override void Prepare()
        {
            base.Prepare();

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

                var descriptorLayoutBuilder = renderEffect.Reflection.Binder.DescriptorReflection.Layouts[perLightingDescriptorSetSlot.Index].Layout;

                if (!parameters.HasLayout)
                {
                    var parameterCollectionLayout = new NextGenParameterCollectionLayout();
                    parameterCollectionLayout.ProcessResources(descriptorLayoutBuilder);

                    // Find material cbuffer
                    var lightingConstantBuffer = renderEffect.Effect.Bytecode.Reflection.ConstantBuffers.FirstOrDefault(x => x.Name == "PerLighting");
                    if (lightingConstantBuffer != null)
                    {
                        parameterCollectionLayout.ProcessConstantBuffer(lightingConstantBuffer);
                        renderSkybox.ConstantBufferSize = lightingConstantBuffer.Size;
                    }

                    parameters.UpdateLayout(parameterCollectionLayout);

                    renderSkybox.RotationParameter = parameters.GetValueParameter(SkyboxKeys.Rotation);
                    renderSkybox.SkyMatrixParameter = parameters.GetValueParameter(SkyboxKeys.SkyMatrix);

                    // TODO: Cache that
                    renderSkybox.DescriptorSetLayout = DescriptorSetLayout.New(RenderSystem.GraphicsDevice, descriptorLayoutBuilder);
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
                    var mappedCB = RenderSystem.BufferPool.Buffer.Data + renderNode.DrawConstantBufferOffset;
                    unsafe
                    {
                        *(Matrix*)(byte*)mappedCB = Matrix.Identity;
                    }
                }

                var descriptorSetPoolOffset = ComputeDescriptorSetOffset(renderNodeReference);
                var descriptorSet = DescriptorSet.New(RenderSystem.GraphicsDevice, RenderSystem.DescriptorPool, renderSkybox.DescriptorSetLayout);
                DescriptorSetPool[descriptorSetPoolOffset + perLightingDescriptorSetSlot.Index] = descriptorSet;

                // Set resource bindings in PerLighting resource set
                for (int resourceSlot = 0; resourceSlot < descriptorLayoutBuilder.ElementCount; ++resourceSlot)
                {
                    descriptorSet.SetValue(resourceSlot, parameters.ResourceValues[resourceSlot]);
                }

                // Process PerLighting cbuffer
                if (renderSkybox.ConstantBufferSize > 0)
                {
                    var lightingConstantBufferOffset = RenderSystem.BufferPool.Allocate(renderSkybox.ConstantBufferSize);

                    // Set constant buffer
                    descriptorSet.SetConstantBuffer(0, RenderSystem.BufferPool.Buffer, lightingConstantBufferOffset, renderSkybox.ConstantBufferSize);

                    var mappedCB = RenderSystem.BufferPool.Buffer.Data + lightingConstantBufferOffset;
                    Utilities.CopyMemory(mappedCB, parameters.DataValues, renderSkybox.ConstantBufferSize);
                }
            }

            transformRenderFeature.Prepare();
        }

        public override void Draw(RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var graphicsDevice = RenderSystem.GraphicsDevice;

            Effect currentEffect = null;
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
                    renderEffect.Effect.ApplyProgram(graphicsDevice);
                }

                renderEffect.Reflection.Binder.Apply(graphicsDevice, DescriptorSetPool, ComputeDescriptorSetOffset(renderNodeReference));

                graphicsDevice.PushState();
                graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.None);
                graphicsDevice.DrawQuad();
                graphicsDevice.PopState();
            }
        }
    }
}