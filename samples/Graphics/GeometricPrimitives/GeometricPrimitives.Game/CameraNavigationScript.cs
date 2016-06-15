using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Core.Mathematics;

namespace GeometricPrimitives
{
    /// <summary>
    /// Script that update the position of the camera.
    /// </summary>
    public class CameraNavigationScript : AsyncScript
    {
        private const int NumberOfPrimitives = 9;
        private const float SpaceBetweenEntities = 2;
        private const float AbsoluteMaxSpeed = 2f;
        private const float Friction = 0.9f;
        private const float MaximumCameraOffset = SpaceBetweenEntities * (NumberOfPrimitives / 2);
        private const float Frametime = 1 / 60.0f;
        private float timeToProcess;
        private float movingSpeed;
        private bool userIsTouchingScreen;

        public override async Task Execute()
        {
            while (Game.IsRunning)
            {
                var elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
                timeToProcess = Math.Max(timeToProcess + elapsedTime, 1.0f);

                // determine if the user is currently touching the screen.
                if (Input.PointerEvents.Count > 0)
                    userIsTouchingScreen = Input.PointerEvents.Last().State != PointerState.Up;

                // calculate the current speed of the camera
                if (userIsTouchingScreen)
                {
                    movingSpeed += -AbsoluteMaxSpeed * Input.PointerEvents.Sum(x => x.DeltaPosition.X);
                    if (Math.Abs(movingSpeed) > AbsoluteMaxSpeed)
                    {
                        movingSpeed = AbsoluteMaxSpeed * Math.Sign(movingSpeed);
                    }
                    timeToProcess = timeToProcess % Frametime;
                    UpdatePosition(movingSpeed, movingSpeed);
                }
                else
                {
                    while (timeToProcess >= Frametime)
                    {
                        var previousSpeed = movingSpeed;
                        timeToProcess -= Frametime;
                        movingSpeed = (float)(movingSpeed * Math.Pow(Friction, Frametime));
                        var snappingPosition = (float)Math.Round(Entity.Transform.Position.X / SpaceBetweenEntities) * SpaceBetweenEntities;
                        var snappingSpeed = 0.03f * (snappingPosition - Entity.Transform.Position.X);
                        if (Math.Abs(movingSpeed) < Math.Abs(snappingSpeed))
                            movingSpeed = snappingSpeed;

                        UpdatePosition(previousSpeed, movingSpeed);
                    }
                }

                // wait until next frame
                await Script.NextFrame();
            }
        }

        private void UpdatePosition(float previousSpeed, float currentSpeed)
        {
            // update the camera position
            var newPosition = Entity.Transform.Position.X + (previousSpeed + currentSpeed) * 0.5f * Frametime;
            Entity.Transform.Position.X = MathUtil.Clamp(newPosition, -MaximumCameraOffset, MaximumCameraOffset);
        }
    }
}
