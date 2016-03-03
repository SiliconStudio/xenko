using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    [DefaultPipelinePlugin(typeof(SpriteStudioPipelinePlugin))]
    public class RenderSpriteStudio : RenderObject
    {
        public SpriteStudioComponent SpriteStudioComponent;
        public TransformComponent TransformComponent;
        public AnimationComponent AnimationComponent;
        public SpriteStudioNodeState RootNode;
        public SpriteStudioSheet Sheet;
    }
}