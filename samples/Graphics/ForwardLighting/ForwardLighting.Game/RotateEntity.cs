using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace ForwardLighting
{
    /// <summary>
    /// Script in charge of rotating the entity
    /// </summary>
    public class RotateEntity : AsyncScript
    {
        /// <summary>
        /// A reference to the stand
        /// </summary>
        public Entity Stand;

        /// <summary>
        /// A reference to the camera
        /// </summary>
        public Entity Camera;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float initialHeight;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private Quaternion initialRotation;

        public override async Task Execute()
        {
            var dragValue = 0f;

            if (!IsLiveReloading)
            {
                initialHeight = Entity.Transform.Position.Y;
                initialRotation = Entity.Transform.Rotation;
            }

            while (Game.IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                // rotate character
                var time = Game.UpdateTime.Total.TotalSeconds;

                Entity.Transform.Rotation = initialRotation * Quaternion.RotationAxis(Vector3.UnitY, (float)time);
                Entity.Transform.Position.Y = initialHeight + 0.1f * (float)Math.Sin(3 * time);

                // rotate camera
                dragValue = 0.95f * dragValue;
                if (Input.PointerEvents.Count > 0)
                    dragValue = Input.PointerEvents.Sum(x => x.DeltaPosition.X);

                var cameraRotation = Quaternion.RotationY((float)(2 * Math.PI * -dragValue));
                Camera.Transform.Position = Vector3.Transform(Camera.Transform.Position, cameraRotation);
                Camera.Transform.Rotation *= cameraRotation;
            }
        }
    }
}