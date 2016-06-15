using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Graphics;

namespace SimpleSprite
{
    public class BallScript : SyncScript
    {
        private const int SphereWidth = 150;
        private const int SphereHeight = 150;
        private const int SphereCountPerRow = 6;
        private const int SphereTotalCount = 32;

        public Texture Sphere;

        private SpriteBatch spriteBatch;

        private Vector2 resolution;
        private Vector2 ballHalfSize;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private Vector2 ballPosition;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private Vector2 ballSpeed;

        private SceneDelegateRenderer delegateRenderer;

        public override void Start()
        {
            // create the ball sprite.
            var virtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 1);
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = virtualResolution };

            // Initialize ball's state related variables.
            resolution = new Vector2(virtualResolution.X, virtualResolution.Y);
            ballHalfSize = new Vector2(SphereWidth / 2f, SphereHeight / 2f);
            if (!IsLiveReloading)
            {
                ballPosition = resolution / 2;
                ballSpeed = new Vector2(600, -400);
            }

            // Add Graphics Layer
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Add(delegateRenderer = new SceneDelegateRenderer(RenderSpheres));
        }

        public override void Update()
        {
            ballPosition += ballSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            if (ballPosition.X < ballHalfSize.X)
            {
                ballPosition.X = ballHalfSize.X + (ballHalfSize.X - ballPosition.X);
                ballSpeed.X = -ballSpeed.X;
            }
            if (ballPosition.X > resolution.X - ballHalfSize.X)
            {
                ballPosition.X = 2 * (resolution.X - ballHalfSize.X) - ballPosition.X;
                ballSpeed.X = -ballSpeed.X;
            }
            if (ballPosition.Y < ballHalfSize.Y)
            {
                ballPosition.Y = ballHalfSize.Y + (ballHalfSize.Y - ballPosition.Y);
                ballSpeed.Y = -ballSpeed.Y;
            }
            if (ballPosition.Y > resolution.Y - ballHalfSize.Y)
            {
                ballPosition.Y = 2 * (resolution.Y - ballHalfSize.Y) - ballPosition.Y;
                ballSpeed.Y = -ballSpeed.Y;
            }
        }

        public override void Cancel()
        {
            // Remove the delegate renderer from the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Remove(delegateRenderer);

            // destroy graphic objects
            spriteBatch.Dispose();
        }

        private void RenderSpheres(RenderDrawContext renderContext, RenderFrame frame)
        {
            spriteBatch.Begin(renderContext.GraphicsContext);

            // draw the ball
            var time = (float)Game.DrawTime.Total.TotalSeconds;
            var rotation = time * (float)Math.PI * 0.5f;
            var sourceRectangle = GetSphereAnimation(1.25f * time);
            spriteBatch.Draw(Sphere, ballPosition, sourceRectangle, Color.White, rotation, ballHalfSize);

            spriteBatch.End();
        }

        /// <summary>
        /// Calculates the rectangle region from the original Sphere bitmap.
        /// </summary>
        /// <param name="time">The current time</param>
        /// <returns>The region from the sphere texture to display</returns>
        private static Rectangle GetSphereAnimation(float time)
        {
            var sphereIndex = MathUtil.Clamp((int)((time % 1.0f) * SphereTotalCount), 0, SphereTotalCount);

            var sphereX = sphereIndex % SphereCountPerRow;
            var sphereY = sphereIndex / SphereCountPerRow;
            return new Rectangle(sphereX * SphereWidth, sphereY * SphereHeight, SphereWidth, SphereHeight);
        }
    }
}
