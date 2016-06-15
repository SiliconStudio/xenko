using NativeLinking.LibraryWrapper;
using SiliconStudio.Xenko.Engine;

namespace NativeLinking
{
    public class AnimateScript : SyncScript
    {
        private MyAnimationEngine animationEngine;

        public override void Start()
        {
            animationEngine = new MyAnimationEngine();
        }

        public override void Update()
        {
            var time = (float)Game.UpdateTime.Total.TotalSeconds;
            Entity.Transform.Position = animationEngine.GetCurrentPosition(time);
            Entity.Transform.RotationEulerXYZ = animationEngine.GetCurrentRotation(time);
        }
    }
}