using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;

namespace MaterialSample
{
    public class CameraScript : AsyncScript
    {
        private readonly Vector3 upVector = new Vector3(0, 1, 0);
        private readonly Vector3 forwardVector = new Vector3(0, 0, -1);

        private float yaw = (float)(Math.PI * 0.25f);
        private float pitch = -(float)(Math.PI * 0.25f);
        private Vector3 position = new Vector3(10, 10, 10);

        private float moveSpeed;
        private float rotationSpeed;

        private CameraComponent CameraComponent { get { return Entity.Get<CameraComponent>(); } }

        /// <summary>
        /// Create a new instance of scene camera.
        /// </summary>
        public CameraScript()
        {
            moveSpeed = 2.5f;
            rotationSpeed = MathUtil.Pi / 2f;
        }

        public override async Task Execute()
        {
            Game.Window.ClientSizeChanged += OnWindowSizeChanged;

            CameraComponent.UseCustomViewMatrix = true;
            OnWindowSizeChanged(this, EventArgs.Empty);
            
            Input.ActivatedGestures.Add(new GestureConfigComposite());
            Input.ActivatedGestures.Add(new GestureConfigDrag());

            while (true)
            {
                // Update rotation according to mouse deltas
                var dragEvent = (GestureEventDrag)Input.GestureEvents.FirstOrDefault(e => e.Type == GestureType.Drag);
                if(dragEvent != null)
                {
                    var direction = Input.HasMouse? -1f: 1f;
                    yaw += direction * dragEvent.DeltaTranslation.X * rotationSpeed;
                    pitch += direction * dragEvent.DeltaTranslation.Y * rotationSpeed;
                }

                // Compute translation speed according to framerate and modifiers
                float translationSpeed = moveSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds;
                if (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift))
                    translationSpeed *= 10;

                // Compute base vectors for camera movement
                var rotation = Quaternion.RotationYawPitchRoll(yaw, pitch, 0);

                var forward = Vector3.Transform(forwardVector, rotation);
                var up = Vector3.Transform(upVector, rotation);
                var right = Vector3.Cross(forward, up);

                // Update position according to movement input
                if (!IsModifierDown(false))
                {
                    if (Input.IsKeyDown(Keys.A))
                        position += -right * translationSpeed;
                    if (Input.IsKeyDown(Keys.D))
                        position += right * translationSpeed;
                    if (Input.IsKeyDown(Keys.S))
                        position += -forward * translationSpeed;
                    if (Input.IsKeyDown(Keys.W))
                        position += forward * translationSpeed;
                    if (Input.IsKeyDown(Keys.E))
                        position += -up * translationSpeed;
                    if (Input.IsKeyDown(Keys.Q))
                        position += up * translationSpeed;
                }
                var compositeEvent = (GestureEventComposite)Input.GestureEvents.FirstOrDefault(e=>e.Type == GestureType.Composite);
                if (compositeEvent != null)
                {
                    position -= 2.5f * right * compositeEvent.DeltaTranslation.X;
                    position += 2.5f * up * compositeEvent.DeltaTranslation.Y;
                    position += forward * compositeEvent.DeltaScale;
                }

                // Update the camera view matrix 
                UpdateViewMatrix();

                await Script.NextFrame();
            }
        }

        private bool IsModifierDown(bool includeShift)
        {
            return (includeShift && (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift)))
                   || Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl)
                   || Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);
        }

        private void UpdateViewMatrix()
        {
            var rotation = Quaternion.Invert(Quaternion.RotationYawPitchRoll(yaw, pitch, 0));
            var viewMatrix = Matrix.Translation(-position) * Matrix.RotationQuaternion(rotation);
            CameraComponent.ViewMatrix = viewMatrix;
        }

        /// <summary>
        /// Called when the size of the windows changed.
        /// </summary>
        private void OnWindowSizeChanged(object sender, EventArgs eventArgs)
        {
            CameraComponent.AspectRatio = GraphicsDevice.Presenter.BackBuffer.Width / (float)GraphicsDevice.Presenter.BackBuffer.Height;
        }
    }
}