// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;
using DirectionalShaderData = SiliconStudio.Xenko.Rendering.Shadows.LightDirectionalShadowMapRenderer.ShaderData;

namespace SiliconStudio.Xenko.Rendering.Images
{
    [DataContract("LightShafts")]
    public class LightShafts : ImageEffect
    {
        private ImageEffectShader scatteringEffectShader;
        private ImageEffectShader minmaxEffectShader;
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
            scatteringEffectShader = ToLoadAndUnload(new ImageEffectShader("LightShaftsShader"));

            minmaxEffectShader = ToLoadAndUnload(new ImageEffectShader("PostEffectMinMax"));

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
                minmaxPipelineState.State.BlendState.RenderTarget0.ColorBlendFunction = i == 0 ? BlendFunction.Min : BlendFunction.Max;
                minmaxPipelineState.State.BlendState.RenderTarget0.ColorWriteChannels = i == 0 ? ColorWriteChannels.Green : ColorWriteChannels.Red;

                minmaxPipelineState.State.RasterizerState.CullMode = i == 0 ? CullMode.Front : CullMode.Back;
                minmaxPipelineState.State.DepthStencilState.DepthBufferEnable = false;

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

            var minmaxBuffer = NewScopedRenderTarget2D(256, 256, PixelFormat.R32G32_Float);

            // Create a single channel light buffer
            int lightBufferDownsampleLevel = 2;
            var lightBuffer = NewScopedRenderTarget2D(depthInput.Width/lightBufferDownsampleLevel, depthInput.Height/lightBufferDownsampleLevel, PixelFormat.R16_Float);
            scatteringEffectShader.SetOutput(lightBuffer);
            //scatteringEffectShader.SetOutput(GetSafeOutput(0));
            scatteringEffectShader.Parameters.Set(DepthBaseKeys.DepthStencil, depthInput); // Bind scene depth

            if (!Initialized)
                Initialize(context.RenderContext);

            var renderView = context.RenderContext.RenderView;
            var viewInverse = Matrix.Invert(renderView.View);
            scatteringEffectShader.Parameters.Set(TransformationKeys.ViewInverse, viewInverse);
            /*scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.Eye, viewInverse.TranslationVector);

            var viewProjectionInverse = Matrix.Invert(renderView.ViewProjection);
            Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            center = Vector3.TransformCoordinate(center, viewProjectionInverse);
            right = Vector3.TransformCoordinate(right, viewProjectionInverse) - center;
            up = Vector3.TransformCoordinate(up, viewProjectionInverse) - center;

            // Basis for constructing world space rays originating from the camera
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ViewBase, center);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ViewRight, right);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ViewUp, up);

            // Used to project an arbitrary world space point into a linear depth value
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.CameraForward, viewInverse.Forward);

            // Used to convert values from the depth buffer to linear depth
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ZProjection, CameraKeys.ZProjectionACalculate(renderView.NearClipPlane, renderView.FarClipPlane));

            // Time % 1.0
            //imageEffectShader.Parameters.Set(LightShaftsShaderKeys.Time, (float)(context.RenderContext.Time.Elapsed.TotalSeconds % 1.0));
            // Time
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.Time, (float)(context.RenderContext.Time.Total.TotalSeconds));*/

            foreach (var lightShaft in lightShaftDatas)
            {
                if (lightShaft.LightComponent == null)
                    continue; // Skip entities without a light component

                if (!shadowMapRenderer.ShadowMaps.TryGetValue(lightShaft.LightComponent, out lightShaft.ShadowMapTexture))
                    continue;

                if (lightShaft.ShadowMapTexture == null)
                    continue; // Skip lights without shadow map

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
                        context.CommandList.Clear(boundingBoxBuffer, new Color4(0.0f, 1.0f, 0.0f, 1.0f));

                        context.CommandList.SetRenderTargetAndViewport(null, boundingBoxBuffer);

                        // If nothing visible, skip second part
                        if (!DrawBoundingVolumeMinMax(context, currentBoundingVolumes))
                            continue;

                        // Perform shadow map min max
                        if (false) // Min-max optim currently disabled
                        {
                            if (lightShaft.ShadowMapTexture.ShaderData is DirectionalShaderData)
                            {
                                var shaderData = (DirectionalShaderData)lightShaft.ShadowMapTexture.ShaderData;
                                minmaxEffectShader.Parameters.Set(PostEffectMinMaxKeys.MinMaxCoords, shaderData.TextureCoords[0]);
                            }

                            minmaxEffectShader.SetInput(0, lightShaft.ShadowMapTexture.Atlas.Texture);
                            minmaxEffectShader.SetOutput(minmaxBuffer);
                            minmaxEffectShader.Draw(context);
                        }
                    }

