namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpriteTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            var renderSprite = (RenderSprite)renderObject;

            var renderStage = renderSprite.SpriteComponent.CurrentSprite.IsTransparent ? TransparentRenderStage : MainRenderStage;
            renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(EffectName);
        }
    }
}