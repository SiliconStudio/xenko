using SiliconStudio.Xenko.Engine;

namespace RenderSceneToTexture
{
    /// <summary>
    /// Play the idle animation of the entity.
    /// </summary>
    public class PlayAnimation : StartupScript
    {
        public override void Start()
        {
            base.Start();

            Entity.Get<AnimationComponent>().Play("Idle");
        }
    }
}