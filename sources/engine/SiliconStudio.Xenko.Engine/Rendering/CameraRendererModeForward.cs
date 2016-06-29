// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A forward rendering mode.
    /// </summary>
    [DataContract("CameraRendererModeForward")]
    [Display("Forward")]
    public class CameraRendererModeForward : CameraRenderModeBase
    {
        private MeshPipelinePlugin meshPipelinePlugin;

        private Texture depthStencilROCached;

        // TODO This should be exposed to the user at some point
        private bool enableDepthAsShaderResource = true;

        [DataMemberIgnore] public RenderStage MainRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage TransparentRenderStage { get; set; }
        //[DataMemberIgnore] public RenderStage GBufferRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage ShadowMapRenderStage { get; set; }

        [DefaultValue(true)]
        [DataMemberIgnore]
        public bool Shadows { get; set; } = true;

        //public bool GBuffer { get; set; } = false;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Create mandatory render stages that don't exist yet
            if (MainRenderStage == null)
                MainRenderStage = RenderSystem.GetOrCreateRenderStage("Main", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            if (TransparentRenderStage == null)
                TransparentRenderStage = RenderSystem.GetOrCreateRenderStage("Transparent", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
           
            // Setup stage RenderOutputDescription (since we have the render frame bound)
            var output = Context.Tags.Get(RenderFrame.Current);
            MainRenderStage.Output = output.GetRenderOutputDescription();
            TransparentRenderStage.Output = output.GetRenderOutputDescription();

            // Setup proper sort modes
            MainRenderStage.SortMode = new StateChangeSortMode();
            TransparentRenderStage.SortMode = new BackToFrontSortMode();

            // Create optional render stages that don't exist yet
            //if (GBufferRenderStage == null)
            //    GBufferRenderStage = RenderSystem.GetOrCreateRenderStage("GBuffer", "GBuffer", new RenderOutputDescription(PixelFormat.R11G11B10_Float, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            if (Shadows)
            {
                if (ShadowMapRenderStage == null)
                {
                    ShadowMapRenderStage = RenderSystem.GetOrCreateRenderStage("ShadowMapCaster", "ShadowMapCaster", new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float));
                    ShadowMapRenderStage.SortMode = new FrontToBackSortMode();
                }

                // Mark this view as requiring shadows
                var shadowPipelinePlugin = RenderSystem.PipelinePlugins.InstantiatePlugin<ShadowPipelinePlugin>();
                shadowPipelinePlugin.RenderViewsWithShadows.Add(MainRenderView);

                meshPipelinePlugin = RenderSystem.PipelinePlugins.InstantiatePlugin<MeshPipelinePlugin>();
            }

            MainRenderView.RenderStages.Add(MainRenderStage);
            MainRenderView.RenderStages.Add(TransparentRenderStage);
        }

        protected override void Unload()
        {
            // This view don't need shadows anymore
            var shadowPipelinePlugin = RenderSystem.PipelinePlugins.GetPlugin<ShadowPipelinePlugin>();
            shadowPipelinePlugin?.RenderViewsWithShadows.Remove(MainRenderView);

            base.Unload();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var currentViewport = context.CommandList.Viewport;

            // GBuffer
            //if (GBuffer)
            //{
            //    context.PushRenderTargets();
            //
            //    var gbuffer = PushScopedResource(Context.Allocator.GetTemporaryTexture2D((int)currentViewport.Width, (int)currentViewport.Height, GBufferRenderStage.Output.RenderTargetFormat0));
            //    context.CommandList.Clear(gbuffer, Color4.Black);
            //    context.CommandList.SetDepthAndRenderTarget(context.CommandList.DepthStencilBuffer, gbuffer);
            //    RenderSystem.Draw(context, mainRenderView, GBufferRenderStage);
            //
            //    context.PopRenderTargets();
            //}

            // Shadow maps
            var shadowMapRenderer = meshPipelinePlugin?.ForwardLightingRenderFeature?.ShadowMapRenderer;
            if (Shadows && shadowMapRenderer != null)
            {
                // Clear atlases
                shadowMapRenderer.PrepareAtlasAsRenderTargets(context.CommandList);

                context.PushRenderTargets();

                // Draw all shadow views generated for the current view
                foreach (var renderView in RenderSystem.Views)
                {
                    var shadowmapRenderView = renderView as ShadowMapRenderView;
                    if (shadowmapRenderView != null && shadowmapRenderView.RenderView == MainRenderView)
                    {
                        var shadowMapRectangle = shadowmapRenderView.Rectangle;
                        shadowmapRenderView.ShadowMapTexture.Atlas.RenderFrame.Activate(context);
                        shadowmapRenderView.ShadowMapTexture.Atlas.MarkClearNeeded();
                        context.CommandList.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                        RenderSystem.Draw(context, shadowmapRenderView, ShadowMapRenderStage);
                    }
                }

                context.PopRenderTargets();

                shadowMapRenderer.PrepareAtlasAsShaderResourceViews(context.CommandList);
            }

            // Draw [main view | main stage]
            RenderSystem.Draw(context, MainRenderView, MainRenderStage);

            // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
            Texture depthStencilSRV = ResolveDepthAsSRV(context);

            // Draw [main view | transparent stage]
            RenderSystem.Draw(context, MainRenderView, TransparentRenderStage);

            // Free the depth texture since we won't need it anymore
            if (depthStencilSRV != null)
            {
                context.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
            }
        }

        private Texture ResolveDepthAsSRV(RenderDrawContext context)
        {
            if (!enableDepthAsShaderResource)
                return null;

            context.PushRenderTargets();

            var currentRenderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            var depthStencilSRV = context.Resolver.ResolveDepthStencil(currentRenderFrame.DepthStencil);

            foreach (var renderFeature in RenderSystem.RenderFeatures)
            {
                if (!(renderFeature is RootEffectRenderFeature))
                    continue;

                var depthLogicalKey = ((RootEffectRenderFeature)renderFeature).CreateViewLogicalGroup("Depth");
                var viewFeature = MainRenderView.Features[renderFeature.Index];

                // Copy ViewProjection to PerFrame cbuffer
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    var resourceGroup = viewLayout.Entries[MainRenderView.Index].Resources;

                    var depthLogicalGroup = viewLayout.GetLogicalGroup(depthLogicalKey);
                    if (depthLogicalGroup.Hash == ObjectId.Empty)
                        continue;

                    // Might want to use ProcessLogicalGroup if more than 1 Recource
                    resourceGroup.DescriptorSet.SetShaderResourceView(depthLogicalGroup.DescriptorSlotStart, depthStencilSRV);
                }
            }

            depthStencilROCached = context.Resolver.GetDepthStencilAsRenderTarget(currentRenderFrame.DepthStencil, depthStencilROCached);
            currentRenderFrame.Activate(context, depthStencilROCached);

            context.PopRenderTargets();

            return depthStencilSRV;
        }
    }
}