                    // Setup parameters for Z reconstruction
                    scatteringEffectShader.Parameters.Set(CameraKeys.ZProjection, CameraKeys.ZProjectionACalculate(renderView.NearClipPlane, renderView.FarClipPlane));
                    scatteringEffectShader.Parameters.Set(TransformationKeys.ProjScreenRay, new Vector2(-1.0f/renderView.Projection.M11, 1.0f/renderView.Projection.M22));

                    if (!lightBufferUsed)
                    {
                        // First pass: replace (avoid a clear and blend state)
                        scatteringEffectShader.BlendState = BlendStates.Opaque;
                        lightBufferUsed = true;
                    }
                    else
                    {
                        // Then: add
                        scatteringEffectShader.BlendState = BlendStates.Additive;
                    }

                    // Set min/max input
                    scatteringEffectShader.SetInput(0, boundingBoxBuffer);
                    scatteringEffectShader.SetInput(1, lightShaft.ShadowMapTexture.Atlas.Texture);
                    scatteringEffectShader.SetInput(2, minmaxBuffer);

                    // Light accumulation pass (on low resolution buffer)
                    DrawLightShaft(context, lightShaft);
                }

                // Everything was outside, skip
                if (!lightBufferUsed)
                    continue;

                // Blur the result
                //blur.Radius = lightBufferDownsampleLevel;
                //blur.SetInput(lightBuffer);
                //blur.SetOutput(lightBuffer);
                //blur.Draw(context);

                // Additive blend pass
                Color3 lightColor = lightShaft.Light.ComputeColor(context.GraphicsDevice.ColorSpace, lightShaft.LightComponent.Intensity);
                applyLightEffectShader.Parameters.Set(AdditiveLightShaderKeys.LightColor, lightColor);
                applyLightEffectShader.SetInput(lightBuffer);
                applyLightEffectShader.SetOutput(GetSafeOutput(0));
                applyLightEffectShader.Draw(context);
            }
        }

        private void DrawLightShaft(RenderDrawContext context, LightShaftData lightShaft)
        {
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionFactor, lightShaft.ExtinctionFactor);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionRatio, lightShaft.ExtinctionRatio);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.DensityFactor, lightShaft.DensityFactor);

            var shadowMapTexture = lightShaft.ShadowMapTexture.Atlas.Texture;

            // Bind shadow atlas
            scatteringEffectShader.Parameters.Set(ShadowMapKeys.Texture, lightShaft.ShadowMapTexture.Atlas.Texture);

            var shadowMapTextureSize = new Vector2(shadowMapTexture.Width, shadowMapTexture.Height);
            var shadowMapTextureTexelSize = 1.0f/shadowMapTextureSize;
            scatteringEffectShader.Parameters.Set(ShadowMapKeys.TextureSize, shadowMapTextureSize);
            scatteringEffectShader.Parameters.Set(ShadowMapKeys.TextureTexelSize, shadowMapTextureTexelSize);

            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.LightColor, lightShaft.LightComponent.Color);

            // Pass in world transform as offset and direction
            //scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowLightOffset, lightShaft.LightWorld.TranslationVector);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowLightOffset, lightShaft.LightWorld.TranslationVector);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowLightDirection, lightShaft.LightWorld.Forward);

            // Change inputs depending on light type
            if (lightShaft.ShadowMapTexture.ShaderData is DirectionalShaderData)
            {
                var shaderData = (DirectionalShaderData)lightShaft.ShadowMapTexture.ShaderData;

                scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowTextureFactor, shaderData.TextureCoords[0]);

                // Use cascade 0 of directional light
                scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowViewProjection, shaderData.WorldToShadowCascadeUV[0]);

                scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowMapDistance, shaderData.CascadeSplits[0]);
            }

            scatteringEffectShader.Draw(context, $"Light Shafts [{lightShaft.LightComponent.Entity.Name}]");
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
