// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.VirtualReality;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Renders your game. It should use current <see cref="RenderContext.RenderView"/> and <see cref="CameraComponentRendererExtensions.GetCurrentCamera"/>.
    /// </summary>
    [Display("Forward Renderer")]
    public partial class ForwardRenderer : SceneRendererBase, ISharedRenderer
    {
        // TODO: should we use GraphicsDeviceManager.PreferredBackBufferFormat?
        public const PixelFormat DepthBufferFormat = PixelFormat.D24_UNorm_S8_UInt;

        private IShadowMapRenderer shadowMapRenderer;
        private Texture depthStencilROCached;
        private MultisampleCount actualMultisampleCount = MultisampleCount.None;
        private VRDeviceSystem vrSystem;

        private readonly FastList<Texture> currentRenderTargets = new FastList<Texture>();
        private readonly FastList<Texture> currentRenderTargetsMSAA = new FastList<Texture>();

        protected Texture ViewOutputTarget;
        protected Texture ViewDepthStencil;
        protected Texture ViewDepthStencilNoMSAA;

        protected int ViewCount { get; private set; }

        protected int ViewIndex { get; private set; }

        public ClearRenderer Clear { get; set; } = new ClearRenderer();

        /// <summary>
        /// Enable Light Probe.
        /// </summary>
        public bool LightProbes { get; set; } = true;

        /// <summary>
        /// The main render stage for opaque geometry.
        /// </summary>
        public RenderStage OpaqueRenderStage { get; set; }

        /// <summary>
        /// The transparent render stage for transparent geometry.
        /// </summary>
        public RenderStage TransparentRenderStage { get; set; }

        /// <summary>
        /// The shadow map render stages for shadow casters. No shadow rendering will happen if null.
        /// </summary>
        [MemberCollection(NotNullItems = true)]
        public List<RenderStage> ShadowMapRenderStages { get; } = new List<RenderStage>();

        /// <summary>
        /// The G-Buffer render stage to render depth buffer and possibly some other extra info to buffers (i.e. normals)
        /// </summary>
        public RenderStage GBufferRenderStage { get; set; }

        /// <summary>
        /// The post effects renderer.
        /// </summary>
        public IPostProcessingEffects PostEffects { get; set; }

        /// <summary>
        /// Light shafts effect
        /// </summary>
        public LightShafts LightShafts { get; set; }

        /// <summary>
        /// Virtual Reality related settings
        /// </summary>
        public VRRendererSettings VRSettings { get; set; } = new VRRendererSettings();

        /// <summary>
        /// The level of multi-sampling
        /// </summary>
        public MultisampleCount MSAALevel { get; set; } = MultisampleCount.None;

        /// <summary>
        /// MSAA Resolver is used to resolve multi-sampled render targets into normal render targets
        /// </summary>
        public MSAAResolver MSAAResolver { get; } = new MSAAResolver();

        /// <summary>
        /// If true, depth buffer generated during <see cref="OpaqueRenderStage"/> will be available as a shader resource named DepthBase.DepthStencil during <see cref="TransparentRenderStage"/>.
        /// </summary>
        /// <remarks>
        /// This is needed by some effects such as particles soft edges.
        /// 
        /// On recent platforms that can bind depth buffer as read-only (<see cref="GraphicsDeviceFeatures.HasDepthAsReadOnlyRT"/>), depth buffer will be used as is. Otherwise, a copy will be generated.
        /// </remarks>
        [DefaultValue(true)]
        public bool BindDepthAsResourceDuringTransparentRendering { get; set; } = true;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            shadowMapRenderer = Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;

            if (MSAALevel != MultisampleCount.None)
            {
                actualMultisampleCount = (MultisampleCount)Math.Min((int)MSAALevel, (int)GraphicsDevice.Features[PixelFormat.R16G16B16A16_Float].MultisampleCountMax);
                actualMultisampleCount = (MultisampleCount)Math.Min((int)actualMultisampleCount, (int)GraphicsDevice.Features[DepthBufferFormat].MultisampleCountMax);

                // Note: we cannot support MSAA on DX10 now
                if(GraphicsDevice.Features.HasMultisampleDepthAsSRV == false)
                    actualMultisampleCount = MultisampleCount.None;
            }

            var camera = Context.GetCurrentCamera();

            vrSystem = (VRDeviceSystem)Services.GetService(typeof(VRDeviceSystem));
            if (vrSystem != null)
            {
                if (VRSettings.Enabled)
                {
                    var requiredDescs = VRSettings.RequiredApis.ToArray();
                    vrSystem.PreferredApis = requiredDescs.Select(x => x.Api).ToArray();
                    vrSystem.PreferredScalings = requiredDescs.ToDictionary(x => x.Api, x => x.ResolutionScale);
                    vrSystem.RequireMirror = true;
                    vrSystem.MirrorWidth = GraphicsDevice.Presenter.BackBuffer.Width;
                    vrSystem.MirrorHeight = GraphicsDevice.Presenter.BackBuffer.Height;

                    vrSystem.Enabled = true; //careful this will trigger the whole chain of initialization!
                    vrSystem.Visible = true;
                    
                    VRSettings.VRDevice = vrSystem.Device;

                    vrSystem.PreviousUseCustomProjectionMatrix = camera.UseCustomProjectionMatrix;
                    vrSystem.PreviousUseCustomViewMatrix = camera.UseCustomViewMatrix;
                    vrSystem.PreviousCameraProjection = camera.ProjectionMatrix;

                    if (VRSettings.VRDevice.SupportsOverlays)
                    {
                        foreach (var overlay in VRSettings.Overlays)
                        {
                            if (overlay != null && overlay.Texture != null)
                            {
                                overlay.Overlay = VRSettings.VRDevice.CreateOverlay(overlay.Texture.Width, overlay.Texture.Height, overlay.Texture.MipLevels, (int)overlay.Texture.MultisampleCount);
                            }
                        }
                    }
                }
                else
                {
                    vrSystem.Enabled = false;
                    vrSystem.Visible = false;

                    VRSettings.VRDevice = null;

                    if (vrSystem.Device != null) //we had a device before so we know we need to restore the camera
                    {
                        camera.UseCustomViewMatrix = vrSystem.PreviousUseCustomViewMatrix;
                        camera.UseCustomProjectionMatrix = vrSystem.PreviousUseCustomProjectionMatrix;
                        camera.ProjectionMatrix = vrSystem.PreviousCameraProjection;
                    }
                }
            }
        }

        protected virtual void CollectStages(RenderContext context)
        {
            if (OpaqueRenderStage != null)
            {
                OpaqueRenderStage.OutputValidator.BeginCustomValidation(context.RenderOutput.DepthStencilFormat, context.RenderOutput.MultisampleCount);
                ValidateOpaqueStageOutput(OpaqueRenderStage.OutputValidator, context);
                OpaqueRenderStage.OutputValidator.EndCustomValidation();
            }

            if (TransparentRenderStage != null)
            {
                TransparentRenderStage.OutputValidator.Validate(ref context.RenderOutput);
            }

            if (GBufferRenderStage != null && LightProbes)
            {
                GBufferRenderStage.Output = new RenderOutputDescription(PixelFormat.None, context.RenderOutput.DepthStencilFormat);
            }
        }

        protected virtual void ValidateOpaqueStageOutput(RenderOutputValidator renderOutputValidator, RenderContext renderContext)
        {
            renderOutputValidator.Add<ColorTargetSemantic>(renderContext.RenderOutput.RenderTargetFormat0);

            if (PostEffects != null)
            {
                if (PostEffects.RequiresNormalBuffer)
                {
                    renderOutputValidator.Add<NormalTargetSemantic>(PixelFormat.R16G16B16A16_Float);
                }

                if (PostEffects.RequiresVelocityBuffer)
                {
                    renderOutputValidator.Add<VelocityTargetSemantic>(PixelFormat.R16G16_Float);
                }

                if (PostEffects.RequiresSsrGBuffers)
                {
                    renderOutputValidator.Add<OctahedronNormalSpecularColorTargetSemantic>(PixelFormat.R16G16B16A16_Float);
                    renderOutputValidator.Add<EnvironmentLightRoughnessTargetSemantic>(PixelFormat.R16G16B16A16_Float);
                }
            }
        }

        protected virtual void CollectView(RenderContext context)
        {
            // Fill RenderStage formats and register render stages to main view
            if (OpaqueRenderStage != null)
            {
                context.RenderView.RenderStages.Add(OpaqueRenderStage);
            }

            if (TransparentRenderStage != null)
            {
                context.RenderView.RenderStages.Add(TransparentRenderStage);
            }

            if (GBufferRenderStage != null && LightProbes)
            {
                context.RenderView.RenderStages.Add(GBufferRenderStage);
            }
        }

        protected override unsafe void CollectCore(RenderContext context)
        {
            var camera = context.GetCurrentCamera();

            if (context.RenderView == null)
                throw new NullReferenceException(nameof(context.RenderView) + " is null. Please make sure you have your camera correctly set.");

            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                // Mark this view as requiring shadows
                shadowMapRenderer?.RenderViewsWithShadows.Add(context.RenderView);

                context.RenderOutput = new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : context.RenderOutput.RenderTargetFormat0, DepthBufferFormat, MSAALevel);

                CollectStages(context);

                if (VRSettings.Enabled && VRSettings.VRDevice != null)
                {
                    Vector3 cameraPos, cameraScale;
                    Matrix cameraRot;

                    if (!vrSystem.PreviousUseCustomViewMatrix)
                    {
                        camera.Entity.Transform.WorldMatrix.Decompose(out cameraScale, out cameraRot, out cameraPos);
                    }
                    else
                    {
                        camera.ViewMatrix.Decompose(out cameraScale, out cameraRot, out cameraPos);
                        cameraRot.Transpose();
                        Vector3.Negate(ref cameraPos, out cameraPos);
                        Vector3.TransformCoordinate(ref cameraPos, ref cameraRot, out cameraPos);
                    }

                    if (VRSettings.IgnoreCameraRotation)
                    {
                        cameraRot = Matrix.Identity;
                    }

                    // Compute both view and projection matrices
                    Matrix* viewMatrices = stackalloc Matrix[2];
                    Matrix* projectionMatrices = stackalloc Matrix[2];
                    for (var i = 0; i < 2; ++i)
                        VRSettings.VRDevice.ReadEyeParameters(i == 0 ? Eyes.Left : Eyes.Right, camera.NearClipPlane, camera.FarClipPlane, ref cameraPos, ref cameraRot, out viewMatrices[i], out projectionMatrices[i]);

                    // if the VRDevice disagreed with the near and far plane, we must re-discover them and follow:
                    var near = projectionMatrices[0].M43 / projectionMatrices[0].M33;
                    var far = near * (-projectionMatrices[0].M33 / (-projectionMatrices[0].M33 - 1));
                    if (Math.Abs(near - camera.NearClipPlane) > 1e-8f)
                        camera.NearClipPlane = near;
                    if (Math.Abs(near - camera.FarClipPlane) > 1e-8f)
                        camera.FarClipPlane = far;

                    // Compute a view matrix and projection matrix that cover both eyes for shadow map and culling
                    ComputeCommonViewMatrices(context, viewMatrices, projectionMatrices);
                    var commonView = context.RenderView;

                    // Notify lighting system this view only purpose is for shared lighting, it is not being drawn directly.
                    commonView.Flags |= RenderViewFlags.NotDrawn;

                    // Collect now, and use result for both eyes
                    CollectView(context);
                    context.VisibilityGroup.TryCollect(commonView);

                    for (var i = 0; i < 2; i++)
                    {
                        using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                        using (context.SaveViewportAndRestore())
                        {
                            context.RenderSystem.Views.Add(context.RenderView);
                            context.RenderView.SceneInstance = commonView.SceneInstance;
                            context.RenderView.LightingView = commonView;
                            context.ViewportState.Viewport0 = new Viewport(0, 0, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2.0f, VRSettings.VRDevice.ActualRenderFrameSize.Height);

                            //change camera params for eye
                            camera.ViewMatrix = viewMatrices[i];
                            camera.ProjectionMatrix = projectionMatrices[i];
                            camera.UseCustomProjectionMatrix = true;
                            camera.UseCustomViewMatrix = true;
                            camera.Update();

                            //write params to view
                            SceneCameraRenderer.UpdateCameraToRenderView(context, context.RenderView, camera);

                            // Copy culling results
                            context.VisibilityGroup.Copy(commonView, context.RenderView);

                            CollectView(context);

                            LightShafts?.Collect(context);

                            PostEffects?.Collect(context);
                        }
                    }

                    if (VRSettings.VRDevice.SupportsOverlays)
                    {
                        foreach (var overlay in VRSettings.Overlays)
                        {
                            if (overlay != null && overlay.Texture != null)
                            {
                                overlay.Overlay.Position = overlay.LocalPosition;
                                overlay.Overlay.Rotation = overlay.LocalRotation;
                                overlay.Overlay.SurfaceSize = overlay.SurfaceSize;
                                overlay.Overlay.FollowHeadRotation = overlay.FollowsHeadRotation;
                            }
                        }
                    }
                }
                else
                {
                    //write params to view
                    SceneCameraRenderer.UpdateCameraToRenderView(context, context.RenderView, camera);

                    CollectView(context);

                    LightShafts?.Collect(context);

                    PostEffects?.Collect(context);
                }

                // Set depth format for shadow map render stages
                // TODO: This format should be acquired from the ShadowMapRenderer instead of being fixed here
                foreach(var shadowMapRenderStage in ShadowMapRenderStages)
                    shadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);
            }

            PostEffects?.Collect(context);
        }

        protected static PixelFormat ComputeNonMSAADepthFormat(PixelFormat format)
        {
            PixelFormat result;

            switch (format)
            {
                case PixelFormat.R16_Float:
                case PixelFormat.R16_Typeless:
                case PixelFormat.D16_UNorm:
                    result = PixelFormat.R16_Float;
                    break;
                case PixelFormat.R32_Float:
                case PixelFormat.R32_Typeless:
                case PixelFormat.D32_Float:
                    result = PixelFormat.R32_Float;
                    break;

                // Note: for those formats we lose stencil buffer information during MSAA -> non-MSAA conversion
                case PixelFormat.R24G8_Typeless:
                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.R24_UNorm_X8_Typeless:
                    result = PixelFormat.R32_Float;
                    break;
                case PixelFormat.R32G8X24_Typeless:
                case PixelFormat.D32_Float_S8X24_UInt:
                case PixelFormat.R32_Float_X8X24_Typeless:
                    result = PixelFormat.R32_Float;
                    break;

                default:
                    throw new NotSupportedException($"Unsupported depth format [{format}]");
            }

            return result;
        }

        protected virtual void ResolveDepthMSAA(RenderDrawContext drawContext)
        {
            if (ViewDepthStencil.MultisampleCount == MultisampleCount.None)
            {
                ViewDepthStencilNoMSAA = ViewDepthStencil;
                return;
            }

            ViewDepthStencilNoMSAA = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(
                ViewDepthStencil.ViewWidth, ViewDepthStencil.ViewHeight, 1, ComputeNonMSAADepthFormat(ViewDepthStencil.Format), TextureFlags.RenderTarget | TextureFlags.ShaderResource)));

            ResolveMSAA(drawContext, ViewDepthStencil, ViewDepthStencilNoMSAA, 1);
        }

        protected virtual void ResolveMSAA(RenderDrawContext drawContext, Texture input, Texture output, int maxResolveSamples = (int)MultisampleCount.X8)
        {
            if (MSAAResolver != null && MSAAResolver.Enabled)
            {
                MSAAResolver.Resolve(drawContext, input, output, maxResolveSamples);
            }
            else
            {
                drawContext.CommandList.CopyMultisample(input, 0, output, 0);
            }
        }

        protected virtual void ResolveMSAA(RenderDrawContext drawContext)
        {
            currentRenderTargetsMSAA.Resize(currentRenderTargets.Count, false);

            for (int index = 0; index < currentRenderTargets.Count; index++)
            {
                var input = currentRenderTargets[index];

                var outputDescription = TextureDescription.New2D(input.ViewWidth, input.ViewHeight, 1, input.Format, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                var output = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(outputDescription));

                currentRenderTargetsMSAA[index] = output;
                ResolveMSAA(drawContext, input, output);
            }
        }

        protected virtual void DrawView(RenderContext context, RenderDrawContext drawContext)
        {
            var renderSystem = context.RenderSystem;

            // Z Prepass
            var lightProbes = LightProbes && GBufferRenderStage != null;
            if (lightProbes)
            {
                // Note: Baking lightprobe before GBuffer prepass because we are updating some cbuffer parameters needed by Opaque pass that GBuffer pass might upload early
                PrepareLightprobeConstantBuffer(context);

                // TODO: Temporarily using ShadowMap shader
                drawContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Green, CompositingProfilingKeys.GBuffer);

                using (drawContext.PushRenderTargetsAndRestore())
                {
                    drawContext.CommandList.Clear(drawContext.CommandList.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
                    drawContext.CommandList.SetRenderTarget(drawContext.CommandList.DepthStencilBuffer, null);

                    // Draw [main view | z-prepass stage]
                    renderSystem.Draw(drawContext, context.RenderView, GBufferRenderStage);
                }

                drawContext.CommandList.GpuQueryProfiler.EndProfile();

                // Bake lightprobes against Z-buffer
                BakeLightProbes(context, drawContext);
            }

            using (drawContext.PushRenderTargetsAndRestore())
            {
                // Draw [main view | main stage]
                if (OpaqueRenderStage != null)
                {
                    drawContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Green, CompositingProfilingKeys.Opaque);
                    renderSystem.Draw(drawContext, context.RenderView, OpaqueRenderStage);
                    drawContext.CommandList.GpuQueryProfiler.EndProfile();
                }

                // Draw [main view | transparent stage]
                Texture depthStencilSRV = null;
                if (TransparentRenderStage != null)
                {
                    drawContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Green, CompositingProfilingKeys.Transparent);

                    // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        depthStencilSRV = ResolveDepthAsSRV(drawContext);

                        // Override depth stencil buffer if it doesn't support SRV
                        if (depthStencilSRV != null)
                            ViewDepthStencil = depthStencilSRV;

                        renderSystem.Draw(drawContext, context.RenderView, TransparentRenderStage);
                    }

                    drawContext.CommandList.GpuQueryProfiler.EndProfile();
                }

                var colorTargetIndex = OpaqueRenderStage?.OutputValidator.Find(typeof(ColorTargetSemantic)) ?? -1;
                if (colorTargetIndex == -1)
                    return;

                if (PostEffects != null)
                {
                    //Make sure we run post effects without MSAA
                    var renderTargets = currentRenderTargets;
                    var depthStencil = ViewDepthStencil;

                    // Resolve MSAA targets
                    if (actualMultisampleCount != MultisampleCount.None)
                    {
                        drawContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Green, CompositingProfilingKeys.MsaaResolve);

                        // If lightprobes (which need Z-Prepass) are enabled, depth is already resolved
                        //if (!lightProbes)
                        // TODO: is that comment above true? i don't know lightprobes rendering stuff but if it does it should override ResolveDepthMSAA? maybe some redesign...
                        renderTargets = currentRenderTargetsMSAA;
                        ResolveDepthMSAA(drawContext);
                        ResolveMSAA(drawContext);

                        depthStencil = ViewDepthStencilNoMSAA;

                        drawContext.CommandList.GpuQueryProfiler.EndProfile();
                    }

                    // Shafts if we have them
                    if (LightShafts != null)
                    {
                        drawContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Green, CompositingProfilingKeys.LightShafts);

                        LightShafts.Draw(drawContext, depthStencil, renderTargets[colorTargetIndex]);

                        drawContext.CommandList.GpuQueryProfiler.EndProfile();
                    }

                    // Run post effects
                    // Note: OpaqueRenderStage can't be null otherwise colorTargetIndex would be -1
                    PostEffects.Draw(drawContext, OpaqueRenderStage.OutputValidator, renderTargets.Items, depthStencil, ViewOutputTarget);
                }
                else
                {
                    if (actualMultisampleCount != MultisampleCount.None)
                    {
                        drawContext.CommandList.GpuQueryProfiler.BeginProfile(Color.Green, CompositingProfilingKeys.MsaaResolve);

                        ResolveMSAA(drawContext);
                        drawContext.CommandList.Copy(currentRenderTargetsMSAA[colorTargetIndex], ViewOutputTarget);

                        drawContext.CommandList.GpuQueryProfiler.EndProfile();
                    }
                }

                // Free the depth texture since we won't need it anymore
                if (depthStencilSRV != null)
                {
                    drawContext.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
                }
            }
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var viewport = drawContext.CommandList.Viewport;

            using (drawContext.PushRenderTargetsAndRestore())
            {
                // Render Shadow maps
                shadowMapRenderer?.Draw(drawContext);

                if (VRSettings.Enabled && VRSettings.VRDevice != null)
                {
                    var isFullViewport = (int)viewport.X == 0 && (int)viewport.Y == 0
                                         && (int)viewport.Width == drawContext.CommandList.RenderTarget.ViewWidth
                                         && (int)viewport.Height == drawContext.CommandList.RenderTarget.ViewHeight;
                    if (!isFullViewport)
                        return;

                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        //make sure we don't use any default targets!
                        drawContext.CommandList.SetRenderTargets(null, null);

                        PrepareRenderTargets(drawContext, new Size2(VRSettings.VRDevice.ActualRenderFrameSize.Width / 2, VRSettings.VRDevice.ActualRenderFrameSize.Height));

                        //also prepare the final VR target
                        var vrFullSurface = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                                                TextureDescription.New2D(VRSettings.VRDevice.ActualRenderFrameSize.Width, VRSettings.VRDevice.ActualRenderFrameSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                                                    TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                     
                        //draw per eye
                        using (context.SaveViewportAndRestore())
                        using (drawContext.PushRenderTargetsAndRestore())
                        {
                            drawContext.CommandList.SetViewport(new Viewport(0.0f, 0.0f, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2.0f, VRSettings.VRDevice.ActualRenderFrameSize.Height));
                            drawContext.CommandList.SetRenderTargets(ViewDepthStencil, currentRenderTargets.Count, currentRenderTargets.Items);

                            ViewCount = 2;

                            for (var i = 0; i < 2; i++)
                            {
                                using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                                {
                                    // Clear render target and depth stencil
                                    Clear?.Draw(drawContext);

                                    ViewIndex = 0;

                                    DrawView(context, drawContext);
                                    drawContext.CommandList.CopyRegion(ViewOutputTarget, 0, null, vrFullSurface, 0, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2 * i);
                                }
                            }

                            if (VRSettings.VRDevice.SupportsOverlays)
                            {
                                foreach (var overlay in VRSettings.Overlays)
                                {
                                    if (overlay != null && overlay.Texture != null)
                                    {
                                        overlay.Overlay.UpdateSurface(drawContext.CommandList, overlay.Texture);
                                    }
                                }
                            }

                            VRSettings.VRDevice.Commit(drawContext.CommandList, vrFullSurface);
                        }
                    }

                    //draw mirror to backbuffer (if size is matching and full viewport)
                    if (VRSettings.VRDevice.MirrorTexture.Size != drawContext.CommandList.RenderTarget.Size)
                    {
                        VRSettings.MirrorScaler.SetInput(0, VRSettings.VRDevice.MirrorTexture);
                        VRSettings.MirrorScaler.SetOutput(drawContext.CommandList.RenderTarget);
                        VRSettings.MirrorScaler.Draw(drawContext);
                    }
                    else
                    {
                        drawContext.CommandList.Copy(VRSettings.VRDevice.MirrorTexture, drawContext.CommandList.RenderTarget);
                    }
                }
                else
                {
                    PrepareRenderTargets(drawContext, new Size2((int)viewport.Width, (int)viewport.Height));

                    ViewCount = 1;
                    ViewIndex = 0;

                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        drawContext.CommandList.SetRenderTargets(ViewDepthStencil, currentRenderTargets.Count, currentRenderTargets.Items);

                        // Clear render target and depth stencil
                        Clear?.Draw(drawContext);

                        DrawView(context, drawContext);
                    }
                }
            }

            currentRenderTargets.Clear();
            currentRenderTargetsMSAA.Clear();
        }

        private Texture ResolveDepthAsSRV(RenderDrawContext context)
        {
            if (!BindDepthAsResourceDuringTransparentRendering)
                return null;

            var depthStencil = context.CommandList.DepthStencilBuffer;
            var depthStencilSRV = context.Resolver.ResolveDepthStencil(context.CommandList.DepthStencilBuffer);

            var renderView = context.RenderContext.RenderView;

            foreach (var renderFeature in context.RenderContext.RenderSystem.RenderFeatures)
            {
                if (!(renderFeature is RootEffectRenderFeature))
                    continue;

                var depthLogicalKey = ((RootEffectRenderFeature)renderFeature).CreateViewLogicalGroup("Depth");
                var viewFeature = renderView.Features[renderFeature.Index];

                // Copy ViewProjection to PerFrame cbuffer
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    var resourceGroup = viewLayout.Entries[renderView.Index].Resources;

                    var depthLogicalGroup = viewLayout.GetLogicalGroup(depthLogicalKey);
                    if (depthLogicalGroup.Hash == ObjectId.Empty)
                        continue;

                    // Might want to use ProcessLogicalGroup if more than 1 Recource
                    resourceGroup.DescriptorSet.SetShaderResourceView(depthLogicalGroup.DescriptorSlotStart, depthStencilSRV);
                }
            }
            
            context.CommandList.SetRenderTargets(null, context.CommandList.RenderTargetCount, context.CommandList.RenderTargets);

            depthStencilROCached = context.Resolver.GetDepthStencilAsRenderTarget(depthStencil, depthStencilROCached);
            context.CommandList.SetRenderTargets(depthStencilROCached, context.CommandList.RenderTargetCount, context.CommandList.RenderTargets);

            return depthStencilSRV;
        }

        private void PrepareRenderTargets(RenderDrawContext drawContext, Texture currentRenderTarget)
        {
            if (OpaqueRenderStage == null)
                return;

            var renderTargets = OpaqueRenderStage.OutputValidator.RenderTargets;

            currentRenderTargets.Resize(renderTargets.Count, false);

            for (int index = 0; index < renderTargets.Count; index++)
            {
                if (renderTargets[index].Semantic is ColorTargetSemantic && PostEffects == null && actualMultisampleCount == MultisampleCount.None)
                {
                    currentRenderTargets[index] = currentRenderTarget;
                }
                else
                { 
                    var description = renderTargets[index];
                    var textureDescription = TextureDescription.New2D(currentRenderTarget.Width, currentRenderTarget.Height, 1, description.Format, TextureFlags.RenderTarget | TextureFlags.ShaderResource, 1, GraphicsResourceUsage.Default, actualMultisampleCount);
                    currentRenderTargets[index] = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(textureDescription));
                }

                drawContext.CommandList.ResourceBarrierTransition(currentRenderTargets[index], GraphicsResourceState.RenderTarget);
            }
        }

        /// <summary>
        /// Prepares targets per frame, caching and handling MSAA etc.
        /// </summary>
        /// <param name="drawContext">The current draw context</param>
        /// <param name="renderTargetsSize"></param>
        protected virtual void PrepareRenderTargets(RenderDrawContext drawContext, Size2 renderTargetsSize)
        {
            var currentRenderTarget = drawContext.CommandList.RenderTarget;
            if (drawContext.CommandList.RenderTargetCount == 0)
                currentRenderTarget = null;

            var currentDepthStencil = drawContext.CommandList.DepthStencilBuffer;

            // Make sure we got a valid NOT MSAA final OUTPUT Target
            if (currentRenderTarget == null)
            {
                currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                        TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
            }

            PrepareRenderTargets(drawContext, currentRenderTarget);

            //MSAA, we definitely need new buffers
            if (actualMultisampleCount != MultisampleCount.None)
            {
                //Handle Depth
                ViewDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, currentDepthStencil?.ViewFormat ?? PixelFormat.D24_UNorm_S8_UInt,
                        TextureFlags.ShaderResource | TextureFlags.DepthStencil, 1, GraphicsResourceUsage.Default, actualMultisampleCount)));
            }
            else
            {
                //Handle Depth
                if (currentDepthStencil == null)
                {
                    currentDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil)));
                }
                ViewDepthStencil = currentDepthStencil;
            }

            ViewOutputTarget = currentRenderTarget;
        }

        protected override void Destroy()
        {
            PostEffects?.Dispose();
        }
    }
}
