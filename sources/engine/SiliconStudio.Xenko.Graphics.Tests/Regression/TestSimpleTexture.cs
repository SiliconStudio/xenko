// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Graphics.Tests.Regression
{
    [TestFixture]
    public class TestSimpleTexture : GraphicsTestBase
    {
        /// <summary>
        /// The texture.
        /// </summary>
        private Texture texture;
        
        public TestSimpleTexture()
        {
            CurrentVersion = 1;
            GraphicsDeviceManager.PreferredBackBufferWidth = 256;
            GraphicsDeviceManager.PreferredBackBufferHeight = 256;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            Console.WriteLine(@"Begin load.");
            texture = Asset.Load<Texture>("small_uv");
            Console.WriteLine(@"End load.");
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.Draw(DrawTexture).TakeScreenshot();
        }

        public void DrawTexture()
        {
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.DrawTexture(texture, GraphicsDevice.SamplerStates.PointClamp);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawTexture();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestSimpleTexture()
        {
            RunGameTest(new TestSimpleTexture());
        }
    }
}
