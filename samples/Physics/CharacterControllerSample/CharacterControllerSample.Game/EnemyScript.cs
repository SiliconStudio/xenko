using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace CharacterControllerSample
{
    /// <summary>
    /// This simple script will start the sprite idle animation
    /// </summary>
    public class EnemyScript : StartupScript
    {
        public override void Start()
        {
            var spriteComponent = Entity.Get<SpriteComponent>();
            var sheet = ((SpriteFromSheet)spriteComponent.SpriteProvider).Sheet;
            SpriteAnimation.Play(spriteComponent, sheet.FindImageIndex("active0"), sheet.FindImageIndex("active1"), AnimationRepeatMode.LoopInfinite, 2);
        }
    }
}