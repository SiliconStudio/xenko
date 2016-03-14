// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    [Description("Check Dynamic Font Japanese characters")]
    public class TestDynamicSpriteFontJapanese : GraphicTestGameBase
    {
        private SpriteFont msMincho10;
        private SpriteFont arialMS;

        private SpriteBatch spriteBatch;

        private const string AssetPrefix = "DynamicFonts/";

        private const string Text = @"
漢字（かんじ）は、古代中国に発祥を持つ文字。
古代において中国から日本、朝鮮、ベトナムな
ど周辺諸国にも伝わり、その形態・機能を利用
して日本語など各地の言語の表記にも使われて
いる（ただし、現在は漢字表記を廃している言
語もある。日本の漢字については日本における
漢字を参照）。
漢字は、現代も使われ続けている文字の中で最
も古く成立した[1][2]。人類史上、最も文字数
が多い文字体系であり、その数は10万文字をは
るかに超え他の文字体系を圧倒している。ただ
し万単位の種類のほとんどは歴史的な文書の中
でしか見られない頻度の低いものである。研究
によると、中国で機能的非識字状態にならない
ようにするには、3000から4000の漢字を知って
いれば充分という[3]";

        public TestDynamicSpriteFontJapanese()
        {
            CurrentVersion = 2;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawText).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            msMincho10 = Content.Load<SpriteFont>(AssetPrefix + "MSMincho10");
            arialMS = Content.Load<SpriteFont>(AssetPrefix + "Meiryo14");

            // Instantiate a SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawText();
        }

        private void DrawText()
        {
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetsAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            // Render the text
            spriteBatch.Begin(GraphicsContext);

            var x = 20;
            var y = 10;
            var title = "Ms Mincho 10 aliased:";
            msMincho10.PreGenerateGlyphs(title, msMincho10.Size * Vector2.One);
            msMincho10.PreGenerateGlyphs(Text, msMincho10.Size * Vector2.One);
            spriteBatch.DrawString(msMincho10, title, new Vector2(x, y), Color.LawnGreen);
            spriteBatch.DrawString(msMincho10, Text, new Vector2(x, y + 10), Color.White);

            x = 320;
            y = 0;
            title = "Meiryo 14 anti-aliased:";
            arialMS.PreGenerateGlyphs(title, arialMS.Size * Vector2.One);
            arialMS.PreGenerateGlyphs(Text, arialMS.Size * Vector2.One);
            spriteBatch.DrawString(arialMS, title, new Vector2(x, y), Color.Red);
            spriteBatch.DrawString(arialMS, Text, new Vector2(x, y + 5), Color.White);

            spriteBatch.End();
        }

        public static void Main()
        {
            using (var game = new TestDynamicSpriteFontJapanese())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunDynamicSpriteFontJapanese()
        {
            RunGameTest(new TestDynamicSpriteFontJapanese());
        }
    }
}