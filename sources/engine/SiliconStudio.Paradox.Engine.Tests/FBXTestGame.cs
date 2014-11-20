// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class FbxTestGame : GraphicsTestBase
    {
        private RenderModel renderModel;
        private ModelViewHierarchyUpdater mvh;

        public FbxTestGame()
        {
            CurrentVersion = 1;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Create Main pass
            var mainPipeline = RenderSystem.Pipeline;
            mainPipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.Blue, RenderTarget = GraphicsDevice.BackBuffer, DepthStencil = GraphicsDevice.DepthStencilBuffer });
            mainPipeline.Renderers.Add(new ModelRenderer(Services, "Default"));

            // Setup view
            var renderPipeline = RenderSystem.Pipeline;
            renderPipeline.Parameters.Set(TransformationKeys.View, Matrix.LookAtLH(new Vector3(100.0f, 80.0f, 100.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ));
            renderPipeline.Parameters.Set(TransformationKeys.Projection, Matrix.PerspectiveFovLH((float)Math.PI * 0.4f, 1.3f, 1.0f, 1000.0f));

            // Load asset
            var sceneModel = Asset.Load<Model>("factory_model");

            // Create render model with main pipeline
            renderModel = new RenderModel(mainPipeline, sceneModel);
            mvh = new ModelViewHierarchyUpdater(sceneModel);
        }

        protected override void Update(GameTime time)
        {
            if (mvh != null)
            {
                mvh.UpdateMatrices();
                mvh.UpdateToRenderModel(renderModel);

                MeshSkinningUpdater.Update(mvh, renderModel);
            }

            base.Update(time);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Collect pipeline
            var renderPipeline = RenderSystem.Pipeline;
            var modelStates = renderPipeline.GetOrCreateModelRendererState();
            modelStates.RenderModels.Clear();
            modelStates.RenderModels.Add(renderModel);

            base.Draw(gameTime);
        }

        [Test]
        public void RunTestGame()
        {
            RunGameTest(new FbxTestGame());
        }

        public static void Main()
        {
            using (var testGame = new FbxTestGame())
                testGame.Run();
        }
    }
}