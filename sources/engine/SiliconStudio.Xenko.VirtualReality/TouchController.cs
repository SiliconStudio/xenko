using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class TouchController : GameSystem
    {
        public static TouchController GetTouchController(Game game, TouchControllerHand hand, TouchControllerApi[] preferredTouchControllerApis)
        {
            foreach (var preferredTouchControllerApi in preferredTouchControllerApis)
            {
                switch (preferredTouchControllerApi)
                {
                    case TouchControllerApi.Oculus:
                        break;
                    case TouchControllerApi.OpenVr:
                        return new OpenVrTouchController(game, hand);
                    case TouchControllerApi.Google:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null;
        }

        public Matrix Pose { get; protected set; }

        public Vector3 LinearVelocity { get; protected set; }

        public Vector3 AngularVelocity { get; protected set; }

        public abstract DeviceState State { get; }

        protected TouchController(IServiceRegistry registry) : base(registry)
        {
            Game.GameSystems.Add(this);
            UpdateOrder = -10000;
        }

        protected override void Destroy()
        {
            Game.GameSystems.Remove(this);
            base.Destroy();
        }

        public abstract bool IsButtonDown(TouchControllerButton button);

        public abstract bool IsButtonPressed(TouchControllerButton button);

        public abstract bool IsButtonReleased(TouchControllerButton button);
    }
}
