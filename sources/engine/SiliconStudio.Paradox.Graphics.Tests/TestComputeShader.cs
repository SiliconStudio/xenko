// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.ComputeEffect;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestComputeShader : TestGameBase
    {
        const int ReductionRatio = 4;

        private SpriteBatch spriteBatch;

        private Texture displayedTexture;

        private Texture outputTexture;
        private Texture inputTexture;

        private Int2 screenSize = new Int2(1200, 900);

        private DrawEffect computeShaderEffect;
        private RenderContext drawEffectContext;

        public TestComputeShader()
        {
            CurrentVersion = 2;
            GraphicsDeviceManager.PreferredBackBufferWidth = screenSize.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenSize.Y;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            inputTexture = Asset.Load<Texture>("uv");
            var groupCounts = new Int3(inputTexture.Width / ReductionRatio, inputTexture.Height / ReductionRatio, 1);
            outputTexture = Texture.New2D(GraphicsDevice, groupCounts.X, groupCounts.Y, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.UnorderedAccess | TextureFlags.ShaderResource);
            displayedTexture = outputTexture;

            drawEffectContext = RenderContext.GetShared(Services);
            computeShaderEffect = new ComputeEffectShader(drawEffectContext) { ShaderSourceName = "ComputeShaderTestEffect", ThreadGroupCounts = groupCounts };
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.I))
                displayedTexture = inputTexture;

            if (Input.IsKeyPressed(Keys.O))
                displayedTexture = outputTexture;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            computeShaderEffect.Parameters.Set(ComputeShaderTestParams.NbOfIterations, ReductionRatio);
            computeShaderEffect.Parameters.Set(ComputeShaderTestKeys.input, inputTexture);
            computeShaderEffect.Parameters.Set(ComputeShaderTestKeys.output, outputTexture);
            computeShaderEffect.Draw();

            if (displayedTexture == null || spriteBatch == null)
                return;

            GraphicsDevice.DrawTexture(displayedTexture);

        }

        [Test]
        public void RunTest()
        {
            RunGameTest(new TestComputeShader());
        }

        public static void Main()
        {
            using (var game = new TestComputeShader())
                game.Run();
        }
    }
}