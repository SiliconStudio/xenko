// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class FbxEntityTestGame : GraphicsTestBase
    {
        public FbxEntityTestGame()
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
            renderPipeline.Parameters.Set(TransformationKeys.View, Matrix.LookAtRH(new Vector3(9000f, 9000f, 12000.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitZ));
            renderPipeline.Parameters.Set(TransformationKeys.Projection, Matrix.PerspectiveFovRH((float)Math.PI * 0.4f, 1.3f, 100.0f, 100000.0f));

            // load the model
            var factoryEntity = Asset.Load<Entity>("factory");

            // Add the model to the entity system.
            Entities.Add(factoryEntity);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        [Test]
        public void RunFbxEntityTest()
        {
            RunGameTest(new FbxEntityTestGame());
        }

        public static void Main()
        {
            using (var testGame = new FbxEntityTestGame())
                testGame.Run();
        }
    }
}