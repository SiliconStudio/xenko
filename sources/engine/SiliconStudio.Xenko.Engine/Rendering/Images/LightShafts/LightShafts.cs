// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Processors;
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
        /// <summary>
        /// The number of times the resolution is lowered for the light buffer
        /// </summary>
        [DataMemberRange(1,32)]
        public int LightBufferDownsampleLevel { get; set; } = 2;

        /// <summary>
        /// The amount of time the resolution is lowered for the bounding volume buffer
        /// </summary>
        [DataMemberRange(1, 32)]
        public int BoundingVolumeBufferDownsampleLevel { get; set; } = 8;

        /// <summary>
        /// Animate jitter
        /// </summary>
        public bool Animated { get; set; } = false;

        // TODO: Permutations
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

        private float time = 0.0f;
        
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

            if(LightBufferDownsampleLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(LightBufferDownsampleLevel));
            if (BoundingVolumeBufferDownsampleLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(BoundingVolumeBufferDownsampleLevel));

            var lightShaftDatas = lightShaftProcessor.LightShafts;

            var depthInput = GetSafeInput(0);

            // Create a min/max buffer generated from scene bounding volumes
            var boundingBoxBuffer = NewScopedRenderTarget2D(depthInput.Width / BoundingVolumeBufferDownsampleLevel, depthInput.Height / BoundingVolumeBufferDownsampleLevel, PixelFormat.R32G32_Float);
            
            // Create a single channel light buffer
            var lightBuffer = NewScopedRenderTarget2D(depthInput.Width/ LightBufferDownsampleLevel, depthInput.Height/ LightBufferDownsampleLevel, PixelFormat.R16_Float);
            lightShaftsEffectShader.SetOutput(lightBuffer);
            var lightShaftsParameters = lightShaftsEffectShader.Parameters;
            lightShaftsParameters.Set(DepthBaseKeys.DepthStencil, depthInput); // Bind scene depth

            if (!Initialized)
                Initialize(context.RenderContext);

            var renderView = context.RenderContext.RenderView;
            var viewInverse = Matrix.Invert(renderView.View);
            lightShaftsParameters.Set(TransformationKeys.ViewInverse, viewInverse);
            lightShaftsParameters.Set(TransformationKeys.Eye, new Vector4(viewInverse.TranslationVector, 1));

            var viewProjectionInverse = Matrix.Invert(renderView.ViewProjection);
            Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            center = Vector3.TransformCoordinate(center, viewProjectionInverse);
            right = Vector3.TransformCoordinate(right, viewProjectionInverse) - center;
            up = Vector3.TransformCoordinate(up, viewProjectionInverse) - center;

            // Basis for constructing world space rays originating from the camera
            lightShaftsParameters.Set(PostEffectBoundingRayKeys.ViewBase, center);
            lightShaftsParameters.Set(PostEffectBoundingRayKeys.ViewRight, right);
            lightShaftsParameters.Set(PostEffectBoundingRayKeys.ViewUp, up);

            // Setup parameters for Z reconstruction
            lightShaftsParameters.Set(CameraKeys.ZProjection, CameraKeys.ZProjectionACalculate(renderView.NearClipPlane, renderView.FarClipPlane));

            lightShaftsParameters.Set(TransformationKeys.ProjScreenRay, new Vector2(-1.0f / renderView.Projection.M11, 1.0f / renderView.Projection.M22));
            lightShaftsParameters.Set(TransformationKeys.Projection, renderView.Projection);
            Matrix projectionInverse;
            Matrix.Invert(ref renderView.Projection, out projectionInverse);
            lightShaftsParameters.Set(TransformationKeys.ProjectionInverse, projectionInverse);

            foreach (var lightShaft in lightShaftDatas)
            {
                if (lightShaft.LightComponent == null)
                    continue; // Skip entities without a light component

                var shadowMapTexture = shadowMapRenderer.FindShadowMap(renderView.LightingView ?? renderView, lightShaft.LightComponent);
                if (shadowMapTexture == null)
                    continue; // Skip lights without shadow map

                // Setup the shader group used for sampling shadows
                SetupLight(context, lightShaft, shadowMapTexture, lightShaftsParameters);

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

                    if (lightShaft.SampleCount < 1)
                        throw new ArgumentOutOfRangeException(nameof(lightShaft.SampleCount));

                    // Set min/max input
                    lightShaftsEffectShader.SetInput(0, boundingBoxBuffer);

                    // Light accumulation pass (on low resolution buffer)
                    DrawLightShaft(context, lightShaft);
                }

                // Everything was outside, skip
                if (!lightBufferUsed)
                    continue;

                if (LightBufferDownsampleLevel != 1)
                {
                    // Blur the result
                    blur.Radius = LightBufferDownsampleLevel;
                    blur.SetInput(lightBuffer);
                    blur.SetOutput(lightBuffer);
                    blur.Draw(context);
                }

                // Additive blend pass
                Color3 lightColor = lightShaft.Light.ComputeColor(context.GraphicsDevice.ColorSpace, lightShaft.LightComponent.Intensity);
                applyLightEffectShader.Parameters.Set(AdditiveLightShaderKeys.LightColor, lightColor);
                applyLightEffectShader.SetInput(lightBuffer);
                applyLightEffectShader.SetOutput(GetSafeOutput(0));
                applyLightEffectShader.Draw(context);
            }

            if(Animated)
                time += (float)Context.Time.Elapsed.TotalSeconds;
        }

        public void Draw(RenderDrawContext drawContext, Texture inputDepthStencil, Texture output)
        {
            SetInput(0, inputDepthStencil);
            SetOutput(output);
            Draw(drawContext);
        }

        private void SetupLight(RenderDrawContext context, LightShaftProcessor.Data lightShaft, LightShadowMapTexture shadowMapTexture, ParameterCollection lightParameterCollection)
        {
            var shadowGroup = shadowMapTexture.Renderer.CreateShaderGroupData(shadowMapTexture.ShadowType);
            BoundingBoxExt box = new BoundingBoxExt(new Vector3(-float.MaxValue), new Vector3(float.MaxValue)); // TODO

            // Some testing
            LightGroupRendererDynamic groupRenderer = null;
            if (lightShaft.Light is LightPoint)
            {
                groupRenderer = new LightPointGroupRenderer();
            }
            else if (lightShaft.Light is LightSpot)
            {
                groupRenderer = new LightSpotGroupRenderer();
            }
            else if (lightShaft.Light is LightDirectional)
            {
                groupRenderer = new LightDirectionalGroupRenderer();
            }
            else
            {
                throw new InvalidOperationException("Unsupported light type");
            }
            
            // TODO: Caching
            var directLightGroup = groupRenderer.CreateLightShaderGroup(context, shadowGroup);
            directLightGroup.SetViews(new FastList<RenderView>(new []{ context.RenderContext.RenderView }));
            directLightGroup.AddView(0, context.RenderContext.RenderView, 1);
            directLightGroup.AddLight(lightShaft.LightComponent, shadowMapTexture);
            directLightGroup.UpdateLayout("lightGroup");
            
            lightParameterCollection.Set(LightShaftsEffectKeys.LightGroup, directLightGroup.ShaderSource);
            lightParameterCollection.Set(LightShaftsEffectKeys.SampleCount, lightShaft.SampleCount);
            lightParameterCollection.Set(LightShaftsEffectKeys.Animated, Animated);

            // Update the effect here so the layout is correct
            lightShaftsEffectShader.EffectInstance.UpdateEffect(GraphicsDevice);

            directLightGroup.ApplyViewParameters(context, 0, lightParameterCollection);
            directLightGroup.ApplyDrawParameters(context, 0, lightParameterCollection, ref box);
        }

        private void DrawLightShaft(RenderDrawContext context, LightShaftProcessor.Data lightShaft)
        {
            lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionFactor, lightShaft.ExtinctionFactor);
            lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionRatio, lightShaft.ExtinctionRatio);
            lightShaftsEffectShader.Parameters.Set(LightShaftsShaderKeys.DensityFactor, lightShaft.DensityFactor);
            lightShaftsEffectShader.Parameters.Set(GlobalKeys.Time, time);
            
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
