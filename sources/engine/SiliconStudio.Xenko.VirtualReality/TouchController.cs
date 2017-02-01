using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class TouchController : IDisposable
    {
        public abstract Vector3 Position { get; }

        public abstract Quaternion Rotation { get; }

        public abstract Vector3 LinearVelocity { get; }

        public abstract Vector3 AngularVelocity { get; }

        public abstract DeviceState State { get; }

        public virtual void Update(GameTime time)
        {           
        }

        public abstract float Trigger { get; }

        public abstract float Grip { get; }

        public abstract bool IsPressedDown(TouchControllerButton button);

        public abstract bool IsPressed(TouchControllerButton button);

        public abstract bool IsPressReleased(TouchControllerButton button);

        public abstract bool IsTouchedDown(TouchControllerButton button);

        public abstract bool IsTouched(TouchControllerButton button);

        public abstract bool IsTouchReleased(TouchControllerButton button);

        public virtual void Dispose()
        {          
        }
    }
}
