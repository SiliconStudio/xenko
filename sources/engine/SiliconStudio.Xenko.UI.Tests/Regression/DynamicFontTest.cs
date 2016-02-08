// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for dynamic sized text rendering tests.
    /// </summary>
    public class DynamicFontTest : UITestGameBase
    {
        private ContentDecorator decorator;
        private TextBlock textBlock;

        public DynamicFontTest()
        {
            CurrentVersion = 4;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            textBlock = new TextBlock
                {
                    Font = Asset.Load<SpriteFont>("MSMincho10"), 
                    Text = "Simple Text - 簡単な文章。", 
                    TextColor = Color.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    SynchronousCharacterGeneration = true
                };

            decorator = new ContentDecorator
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                BackgroundImage = new Sprite(Asset.Load<Texture>("DumbWhite")),
                Content = textBlock
            };

            UIComponent.RootElement = decorator;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            const float ChangeFactor = 1.1f;
            const float ChangeFactorInverse = 1 / ChangeFactor;

            // change the size of the virtual resolution
            if (Input.IsKeyReleased(Keys.NumPad0))
                UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width / 2f, GraphicsDevice.Presenter.BackBuffer.Height / 2f, 400);
            if (Input.IsKeyReleased(Keys.NumPad1))
                UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 400);
            if (Input.IsKeyReleased(Keys.NumPad2))
                UIComponent.VirtualResolution = new Vector3(2 * GraphicsDevice.Presenter.BackBuffer.Width, 2 * GraphicsDevice.Presenter.BackBuffer.Height, 400);
            if (Input.IsKeyReleased(Keys.Right))
                UIComponent.VirtualResolution = new Vector3((ChangeFactor * UIComponent.VirtualResolution.X), UIComponent.VirtualResolution.Y, UIComponent.VirtualResolution.Z);
            if (Input.IsKeyReleased(Keys.Left))
                UIComponent.VirtualResolution = new Vector3((ChangeFactorInverse * UIComponent.VirtualResolution.X), UIComponent.VirtualResolution.Y, UIComponent.VirtualResolution.Z);
            if (Input.IsKeyReleased(Keys.Up))
                UIComponent.VirtualResolution = new Vector3(UIComponent.VirtualResolution.X, (ChangeFactor * UIComponent.VirtualResolution.Y), UIComponent.VirtualResolution.Z);
            if (Input.IsKeyReleased(Keys.Down))
                UIComponent.VirtualResolution = new Vector3(UIComponent.VirtualResolution.X, (ChangeFactorInverse * UIComponent.VirtualResolution.Y), UIComponent.VirtualResolution.Z);

            if (Input.IsKeyReleased(Keys.D1))
                decorator.LocalMatrix = Matrix.Scaling(1);
            if (Input.IsKeyReleased(Keys.D2))
                decorator.LocalMatrix = Matrix.Scaling(1.5f);
            if (Input.IsKeyReleased(Keys.D3))
                decorator.LocalMatrix = Matrix.Scaling(2);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(DrawTest0).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest1).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest2).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest3).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest4).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest5).TakeScreenshot();
        }

        public void DrawTest0()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest1()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = 2*textBlock.Font.Size;
            UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest2()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width / 2f, GraphicsDevice.Presenter.BackBuffer.Height / 2f, 500);
        }

        public void DrawTest3()
        {
            decorator.LocalMatrix = Matrix.Scaling(2);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest4()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width / 2f, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest5()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.VirtualResolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height / 2f, 500);
        }

        [Test]
        public void RunDynamicFontTest()
        {
            RunGameTest(new DynamicFontTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new DynamicFontTest())
                game.Run();
        }
    }
}