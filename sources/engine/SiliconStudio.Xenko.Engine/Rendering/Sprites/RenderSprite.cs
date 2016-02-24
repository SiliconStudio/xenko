using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    [DefaultPipelinePlugin(typeof(SpritePipelinePlugin))]
    public class RenderSprite : RenderObject
    {
        public SpriteComponent SpriteComponent;

        public TransformComponent TransformComponent;
    }
}