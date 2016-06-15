using SiliconStudio.Xenko.Engine;

namespace SpaceEscape
{
    /// <summary>
    /// Plays the idle animation of the entity if any
    /// </summary>
    public class PlayAnimationScript : StartupScript
    {
        public string AnimationName;

        public override void Start()
        {
            var animation = Entity.Get<AnimationComponent>();
            if (animation != null)
                animation.Play(AnimationName);
        }
    }
}