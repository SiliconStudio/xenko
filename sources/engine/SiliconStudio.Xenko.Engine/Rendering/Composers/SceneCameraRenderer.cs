using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// Defines and sets a <see cref="RenderView"/> and set it up using <see cref="Camera"/> or current context camera.
    /// </summary>
    /// <remarks>
    /// Since it sets a view, it is usually not shareable for multiple rendering.
    /// </remarks>
    public partial class SceneCameraRenderer : SceneRendererBase
    {
        [DataMemberIgnore]
        public RenderView MainRenderView { get; } = new RenderView();

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        /// <userdoc>The camera to use to render the scene.</userdoc>
        public SceneCameraSlotIndex Camera { get; set; } = new SceneCameraSlotIndex(0);

        public ISceneRenderer Child { get; set; }

        protected override void CollectCore(RenderContext renderContext)
        {
            base.CollectCore(renderContext);

            renderContext.RenderSystem.Views.Add(MainRenderView);
            
            // Setup renderview
            MainRenderView.SceneInstance = renderContext.SceneInstance;
            var camera = renderContext.GetCameraFromSlot(Camera);
            UpdateCameraToRenderView(renderContext, MainRenderView, camera);

            using (renderContext.PushRenderViewAndRestore(MainRenderView))
            using (renderContext.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                CollectInner(renderContext);
            }
        }

        protected override void DrawCore(RenderDrawContext renderContext)
        {
            var oldRenderView = renderContext.RenderContext.RenderView;
            renderContext.RenderContext.RenderView = MainRenderView;

            var camera = renderContext.RenderContext.GetCameraFromSlot(Camera);
            using (renderContext.RenderContext.PushRenderViewAndRestore(MainRenderView))
            using (renderContext.RenderContext.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                DrawInner(renderContext);
            }

            renderContext.RenderContext.RenderView = oldRenderView;
        }

        protected virtual void CollectInner(RenderContext renderContext)
        {
            Child?.Collect(renderContext);
        }

        protected virtual void DrawInner(RenderDrawContext renderContext)
        {
            Child?.Draw(renderContext);
        }

        internal static void UpdateCameraToRenderView(RenderContext context, RenderView renderView, CameraComponent camera)
        {
            //// Copy scene camera renderer data
            //renderView.CullingMask = sceneCameraRenderer.CullingMask;
            //renderView.CullingMode = sceneCameraRenderer.CullingMode;
            //renderView.ViewSize = new Vector2(sceneCameraRenderer.ComputedViewport.Width, sceneCameraRenderer.ComputedViewport.Height);

            // TODO: Multiple viewports?
            var currentViewport = context.ViewportState.Viewport0;
            renderView.ViewSize = new Vector2(currentViewport.Width, currentViewport.Height);

            if (camera != null)
            {
                // Setup viewport size
                var aspectRatio = currentViewport.AspectRatio;

                // Update the aspect ratio
                if (camera.UseCustomAspectRatio)
                {
                    aspectRatio = camera.AspectRatio;
                }

                // If the aspect ratio is calculated automatically from the current viewport, update matrices here
                camera.Update(aspectRatio);

                // Copy camera data
                renderView.View = camera.ViewMatrix;
                renderView.Projection = camera.ProjectionMatrix;
                renderView.NearClipPlane = camera.NearClipPlane;
                renderView.FarClipPlane = camera.FarClipPlane;
                renderView.Frustum = camera.Frustum;

                Matrix.Multiply(ref renderView.View, ref renderView.Projection, out renderView.ViewProjection);
            }
        }
    }
}