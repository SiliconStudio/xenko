// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests to test batching ordering for transparency.
    /// </summary>
    public class ClickTests : UITestGameBase
    {
        private List<Button> elements;

        public ClickTests()
        {
            CurrentVersion = 9;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var element1 = new Button { Name = "1", Width = 800, Height = 400, Content = new TextBlock { Font = Content.Load<SpriteFont>("CourierNew12"), SynchronousCharacterGeneration = true } };
            element1.DependencyProperties.Set(Canvas.AbsolutePositionPropertyKey, new Vector3(100, 50, 0));
            element1.DependencyProperties.Set(Panel.ZIndexPropertyKey, -1);

            var element2 = new Button { Name = "2", Width = 400, Height = 200, Content = new TextBlock { Font = Content.Load<SpriteFont>("CourierNew12"), SynchronousCharacterGeneration = true } };
            element2.DependencyProperties.Set(Canvas.AbsolutePositionPropertyKey, new Vector3(300, 150, 0));
            element2.DependencyProperties.Set(Panel.ZIndexPropertyKey, 1);

            var element3 = new Button { Name = "3", Width = 400, Height = 200, Content = new TextBlock { Font = Content.Load<SpriteFont>("CourierNew12"), SynchronousCharacterGeneration = true } };
            element3.DependencyProperties.Set(Canvas.AbsolutePositionPropertyKey, new Vector3(150, 225, 0));
            element3.DependencyProperties.Set(Panel.ZIndexPropertyKey, 2);

            var element4 = new Button { Name = "4", Width = 400, Height = 200, Content = new TextBlock { Font = Content.Load<SpriteFont>("CourierNew12"), SynchronousCharacterGeneration = true } };
            element4.DependencyProperties.Set(Canvas.AbsolutePositionPropertyKey, new Vector3(450, 75, 0));
            element4.DependencyProperties.Set(Panel.ZIndexPropertyKey, 0);

            var canvas = new Canvas();
            canvas.Children.Add(element1);
            canvas.Children.Add(element2);
            canvas.Children.Add(element3);
            canvas.Children.Add(new Canvas { Children = { element4 } });

            elements = new List<Button> { element1, element2, element3, element4 };

            UIComponent.RootElement = canvas;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            const float depthIncrement = 1f;
            const float rotationIncrement = 0.1f;

            var localMatrix = elements[1].LocalMatrix;

            if (Input.IsKeyPressed(Keys.Up))
                localMatrix.M43 -= depthIncrement;
            if (Input.IsKeyPressed(Keys.Down))
                localMatrix.M43 += depthIncrement;
            if (Input.IsKeyPressed(Keys.NumPad4))
                localMatrix = localMatrix * Matrix.RotationY(-rotationIncrement);
            if (Input.IsKeyPressed(Keys.NumPad6))
                localMatrix = localMatrix * Matrix.RotationY(+rotationIncrement);
            if (Input.IsKeyPressed(Keys.NumPad2))
                localMatrix = localMatrix * Matrix.RotationX(+rotationIncrement);
            if (Input.IsKeyPressed(Keys.NumPad8))
                localMatrix = localMatrix * Matrix.RotationX(-rotationIncrement);
            if (Input.IsKeyPressed(Keys.NumPad1))
                localMatrix = localMatrix * Matrix.RotationZ(-rotationIncrement);
            if (Input.IsKeyPressed(Keys.NumPad9))
                localMatrix = localMatrix * Matrix.RotationZ(+rotationIncrement);

            if (Input.KeyEvents.Any())
            {
                elements[1].LocalMatrix = localMatrix;

                UpdateTextBlockText();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (gameTime.FrameCount <= 2)
                UpdateTextBlockText();
        }

        private void UpdateTextBlockText()
        {
            foreach (var element in elements)
                ((TextBlock)element.Content).Text = "Element " + element.Name + "\nActual Depth: " + element.LocalMatrix.M43 + "\nDepth Bias: " + element.DepthBias;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(0, UpdateTextBlockText).TakeScreenshot(0);
            FrameGameSystem.Draw(1, Draw1).TakeScreenshot(1);
            FrameGameSystem.Draw(2, Draw2).TakeScreenshot(2);
            FrameGameSystem.Draw(3, () => SetElement2Matrix(Matrix.Translation(0, 0, -110))).Draw(4, Draw3).TakeScreenshot(4);
            FrameGameSystem.Draw(5, () => SetElement2Matrix(Matrix.Translation(0, 0, 170))).Draw(6, Draw4).TakeScreenshot(6);
            FrameGameSystem.Draw(7, () => SetElement2Matrix(Matrix.RotationYawPitchRoll(-0.1f, -0.2f, 0.3f))).Draw(8, Draw5).TakeScreenshot(8);
            FrameGameSystem.Draw(Draw6).TakeScreenshot();
        }

        public void SetElement2Matrix(Matrix matrix)
        {
            elements[1].LocalMatrix = matrix;
        }

        public void Draw1()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.4f, 0.6f)));
        }

        public void Draw2()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.4f, 0.6f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.6f, 0.4f)));
        }

        public void Draw3()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.6f, 0.4f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.4f, 0.6f)));
        }

        public void Draw4()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.4f, 0.6f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.4f, 0.4f)));
        }

        public void Draw5()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.4f, 0.6f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.27625f, 0.5667f)));
        }

        public void Draw6()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.348f, 0.231f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.441f, 0.418f)));
        }

        [Test]
        public void RunClickTests()
        {
            RunGameTest(new ClickTests());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ClickTests())
            {
                game.Run();
            }
        }
    } 
}