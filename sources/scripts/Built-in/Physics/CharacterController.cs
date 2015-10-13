using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Rendering;

namespace VolumeTrigger13
{
    public class CharacterController : SyncScript
    {
        private static readonly Vector3 ForwardVector = new Vector3(0, 0, -1);

        public float Speed { get; set; } = 0.25f;

        private Character character;

        public override void Start()
        {
            desiredYaw =
                yaw =
                    (float)
                        Math.Asin(2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.Y +
                                  2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.W);

            desiredPitch =
                pitch =
                    (float)
                        Math.Atan2(
                            2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.W -
                            2 * Entity.Transform.Rotation.Y * Entity.Transform.Rotation.Z,
                            1 - 2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.X -
                            2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.Z);

            if (!Platform.IsWindowsDesktop)
            {
                Input.ActivatedGestures.Add(new GestureConfigDrag());
            }

            Input.LockMousePosition(true);
            Game.IsMouseVisible = false;

            character = Entity.Get<PhysicsComponent>().Elements.First(x => x is CharacterElement).Character;
        }

        private Vector3 pointerVector;

        private float yaw, desiredYaw;
        private float pitch, desiredPitch;

        /// <summary>
        /// Gets or sets the rate at which orientation is adapted to a target value.
        /// </summary>
        /// <value>
        /// The adaptation rate.
        /// </value>
        public float RotationAdaptationSpeed { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the rotation speed of the camera (in radian/screen units)
        /// </summary>
        public float RotationSpeed { get; set; } = 2.355f;

        public override void Update()
        {
            var rotationDelta = Input.MouseDelta;
            foreach (var gestureEvent in Input.GestureEvents)
            {
                switch (gestureEvent.Type)
                {
                    case GestureType.Drag:
                        {
                            var drag = (GestureEventDrag)gestureEvent;
                            rotationDelta = drag.DeltaTranslation;
                        }
                        break;

                    case GestureType.Flick:
                        break;

                    case GestureType.LongPress:
                        break;

                    case GestureType.Composite:
                        break;

                    case GestureType.Tap:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Compute translation speed according to framerate and modifiers
            var translationSpeed = Speed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Take shortest path
            var deltaPitch = desiredPitch - pitch;
            var deltaYaw = (desiredYaw - yaw) % MathUtil.TwoPi;
            if (deltaYaw < 0)
                deltaYaw += MathUtil.TwoPi;
            if (deltaYaw > MathUtil.Pi)
                deltaYaw -= MathUtil.TwoPi;
            desiredYaw = yaw + deltaYaw;

            // Perform orientation transition
            var rotationAdaptation = (float)Game.UpdateTime.Elapsed.TotalSeconds * RotationAdaptationSpeed;
            yaw = Math.Abs(deltaYaw) < rotationAdaptation ? desiredYaw : yaw + rotationAdaptation * Math.Sign(deltaYaw);
            pitch = Math.Abs(deltaPitch) < rotationAdaptation ? desiredPitch : pitch + rotationAdaptation * Math.Sign(deltaPitch);

            desiredYaw = yaw -= 1.333f * rotationDelta.X * RotationSpeed; // we want to rotate faster Horizontally and Vertically
            desiredPitch = pitch = MathUtil.Clamp(pitch - rotationDelta.Y * RotationSpeed, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

            Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(yaw, pitch, 0);

            var move = new Vector3();

            var forward = Vector3.Transform(ForwardVector, Entity.Transform.Rotation);
            var projectedForward = Vector3.Normalize(new Vector3(forward.X, 0, forward.Z));

            if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            {
                move += -Vector3.UnitX;
            }
            if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            {
                move += Vector3.UnitX;
            }
            if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up))
            {
                move += projectedForward;
            }
            if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down))
            {
                move += -projectedForward;
            }

            //            if (Input.PointerEvents.Any())
            //            {
            //                var last = Input.PointerEvents.Last();
            //                if (last != null)
            //                {
            //                    switch (last.State)
            //                    {
            //                        case PointerState.Down:
            //                            if (last.Position.X < 0.5)
            //                            {
            //                                pointerVector = -Vector3.UnitX;
            //                            }
            //                            else
            //                            {
            //                                pointerVector = Vector3.UnitX;
            //                            }
            //                            break;
            //                        case PointerState.Up:
            //                        case PointerState.Out:
            //                        case PointerState.Cancel:
            //                            pointerVector = Vector3.Zero;
            //                            break;
            //                    }
            //                }
            //            }
            //
            //            if (pointerVector != Vector3.Zero)
            //            {
            //                move = pointerVector;
            //            }

            move *= translationSpeed;

            character.Move(move);
        }
    }
}
