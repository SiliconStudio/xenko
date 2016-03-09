using SiliconStudio.Core;
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
                SceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current),
                SceneCameraSlotCollection = Context.Tags.Get(SceneCameraSlotCollection.Current)
            };
            RenderSystem.Views.Add(MainRenderView);
        }

        public override void Collect(RenderContext context)
        {
            base.Collect(context);

            // Update view parameters
            MainRenderView.UpdateCameraToRenderView();

            // Collect render objects
            var visibilityGroup = context.Tags.Get(SceneInstance.CurrentVisibilityGroup);
            visibilityGroup.Collect(MainRenderView);
        }
    }
}