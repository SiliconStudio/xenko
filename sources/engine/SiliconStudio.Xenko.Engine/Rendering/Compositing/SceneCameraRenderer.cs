using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Defines and sets a <see cref="Rendering.RenderView"/> and set it up using <see cref="Camera"/> or current context camera.
    /// </summary>
    /// <remarks>
    /// Since it sets a view, it is usually not shareable for multiple rendering.
    /// </remarks>
    [Display("Render Camera")]
    public partial class SceneCameraRenderer : SceneRendererBase
    {
        [DataMemberIgnore]
        public RenderView RenderView { get; } = new RenderView();

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        /// <userdoc>The camera to use to render the scene.</userdoc>
        public SceneCameraSlot Camera { get; set; }

        public ISceneRenderer Child { get; set; }

        protected override void CollectCore(RenderContext context)
        {
            base.CollectCore(context);

            // Find camera
            var camera = ResolveCamera(context);
            if (camera == null)
                return;

            // Setup render view
            context.RenderSystem.Views.Add(RenderView);
            RenderView.SceneInstance = context.SceneInstance;
            UpdateCameraToRenderView(context, RenderView, camera);

            using (context.PushRenderViewAndRestore(RenderView))
            using (context.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                CollectInner(context);
            }
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            // Find camera
            var camera = ResolveCamera(context);
            if (camera == null)
                return;

            using (context.PushRenderViewAndRestore(RenderView))
            using (context.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                DrawInner(drawContext);
            }
        }

        protected virtual CameraComponent ResolveCamera(RenderContext renderContext)
        {
            var camera = Camera?.Camera;
            if (camera == null) throw new InvalidOperationException($"A {nameof(SceneCameraRenderer)} in use has not camera set.");
            return camera;
        }

        protected virtual void CollectInner(RenderContext renderContext)
        {
            Child?.Collect(renderContext);
        }

        protected virtual void DrawInner(RenderDrawContext renderContext)
        {
            Child?.Draw(renderContext);
        }

        public static void UpdateCameraToRenderView(RenderContext context, RenderView renderView, CameraComponent camera)
        {
            if (context == null || renderView == null)
                return;

            // TODO: Multiple viewports?
            var currentViewport = context.ViewportState.Viewport0;
            renderView.ViewSize = new Vector2(currentViewport.Width, currentViewport.Height);

            if (camera == null)
                return;

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

            // Enable frustum culling
            renderView.CullingMode = CameraCullingMode.Frustum;

            Matrix.Multiply(ref renderView.View, ref renderView.Projection, out renderView.ViewProjection);
        }
    }
}