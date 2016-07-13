// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ContentDecorator"/> 
    /// </summary>
    public class ContentDecoratorTest : UITestGameBase
    {
        private TextBlock textBlock;

        public ContentDecoratorTest()
        {
            CurrentVersion = 6; // Font type, names & sizes changed slightly
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            textBlock = new TextBlock
            {
                TextColor = Color.Black,
                Font = Content.Load<SpriteFont>("HanSans13"),
                Text = @"Simple sample text surrounded by decorator.",
                SynchronousCharacterGeneration = true
            };

            var decorator = new ContentDecorator
            {
                Content = textBlock, 
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                BackgroundImage = (SpriteFromTexture)new Sprite(Content.Load<Texture>("DumbWhite"))
            };

            UIComponent.RootElement = decorator;
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
