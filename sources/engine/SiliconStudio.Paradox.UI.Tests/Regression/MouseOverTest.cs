// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Test the mouse over event/property/designs
    /// </summary>
    public class MouseOverTest : UnitTestGameBase
    {
        private Button button1;
        private Button button2;
        private EditText edit1;
        private EditText edit2;
        private Canvas canvas;
        private StackPanel stackPanel;

        private bool triggeredButton1;
        private bool triggeredButton2;
        private bool triggeredEdit1;
        private bool triggeredEdit2;
        private bool triggeredCanvas;
        private bool triggeredStackPanel;

        private MouseOverState oldValueButton1;
        private MouseOverState newValueButton1;

        public MouseOverTest()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            button1 = new Button { Content = new TextBlock { Text = "text block button 1", Font = Asset.Load<SpriteFont>("CourierNew12")} };
            button1.SetCanvasRelativePosition(new Vector3(0.025f, 0.05f, 0f));

            edit1 = new EditText(Services) {  Text = "Edit text 1", Font = Asset.Load<SpriteFont>("CourierNew12"), };
            edit1.SetCanvasRelativePosition(new Vector3(0.025f, 0.15f, 0f));

            button2 = new Button { Content = new TextBlock { Text = "text block button 2", Font = Asset.Load<SpriteFont>("MicrosoftSansSerif15") } };
            edit2 = new EditText(Services) { Text = "Edit text 2", Font = Asset.Load<SpriteFont>("MicrosoftSansSerif15"), };

            stackPanel = new StackPanel
            {
                Children = { button2, edit2 }, 
                HorizontalAlignment = HorizontalAlignment.Center, 
                VerticalAlignment = VerticalAlignment.Center, 
                Orientation = Orientation.Horizontal
            };
            stackPanel.SetCanvasRelativePosition(new Vector3(0.5f, 0.5f, 0f));
            stackPanel.SetCanvasPinOrigin(new Vector3(.5f));

            canvas = new Canvas { Children = {button1, edit1, stackPanel}, CanBeHitByUser = true };

            button1.MouseOverStateChanged += (sender, args) => { triggeredButton1 = true; oldValueButton1 = args.OldValue; newValueButton1 = args.NewValue;};
            button2.MouseOverStateChanged += (sender, args) => { triggeredButton2 = true;};
            edit1.MouseOverStateChanged += (sender, args) => { triggeredEdit1 = true;};
            edit2.MouseOverStateChanged += (sender, args) => { triggeredEdit2 = true;};
            canvas.MouseOverStateChanged += (sender, args) => { triggeredCanvas = true;};
            stackPanel.MouseOverStateChanged += (sender, args) => { triggeredStackPanel = true;};

            UI.RootElement = canvas;
        }

        protected override void CreatePipeline()
        {
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services) { BackgroundTexture = Asset.Load<Texture>("ParadoxBackground")});
            RenderSystem.Pipeline.Renderers.Add(UIRenderer = new UIRenderer(Services));
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.TakeScreenshot();
            FrameGameSystem.Draw(Test1);
            FrameGameSystem.Draw(Test2);
            FrameGameSystem.Draw(Test3);
            FrameGameSystem.Draw(Test4);
            FrameGameSystem.Draw(Test5);
            FrameGameSystem.Draw(Test6);
            FrameGameSystem.Draw(Draw1).TakeScreenshot();
            FrameGameSystem.Draw(Draw2).TakeScreenshot();
        }

        private void Test1()
        {
            ResetStates();

            Assert.AreEqual(MouseOverState.MouseOverNone, canvas.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, stackPanel.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button2.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit2.MouseOverState);

            Assert.IsFalse(triggeredButton1);
            Assert.IsFalse(triggeredButton2);
            Assert.IsFalse(triggeredEdit1);
            Assert.IsFalse(triggeredEdit2);
            Assert.IsFalse(triggeredCanvas);
            Assert.IsFalse(triggeredStackPanel);
        }

        private void Test2()
        {
            ResetStates();

            Input.CurrentMousePosition = new Vector2(0.1f, 0.08f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());

            Assert.AreEqual(MouseOverState.MouseOverChild, canvas.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, stackPanel.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverElement, button1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button2.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit2.MouseOverState);

            Assert.IsTrue(triggeredButton1);
            Assert.IsTrue(triggeredCanvas);
            Assert.IsFalse(triggeredButton2);
            Assert.IsFalse(triggeredEdit1);
            Assert.IsFalse(triggeredEdit2);
            Assert.IsFalse(triggeredStackPanel);

            Assert.AreEqual(MouseOverState.MouseOverNone, oldValueButton1);
            Assert.AreEqual(MouseOverState.MouseOverElement, newValueButton1);
        }

        private void Test3()
        {
            ResetStates();

            Input.CurrentMousePosition = new Vector2(0.1f, 0.18f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());

            Assert.AreEqual(MouseOverState.MouseOverChild, canvas.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, stackPanel.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button2.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverElement, edit1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit2.MouseOverState);

            Assert.IsTrue(triggeredButton1);
            Assert.IsTrue(triggeredEdit1);
            Assert.IsFalse(triggeredCanvas);
            Assert.IsFalse(triggeredButton2);
            Assert.IsFalse(triggeredEdit2);
            Assert.IsFalse(triggeredStackPanel);
        }

        private void Test4()
        {
            ResetStates();

            Input.CurrentMousePosition = new Vector2(0.1f, 0.3f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());

            Assert.AreEqual(MouseOverState.MouseOverElement, canvas.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, stackPanel.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button2.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit2.MouseOverState);

            Assert.IsTrue(triggeredEdit1);
            Assert.IsTrue(triggeredCanvas);
            Assert.IsFalse(triggeredButton2);
            Assert.IsFalse(triggeredButton1);
            Assert.IsFalse(triggeredEdit2);
            Assert.IsFalse(triggeredStackPanel);
        }

        private void Test5()
        {
            ResetStates();

            Input.CurrentMousePosition = new Vector2(0.5f, 0.5f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());

            Assert.AreEqual(MouseOverState.MouseOverChild, canvas.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverChild, stackPanel.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverElement, button2.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit2.MouseOverState);

            Assert.IsTrue(triggeredCanvas);
            Assert.IsTrue(triggeredButton2);
            Assert.IsTrue(triggeredStackPanel);
            Assert.IsFalse(triggeredButton1);
            Assert.IsFalse(triggeredEdit2);
            Assert.IsFalse(triggeredEdit1);
        }

        private void Test6()
        {
            ResetStates();

            Input.CurrentMousePosition = new Vector2(0.56f, 0.5f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());

            Assert.AreEqual(MouseOverState.MouseOverChild, canvas.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverChild, stackPanel.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, button2.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverNone, edit1.MouseOverState);
            Assert.AreEqual(MouseOverState.MouseOverElement, edit2.MouseOverState);

            Assert.IsTrue(triggeredEdit2);
            Assert.IsTrue(triggeredButton2);
            Assert.IsFalse(triggeredCanvas);
            Assert.IsFalse(triggeredStackPanel);
            Assert.IsFalse(triggeredButton1);
            Assert.IsFalse(triggeredEdit1);
        }

        private void ResetStates()
        {
            triggeredButton1 = false;
            triggeredButton2 = false;
            triggeredEdit1 = false;
            triggeredEdit2 = false;
            triggeredCanvas = false;
            triggeredStackPanel = false;
        }

        private void Draw1()
        {
            Input.CurrentMousePosition = new Vector2(0.1f, 0.08f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());
        }

        private void Draw2()
        {
            Input.CurrentMousePosition = new Vector2(0.1f, 0.18f);
            Input.Update(new GameTime());
            UI.Update(new GameTime());
        }

        [Test]
        public void RunMouseOversTest()
        {
            RunGameTest(new MouseOverTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new MouseOverTest())
                game.Run();
        }
    }
}