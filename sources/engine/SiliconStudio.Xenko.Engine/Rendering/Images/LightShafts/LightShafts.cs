// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;
using DirectionalShaderData = SiliconStudio.Xenko.Rendering.Shadows.LightDirectionalShadowMapRenderer.ShaderData;

namespace SiliconStudio.Xenko.Rendering.Images
{
    [DataContract("LightShafts")]
    public class LightShafts : ImageEffect
    {
        private ImageEffectShader lightShaftsEffectShader;
        private ImageEffectShader applyLightEffectShader;
        private DynamicEffectInstance minmaxVolumeEffectShader;
        private GaussianBlur blur;

        private IShadowMapRenderer shadowMapRenderer;
        private LightShaftProcessor lightShaftProcessor;
        private LightShaftBoundingVolumeProcessor lightShaftBoundingVolumeProcessor;

        private MutablePipelineState[] minmaxPipelineStates = new MutablePipelineState[2];
        private EffectBytecode previousMinmaxEffectBytecode;

        private LightShaftBoundingVolumeData[] singleBoundingVolume = new LightShaftBoundingVolumeData[1];

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Light accumulation shader
            lightShaftsEffectShader = ToLoadAndUnload(new ImageEffectShader("LightShaftsEffect"));

            // Additive blending shader
            applyLightEffectShader = ToLoadAndUnload(new ImageEffectShader("AdditiveLightShader"));
            applyLightEffectShader.BlendState = new BlendStateDescription(Blend.One, Blend.One);

            minmaxVolumeEffectShader = new DynamicEffectInstance("VolumeMinMaxShader");
            minmaxVolumeEffectShader.Initialize(Context.Services);

            blur = ToLoadAndUnload(new GaussianBlur());

            // Need the shadow map renderer in order to render light shafts
            var meshRenderFeature = Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault();
            if(meshRenderFeature == null)
                throw new ArgumentNullException("Missing mesh render feature");

            var forwardLightingFeature = meshRenderFeature.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault();
            if (forwardLightingFeature == null)
                throw new ArgumentNullException("Missing forward lighting render feature");

            shadowMapRenderer = forwardLightingFeature.ShadowMapRenderer;

            for (int i = 0; i < 2; ++i)
            {
                var minmaxPipelineState = new MutablePipelineState(Context.GraphicsDevice);
                minmaxPipelineState.State.SetDefaults();

                minmaxPipelineState.State.BlendState.RenderTarget0.BlendEnable = true;
                minmaxPipelineState.State.BlendState.RenderTarget0.ColorSourceBlend = Blend.One;
                minmaxPipelineState.State.BlendState.RenderTarget0.ColorDestinationBlend = Blend.One;
                minmaxPipelineState.State.BlendState.RenderTarget0.ColorBlendFunction = i == 0 ? BlendFunction.Max : BlendFunction.Min;
                minmaxPipelineState.State.BlendState.RenderTarget0.ColorWriteChannels = i == 0 ? ColorWriteChannels.Red : ColorWriteChannels.Green;
                
                minmaxPipelineState.State.DepthStencilState.DepthBufferEnable = false;
                minmaxPipelineState.State.DepthStencilState.DepthBufferWriteEnable = false;
                minmaxPipelineState.State.RasterizerState.DepthClipEnable = false;
                minmaxPipelineState.State.RasterizerState.CullMode = i == 0 ? CullMode.Back : CullMode.Front;

                minmaxPipelineStates[i] = minmaxPipelineState;
            }
        }

        protected override void Destroy()
        {
            base.Destroy();
            minmaxVolumeEffectShader.Dispose();
        }

        public void Collect(RenderContext context)
        {
            lightShaftProcessor = context.SceneInstance.GetProcessor<LightShaftProcessor>();
            lightShaftBoundingVolumeProcessor = context.SceneInstance.GetProcessor<LightShaftBoundingVolumeProcessor>();
        }
        
