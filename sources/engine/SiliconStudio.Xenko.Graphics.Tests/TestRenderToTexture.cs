// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    class TestRenderToTexture : GraphicTestGameBase
    {
        private Texture offlineTarget0;
        private Texture offlineTarget1;
        private Texture offlineTarget2;
        private Texture depthBuffer;
        private Matrix worldViewProjection;
        private GeometricPrimitive geometry;
        private EffectInstance simpleEffect;
        private bool firstSave;

        private int width;
        private int height;

        public TestRenderToTexture()
        {
            CurrentVersion = 2;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(RenderToTexture).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var view = Matrix.LookAtRH(new Vector3(2,2,2), new Vector3(0, 0, 0), Vector3.UnitY);
            var projection = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.ViewWidth / GraphicsDevice.BackBuffer.ViewHeight, 0.1f, 100.0f);
            worldViewProjection = Matrix.Multiply(view, projection);

            geometry = GeometricPrimitive.Cube.New(GraphicsDevice);
            simpleEffect = new EffectInstance(new Effect(GraphicsDevice, SpriteEffect.Bytecode));
            simpleEffect.Parameters.SetResourceSlow(TexturingKeys.Texture0, UVTexture);
            
            // TODO DisposeBy is not working with device reset
            offlineTarget0 = Texture.New2D(GraphicsDevice, 512, 512, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this);

            offlineTarget1 = Texture.New2D(GraphicsDevice, 512, 512, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this);
            offlineTarget2 = Texture.New2D(GraphicsDevice, 512, 512, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).DisposeBy(this);

            depthBuffer = Texture.New2D(GraphicsDevice, 512, 512, PixelFormat.D16_UNorm, TextureFlags.DepthStencil).DisposeBy(this);

            width = GraphicsDevice.BackBuffer.ViewWidth;
            height = GraphicsDevice.BackBuffer.ViewHeight;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                RenderToTexture();

            if (firstSave)
            {
                SaveTexture(UVTexture, "a_uvTex.png");
                SaveTexture(offlineTarget0, "a_firstRT.png");
                SaveTexture(offlineTarget2, "a_secondRT.png");
                firstSave = false;
            }
        }

        private void RenderToTexture()
        {
            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.Clear(offlineTarget0, Color.Black);
            GraphicsDevice.Clear(offlineTarget1, Color.Black);
            GraphicsDevice.Clear(offlineTarget2, Color.Black);

            // direct render
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            GraphicsDevice.SetViewport(new Viewport(0, 0, width / 2, height / 2));
            DrawGeometry();

            // 1 intermediate RT
            GraphicsDevice.Clear(depthBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(depthBuffer, offlineTarget0);
            DrawGeometry();

            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            GraphicsDevice.SetViewport(new Viewport(width / 2, 0, width / 2, height / 2));
            GraphicsDevice.DrawTexture(offlineTarget0);

            // 2 intermediate RTs
            GraphicsDevice.Clear(depthBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(depthBuffer, offlineTarget1);
            DrawGeometry();

            GraphicsDevice.Clear(depthBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(depthBuffer, offlineTarget2);
            GraphicsDevice.DrawTexture(offlineTarget1);

            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            GraphicsDevice.SetViewport(new Viewport(0, height / 2, width / 2, height / 2));
            GraphicsDevice.DrawTexture(offlineTarget2);

            // draw quad on screen
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            GraphicsDevice.SetViewport(new Viewport(width / 2, height / 2, width / 2, height / 2));
            GraphicsDevice.DrawTexture(UVTexture);
        }

        private void DrawGeometry()
        {
            simpleEffect.Parameters.SetValueSlow(SpriteBaseKeys.MatrixTransform, worldViewProjection);
            simpleEffect.Apply(GraphicsDevice);
            geometry.Draw();
        }

        public static void Main()
        {
            using (var game = new TestRenderToTexture())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunRenderToTexture()
        {
            RunGameTest(new TestRenderToTexture());
        }
    }
}
