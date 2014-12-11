// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class FbxEntityAnimTestGame : GraphicsTestBase
    {
        private AnimationComponent animationComponent;

        private PlayingAnimation walkAnimation;

        public FbxEntityAnimTestGame()
        {
            CurrentVersion = 2;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Setup render pipeline
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.Blue, RenderTarget = GraphicsDevice.BackBuffer, DepthStencil = GraphicsDevice.DepthStencilBuffer });
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "Default"));

            // Load asset
            var dudeEntity = Asset.Load<Entity>("AnimatedModel");
            animationComponent = dudeEntity.Get(AnimationComponent.Key);

            Entities.Add(dudeEntity);

            // Setup view
            RenderSystem.Pipeline.Parameters.Set(TransformationKeys.View, Matrix.LookAtRH(new Vector3(200, 0.0f, 100f), new Vector3(0f, 0f, 80.0f), Vector3.UnitZ));
            RenderSystem.Pipeline.Parameters.Set(TransformationKeys.Projection, Matrix.PerspectiveFovRH((float)Math.PI * 0.4f, 1.3f, 1.0f, 1000.0f));
            
            animationComponent.Play("Run");
            walkAnimation = animationComponent.PlayingAnimations[0];
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Update(()=> walkAnimation.IsPlaying = false).TakeScreenshot();
            FrameGameSystem.Update(() => SetAnimationTime(0.2)).TakeScreenshot();
            FrameGameSystem.Update(() => SetAnimationTime(0.9)).TakeScreenshot();
        }

        private void SetAnimationTime(double time)
        {
            walkAnimation.CurrentTime = TimeSpan.FromSeconds(time);
            walkAnimation.RemainingTime = walkAnimation.Clip.Duration - walkAnimation.CurrentTime;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyReleased(Keys.Space))
                walkAnimation.IsPlaying = !walkAnimation.IsPlaying;
        }

        [Test]
        public void RunFbxEntityAnimTest()
        {
            RunGameTest(new FbxEntityAnimTestGame());
        }

        public static void Main()
        {
            using (var testGame = new FbxEntityAnimTestGame())
                testGame.Run();
        }
    }
}