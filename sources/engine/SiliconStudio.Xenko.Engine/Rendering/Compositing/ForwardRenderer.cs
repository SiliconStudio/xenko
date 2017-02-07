using System;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.VirtualReality;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Renders your game. It should use current <see cref="RenderContext.RenderView"/> and <see cref="CameraComponentRendererExtensions.GetCurrentCamera"/>.
    /// </summary>
    public class ForwardRenderer : SceneRendererBase, ISharedRenderer
    {
        private IShadowMapRenderer shadowMapRenderer;
        private Texture depthStencilROCached;
        private MSAALevel actualMSAALevel = MSAALevel.None;

        protected readonly FastList<Texture> ViewRenderTargets = new FastList<Texture>();
        protected Texture ViewOutputTarget;
        protected Texture ViewDepthStencil;

        public ClearRenderer Clear { get; set; } = new ClearRenderer();

        /// <summary>
        /// The main render stage for opaque geometry.
        /// </summary>
        public RenderStage OpaqueRenderStage { get; set; }

        /// <summary>
        /// The transparent render stage for transparent geometry.
        /// </summary>
        public RenderStage TransparentRenderStage { get; set; }

        /// <summary>
        /// The shadow map render stage for shadow casters. No shadows rendering will happen if null.
        /// </summary>
        public RenderStage ShadowMapRenderStage { get; set; }

        /// <summary>
        /// The post effects renderer.
        /// </summary>
        public IPostProcessingEffects PostEffects { get; set; }

        /// <summary>
        /// Virtual Reality related settings
        /// </summary>
        public VRRendererSettings VRSettings { get; set; } = new VRRendererSettings();

        /// <summary>
        /// The level of multi-sampling
        /// </summary>
        public MSAALevel MSAALevel { get; set; } = MSAALevel.None;

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

            shadowMapRenderer =
                Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;

            if (MSAALevel != MSAALevel.None)
            {
                actualMSAALevel = (MSAALevel)Math.Min((int)MSAALevel, (int)GraphicsDevice.Features[PixelFormat.R16G16B16A16_Float].MSAALevelMax);
                actualMSAALevel = (MSAALevel)Math.Min((int)actualMSAALevel, (int)GraphicsDevice.Features[PixelFormat.D24_UNorm_S8_UInt].MSAALevelMax);
            }

            var camera = Context.GetCurrentCamera();

            var vrSystem = (VRDeviceSystem)Services.GetService(typeof(VRDeviceSystem));
            if (vrSystem != null)
            {
                if (VRSettings.Enabled)
                {
                    vrSystem.PreferredApis = VRSettings.RequiredApis.ToArray();
                    vrSystem.RequireMirror = true;
                    vrSystem.MirrorWidth = GraphicsDevice.Presenter.BackBuffer.Width;
                    vrSystem.MirrorHeight = GraphicsDevice.Presenter.BackBuffer.Height;

                    vrSystem.Enabled = true; //careful this will trigger the whole chain of initialization!
                    vrSystem.Visible = true;
                    
                    VRSettings.VRDevice = vrSystem.Device;

                    vrSystem.PreviousUseCustomProjectionMatrix = camera.UseCustomProjectionMatrix;
                    vrSystem.PreviousUseCustomViewMatrix = camera.UseCustomViewMatrix;
                    vrSystem.PreviousCameraProjection = camera.ProjectionMatrix;
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

        protected virtual void CollectView(RenderContext context)
        {
            // Mark this view as requiring shadows
            shadowMapRenderer?.RenderViewsWithShadows.Add(context.RenderView);

            // Fill RenderStage formats and register render stages to main view
            if (OpaqueRenderStage != null)
            {
                context.RenderView.RenderStages.Add(OpaqueRenderStage);
                OpaqueRenderStage.Output = context.RenderOutput;
            }
            if (TransparentRenderStage != null)
            {
                context.RenderView.RenderStages.Add(TransparentRenderStage);
                TransparentRenderStage.Output = context.RenderOutput;
            }
        }

        protected override void CollectCore(RenderContext context)
        {
            var camera = context.GetCurrentCamera();

            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                context.RenderOutput = new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : context.RenderOutput.RenderTargetFormat0, PixelFormat.D24_UNorm_S8_UInt);

                if (VRSettings.Enabled && VRSettings.VRDevice != null)
                {
                    Vector3 cameraPos, cameraScale;
                    Matrix cameraRot;

                    if (!camera.UseCustomViewMatrix)
                    {
                        camera.Entity.Transform.WorldMatrix.Decompose(out cameraScale, out cameraRot, out cameraPos);
                    }
                    else
                    {
                        camera.ViewMatrix.Decompose(out cameraScale, out cameraRot, out cameraPos);
                    }

                    if (VRSettings.IgnoreCameraRotation)
                    {
                        cameraRot = Matrix.Identity;
                    }

                    var sceneInstance = context.RenderView.SceneInstance;

                    for (var i = 0; i < 2; i++)
                    {
                        using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                        using (context.SaveViewportAndRestore())
                        {
                            context.RenderSystem.Views.Add(context.RenderView);
                            context.RenderView.SceneInstance = sceneInstance;
                            context.ViewportState.Viewport0 = new Viewport(0, 0, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2.0f, VRSettings.VRDevice.ActualRenderFrameSize.Height);

                            //change camera params for eye
                            VRSettings.VRDevice.ReadEyeParameters(i == 0 ? Eyes.Left : Eyes.Right, camera.NearClipPlane, camera.FarClipPlane, ref cameraPos, ref cameraRot, out camera.ViewMatrix, out camera.ProjectionMatrix);
                            camera.UseCustomProjectionMatrix = true;
                            camera.UseCustomViewMatrix = true;
                            camera.Update();

                            //write params to view
                            SceneCameraRenderer.UpdateCameraToRenderView(context, context.RenderView, camera);

                            CollectView(context);
                        }
                    }
                }
                else
                {
                    //write params to view
                    SceneCameraRenderer.UpdateCameraToRenderView(context, context.RenderView, camera);

                    CollectView(context);
                }

                if (ShadowMapRenderStage != null)
                    ShadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);
            }
        }

        protected virtual void DrawView(RenderContext context, RenderDrawContext drawContext)
        {
            var renderSystem = context.RenderSystem;

            using (drawContext.PushRenderTargetsAndRestore())
            {
                // Render Shadow maps
                if (ShadowMapRenderStage != null && shadowMapRenderer != null)
                {
                    // Clear atlases
                    shadowMapRenderer.PrepareAtlasAsRenderTargets(drawContext.CommandList);

                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        // Draw all shadow views generated for the current view
                        foreach (var renderView in renderSystem.Views)
                        {
                            var shadowmapRenderView = renderView as ShadowMapRenderView;
                            if (shadowmapRenderView != null && shadowmapRenderView.RenderView == context.RenderView)
                            {
                                var shadowMapRectangle = shadowmapRenderView.Rectangle;
                                drawContext.CommandList.SetRenderTarget(shadowmapRenderView.ShadowMapTexture.Atlas.Texture, null);
                                shadowmapRenderView.ShadowMapTexture.Atlas.MarkClearNeeded();
                                drawContext.CommandList.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                                renderSystem.Draw(drawContext, shadowmapRenderView, ShadowMapRenderStage);
                            }
                        }
                    }

                    shadowMapRenderer.PrepareAtlasAsShaderResourceViews(drawContext.CommandList);
                }

                // Draw [main view | main stage]
                if (OpaqueRenderStage != null)
                {
                    renderSystem.Draw(drawContext, context.RenderView, OpaqueRenderStage);
                }

                // Draw [main view | transparent stage]
                if (TransparentRenderStage != null)
                {
                    // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        var depthStencilSRV = ResolveDepthAsSRV(drawContext);

                        renderSystem.Draw(drawContext, context.RenderView, TransparentRenderStage);

                        // Free the depth texture since we won't need it anymore
                        if (depthStencilSRV != null)
                        {
                            drawContext.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
                        }
                    }
                }

                if (PostEffects != null)
                {
                    //Make sure we run also post effects with MSAA
                    var peOutputTarget = ViewOutputTarget;
                    if (actualMSAALevel != MSAALevel.None && ViewOutputTarget.MultiSampleLevel == MSAALevel.None)
                    {
                        peOutputTarget =
                            PushScopedResource(
                                drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(ViewOutputTarget.Size.Width, ViewOutputTarget.Size.Height,
                                    1, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)));
                    }

                    // Run post effects
                    // TODO: output in proper renderTarget location according to viewport
                    PostEffects.Draw(drawContext, ViewRenderTargets, ViewDepthStencil, peOutputTarget);

                    if (actualMSAALevel != MSAALevel.None)
                    {
                        drawContext.CommandList.CopyMultiSample(peOutputTarget, 0, ViewOutputTarget, 0);
                    }
                }
                else
                {
                    if (actualMSAALevel != MSAALevel.None)
                    {
                        drawContext.CommandList.CopyMultiSample(ViewRenderTargets[0], 0, ViewOutputTarget, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Prepares targets per frame, caching and handling MSAA etc.
        /// </summary>
        /// <param name="drawContext">The current draw context</param>
        /// <param name="width">The desired width</param>
        /// <param name="height">The desired height</param>
        protected virtual void PrepareRenderTargets(RenderDrawContext drawContext, int width, int height)
        {
            ViewRenderTargets.Clear();

            var currentRenderTarget = drawContext.CommandList.RenderTarget;
            var currentDepthStencil = drawContext.CommandList.DepthStencilBuffer;

            //MSAA, we definitely need new buffers
            if (actualMSAALevel != MSAALevel.None)
            {
                //Handle render targets
                var renderTarget = PostEffects != null
                    ? PushScopedResource(
                        drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(width, height, 1, PixelFormat.R16G16B16A16_Float,
                            TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)))

                    : PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D( //msaa but no HDR, use RGB8 temp buffer
                        TextureDescription.New2D(width, height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                            TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)));

                ViewRenderTargets.Add(renderTarget);

                //Make sure we got a valid NOT MSAA Target
                if (currentRenderTarget == null)
                {
                    currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(width, height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                            TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                }

                //Handle Depth
                ViewDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(width, height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil, 1, GraphicsResourceUsage.Default, actualMSAALevel)));

            }
            else
            {
                if (PostEffects == null) //NO Post-Effects
                {
                    //Handle render targets
                    if (currentRenderTarget != null)
                    {
                        ViewRenderTargets.Add(currentRenderTarget);
                    }
                    else
                    {
                        currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                            TextureDescription.New2D(width, height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                                TextureFlags.ShaderResource | TextureFlags.RenderTarget)));

                        ViewRenderTargets.Add(currentRenderTarget);
                    }
                }
                else // WITH Post-Effects
                {
                    //Create a new output target if we don't have one
                    if (currentRenderTarget == null)
                    {
                        currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                            TextureDescription.New2D(width, height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                                TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                    }

                    ViewRenderTargets.Add(PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                            TextureDescription.New2D(width, height, 1, PixelFormat.R16G16B16A16_Float,
                                TextureFlags.ShaderResource | TextureFlags.RenderTarget))));
                }

                //Handle Depth
                if (currentDepthStencil == null)
                {
                    currentDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(width, height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil)));
                }

                ViewDepthStencil = currentDepthStencil;
            }

            ViewOutputTarget = currentRenderTarget;
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var viewport = drawContext.CommandList.Viewport;

            using (drawContext.PushRenderTargetsAndRestore())
            {
                if (VRSettings.Enabled && VRSettings.VRDevice != null)
                {
                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        var width = VRSettings.VRDevice.ActualRenderFrameSize.Width;
                        var height = VRSettings.VRDevice.ActualRenderFrameSize.Height;

                        //make sure we don't use any default targets!
                        drawContext.CommandList.SetRenderTarget(null, null);

                        PrepareRenderTargets(drawContext, width, height);

                        //draw per eye
                        using (drawContext.PushRenderTargetsAndRestore())
                        {
                            drawContext.CommandList.SetRenderTargets(ViewDepthStencil, ViewRenderTargets.Count, ViewRenderTargets.Items);

                            // Clear render target and depth stencil
                            Clear?.Draw(drawContext);

                            for (var i = 0; i < 2; i++)
                            {
                                using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                                {
                                    drawContext.CommandList.SetViewport(new Viewport(i == 0 ? 0 : width / 2, 0, width / 2, height));
                                    DrawView(context, drawContext);
                                }
                            }

                            VRSettings.VRDevice.Commit(drawContext.CommandList, ViewOutputTarget);
                        }
                    }

                    //draw mirror to backbuffer
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
                    PrepareRenderTargets(drawContext, (int)viewport.Width, (int)viewport.Height);

                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        drawContext.CommandList.SetRenderTargetsAndViewport(ViewDepthStencil, ViewRenderTargets.Count, ViewRenderTargets.Items);

                        // Clear render target and depth stencil
                        Clear?.Draw(drawContext);

                        DrawView(context, drawContext);
                    }
                }
            }
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
    }
}