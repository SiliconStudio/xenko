using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract("CameraRendererModeForward")]
    [Display("Forward")]
    public class CameraRendererModeForward : CameraRenderModeBase
    {
        private ForwardLightingRenderFeature forwardLightingRenderFeasture;

        [DataMemberIgnore] public RenderStage MainRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage TransparentRenderStage { get; set; }
        //[DataMemberIgnore] public RenderStage GBufferRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage ShadowMapRenderStage { get; set; }

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

            // Setup proper sort modes
            MainRenderStage.SortMode = new StateChangeSortMode();
            TransparentRenderStage.SortMode = new FrontToBackSortMode();

            // Create optional render stages that don't exist yet
            //if (GBufferRenderStage == null)
            //    GBufferRenderStage = RenderSystem.GetOrCreateRenderStage("GBuffer", "GBuffer", new RenderOutputDescription(PixelFormat.R11G11B10_Float, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
            if (Shadows && ShadowMapRenderStage == null)
            {
                ShadowMapRenderStage = RenderSystem.GetOrCreateRenderStage("ShadowMapCaster", "ShadowMapCaster", new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float));
                ShadowMapRenderStage.SortMode = new FrontToBackSortMode();
            }

            MainRenderView.RenderStages.Add(MainRenderStage);
            MainRenderView.RenderStages.Add(TransparentRenderStage);
        }

        public override void BeforeExtract(RenderContext context)
        {
            base.BeforeExtract(context);

            // Make sure required plugins are instantiated
            // TODO GRAPHICS REFACTOR this system is temporary; probably want to make it more descriptive
            if (Shadows && RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have shadows, let's enable ShadowMeshPipelinePlugin
                RenderSystem.GetPipelinePlugin<ShadowMeshPipelinePlugin>(true);
            }

            // TODO GRAPHICS REFACTOR: Make this non-explicit?
            RenderSystem.forwardLightingRenderFeature?.BeforeExtract();
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
            var shadowMapRenderer = RenderSystem.forwardLightingRenderFeature?.ShadowMapRenderer;
            if (Shadows && shadowMapRenderer != null)
            {
                // Clear atlases
                shadowMapRenderer.ClearAtlasRenderTargets(context.CommandList);

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
            }

            RenderSystem.Draw(context, MainRenderView, MainRenderStage);
            RenderSystem.Draw(context, MainRenderView, TransparentRenderStage);
        }
    }
}