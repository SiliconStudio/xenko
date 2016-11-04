using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace AnimatedModel
{
    /// <summary>
    /// Set the source of the sprite component.
    /// </summary>
    public class SetSpriteSource : StartupScript
    {
        public RenderFrame RenderFrame;

        public override void Start()
        {
            base.Start();

            var spriteComponent = Entity.Get<SpriteComponent>();
            var spriteSource = RenderFrame.RenderTargets[0];
			var spriteFromTexture = spriteComponent.SpriteProvider as SpriteFromTexture;
			if(spriteFromTexture != null)
				spriteFromTexture.Texture = spriteSource;
        }
    }
}