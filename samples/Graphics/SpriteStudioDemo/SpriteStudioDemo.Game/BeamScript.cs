using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SpriteStudioDemo
{
    public class BeamScript : AsyncScript
    {
        private const float maxWidthX = 8f + 2f;
        private const float minWidthX = -8f - 2f;

        private bool dead;

        public void Die()
        {
            dead = true;
        }

        public override async Task Execute()
        {
            while(Game.IsRunning)
            {
                await Script.NextFrame();

                if ((Entity.Transform.Position.X <= minWidthX) || (Entity.Transform.Position.X >= maxWidthX) || dead)
                {
                    SceneSystem.SceneInstance.Scene.Entities.Remove(Entity);
                    return;
                }
            }
        }
    }
}
