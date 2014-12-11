// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ContentDecorator"/> 
    /// </summary>
    public class ContentDecoratorTest : UnitTestGameBase
    {
        private TextBlock textBlock;

        public ContentDecoratorTest()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            textBlock = new TextBlock
            {
                TextColor = Color.Black,
                Font = Asset.Load<SpriteFont>("MSMincho10"),
                Text = @"Simple sample text surrounded by decorator.",
                SynchronousCharacterGeneration = true
            };

            var decorator = new ContentDecorator
            {
                Content = textBlock, 
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                BackgroundImage = new UIImage(Asset.Load<Texture>("DumbWhite"))
            };

            UI.RootElement = decorator;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.Left))
                textBlock.TextSize = 3 * textBlock.TextSize / 4;
            if (Input.IsKeyPressed(Keys.Right))
                textBlock.TextSize = 4 * textBlock.TextSize / 3;
            if (Input.IsKeyPressed(Keys.Delete))
                textBlock.TextSize = textBlock.Font.Size;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(DrawTest0).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest1).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest2).TakeScreenshot();
        }

        public void DrawTest0()
        {
            textBlock.TextSize = 12;
        }

        public void DrawTest1()
        {
            textBlock.TextSize = 18;
        }

        public void DrawTest2()
        {
            textBlock.TextSize = 24;
        }

        [Test]
        public void RunContentDecoratorTest()
        {
            RunGameTest(new ContentDecoratorTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ContentDecoratorTest())
                game.Run();
        }
    }
}