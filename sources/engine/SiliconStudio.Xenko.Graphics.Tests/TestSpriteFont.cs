// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public class TestSpriteFont : GraphicTestGameBase
    {
        private SpriteBatch spriteBatch;
        private SpriteFont arial13;
        private SpriteFont msSansSerif10;
        private SpriteFont arial16;
        private SpriteFont arial16ClearType;
        private SpriteFont arial16Bold;
        private SpriteFont courrierNew10;
        private SpriteFont calibri64;
        private Texture colorTexture;

        private float rotationAngle;

        private readonly string assetPrefix = "";
        private readonly string saveImageSuffix = "";

        public TestSpriteFont(string assetPrefix, string saveImageSuffix)
        {
            CurrentVersion = 4;

            this.assetPrefix = assetPrefix;
            this.saveImageSuffix = saveImageSuffix;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(() => SetRotationAndDraw(0)).TakeScreenshot();
            FrameGameSystem.Draw(() => SetRotationAndDraw(3.1415f)).TakeScreenshot();
            FrameGameSystem.Draw(() => SetRotationAndDraw(4)).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            arial13 = Content.Load<SpriteFont>(assetPrefix+"Arial13");
            msSansSerif10 = Content.Load<SpriteFont>(assetPrefix+"MicrosoftSansSerif10");
            arial16 = Content.Load<SpriteFont>(assetPrefix+"Arial16");
            arial16ClearType = Content.Load<SpriteFont>(assetPrefix+"Arial16ClearType");
            arial16Bold = Content.Load<SpriteFont>(assetPrefix+"Arial16Bold");
            calibri64 = Content.Load<SpriteFont>(assetPrefix+"Calibri64");
            courrierNew10 = Content.Load<SpriteFont>(assetPrefix+"CourierNew10");
            
            // Instantiate a SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            colorTexture = Texture.New2D(GraphicsDevice, 1, 1, PixelFormat.R8G8B8A8_UNorm, new[] { Color.White });

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            if (!ScreenShotAutomationEnabled)
            {
                rotationAngle += 1 / 60f; // frame-dependent and not time-dependent

                DrawSpriteFont();
            }
        }

        private void SetRotationAndDraw(float rotation)
        {
            rotationAngle = rotation;

            DrawSpriteFont();
        }

        private void DrawSpriteFont()
        {
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            // Render the text
            spriteBatch.Begin(GraphicsContext);

            var text = "This text is in Arial 16 with anti-alias\nand multiline...";
            var dim = arial16.MeasureString(text);

            int x = 20, y = 20;
            spriteBatch.Draw(colorTexture, new Rectangle(x, y, (int)dim.X, (int)dim.Y), Color.Green);


            arial16.PreGenerateGlyphs(text, arial16.Size * Vector2.One);
            spriteBatch.DrawString(arial16, text, new Vector2(x, y), Color.White);

            text = "Measured: " + dim;
            courrierNew10.PreGenerateGlyphs(text, courrierNew10.Size * Vector2.One);
            spriteBatch.DrawString(courrierNew10, text, new Vector2(x, y + dim.Y + 5), Color.GreenYellow);

            text = @"
-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_
Text using Courier New 10 fixed font
0123456789 - 0123456789 - 0123456789
ABCDEFGHIJ - ABCDEFGHIJ - A1C3E5G7I9
-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_";

            courrierNew10.PreGenerateGlyphs(text, courrierNew10.Size * Vector2.One);
            spriteBatch.DrawString(courrierNew10, text, new Vector2(x, y + dim.Y + 8), Color.White);

            text = "Arial 13, font with with antialias.";
            arial13.PreGenerateGlyphs(text, arial13.Size * Vector2.One);
            spriteBatch.DrawString(arial13, text, new Vector2(x, y + 150), Color.White);

            text = "Microsoft Sans Serif 10, font with cleartype antialias.";
            msSansSerif10.PreGenerateGlyphs(text, msSansSerif10.Size * Vector2.One);
            spriteBatch.DrawString(msSansSerif10, text, new Vector2(x, y + 175), Color.White);

            text = "Font is in bold - Arial 16";
            arial16Bold.PreGenerateGlyphs(text, arial16Bold.Size * Vector2.One);
            spriteBatch.DrawString(arial16Bold, text, new Vector2(x, y + 190), Color.White);

            text = "Bigger font\nCalibri 64";
            y = 240;
            dim = calibri64.MeasureString(text);
            spriteBatch.Draw(colorTexture, new Rectangle(x, y, (int)dim.X, (int)dim.Y), Color.Red);
            calibri64.PreGenerateGlyphs(text, calibri64.Size * Vector2.One);
            spriteBatch.DrawString(calibri64, text, new Vector2(x, y), Color.White);

            text = "Rendering test\nRotated On Center";
            dim = arial16.MeasureString(text);
            arial16.PreGenerateGlyphs(text, arial16.Size * Vector2.One);
            spriteBatch.DrawString(arial16, text, new Vector2(600, 120), Color.White, -rotationAngle, new Vector2(dim.X / 2.0f, dim.Y / 2.0f), Vector2.One, SpriteEffects.None, 0.0f, TextAlignment.Left);
            
            text = "Arial16 - ClearType\nAbc /\\Z Ghi SWy {}:;=&%@";
            arial16ClearType.PreGenerateGlyphs(text, arial16ClearType.Size * Vector2.One);
            spriteBatch.DrawString(arial16ClearType, text, new Vector2(470, 250), Color.White);

            text = "Abc /\\Z Ghi SWy {}:;=&%@\nArial16 - Standard";
            arial16.PreGenerateGlyphs(text, arial16.Size * Vector2.One);
            spriteBatch.DrawString(arial16, text, new Vector2(470, 300), Color.White);

            text = "Arial16 simulate shadow";
            arial16.PreGenerateGlyphs(text, arial16.Size * Vector2.One);
            spriteBatch.DrawString(arial16, text, new Vector2(471, 391), Color.Red);
            spriteBatch.DrawString(arial16, text, new Vector2(470, 390), Color.White);

            text = "Arial16 scaled x1.5";
            arial16.PreGenerateGlyphs(text, arial16.Size * Vector2.One);
            spriteBatch.DrawString(arial16, text, new Vector2(470, 420), Color.White, 0.0f, Vector2.Zero, 1.5f * Vector2.One, SpriteEffects.None, 0.0f, TextAlignment.Left);

            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(Input.IsKeyReleased(Keys.S))
                SaveTexture(GraphicsDevice.Presenter.BackBuffer, "sprite-font-" + saveImageSuffix + ".png");
        }
    }
}