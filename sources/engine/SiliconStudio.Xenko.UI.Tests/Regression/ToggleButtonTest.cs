// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
            CurrentVersion = 5;
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
            ApplyToggleButtonBlockDefaultStyle(toggle);

            UIComponent.Page = new Engine.UIPage { RootElement = toggle };
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
            // Since click are evaluated before measuring/arranging/drawing, we need to render the UI at least once (see UIRenderFeature.Draw)
            FrameGameSystem.Draw(() => { }).TakeScreenshot();
            FrameGameSystem.Draw(Click).TakeScreenshot();
            FrameGameSystem.Draw(Click).TakeScreenshot();
            FrameGameSystem.Draw(Click).TakeScreenshot();
        }

        private void Click()
        {
            PointerEvents.Clear();
            PointerEvents.Add(CreatePointerEvent(PointerEventType.Pressed, new Vector2(0.5f)));
            PointerEvents.Add(CreatePointerEvent(PointerEventType.Released, new Vector2(0.5f)));
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
