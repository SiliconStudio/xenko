using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class CameraRenderModeBase : CameraRendererMode
    {
        [DataMemberIgnore]
        public NextGenRenderSystem RenderSystem { get; private set; }

        protected RenderView MainRenderView { get; private set; }

        public override string ModelEffect { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem = Context.Tags.Get(SceneInstance.CurrentRenderSystem);

            var sceneInstance = SceneInstance.GetCurrent(Context);

            // Describe views
            MainRenderView = new RenderView();
            MainRenderView.SceneInstance = sceneInstance;
            MainRenderView.SceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current);
            MainRenderView.SceneCameraSlotCollection = Context.Tags.Get(SceneCameraSlotCollection.Current);
            RenderSystem.Views.Add(MainRenderView);
        }
    }
}