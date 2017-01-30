using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.VirtualReality;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    [DataContract]
    public class VrRendererSettings
    {
        public bool Enabled { get; set; }

        public List<HmdApi> RequiredApis { get; } = new List<HmdApi>();

        [DataMemberIgnore]
        internal RenderView[] RenderViews = { new RenderView(), new RenderView() };

        [DataMemberIgnore]
        internal Hmd Hmd;
    }

    /// <summary>
    /// Renders your game. It should use current <see cref="RenderContext.RenderView"/> and <see cref="CameraComponentRendererExtensions.GetCurrentCamera"/>.
    /// </summary>
    public partial class ForwardRenderer : SceneRendererBase, ISharedRenderer
    {
        private IShadowMapRenderer shadowMapRenderer;
        private Texture depthStencilROCached;

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
        public PostProcessingEffects PostEffects { get; set; }

        public VrRendererSettings VrSettings { get; set; } = new VrRendererSettings();

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

            if (VrSettings.Enabled)
            {
                try
                {
                    VrSettings.Hmd = Hmd.GetHmd(VrSettings.RequiredApis.ToArray());
                    VrSettings.Hmd.Initialize(GraphicsDevice, BindDepthAsResourceDuringTransparentRendering, false);
                }
                catch (NoHmdDeviceException)
                {
                    VrSettings.Enabled = false;
                    throw;
                }               
            }
        }

        protected override void CollectCore(RenderContext context)
        {
            var camera = context.GetCurrentCamera();

            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                if (PostEffects != null)
                {
                    context.RenderOutput = new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : context.RenderOutput.RenderTargetFormat0, PixelFormat.D24_UNorm_S8_UInt);
                }

                if (VrSettings.Enabled)
                {
                    VrSettings.Hmd.UpdateEyeParameters(ref camera.Entity.Transform.WorldMatrix);

                    for (var i = 0; i < 2; i++)
                    {
                        context.RenderSystem.Views.Add(VrSettings.RenderViews[i]);
                        VrSettings.RenderViews[i].SceneInstance = context.RenderView.SceneInstance;

                        //change camera params for eye
                        VrSettings.Hmd.ReadEyeParameters(i, camera.NearClipPlane, camera.FarClipPlane, out camera.ViewMatrix, out camera.ProjectionMatrix);
                        camera.UseCustomProjectionMatrix = true;
                        camera.UseCustomViewMatrix = true;
                        camera.Update();

                        //write params to view
                        SceneCameraRenderer.UpdateCameraToRenderView(context, VrSettings.RenderViews[i], camera);

                        //fix view size
                        VrSettings.RenderViews[i].ViewSize = new Vector2(VrSettings.Hmd.RenderFrameSize.Width / 2.0f , VrSettings.Hmd.RenderFrameSize.Height);

                        // Mark this view as requiring shadows
                        shadowMapRenderer?.RenderViewsWithShadows.Add(VrSettings.RenderViews[i]);

                        // Fill RenderStage formats and register render stages to main view
                        if (OpaqueRenderStage != null)
                        {
                            VrSettings.RenderViews[i].RenderStages.Add(OpaqueRenderStage);
                            OpaqueRenderStage.Output = context.RenderOutput;
                        }
                        if (TransparentRenderStage != null)
                        {
                            VrSettings.RenderViews[i].RenderStages.Add(TransparentRenderStage);
                            TransparentRenderStage.Output = context.RenderOutput;
                        } 
                    }
                }
                else
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

                if (ShadowMapRenderStage != null)
                    ShadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);
            }
        }

        private void DrawView(RenderContext context, RenderDrawContext drawContext, RenderView currentRenderView, Texture renderTarget, Texture currentDepthStencil, Texture currentRenderTarget)
        {
            var renderSystem = drawContext.RenderContext.RenderSystem;

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
                            if (shadowmapRenderView != null && shadowmapRenderView.RenderView == currentRenderView)
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
                    renderSystem.Draw(drawContext, currentRenderView, OpaqueRenderStage);

                // Draw [main view | transparent stage]
                if (TransparentRenderStage != null)
                {
                    // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
                    var depthStencilSRV = ResolveDepthAsSRV(drawContext);

                    renderSystem.Draw(drawContext, currentRenderView, TransparentRenderStage);

                    // Free the depth texture since we won't need it anymore
                    if (depthStencilSRV != null)
                    {
                        drawContext.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
                    }
                }

                // Run post effects
                if (PostEffects != null)
                {
                    // TODO: output in proper renderTarget location according to viewport
                    PostEffects.Draw(drawContext, renderTarget, currentDepthStencil, currentRenderTarget);
                }
            }
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var viewport = drawContext.CommandList.Viewport;

            var currentRenderTarget = drawContext.CommandList.RenderTarget;
            var currentDepthStencil = drawContext.CommandList.DepthStencilBuffer;

            if (VrSettings.Enabled)
            {
                // Allocate render targets
                var renderTarget = PostEffects != null ? 
                    PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(VrSettings.Hmd.RenderFrameSize.Width, VrSettings.Hmd.RenderFrameSize.Height, 1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget))) : 
                    VrSettings.Hmd.RenderFrame;

                //draw per eye
                using (drawContext.PushRenderTargetsAndRestore())
                {
                    drawContext.CommandList.SetRenderTarget(VrSettings.Hmd.RenderFrameDepthStencil, renderTarget);

                    // Clear render target and depth stencil
                    Clear?.Draw(drawContext);

                    for (var i = 0; i < 2; i++)
                    {
                        drawContext.CommandList.SetViewport(new Viewport(i == 0 ? 0 : VrSettings.Hmd.RenderFrameSize.Width / 2, 0, VrSettings.Hmd.RenderFrameSize.Width / 2, VrSettings.Hmd.RenderFrameSize.Height));                      
                        DrawView(context, drawContext, VrSettings.RenderViews[i], renderTarget, VrSettings.Hmd.RenderFrameDepthStencil, VrSettings.Hmd.RenderFrame);
                    }

                    VrSettings.Hmd.Commit(drawContext.CommandList);
                }
            }
            else
            {
                // Allocate render targets
                var renderTarget = PostEffects != null ? PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D((int)viewport.Width, (int)viewport.Height, 1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget))) : currentRenderTarget;

                using (drawContext.PushRenderTargetsAndRestore())
                {
                    if (PostEffects != null)
                        drawContext.CommandList.SetRenderTargetAndViewport(currentDepthStencil, renderTarget);

                    // Clear render target and depth stencil
                    Clear?.Draw(drawContext);

                    DrawView(context, drawContext, context.RenderView, renderTarget, currentDepthStencil, currentRenderTarget);
                }
            }          
        }

        private Texture ResolveDepthAsSRV(RenderDrawContext context)
        {
            if (!BindDepthAsResourceDuringTransparentRendering)
                return null;

            using (context.PushRenderTargetsAndRestore())
            {
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

                depthStencilROCached = context.Resolver.GetDepthStencilAsRenderTarget(context.CommandList.DepthStencilBuffer, depthStencilROCached);
                context.CommandList.SetRenderTargets(depthStencilROCached, context.CommandList.RenderTargetCount, context.CommandList.RenderTargets);

                return depthStencilSRV;
            }
        }
    }
}