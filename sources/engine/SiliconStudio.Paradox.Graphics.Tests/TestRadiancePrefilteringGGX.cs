// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.ComputeEffect.GGXPrefiltering;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestRadiancePrefilteringGGX : TestGameBase
    {
        private SpriteBatch spriteBatch;

        private RenderContext drawEffectContext;

        private Texture inputCubemap;
        private Texture outputCubemap;
        private Texture displayedCubemap;
        private Texture[] displayedViews = new Texture[6];

        private RadiancePrefilteringGGX radianceFilter;

        private Int2 screenSize = new Int2(768, 1024);

        private int outputSize = 256;

        private int displayedLevel = 0;
        private int mipmapCount = 6;
        private int samplingCounts = 1024;

        private bool skipHighestLevel;

        private Effect spriteEffect;

        private bool filterAtEachFrame = true;
        private bool hasBeenFiltered;

        public TestRadiancePrefilteringGGX() : this(false)
        {
            
        }

        public TestRadiancePrefilteringGGX(bool filterAtEachFrame)
        {
            CurrentVersion = 2;
            this.filterAtEachFrame = filterAtEachFrame;
            GraphicsDeviceManager.PreferredBackBufferWidth = screenSize.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenSize.Y;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DisplayNextMipmapLevel).TakeScreenshot();
            FrameGameSystem.Draw(DisplayNextMipmapLevel).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            drawEffectContext = RenderContext.GetShared(Services);
            radianceFilter = new RadiancePrefilteringGGX(drawEffectContext);
            skipHighestLevel = radianceFilter.DoNotFilterHighestLevel;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            inputCubemap = Asset.Load<Texture>("CubeMap");
            outputCubemap = Texture.New2D(GraphicsDevice, outputSize, outputSize, MathUtil.Log2(outputSize), PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess, 6).DisposeBy(this);
            CreateViewsFor(outputCubemap);

            //RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = PrefilterCubeMap });
            //RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.Zero });
            //RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderCubeMap });
        }

        private void PrefilterCubeMap()
        {
            if (!filterAtEachFrame && hasBeenFiltered)
                return;

            radianceFilter.DoNotFilterHighestLevel = skipHighestLevel;
            radianceFilter.MipmapGenerationCount = mipmapCount;
            radianceFilter.SamplingsCount = samplingCounts;
            radianceFilter.RadianceMap = inputCubemap;
            radianceFilter.PrefilteredRadiance = outputCubemap;
            radianceFilter.Draw();

            hasBeenFiltered = true;
        }

        private void RenderCubeMap()
        {
            if (displayedViews == null || spriteBatch == null)
                return;

            spriteEffect = EffectSystem.LoadEffect("SpriteEffectWithGamma").WaitForResult();

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

            if (Input.IsKeyPressed(Keys.NumPad1))
                samplingCounts = Math.Max(1, samplingCounts / 2);

            if (Input.IsKeyPressed(Keys.NumPad3))
                samplingCounts = Math.Min(1024, samplingCounts * 2);

            if (Input.IsKeyPressed(Keys.Left))
                DisplayPreviousMipmapLevel();

            if (Input.IsKeyPressed(Keys.Right))
                DisplayNextMipmapLevel();

            if (Input.IsKeyPressed(Keys.I))
                CreateViewsFor(inputCubemap);

            if (Input.IsKeyPressed(Keys.O))
                CreateViewsFor(outputCubemap);

            if (Input.IsKeyPressed(Keys.S))
                SaveTexture(GraphicsDevice.BackBuffer, "RadiancePrefilteredGGXCross_level{0}.png".ToFormat(displayedLevel));
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            PrefilterCubeMap();

            RenderCubeMap();
        }

        private void DisplayPreviousMipmapLevel()
        {
            displayedLevel = Math.Max(0, displayedLevel - 1);
            CreateViewsFor(displayedCubemap);
        }

        private void DisplayNextMipmapLevel()
        {
            displayedLevel = Math.Min(mipmapCount - 1, displayedLevel + 1);
            CreateViewsFor(displayedCubemap);
        }

        private void CreateViewsFor(Texture texture)
        {
            displayedCubemap = texture;
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

        [Test]
        public void RunTest()
        {
            RunGameTest(new TestRadiancePrefilteringGGX());
        }

        public static void Main()
        {
            using (var game = new TestRadiancePrefilteringGGX(true))
                game.Run();
        }
    }
}