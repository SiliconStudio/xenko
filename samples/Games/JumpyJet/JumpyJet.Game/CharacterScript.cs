using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace JumpyJet
{
    /// <summary>
    /// CharacterScript is controlled by a user.
    /// The control is as follow, tapping a screen/clicking a mouse will make the agent jump up.
    /// </summary>
    public class CharacterScript : AsyncScript
    {
        private static readonly Vector3 Gravity = new Vector3(0, -1700, 0);
        private static readonly Vector3 StartPos = new Vector3(-100, 0, 0);
        private static readonly Vector3 StartVelocity = new Vector3(0, 700, 0);

        // Collider rectangles of CharacterScript
        private static readonly RectangleF BodyRectangle = new RectangleF(30, 19, 60, 34);
        private static readonly RectangleF HeadRectangle = new RectangleF(36, 63, 20, 20);

        private const int TopLimit = 568 - 200;
        private const float NormalVelocityY = 650;
        private const float VelocityAboveTopLimit = 200;
        private const int FlyingSpriteFrameIndex = 1;
        private const int FallingSpriteFrameIndex = 0;

        private Vector3 position;
        private Vector3 rotation;

        private bool isRunning;
        private float agentWidth;
        private float agentHeight;

        private Vector3 velocity;

        private RectangleF[] colliders;

        /// <summary>
        /// The position of the back of the character along the X axis.
        /// </summary>
        public float PositionBack => Entity.Transform.Position.X - agentWidth / 2f;

        public void Start()
        {
            // Get texture region from the sprite
            var textureRegion = Entity.Get<SpriteComponent>().SpriteProvider.GetSprite().Region;
            agentWidth = textureRegion.Width;
            agentHeight = textureRegion.Height;

            position = StartPos;
            velocity = StartVelocity;

            colliders = new[]
            {
                BodyRectangle,
                HeadRectangle
            };

            Reset();
        }

        /// <summary>
        /// Reset CharacterScript parameters: position, velocity and set state.
        /// </summary>
        public void Reset()
        {
            position.Y = 0;
            rotation.Z = 0f;
            UpdateTransformation();

            velocity = StartVelocity;
            isRunning = false;

            var provider = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            if (provider != null)
                provider.CurrentFrame = FallingSpriteFrameIndex;
        }

        /// <summary>
        /// Restart the character
        /// </summary>
        public void Restart()
        {
            Reset();
            isRunning = true;
        }

        /// <summary>
        /// Stop to update the character
        /// </summary>
        public void Stop()
        {
            isRunning = false;
        }

        /// <summary>
        /// Update the agent according to its states: {Idle, Alive, Die}
        /// </summary>
        public override async Task Execute()
        {
            Start();

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (!isRunning)
                    continue;

                var elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // apply impulse on the touch/space
                if (Input.IsKeyPressed(Keys.Space) || UserTappedScreen())
                    velocity.Y = position.Y > TopLimit ? VelocityAboveTopLimit : NormalVelocityY;

                // update position/velocity
                velocity += Gravity * elapsedTime;
                position += velocity * elapsedTime;

                // update animation and rotation value
                UpdateAgentAnimation();
                
                // update the position/rotation
                UpdateTransformation();
            }
        }

        private void UpdateTransformation()
        {
            Entity.Transform.Position= position;
            Entity.Transform.RotationEulerXYZ = rotation;
        }

        private bool UserTappedScreen()
        {
            return Input.PointerEvents.Any(pointerEvent => pointerEvent.State == PointerState.Down);
        }

        private void UpdateAgentAnimation()
        {
            var isFalling = velocity.Y < 0;
            var rotationSign = isFalling ? -1 : 1;

            // Set falling sprite frame
            var provider = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            if (provider != null)
                provider.CurrentFrame = isFalling? FallingSpriteFrameIndex: FlyingSpriteFrameIndex;

            // Rotate a sprite
            rotation.Z += rotationSign * MathUtil.Pi * 0.01f;
            if (rotationSign * rotation.Z > Math.PI / 10f)
                rotation.Z = rotationSign * MathUtil.Pi / 10f;
        }

        /// <summary>
        /// Check if the pipe set is colliding with the character or not.
        /// </summary>
        /// <returns></returns>
        public bool IsColliding(PipeSet nextPipeSet)
        {
            for (var i=0; i<colliders.Length; ++i)
            {
                var collider = colliders[i];
                collider.X = colliders[i].X + position.X - agentWidth / 2;
                collider.Y = colliders[i].Y + position.Y - agentHeight / 2;

                if (collider.Intersects(nextPipeSet.GetBottomPipeCollider()) ||
                    collider.Intersects(nextPipeSet.GetTopPipeCollider()))
                    return true;
            }

            return false;
        }
    }
}
