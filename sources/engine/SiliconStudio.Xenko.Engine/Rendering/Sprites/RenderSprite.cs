using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    [PipelineRenderer(typeof(SpritePipelineRenderer))]
    public class RenderSprite : RenderObject
    {
        public SpriteComponent SpriteComponent;

        public TransformComponent TransformComponent;
    }
}