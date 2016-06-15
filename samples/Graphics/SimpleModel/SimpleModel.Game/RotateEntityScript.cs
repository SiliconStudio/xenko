using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SimpleModel
{
    /// <summary>
    /// This script rotates around Oy the entity it is attached to.
    /// </summary>
    public class RotateEntityScript : AsyncScript
    {
        public override async Task Execute()
        {
            while (Game.IsRunning)
            {
                var time = (float)Game.UpdateTime.Total.TotalSeconds;
                Entity.Transform.Rotation = Quaternion.RotationY((0.3f * time) * (float)Math.PI);

                await Script.NextFrame();
            }
        }
    }
}
