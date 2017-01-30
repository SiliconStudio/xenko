using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public class RenderTextureSceneRenderer : SceneRendererBase
    {
        public Texture RenderTexture { get; set; }

        public ISceneRenderer Child { get; set; }

        protected override void CollectCore(RenderContext renderContext)
        {
            base.CollectCore(renderContext);

            if (RenderTexture == null)
                return;

            using (renderContext.SaveRenderOutputAndRestore())
            using (renderContext.SaveViewportAndRestore())
            {
                renderContext.RenderOutput.RenderTargetFormat0 = RenderTexture.ViewFormat;
                renderContext.ViewportState.Viewport0 = new Viewport(0, 0, RenderTexture.ViewWidth, RenderTexture.ViewHeight);

                Child?.Collect(renderContext);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (RenderTexture == null)
                return;

            using (context.PushRenderTargetsAndRestore())
            {
                var depthBuffer = PushScopedResource(context.RenderContext.Allocator.GetTemporaryTexture2D(RenderTexture.ViewWidth, RenderTexture.ViewHeight, context.CommandList.DepthStencilBuffer.ViewFormat, TextureFlags.DepthStencil));
                context.CommandList.SetRenderTargetAndViewport(depthBuffer, RenderTexture);

                Child?.Draw(context);
            }
        }
    }
}