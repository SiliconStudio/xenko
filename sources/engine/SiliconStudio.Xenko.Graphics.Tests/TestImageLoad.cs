// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using NUnit.Framework;

using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestImageLoad : GraphicTestGameBase
    {
        private SpriteBatch spriteBatch;
        private Texture jpg;
        private Texture png;

        public TestImageLoad()
        {
            CurrentVersion = 3;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawImages).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            using (var pngStream = AssetManager.FileProvider.OpenStream("PngImage", VirtualFileMode.Open, VirtualFileAccess.Read))
            using (var pngImage = Image.Load(pngStream, GraphicsDevice.ColorSpace == ColorSpace.Linear))
                png = Texture.New(GraphicsDevice, pngImage);

            using (var jpgStream = AssetManager.FileProvider.OpenStream("JpegImage", VirtualFileMode.Open, VirtualFileAccess.Read))
            using (var jpgImage = Image.Load(jpgStream, GraphicsDevice.ColorSpace == ColorSpace.Linear))
                jpg = Texture.New(GraphicsDevice, jpgImage);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawImages();
        }

        private void DrawImages()
        {
            GraphicsCommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.AntiqueWhite);
            GraphicsCommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsCommandList.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            spriteBatch.Begin(GraphicsCommandList);

            var screenSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.ViewWidth, GraphicsDevice.Presenter.BackBuffer.ViewHeight);

            spriteBatch.Draw(jpg, new Rectangle(0, 0, (int)screenSize.X, (int)(screenSize.Y / 2)), Color.White);
            spriteBatch.Draw(png, new Rectangle(0, (int)(screenSize.Y / 2), (int)screenSize.X, (int)(screenSize.Y / 2)), Color.White);

            spriteBatch.End();
        }

        public static void Main()
        {
            using (var game = new TestImageLoad())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunImageLoad()
        {
            RunGameTest(new TestImageLoad());
        }
    }
}
