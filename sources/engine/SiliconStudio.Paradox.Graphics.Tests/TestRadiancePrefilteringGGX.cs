// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.ComputeEffect.GGXPrefiltering;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestRadiancePrefilteringGGX : TestGameBase
    {
        private SpriteBatch spriteBatch;

        private DrawEffectContext drawEffectContext;

        private Texture inputCubemap;
        private Texture outputCubemap;
        private Texture[] displayedViews = new Texture[6];

        private RadiancePrefilteringGGX radianceFilter;

        private Int2 screenSize = new Int2(768, 1024);

        private int outputSize = 256;

        private int displayedLevel = 0;
        private int mipmapCount = 6;
        private int samplingCounts = 1024;

        private bool skipHighestLevel;

        private Effect spriteEffect;

        public TestRadiancePrefilteringGGX()
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = screenSize.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenSize.Y;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var outputSizeLog2 = (int)Math.Round(Math.Log(outputSize) / Math.Log(2));

            drawEffectContext = new DrawEffectContext(this);
            radianceFilter = new RadiancePrefilteringGGX(drawEffectContext);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            inputCubemap = Asset.Load<Texture>("CubeMap");
            outputCubemap = Texture.New2D(GraphicsDevice, outputSize, outputSize, outputSizeLog2, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource, 6).DisposeBy(this);
            CreateViewsFor(outputCubemap);

            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.Zero });
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderCubeMap });
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = PrefilterCubeMap });
        }

        private void PrefilterCubeMap(RenderContext obj)
        {
            radianceFilter.DoNotFilterHighestLevel = skipHighestLevel;
            radianceFilter.MipmapGenerationCount = mipmapCount;
            radianceFilter.SamplingsCount = samplingCounts;
            radianceFilter.RadianceMap = inputCubemap;
            radianceFilter.PrefilteredRadiance = outputCubemap;
            radianceFilter.Draw();
        }

        private void RenderCubeMap(RenderContext obj)
        {
            if (displayedViews == null || spriteBatch == null)
                return;

            spriteEffect = EffectSystem.LoadEffect("SpriteEffectWithGamma");

            var size = new Vector2(screenSize.X / 3f, screenSize.Y / 4f);

            spriteBatch.Begin(SpriteSortMode.Texture, spriteEffect);
            spriteBatch.Draw(displayedViews[1], new RectangleF(0, size.Y, size.X, size.Y), Color.White);
            spriteBatch.Draw(displayedViews[2], new RectangleF(size.X, 0f, size.X, size.Y), Color.White);
            spriteBatch.Draw(displayedViews[4], new RectangleF(size.X, size.Y, size.X, size.Y), Color.White);
            spriteBatch.Draw(displayedViews[3], new RectangleF(size.X, 2f * size.Y, size.X, size.Y), Color.White);
            spriteBatch.Draw(displayedViews[5], new RectangleF(size.X, 3f * size.Y, size.X, size.Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
            spriteBatch.Draw(displayedViews[0], new RectangleF(2f * size.X, size.Y, size.X, size.Y), Color.White);
            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.NumPad0))
                skipHighestLevel = !skipHighestLevel;

            if (Input.IsKeyPressed(Keys.NumPad4))
                mipmapCount = Math.Max(1, mipmapCount - 1);

            if (Input.IsKeyPressed(Keys.NumPad6))
                mipmapCount = Math.Min(10, mipmapCount + 1);

            if (Input.IsKeyPressed(Keys.Left))
                samplingCounts = Math.Max(1, samplingCounts / 2);

            if (Input.IsKeyPressed(Keys.Right))
                samplingCounts = Math.Min(1024, samplingCounts * 2);

            if (Input.IsKeyPressed(Keys.I))
                CreateViewsFor(inputCubemap);

            if (Input.IsKeyPressed(Keys.O))
                CreateViewsFor(outputCubemap);

            if (Input.IsKeyPressed(Keys.S))
                SaveTexture(GraphicsDevice.BackBuffer, "RadiancePrefilteredGGXCross_level{0}.png".ToFormat(displayedLevel));
        }

        private void CreateViewsFor(Texture texture)
        {
            for (int i = 0; i < displayedViews.Length; i++)
            {
                if (displayedViews[i] != null)
                {
                    displayedViews[i].Dispose();
                    displayedViews[i] = null;
                }
            }
            for (int i = 0; i < texture.ArraySize; i++)
            {
                displayedViews[i] = texture.ToTextureView(ViewType.Single, i, displayedLevel);
            }
        }

        public static void Main()
        {
            using (var game = new TestRadiancePrefilteringGGX())
                game.Run();
        }
    }
}