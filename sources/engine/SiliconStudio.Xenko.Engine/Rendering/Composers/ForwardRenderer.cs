using System.Linq;
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
                MainRenderStage.Output = context.RenderOutputs.Peek();
            }
            if (TransparentRenderStage != null)
            {
                context.RenderView.RenderStages.Add(TransparentRenderStage);
                TransparentRenderStage.Output = context.RenderOutputs.Peek();
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

            if (MainRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, MainRenderStage);
            if (TransparentRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, TransparentRenderStage);
        }
    }
}