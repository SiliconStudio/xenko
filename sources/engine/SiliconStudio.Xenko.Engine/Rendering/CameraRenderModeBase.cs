using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class CameraRenderModeBase : CameraRendererMode
    {
        [DataMemberIgnore]
        public RenderSystem RenderSystem { get; private set; }

        protected RenderView MainRenderView { get; private set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem = Context.Tags.Get(SceneInstance.CurrentRenderSystem);

            var sceneInstance = SceneInstance.GetCurrent(Context);

            // Describe views
            MainRenderView = new RenderView
            {
                SceneInstance = sceneInstance,
            };
            RenderSystem.Views.Add(MainRenderView);
        }

        public override void Collect(RenderContext context)
        {
            base.Collect(context);

            // Update view parameters
            UpdateCameraToRenderView(context, MainRenderView);

            // Collect render objects
            var visibilityGroup = context.Tags.Get(SceneInstance.CurrentVisibilityGroup);
            visibilityGroup.Collect(MainRenderView);
        }

        public static void UpdateCameraToRenderView(RenderContext context, RenderView renderView)
        {
            var camera = context.Tags.Get(CameraComponentRendererExtensions.Current);
            var sceneCameraRenderer = context.Tags.Get(SceneCameraRenderer.Current);

            if (sceneCameraRenderer == null)
                return;

            // Copy scene camera renderer data
            renderView.CullingMask = sceneCameraRenderer.CullingMask;
            renderView.CullingMode = sceneCameraRenderer.CullingMode;
            renderView.ViewSize = new Vector2(sceneCameraRenderer.ComputedViewport.Width, sceneCameraRenderer.ComputedViewport.Height);

            if (camera != null)
            {
                // Setup viewport size
                var currentViewport = sceneCameraRenderer.ComputedViewport;
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
