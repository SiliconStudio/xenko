// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Effects.Modules.Processors;
using SiliconStudio.Paradox.Effects.Modules.Renderers;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestCubemapDeferred : TestGameBase
    {
        private LightingIBLRenderer IBLRenderer;

        private Entity teapotEntity;

        private Entity cubemapEntity;

        public TestCubemapDeferred()
        {
            // cannot render cubemap in level below 10.1
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // create pipeline
            CreatePipeline();

            // setup the scene
            var material = Asset.Load<Material>("BasicMaterial");
            teapotEntity = new Entity()
            {
                new ModelComponent()
                {
                    Model = new Model()
                    {
                        new Mesh()
                        {
                            Draw = GeometricPrimitive.Teapot.New(GraphicsDevice).ToMeshDraw(),
                            Material = material
                        }
                    }
                }
            };
            Entities.Add(teapotEntity);

            var textureCube = Asset.Load<TextureCube>("uv_cube");
            cubemapEntity = new Entity()
            {
                new CubemapSourceComponent(textureCube) { Enabled = true, InfluenceRadius = 1.5f, IsDynamic = false },
                new TransformationComponent() { Translation = Vector3.UnitZ }
            };
            Entities.Add(cubemapEntity);

            var mainCamera = new Entity()
            {
                new CameraComponent
                {
                    AspectRatio = 8/4.8f,
                    FarPlane = 1000,
                    NearPlane = 1,
                    VerticalFieldOfView = 0.6f,
                    Target = teapotEntity,
                    TargetUp = Vector3.UnitY,
                },
                new TransformationComponent
                {
                    Translation = new Vector3(4, 3, 0)
                }
            };
            Entities.Add(mainCamera);

            RenderSystem.Pipeline.SetCamera(mainCamera.Get<CameraComponent>());

            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            // Processor
            Entities.Processors.Add(new CubemapSourceProcessor(GraphicsDevice));

            // Rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            
            // Create G-buffer pass
            var gbufferPipeline = new RenderPipeline("GBuffer");
            // Renders the G-buffer for opaque geometry.
            gbufferPipeline.Renderers.Add(new ModelRenderer(Services, "CubemapIBLEffect.ParadoxGBufferShaderPass"));
            var gbufferProcessor = new GBufferRenderProcessor(Services, gbufferPipeline, GraphicsDevice.DepthStencilBuffer, false);

            // Add sthe G-buffer pass to the pipeline.
            RenderSystem.Pipeline.Renderers.Add(gbufferProcessor);
            IBLRenderer = new LightingIBLRenderer(Services, GraphicsDevice.DepthStencilBuffer);
            RenderSystem.Pipeline.Renderers.Add(IBLRenderer);
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services) { ClearColor = Color.CornflowerBlue });
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "CubemapIBLEffect"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = ShowIBL });
        }

        private void ShowIBL(RenderContext context)
        {
            GraphicsDevice.DrawTexture(IBLRenderer.IBLTexture);
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                teapotEntity.Transformation.Rotation = Quaternion.RotationY((float)(2 * Math.PI * UpdateTime.Total.TotalMilliseconds / 5000.0f));
                cubemapEntity.Transformation.Translation = new Vector3(0, 0, 1 + (float)Math.Cos(2 * Math.PI * UpdateTime.Total.TotalMilliseconds / 5000.0f));
            }
        }

        public static void Main()
        {
            using (var game = new TestCubemapDeferred())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunCubemapRendering()
        {
            RunGameTest(new TestCubemapDeferred());
        }
    }
}