        protected override void DrawCore(RenderDrawContext context)
        {
            if (lightShaftProcessor == null || lightShaftBoundingVolumeProcessor == null)
                return; // Not collected

            var lightShaftDatas = lightShaftProcessor.LightShafts;

            var depthInput = GetSafeInput(0);

            // Create a min/max buffer generated from scene bounding volumes
            int boundingBoxBufferDownsampleLevel = 8;
            var boundingBoxBuffer = NewScopedRenderTarget2D(depthInput.Width / boundingBoxBufferDownsampleLevel, depthInput.Height / boundingBoxBufferDownsampleLevel, PixelFormat.R32G32_Float);
            
            // Create a single channel light buffer
            int lightBufferDownsampleLevel = 2;
            var lightBuffer = NewScopedRenderTarget2D(depthInput.Width/lightBufferDownsampleLevel, depthInput.Height/lightBufferDownsampleLevel, PixelFormat.R16_Float);
            lightShaftsEffectShader.SetOutput(lightBuffer);
            lightShaftsEffectShader.Parameters.Set(DepthBaseKeys.DepthStencil, depthInput); // Bind scene depth

            if (!Initialized)
                Initialize(context.RenderContext);

            var renderView = context.RenderContext.RenderView;
            var viewInverse = Matrix.Invert(renderView.View);
            lightShaftsEffectShader.Parameters.Set(TransformationKeys.ViewInverse, viewInverse);

            // Setup parameters for Z reconstruction
            lightShaftsEffectShader.Parameters.Set(CameraKeys.ZProjection, CameraKeys.ZProjectionACalculate(renderView.NearClipPlane, renderView.FarClipPlane));
            lightShaftsEffectShader.Parameters.Set(TransformationKeys.ProjScreenRay, new Vector2(-1.0f / renderView.Projection.M11, 1.0f / renderView.Projection.M22));
            lightShaftsEffectShader.Parameters.Set(TransformationKeys.Projection, renderView.Projection);
            Matrix projectionInverse;
            Matrix.Invert(ref renderView.Projection, out projectionInverse);
            lightShaftsEffectShader.Parameters.Set(TransformationKeys.ProjectionInverse, projectionInverse);

            foreach (var lightShaft in lightShaftDatas)
            {
                if (lightShaft.LightComponent == null)
                    continue; // Skip entities without a light component

                var shadowMapTexture = shadowMapRenderer.FindShadowMap(renderView.LightingView ?? renderView, lightShaft.LightComponent);
                if (shadowMapTexture == null)
                    continue; // Skip lights without shadow map

                // Setup the shader group used for sampling shadows
                SetupLight(context, lightShaft, shadowMapTexture);

                // Additional information needed for some shadows
                lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.LightPosition, lightShaft.LightWorld.TranslationVector);
                lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.LightDirection, lightShaft.LightWorld.Forward);

                var boundingVolumes = lightShaftBoundingVolumeProcessor.GetBoundingVolumesForComponent(lightShaft.Component);
                if (boundingVolumes == null)
                    continue;

                // Check if we can pack bounding volumes together or need to draw them one by one
                var boundingVolumeLoop = lightShaft.SeparateBoundingVolumes ? boundingVolumes.Count : 1;
                var lightBufferUsed = false;
                for (int i = 0; i < boundingVolumeLoop; ++i)
                {
                    // Generate list of bounding volume (either all or one by one depending on SeparateBoundingVolumes)
                    var currentBoundingVolumes = (lightShaft.SeparateBoundingVolumes) ? singleBoundingVolume : boundingVolumes;
                    if (lightShaft.SeparateBoundingVolumes)
                        singleBoundingVolume[0] = boundingVolumes[i];

                    using (context.PushRenderTargetsAndRestore())
                    {
                        // Clear bounding box buffer
                        context.CommandList.Clear(boundingBoxBuffer, new Color4(0.0f, 1.0f, 0.0f, 0.0f));

                        context.CommandList.SetRenderTargetAndViewport(null, boundingBoxBuffer);

                        // If nothing visible, skip second part
                        if (!DrawBoundingVolumeMinMax(context, currentBoundingVolumes))
                            continue;
                    }

                    if (!lightBufferUsed)
                    {
                        // First pass: replace (avoid a clear and blend state)
                        lightShaftsEffectShader.BlendState = BlendStates.Opaque;
                        lightBufferUsed = true;
                    }
                    else
                    {
                        // Then: add
                        lightShaftsEffectShader.BlendState = BlendStates.Additive;
                    }

                    // Set min/max input
                    lightShaftsEffectShader.SetInput(0, boundingBoxBuffer);

                    // Light accumulation pass (on low resolution buffer)
                    DrawLightShaft(context, lightShaft);
                }

