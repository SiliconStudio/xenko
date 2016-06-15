using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace RibbonParticles
{
    /// <summary>
    /// Rotate the entity when user slide its finger on the screen.
    /// </summary>
    public class RotateEntity : AsyncScript
    {
        public override async Task Execute()
        {
            var rotationSpeed = 0f;

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (Input.PointerEvents.Any())
                    rotationSpeed = 200f * Input.PointerEvents.Sum(x => x.DeltaPosition.X);

                rotationSpeed *= 0.93f;
                var elapsedTime = (float) Game.UpdateTime.Elapsed.TotalSeconds;
                Entity.Transform.Rotation *= Quaternion.RotationY(rotationSpeed * elapsedTime);
            }
        }
    }
}