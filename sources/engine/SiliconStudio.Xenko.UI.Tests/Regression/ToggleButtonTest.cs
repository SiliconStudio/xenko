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
    /// Class for rendering tests on the <see cref="Button"/> 
    /// </summary>
    public class ToggleButtonTest : UITestGameBase
    {
        private ToggleButton toggle;

        public ToggleButtonTest()
        {
            CurrentVersion = 4;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            toggle = new ToggleButton 
            {
                IsThreeState = true,
                Content = new TextBlock { TextColor = Color.Black, Text = "Toggle button test", Font = Content.Load<SpriteFont>("MicrosoftSansSerif15"), VerticalAlignment = VerticalAlignment.Center },
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            UIComponent.RootElement = toggle;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.KeyEvents.Count > 0)
                toggle.IsThreeState = !toggle.IsThreeState;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(Draw0).TakeScreenshot();
            FrameGameSystem.Draw(Draw1).TakeScreenshot();
            FrameGameSystem.Draw(Draw2).TakeScreenshot();
        }

        public void Draw0()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.5f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.5f)));

            UI.Update(new GameTime());
        }

        public void Draw1()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.5f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.5f)));

            UI.Update(new GameTime());
        }

        public void Draw2()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.5f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.5f)));

            UI.Update(new GameTime());
        }

        [Test]
        public void RunToggleButtonTest()
        {
            RunGameTest(new ToggleButtonTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ToggleButtonTest())
                game.Run();
        }
    }
}