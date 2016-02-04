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
        public static readonly ParameterKey<Vector4> ColorFactor2 = ParameterKeys.New<Vector4>();
    }

    [TestFixture]
    public class TestCustomEffect : GraphicTestGameBase
    {
        private ParameterCollection effectParameters;

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


            effectParameters = new ParameterCollection();
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
            GraphicsCommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsCommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsCommandList.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            effectInstance.Parameters.SetValueSlow(MyCustomShaderKeys.ColorFactor2, (Vector4)Color.Red);
            effectInstance.Parameters.SetValueSlow(CustomShaderKeys.SwitchEffectLevel, switchEffectLevel);
            effectInstance.Parameters.SetResourceSlow(TexturingKeys.Texture0, UVTexture);
            switchEffectLevel++; // TODO: Add switch Effect to test and capture frames

            GraphicsCommandList.DrawQuad(effectInstance);
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