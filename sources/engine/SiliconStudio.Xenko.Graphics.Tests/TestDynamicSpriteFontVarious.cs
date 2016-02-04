// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    [Description("Check Dynamic Font various")]
    public class TestDynamicSpriteFontVarious : GraphicTestGameBase
    {
        private SpriteFont msMincho10;

        private SpriteBatch spriteBatch;
        
        private const string AssetPrefix = "DynamicFonts/";
        
        private readonly StringBuilder varyingString = new StringBuilder(VaryingStringLength);
        private const int VaryingStringLength = 200;
        private int varyingStringCurrentIndex = VaryingStringStartIndex;
        private const int VaryingStringStartIndex = 0x4e00;
        private const int VaryingStringEndIndex = 0x9faf;
        private const double VaryingStringTimeInterval = 1;
        private double accumulatedSeconds;

        public TestDynamicSpriteFontVarious()
        {
            CurrentVersion = 1;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(()=>SetTimeAndDraw(0)).TakeScreenshot();
            FrameGameSystem.Draw(()=>SetTimeAndDraw(1.1f)).TakeScreenshot();
            FrameGameSystem.Draw(()=>SetTimeAndDraw(3.5f)).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            msMincho10 = Asset.Load<SpriteFont>(AssetPrefix + "MSMincho10");

            // Instantiate a SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);

            for (int i = 0; i < VaryingStringLength; i++)
                varyingString.Append(' ');
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            accumulatedSeconds += 1 / 60f;

            if(!ScreenShotAutomationEnabled)
                DrawText();
        }

        private void SetTimeAndDraw(float time)
        {
            accumulatedSeconds = time;

            DrawText();
        }

        private void DrawText()
        {
            GraphicsCommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsCommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsCommandList.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            // Render the text
            spriteBatch.Begin(GraphicsCommandList);

            var x = 20;
            var y = 10;

            var size = 8;
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            var dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);
            size = 10;
            y += (int)Math.Ceiling(dim.Y);
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);
            size = 12;
            y += (int)Math.Ceiling(dim.Y);
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);
            size = 14;
            y += (int)Math.Ceiling(dim.Y);
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);
            size = 16;
            y += (int)Math.Ceiling(dim.Y);
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);
            size = 20;
            y += (int)Math.Ceiling(dim.Y);
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);
            size = 25;
            y += (int)Math.Ceiling(dim.Y);
            msMincho10.PreGenerateGlyphs(BuildTextSize(size), FontHelper.PointsToPixels(size) * Vector2.One);
            dim = msMincho10.MeasureString(BuildTextSize(size), FontHelper.PointsToPixels(size));
            spriteBatch.DrawString(msMincho10, BuildTextSize(size), FontHelper.PointsToPixels(size), new Vector2(x, y), Color.White);

            // change the varying string if necessary
            if (accumulatedSeconds > VaryingStringTimeInterval)
            {
                accumulatedSeconds = 0;
                for (int i = 0; i < VaryingStringLength;)
                {
                    for (int j = 0; j < 50 && i < VaryingStringLength; j++, ++i)
                    {
                        varyingString[i] = (char)varyingStringCurrentIndex;

                        ++varyingStringCurrentIndex;
                        if (varyingStringCurrentIndex > VaryingStringEndIndex)
                            varyingStringCurrentIndex = VaryingStringStartIndex;
                    }

                    // add return lines
                    if (i < VaryingStringLength)
                    {
                        varyingString[i] = '\n';
                        ++i;
                    }
                }
            }

            // print varying text
            y += (int)Math.Ceiling(dim.Y) + 10;
            msMincho10.PreGenerateGlyphs(varyingString.ToString(), 16 * Vector2.One);
            spriteBatch.DrawString(msMincho10, varyingString, 16, new Vector2(x, y), Color.White);

            spriteBatch.End();
        }

        private string BuildTextSize(int size)
        {
            return "MS Mincho size " + size +" pts. 漢字のサイズは" + size + "点。";
        }

        public static void Main()
        {
            using (var game = new TestDynamicSpriteFontVarious())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunDynamicSpriteFontVarious()
        {
            RunGameTest(new TestDynamicSpriteFontVarious());
        }
    }
}