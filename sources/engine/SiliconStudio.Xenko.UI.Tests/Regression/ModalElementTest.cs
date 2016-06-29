// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ModalElement"/> 
    /// </summary>
    public class ModalElementTest : UITestGameBase
    {
        private UniformGrid uniformGrid;

        private ModalElement modal1;
        private ModalElement modal2;

        private TextBlock modalButton1Text;

        private TextBlock modalButton2Text;

        private SpriteSheet sprites;

        public ModalElementTest()
        {
            CurrentVersion = 8;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            sprites = Content.Load<SpriteSheet>("UIImages");

            var lifeBar = new ImageElement { Source = SpriteFromSheet.Create(sprites, "Logo"), HorizontalAlignment = HorizontalAlignment.Center };
            lifeBar.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 3);

            var quitGameButton = new Button
                {
                    Content = new TextBlock { Text = "Quit Game", Font = Content.Load<SpriteFont>("MicrosoftSansSerif15") },
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Padding = Thickness.UniformRectangle(10),
                };
            quitGameButton.DependencyProperties.Set(GridBase.ColumnPropertyKey, 0);
            quitGameButton.DependencyProperties.Set(GridBase.RowPropertyKey, 2);
            quitGameButton.Click += (sender, args) => Exit();

            modalButton1Text = new TextBlock { Text = "Close Modal window 1", Font = Content.Load<SpriteFont>("MicrosoftSansSerif15") };
            var modalButton1 = new Button
            {
                Name = "Button Modal 1",
                Content = modalButton1Text,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = Thickness.UniformRectangle(10),
            };
            modalButton1.Click += ModalButton1OnClick;
            modal1 = new ModalElement { Content = modalButton1, Name = "Modal 1"};
            modal1.DependencyProperties.Set(Panel.ZIndexPropertyKey, 1);
            modal1.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);
            modal1.DependencyProperties.Set(GridBase.RowPropertyKey, 1);
            modal1.OutsideClick += Modal1OnOutsideClick;

            modalButton2Text = new TextBlock { Text = "Close Modal window 2", Font = Content.Load<SpriteFont>("MicrosoftSansSerif15") };
            var modalButton2 = new Button
            {
                Name = "Button Modal 2",
                Content = modalButton2Text,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = Thickness.UniformRectangle(10),
            };
            modalButton2.Click += ModalButton2OnClick;
            modal2 = new ModalElement { Content = modalButton2, Name = "Modal 2" };
            modal2.DependencyProperties.Set(Panel.ZIndexPropertyKey, 2);
            modal2.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);
            modal2.DependencyProperties.Set(GridBase.RowPropertyKey, 2);
            modal2.OutsideClick += Modal2OnOutsideClick;

            uniformGrid = new UniformGrid { Columns = 3, Rows = 3 };
            uniformGrid.Children.Add(modal1);
            uniformGrid.Children.Add(modal2);
            uniformGrid.Children.Add(lifeBar);
            uniformGrid.Children.Add(quitGameButton);
            
            UIComponent.RootElement = uniformGrid;
        }

        private void Modal1OnOutsideClick(object sender, RoutedEventArgs routedEventArgs)
        {
            modalButton1Text.Text = "Click on the Button, please";
        }

        private void Modal2OnOutsideClick(object sender, RoutedEventArgs routedEventArgs)
        {
            modalButton2Text.Text = "Click on the Button, please";
        }

        private void ModalButton1OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            uniformGrid.Children.Remove(modal1);
        }
        private void ModalButton2OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            uniformGrid.Children.Remove(modal2);
        }

        protected override void SpecificDrawBeforeUI(RenderDrawContext context, RenderFrame renderFrame)
        {
            base.SpecificDrawBeforeUI(context, renderFrame);

            context.GraphicsContext.DrawTexture(sprites["GameScreen"].Texture);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyReleased(Keys.D1))
                uniformGrid.Children.Add(modal1);
            if (Input.IsKeyReleased(Keys.D2))
                uniformGrid.Children.Add(modal2);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.TakeScreenshot(5); // skip some frames in order to be sure that the picking will work
            FrameGameSystem.Draw(Draw1).TakeScreenshot();
            FrameGameSystem.Draw(Draw2).TakeScreenshot();
            FrameGameSystem.Draw(Draw3).TakeScreenshot();
            FrameGameSystem.Draw(Draw4).TakeScreenshot();
        }

        public void Draw1()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.125f, 0.15f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up,   new Vector2(0.125f, 0.15f)));

            UI.Update(new GameTime());
        }

        public void Draw2()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.5f, 0.85f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.5f, 0.85f)));

            UI.Update(new GameTime());
        }

        public void Draw3()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.05f, 0.95f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.05f, 0.95f)));

            UI.Update(new GameTime());
        }

        public void Draw4()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.5f, 0.5f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.5f, 0.5f)));

            UI.Update(new GameTime());
        }

        [Test]
        public void RunModalElementTest()
        {
            RunGameTest(new ModalElementTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ModalElementTest())
                game.Run();
        }
    }
}
