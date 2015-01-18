// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public static partial class MyCustomShaderKeys
    {
        public static readonly ParameterKey<Vector4> ColorFactor2 = ParameterKeys.New<Vector4>();
    }

    [TestFixture]
    public class TestCustomEffect : TestGameBase
    {
        private ParameterCollection effectParameters;
        private DynamicEffectCompiler dynamicEffectCompiler;

        private DefaultEffectInstance effectInstance;

        private float switchEffectLevel;


        public TestCustomEffect()
        {
            CurrentVersion = 1;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawCustomEffect).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();


            dynamicEffectCompiler = new DynamicEffectCompiler(Services, "CustomEffect.CustomSubEffect");
            effectParameters = new ParameterCollection();
            effectInstance = new DefaultEffectInstance(effectParameters);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawCustomEffect();
        }

        private void DrawCustomEffect()
        {
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            effectParameters.Set(MyCustomShaderKeys.ColorFactor2, (Vector4)Color.Red);
            effectParameters.Set(CustomShaderKeys.SwitchEffectLevel, switchEffectLevel);
            effectParameters.Set(TexturingKeys.Texture0, UVTexture);
            // TODO: Add switch Effect to test and capture frames
            //switchEffectLevel++;
            dynamicEffectCompiler.Update(effectInstance);

            GraphicsDevice.DrawQuad(effectInstance.Effect, effectParameters);
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