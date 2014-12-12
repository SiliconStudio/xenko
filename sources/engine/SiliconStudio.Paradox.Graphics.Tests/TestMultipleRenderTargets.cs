// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestMultipleRenderTargets : TestGameBase
    {
        private Texture[] textures;
        private int renderTargetToDisplayIndex = 0;
        private Entity teapot;

        public TestMultipleRenderTargets()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            var primitive = GeometricPrimitive.Teapot.New(GraphicsDevice);
            var material = Asset.Load<Material>("BasicMaterial");

            teapot = new Entity()
            {
                new ModelComponent()
                {
                    Model = new Model()
                    {
                        new Mesh()
                        {
                            Draw = primitive.ToMeshDraw(),
                            Material = material
                        }
                    }
                },
                new TransformationComponent()
            };
            Entities.Add(teapot);

            var mainCameraTargetEntity = new Entity(Vector3.Zero);
            Entities.Add(mainCameraTargetEntity);
            var mainCamera = new Entity()
            {
                new CameraComponent
                {
                    AspectRatio = 8/4.8f,
                    FarPlane = 5,
                    NearPlane = 1,
                    VerticalFieldOfView = 0.6f,
                    Target = mainCameraTargetEntity,
                    TargetUp = Vector3.UnitY,
                },
                new TransformationComponent
                {
                    Translation = new Vector3(2,1,2)
                }
            };
            Entities.Add(mainCamera);

            RenderSystem.Pipeline.SetCamera(mainCamera.Get<CameraComponent>());

            // Add a custom script
            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            // Create render targets
            textures = new Texture[3]
            {
                Texture.New2D(GraphicsDevice, 800, 480, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                Texture.New2D(GraphicsDevice, 800, 480, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                Texture.New2D(GraphicsDevice, 800, 480, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
            };

            var depthBuffer = Texture.New2D(GraphicsDevice, 800, 480, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);

            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new MultipleRenderTargetsSetter(Services)
            {
                ClearColor = Color.CornflowerBlue,
                RenderTargets = textures,
                DepthStencil = depthBuffer,
                ClearColors = new Color[] { Color.Black, Color.White, Color.Black }
            });
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "MultipleRenderTargetsEffect"));
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = DisplayGBuffer });
        }

        private void DisplayGBuffer(RenderContext context)
        {
            GraphicsDevice.DrawTexture(textures[renderTargetToDisplayIndex]);
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                var period = (float) (2 * Math.PI * UpdateTime.Total.TotalMilliseconds / 15000);
                teapot.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitY, period);

                if (Input.PointerEvents.Any(x => x.State == PointerState.Down))
                    renderTargetToDisplayIndex = (renderTargetToDisplayIndex + 1) % 3;
            }
        }

        public static void Main()
        {
            using (var game = new TestMultipleRenderTargets())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunMultipleRenderTargets()
        {
            RunGameTest(new TestMultipleRenderTargets());
        }
    }
}