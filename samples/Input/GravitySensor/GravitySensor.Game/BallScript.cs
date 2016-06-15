using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine;

namespace GravitySensor
{
    public class BallScript : StartupScript
    {
        public override void Start()
        {
            var sprite = Entity.Get<SpriteComponent>();
            SpriteAnimation.Play(sprite, 0, sprite.SpriteProvider.SpritesCount - 1, AnimationRepeatMode.LoopInfinite, 2);
        }
    }
}