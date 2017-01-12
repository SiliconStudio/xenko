using System.Linq;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Renders object with proper opaque/transparent separation using Forward or Forward+ rendering.
    /// </summary>
    /// <remarks>
    /// Later, we might split this class to be able to reuse "shared" shadow maps.
    /// </remarks>
    public partial class ForwardRenderer : SceneRendererBase, ISharedRenderer
    {
        private IShadowMapRenderer shadowMapRenderer;
        private Texture depthStencilROCached;

        // TODO This should be exposed to the user at some point
        private bool enableDepthAsShaderResource = true;

        public RenderStage MainRenderStage;
        public RenderStage TransparentRenderStage;

        public RenderStage ShadowMapRenderStage;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            shadowMapRenderer = Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;
        }

        protected override void CollectCore(RenderContext context)
        {
            // Mark this view as requiring shadows
            shadowMapRenderer?.RenderViewsWithShadows.Add(context.RenderView);

            // Fill RenderStage formats and register render stages to main view
            if (MainRenderStage != null)
            {
                context.RenderView.RenderStages.Add(MainRenderStage);
                MainRenderStage.Output = context.RenderOutput;
            }
            if (TransparentRenderStage != null)
            {
                context.RenderView.RenderStages.Add(TransparentRenderStage);
                TransparentRenderStage.Output = context.RenderOutput;
            }

            if (ShadowMapRenderStage != null)
                ShadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderSystem = context.RenderContext.RenderSystem;

            // Render Shadow maps
            if (ShadowMapRenderStage != null && shadowMapRenderer != null)
            {
                // Clear atlases
                shadowMapRenderer.PrepareAtlasAsRenderTargets(context.CommandList);

                context.PushRenderTargets();

                // Draw all shadow views generated for the current view
                foreach (var renderView in renderSystem.Views)
                {
                    var shadowmapRenderView = renderView as ShadowMapRenderView;
                    if (shadowmapRenderView != null && shadowmapRenderView.RenderView == context.RenderContext.RenderView)
                    {
                        var shadowMapRectangle = shadowmapRenderView.Rectangle;
                        shadowmapRenderView.ShadowMapTexture.Atlas.RenderFrame.Activate(context);
                        shadowmapRenderView.ShadowMapTexture.Atlas.MarkClearNeeded();
                        context.CommandList.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                        renderSystem.Draw(context, shadowmapRenderView, ShadowMapRenderStage);
                    }
                }

                context.PopRenderTargets();

                shadowMapRenderer.PrepareAtlasAsShaderResourceViews(context.CommandList);
            }

            // Draw [main view | main stage]
            if (MainRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, MainRenderStage);

            // Draw [main view | transparent stage]
            if (TransparentRenderStage != null)
            {
                // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
                var depthStencilSRV = ResolveDepthAsSRV(context);

                renderSystem.Draw(context, context.RenderContext.RenderView, TransparentRenderStage);

                // Free the depth texture since we won't need it anymore
                if (depthStencilSRV != null)
                {
                    context.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
                }
            }
        }

        private Texture ResolveDepthAsSRV(RenderDrawContext context)
        {
            if (!enableDepthAsShaderResource)
                return null;

            context.PushRenderTargets();

            var currentRenderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            var depthStencilSRV = context.Resolver.ResolveDepthStencil(currentRenderFrame.DepthStencil);

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

            depthStencilROCached = context.Resolver.GetDepthStencilAsRenderTarget(currentRenderFrame.DepthStencil, depthStencilROCached);
            currentRenderFrame.Activate(context, depthStencilROCached);

            context.PopRenderTargets();

            return depthStencilSRV;
        }

    }
}