// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class SpriteTestGame : GraphicsTestBase
    {
        private SpriteGroup ballSprite1;

        private SpriteGroup ballSprite2;

        private Entity ball;

        private SpriteComponent spriteComponent;

        private Vector2 areaSize;

        private TransformationComponent transfoComponent;

        private Vector2 ballSpeed = new Vector2(-300, 200);

        private Entity foreground;

        private Entity background;

        private SpriteGroup groundSprites;

        public SpriteTestGame()
        {
            CurrentVersion = 4;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            // sets the virtual resolution
            areaSize = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);
            VirtualResolution = new Vector3(areaSize.X, areaSize.Y, 1000);

            // Creates the camera
            var cameraComponent = new CameraComponent { UseProjectionMatrix = true, ProjectionMatrix = SpriteBatch.CalculateDefaultProjection(new Vector3(areaSize, 200))};
            var camera = new Entity("Camera") { cameraComponent };

            // Create Main pass
            var mainPipeline = RenderSystem.Pipeline;
            mainPipeline.Renderers.Add(new CameraSetter(Services) { Camera = cameraComponent });
            mainPipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.LightBlue });
            mainPipeline.Renderers.Add(new SpriteRenderer(Services));
            
            // Load assets
            groundSprites = Asset.Load<SpriteGroup>("GroundSprite");
            ballSprite1 = Asset.Load<SpriteGroup>("BallSprite1");
            ballSprite2 = Asset.Load<SpriteGroup>("BallSprite2");
            ball = Asset.Load<Entity>("Ball");

            // create fore/background entities
            foreground = new Entity();
            background = new Entity();
            foreground.Add(new SpriteComponent { SpriteGroup = groundSprites, CurrentFrame = 1 });
            background.Add(new SpriteComponent { SpriteGroup = groundSprites, CurrentFrame = 0 });
            
            Entities.Add(camera);
            Entities.Add(ball);
            Entities.Add(foreground);
            Entities.Add(background);

            spriteComponent = ball.Get(SpriteComponent.Key);
            transfoComponent = ball.Get(TransformationComponent.Key);

            transfoComponent.Translation.X = areaSize.X / 2;
            transfoComponent.Translation.Y = areaSize.Y / 2;

            var backgroundSpriteRegion = background.Get(SpriteComponent.Key).SpriteGroup.Images[0].Region;
            var decorationScalings = new Vector3(areaSize.X / backgroundSpriteRegion.Width, areaSize.Y / backgroundSpriteRegion.Height, 1);
            background.Get(TransformationComponent.Key).Scaling = decorationScalings;
            foreground.Get(TransformationComponent.Key).Scaling = decorationScalings;
            background.Get(TransformationComponent.Key).Translation = new Vector3(0, 0, -1);
            foreground.Get(TransformationComponent.Key).Translation = new Vector3(0, areaSize.Y, 1);

            SpriteAnimation.Play(spriteComponent, 0, spriteComponent.SpriteGroup.Images.Count-1, AnimationRepeatMode.LoopInfinite, 30);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(() => SpriteAnimation.Stop(spriteComponent)).TakeScreenshot();
            FrameGameSystem.Update(() => SetFrameAndUpdateBall(20, 15)).TakeScreenshot();
            FrameGameSystem.Update(() => spriteComponent.SpriteGroup = ballSprite2).TakeScreenshot();
        }

        protected override void Update(GameTime time)
        {
            base.Update(time);

            if(!ScreenShotAutomationEnabled)
                UpdateBall((float)time.Elapsed.TotalSeconds);

            if (Input.IsKeyPressed(Keys.D1))
                spriteComponent.SpriteGroup = ballSprite1;
            if (Input.IsKeyPressed(Keys.D2))
                spriteComponent.SpriteGroup = ballSprite2;

            if (Input.IsKeyDown(Keys.Space))
                spriteComponent.CurrentFrame = 0;
        }

        private void SetFrameAndUpdateBall(int updateTimes, int frame)
        {
            spriteComponent.CurrentFrame = frame;

            for (int i = 0; i < updateTimes; i++)
                UpdateBall(0.033f);
        }

        private void UpdateBall(float totalSeconds)
        {
            const float RotationSpeed = (float)Math.PI / 2;

            var deltaRotation = RotationSpeed * totalSeconds;

            transfoComponent.RotationEulerXYZ = new Vector3(0,0, transfoComponent.RotationEulerXYZ.Z + deltaRotation);

            var sprite = spriteComponent.SpriteGroup.Images[spriteComponent.CurrentFrame];
            var spriteSize = new Vector2(sprite.Region.Width, sprite.Region.Height);

            for (int i = 0; i < 2; i++)
            {
                var nextPosition = transfoComponent.Translation[i] + totalSeconds * ballSpeed[i];

                var infBound = sprite.Center[i];
                var supBound = areaSize[i] - (spriteSize[i] - sprite.Center[i]);

                if (nextPosition > supBound || nextPosition<infBound)
                {
                    ballSpeed[i] = -ballSpeed[i];

                    if (nextPosition > supBound)
                        nextPosition = supBound - (nextPosition - supBound);
                    else
                        nextPosition = infBound + (infBound - nextPosition);
                }

                transfoComponent.Translation[i] = nextPosition;

            }
        }

        protected override void Destroy()
        {
            if (ball != null)
            {
                Asset.Unload(ball);
                Asset.Unload(ballSprite1);
                Asset.Unload(ballSprite2);
                Asset.Unload(groundSprites);
            }

            base.Destroy();
        }

        [Test]
        public void RunTestGame()
        {
            RunGameTest(new SpriteTestGame());
        }

        public static void Main()
        {
            using (var testGame = new SpriteTestGame())
                testGame.Run();
        }
    }
}