using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace GeometricPrimitives
{
    /// <summary>
    /// This script performs a rotation of the entity along the Oy axis.
    /// </summary>
    public class RotateEntityScript : AsyncScript
    {
        public override async Task Execute()
        {
            while (Game.IsRunning)
            {
                var time = (float)Game.UpdateTime.Total.TotalSeconds;
                Entity.Transform.Rotation = Quaternion.RotationY((0.3f * time % 2f) * (float)Math.PI);

                await Script.NextFrame();
            }
        }

    }
}
