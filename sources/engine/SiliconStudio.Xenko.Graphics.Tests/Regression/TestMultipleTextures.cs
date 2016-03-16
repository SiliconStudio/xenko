// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Graphics.Tests.Regression
{
    [TestFixture]
    public class TestMultipleTextures : GameTestBase
    {
        /// <summary>
        /// The texture.
        /// </summary>
        private Texture texture;

        public TestMultipleTextures()
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = 256;
            GraphicsDeviceManager.PreferredBackBufferHeight = 256;
        }

        /// <summary>
        /// Load the necessary contents for the tests.
        /// </summary>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            Console.WriteLine(@"Begin load.");
            texture = Content.Load<Texture>("small_uv");
            Console.WriteLine(@"End load.");
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.Draw(DrawTexture).TakeScreenshot();
            FrameGameSystem.Draw(DrawTexture).TakeScreenshot();
            FrameGameSystem.Draw(DrawTexture).TakeScreenshot();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawTexture();
        }

        public void DrawTexture()
        {
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            GraphicsContext.DrawTexture(texture, GraphicsDevice.SamplerStates.PointClamp);
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestMultipleTextures()
        {
            RunGameTest(new TestMultipleTextures());
        }
    }
}
