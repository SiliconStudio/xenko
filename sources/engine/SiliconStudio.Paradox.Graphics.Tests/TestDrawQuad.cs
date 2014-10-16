// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    public class TestDrawQuad : TestGameBase
    {
        private RenderTarget offlineTarget;
        private bool firstSave;

        public TestDrawQuad()
        {
            CurrentVersion = 1;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawQuad).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // TODO DisposeBy is not working with device reset
            offlineTarget = Texture2D.New(GraphicsDevice, 512, 512, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this).ToRenderTarget().DisposeBy(this);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawQuad();

            if (firstSave)
            {
                SaveTexture(offlineTarget.Texture, "offlineTarget.png");
                SaveTexture(GraphicsDevice.BackBuffer.Texture, "backBuffer.png");
                firstSave = false;
            }
        }

        private void DrawQuad()
        {
            // Clears the screen 
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.LimeGreen);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);

            // Render to the backbuffer
            GraphicsDevice.SetRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);
            GraphicsDevice.DrawTexture(UVTexture);

            // -> Render to back by using intermediate texture
            //GraphicsDevice.SetRenderTarget(offlineTarget);
            //GraphicsDevice.DrawTexture(UVTexture);
            //
            //// Render to the backbuffer using offline texture
            //GraphicsDevice.SetRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);
            //GraphicsDevice.DrawTexture(offlineTarget.Texture);
        }

        public static void Main()
        {
            using (var game = new TestDrawQuad())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunDrawQuad()
        {
            RunGameTest(new TestDrawQuad());
        }
    }
}