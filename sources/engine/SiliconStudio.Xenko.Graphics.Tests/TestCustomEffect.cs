// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public static class MyCustomShaderKeys
    {
        public static readonly ValueParameterKey<Vector4> ColorFactor2 = ParameterKeys.NewValue<Vector4>();
    }

    [TestFixture]
    public class TestCustomEffect : GraphicTestGameBase
    {
        private DynamicEffectInstance effectInstance;

        private float switchEffectLevel;

        public TestCustomEffect()
        {
            CurrentVersion = 2;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawCustomEffect).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            effectInstance = new DynamicEffectInstance("CustomEffect.CustomSubEffect");
            effectInstance.Initialize(Services);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawCustomEffect();
        }

        private void DrawCustomEffect()
        {
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            effectInstance.Parameters.Set(MyCustomShaderKeys.ColorFactor2, (Vector4)Color.Red);
            effectInstance.Parameters.Set(CustomShaderKeys.SwitchEffectLevel, switchEffectLevel);
            effectInstance.Parameters.Set(TexturingKeys.Texture0, UVTexture);
            switchEffectLevel++; // TODO: Add switch Effect to test and capture frames

            GraphicsContext.DrawQuad(effectInstance);
        }

        public static void Main()
        {
            using (var game = new TestCustomEffect())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunCustomEffect()
        {
            RunGameTest(new TestCustomEffect());
        }
    }
}