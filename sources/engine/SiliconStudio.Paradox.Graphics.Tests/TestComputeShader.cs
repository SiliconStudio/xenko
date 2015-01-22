// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.ComputeEffect;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestComputeShader : Game
    {
        const int ReductionRatio = 4;

        private SpriteBatch spriteBatch;

        private Texture displayedTexture;

        private Texture outputTexture;
        private Texture inputTexture;

        private Int2 screenSize = new Int2(1200, 900);

        private DrawEffect computeShaderEffect;
        private DrawEffectContext drawEffectContext;

        public TestComputeShader()
        {
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
            
            drawEffectContext = new DrawEffectContext(Services);
            computeShaderEffect = new ComputeEffectShader(drawEffectContext) { ShaderSourceName = "ComputeShaderTest", ThreadGroupCounts = groupCounts };

            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderResult });
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderComputeShader });
        }

        private void RenderComputeShader(RenderContext obj)
        {
            computeShaderEffect.Parameters.Set(ComputeShaderTestKeys.NbOfIterations, ReductionRatio);
            computeShaderEffect.Parameters.Set(ComputeShaderTestKeys.input, inputTexture);
            computeShaderEffect.Parameters.Set(ComputeShaderTestKeys.output, outputTexture);
            computeShaderEffect.Draw();
        }

        private void RenderResult(RenderContext obj)
        {
            if (displayedTexture == null || spriteBatch == null)
                return;

            GraphicsDevice.DrawTexture(displayedTexture);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.I))
                displayedTexture = inputTexture;

            if (Input.IsKeyPressed(Keys.O))
                displayedTexture = outputTexture;
        }

        public static void Main()
        {
            using (var game = new TestComputeShader())
                game.Run();
        }
    }
}