                // Everything was outside, skip
                if (!lightBufferUsed)
                    continue;

                // Blur the result
                blur.Radius = lightBufferDownsampleLevel;
                blur.SetInput(lightBuffer);
                blur.SetOutput(lightBuffer);
                blur.Draw(context);

                // Additive blend pass
                Color3 lightColor = lightShaft.Light.ComputeColor(context.GraphicsDevice.ColorSpace, lightShaft.LightComponent.Intensity);
                applyLightEffectShader.Parameters.Set(AdditiveLightShaderKeys.LightColor, lightColor);
                applyLightEffectShader.SetInput(lightBuffer);
                applyLightEffectShader.SetOutput(GetSafeOutput(0));
                applyLightEffectShader.Draw(context);
            }
        }

        public void Draw(RenderDrawContext drawContext, RenderOutputValidator outputValidator, Texture[] inputs, Texture inputDepthStencil, Texture outputTarget)
        {
            SetInput(0, inputDepthStencil);
            int colorTarget = outputValidator.Find<ColorTargetSemantic>();
            if(colorTarget < 0)
                throw new InvalidOperationException("Missing color input");
            SetOutput(inputs[colorTarget]);
            Draw(drawContext);
        }

        private void SetupLight(RenderDrawContext context, LightShaftData lightShaft, LightShadowMapTexture shadowMapTexture)
        {
            var group = shadowMapTexture.Renderer.CreateShaderGroupData(shadowMapTexture.ShadowType);
            group.UpdateLayout("shadowGroup");

            // Create a temporary group of 1 light so we can sample shadows in the same way as the forward rendering does
            group.UpdateLightCount(1, 1);
            FastListStruct<LightDynamicEntry> lights = new FastListStruct<LightDynamicEntry>(new FastList<LightDynamicEntry>(1));
            lights.Add(new LightDynamicEntry(lightShaft.LightComponent, shadowMapTexture));

            ShaderMixinSource source = new ShaderMixinSource();
            group.ApplyShader(source);
            lightShaftsEffectShader.Parameters.Set(LightShaftsEffectKeys.ShadowGroup, source);

            BoundingBoxExt box = new BoundingBoxExt(new Vector3(-float.MaxValue), new Vector3(float.MaxValue)); // TODO

            group.ApplyDrawParameters(context, lightShaftsEffectShader.Parameters, lights, ref box);
            group.ApplyViewParameters(context, lightShaftsEffectShader.Parameters, lights);

            string attenuationShaderClassName = "LightShaftsAttenuation";
            var pointLight = lightShaft.Light as LightPoint;
            if (pointLight != null)
            {
                var invSquareRadius = pointLight.InvSquareRadius;
                lightShaftsEffectShader.Parameters.Set(LightShaftsPointAttenuationKeys.InvSquareRadius, invSquareRadius);
                attenuationShaderClassName = "LightShaftsPointAttenuation";
            }
            else
            {
                var spotLight = lightShaft.Light as LightSpot;
                if (spotLight != null)
                {
                    var angleOffsetAndInvSquareRadius = new Vector3(spotLight.LightAngleScale, spotLight.LightAngleOffset, spotLight.InvSquareRange);
                    lightShaftsEffectShader.Parameters.Set(LightShaftsSpotAttenuationKeys.AngleOffsetAndInvSquareRadius, angleOffsetAndInvSquareRadius);
                    attenuationShaderClassName = "LightShaftsSpotAttenuation";
                }
            }
            lightShaftsEffectShader.Parameters.Set(LightShaftsEffectKeys.AttenuationModel, new ShaderClassSource(attenuationShaderClassName));
        }

        private void DrawLightShaft(RenderDrawContext context, LightShaftData lightShaft)
        {
            lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionFactor, lightShaft.ExtinctionFactor);
            lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionRatio, lightShaft.ExtinctionRatio);
            lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.DensityFactor, lightShaft.DensityFactor);
            
            lightShaftsEffectShader.Draw(context, $"Light Shafts [{lightShaft.LightComponent.Entity.Name}]");
        }
        
        private bool DrawBoundingVolumeMinMax(RenderDrawContext context, IReadOnlyList<LightShaftBoundingVolumeData> boundingVolumes)
        {
            var commandList = context.CommandList;

            bool effectUpdated = minmaxVolumeEffectShader.UpdateEffect(GraphicsDevice);
            if (minmaxVolumeEffectShader.Effect == null)
                return false;

            var needEffectUpdate = effectUpdated || previousMinmaxEffectBytecode != minmaxVolumeEffectShader.Effect.Bytecode;
            bool visibleMeshes = false;
            
            for (int pass = 0; pass < 2; ++pass)
            {
                var minmaxPipelineState = minmaxPipelineStates[pass];

                bool pipelineDirty = false;
                if (needEffectUpdate)
                {
                    // The EffectInstance might have been updated from outside
                    previousMinmaxEffectBytecode = minmaxVolumeEffectShader.Effect.Bytecode;

                    minmaxPipelineState.State.RootSignature = minmaxVolumeEffectShader.RootSignature;
                    minmaxPipelineState.State.EffectBytecode = minmaxVolumeEffectShader.Effect.Bytecode;

                    minmaxPipelineState.State.Output.RenderTargetCount = 1;
                    minmaxPipelineState.State.Output.RenderTargetFormat0 = commandList.RenderTarget.Format;
                    pipelineDirty = true;
                }

                Matrix viewProjection = context.RenderContext.RenderView.ViewProjection;

                MeshDraw currentDraw = null;
                var frustum = new BoundingFrustum(ref viewProjection);
                foreach (var volume in boundingVolumes)
                {
                    if (volume.Model == null)
                        continue;

                    // Update parameters for the minmax shader
                    Matrix worldViewProjection = Matrix.Multiply(volume.World, viewProjection);
                    minmaxVolumeEffectShader.Parameters.Set(VolumeMinMaxShaderKeys.WorldViewProjection, worldViewProjection);

                    foreach (var mesh in volume.Model.Meshes)
                    {
                        // Frustum culling
                        BoundingBox meshBoundingBox;
                        BoundingBox.Transform(ref mesh.BoundingBox, ref volume.World, out meshBoundingBox);
                        var boundingBoxExt = new BoundingBoxExt(meshBoundingBox);
                        if (boundingBoxExt.Extent != Vector3.Zero
                            && !VisibilityGroup.FrustumContainsBox(ref frustum, ref boundingBoxExt, true))
                            continue;

                        visibleMeshes = true;

                        var draw = mesh.Draw;

                        if (currentDraw != draw)
                        {
                            if (minmaxPipelineState.State.PrimitiveType != draw.PrimitiveType)
                            {
                                minmaxPipelineState.State.PrimitiveType = draw.PrimitiveType;
                                pipelineDirty = true;
                            }

                            var inputElements = draw.VertexBuffers.CreateInputElements();
                            if (inputElements.ComputeHash() != minmaxPipelineState.State.InputElements.ComputeHash())
                            {
                                minmaxPipelineState.State.InputElements = inputElements;
                                pipelineDirty = true;
                            }

                            // Update mesh
                            for (int i = 0; i < draw.VertexBuffers.Length; i++)
                            {
                                var vertexBuffer = draw.VertexBuffers[i];
                                commandList.SetVertexBuffer(i, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Stride);
                            }
                            if (draw.IndexBuffer != null)
                                commandList.SetIndexBuffer(draw.IndexBuffer.Buffer, draw.IndexBuffer.Offset, draw.IndexBuffer.Is32Bit);
                            currentDraw = draw;
                        }

                        if (pipelineDirty)
                        {
                            minmaxPipelineState.Update();
                            pipelineDirty = false;
                        }

                        context.CommandList.SetPipelineState(minmaxPipelineState.CurrentState);

                        minmaxVolumeEffectShader.Apply(context.GraphicsContext);

                        // Draw
                        if (currentDraw.IndexBuffer == null)
                            commandList.Draw(currentDraw.DrawCount, currentDraw.StartLocation);
                        else
                            commandList.DrawIndexed(currentDraw.DrawCount, currentDraw.StartLocation);
                    }
                }
            }

            return visibleMeshes;
        }
    }
}
