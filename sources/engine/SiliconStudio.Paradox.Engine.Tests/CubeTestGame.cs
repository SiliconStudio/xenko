// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class CubeTestGame : GraphicsTestBase
    {
        private RenderModel renderModel;

        public CubeTestGame()
        {
            CurrentVersion = 1;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Create Main pass
            var mainPipeline = RenderSystem.Pipeline;
            mainPipeline.Renderers.Add(new ModelRenderer(Services, "Default"));
            mainPipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.Blue, RenderTarget = GraphicsDevice.BackBuffer, DepthStencil = GraphicsDevice.DepthStencilBuffer });

            // Create Mesh with Color stream
            var mesh = new Mesh { Draw = GeometricPrimitive.Cone.New(GraphicsDevice, Color.GreenYellow, 500.0f, 500.0f).ToMeshDraw(), Material = new Material()};
            mesh.Material.Parameters.Set(MaterialParameters.AlbedoMaterial, new ShaderMixinSource { Compositions = { { "albedoDiffuse", new ShaderClassSource("ComputeColorStream") } } });

            // Setup view
            var renderPipeline = RenderSystem.Pipeline;
            renderPipeline.Parameters.Set(TransformationKeys.View, Matrix.LookAtRH(new Vector3(1000.0f, 800.0f, 1000.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ));
            renderPipeline.Parameters.Set(TransformationKeys.Projection, Matrix.PerspectiveFovRH((float)Math.PI * 0.4f, 1.3f, 1.0f, 10000.0f));

            var model = new Model { mesh };

            // Create render model with main pipeline
            renderModel = new RenderModel(mainPipeline, model);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Collect pipeline
            var renderPipeline = RenderSystem.Pipeline;

            // Add model to rendering
            var modelStates = renderPipeline.GetOrCreateModelRendererState();
            modelStates.RenderModels.Clear();
            modelStates.RenderModels.Add(renderModel);

            base.Draw(gameTime);
        }

        [Test]
        public void RunCubeTest()
        {
            RunGameTest(new CubeTestGame());
        }

        public static void Main()
        {
            using (var testGame = new CubeTestGame())
                testGame.Run();
        }
    }
}