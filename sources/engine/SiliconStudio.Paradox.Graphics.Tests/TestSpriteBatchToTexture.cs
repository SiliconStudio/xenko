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
    class TestSpriteBatchToTexture : TestGameBase
    {
        private const int OfflineWidth = 512;
        private const int OfflineHeight = 512;

        private Texture offlineTarget;
        private Texture depthBuffer;

        private SpriteBatch spriteBatch;

        private Texture uv;
        private SpriteGroup spheres;

        private SpriteFont arial;

        private int width;
        private int height;

        public TestSpriteBatchToTexture()
        {
            CurrentVersion = 2;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(RenderToTexture).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            offlineTarget = Texture.New2D(GraphicsDevice, OfflineWidth, OfflineHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this);
            depthBuffer = Texture.New2D(GraphicsDevice, OfflineWidth, OfflineHeight, PixelFormat.D16_UNorm, TextureFlags.DepthStencil).DisposeBy(this);

            uv = Asset.Load<Texture>("uv");
            spheres = Asset.Load<SpriteGroup>("SpriteSphere");

            arial = Asset.Load<SpriteFont>("StaticFonts/Arial13");

            width = GraphicsDevice.BackBuffer.ViewWidth;
            height = GraphicsDevice.BackBuffer.ViewHeight;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!ScreenShotAutomationEnabled)
                RenderToTexture();
        }

        private void RenderToTexture()
        {
            // render into texture
            GraphicsDevice.Clear(offlineTarget, new Color4(0,0,0,0));
            GraphicsDevice.Clear(depthBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(depthBuffer, offlineTarget);

            spriteBatch.Begin();
            spriteBatch.Draw(uv, new RectangleF(0, 0, OfflineWidth, OfflineHeight), null, Color.White, 0, Vector2.Zero);
            spriteBatch.Draw(spheres[0].Texture, Vector2.Zero, spheres[0].Region, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, ImageOrientation.AsIs, 1);
            spriteBatch.DrawString(arial, "Text on Top", new Vector2(75, 75), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 2f, TextAlignment.Left);
            spriteBatch.End();

            // copy texture on screen
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            spriteBatch.Begin();
            spriteBatch.Draw(offlineTarget, new RectangleF(0, 0, width, height), Color.White);
            spriteBatch.End();
        }

        public static void Main()
        {
            using (var game = new TestSpriteBatchToTexture())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunSpriteBatchToTexture()
        {
            RunGameTest(new TestSpriteBatchToTexture());
        }
    }
}