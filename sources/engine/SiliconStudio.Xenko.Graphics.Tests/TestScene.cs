// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.ProceduralModels;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestScene : GraphicTestGameBase
    {
        private Entity cubeEntity;

        public TestScene()
        {
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            Window.AllowUserResizing = true;

            // Instantiate a scene with a single entity and model component
            var scene = new Scene();

            // Create a cube entity
            cubeEntity = new Entity();

            // Create a procedural model with a diffuse material
            var model = new Model();
            var material = Material.New(GraphicsDevice, new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.White)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
            });
            model.Materials.Add(material);
            cubeEntity.Add(new ModelComponent(model));

            var modelDescriptor = new ProceduralModelDescriptor(new CubeProceduralModel());
            modelDescriptor.GenerateModel(Services, model);

            // Add the cube to the scene
            scene.Entities.Add(cubeEntity);

            // Create a camera entity and add it to the scene
            var cameraEntity = new Entity { new CameraComponent() };
            cameraEntity.Transform.Position = new Vector3(0, 0, 5);
            scene.Entities.Add(cameraEntity);

            // Create a light
            var lightEntity = new Entity()
            {
                new LightComponent()
            };

            lightEntity.Transform.Position = new Vector3(0, 0, 1);
            lightEntity.Transform.Rotation = Quaternion.RotationY(MathUtil.DegreesToRadians(45));
            scene.Entities.Add(lightEntity);

            // Create a graphics compositor
            var compositor = new SceneGraphicsCompositorLayers();

            bool isLDR = false;
            if (isLDR)
            {
                compositor.Master.Renderers.Add(new ClearRenderFrameRenderer());
                compositor.Master.Renderers.Add(new SceneCameraRenderer() {});
            }
            else
            {
                var layer = new SceneGraphicsLayer();
                var renderHDROutput = new LocalRenderFrameProvider { Descriptor = { Format = RenderFrameFormat.HDR, DepthFormat = RenderFrameDepthFormat.Shared} };
                layer.Output = renderHDROutput;
                layer.Renderers.Add(new ClearRenderFrameRenderer());
                layer.Renderers.Add(new SceneCameraRenderer());
                compositor.Layers.Add(layer);
                compositor.Master.Renderers.Add(new SceneEffectRenderer()
                {
                    Effect = new PostProcessingEffects()
                });
            }
            compositor.Cameras.Add(cameraEntity.Get<CameraComponent>());

            // Use this graphics compositor
            scene.Settings.GraphicsCompositor = compositor;

            // Create a scene instance
            SceneSystem.SceneInstance = new SceneInstance(Services, scene);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var time = (float)gameTime.Total.TotalSeconds;
            cubeEntity.Transform.Rotation = Quaternion.RotationY(time) * Quaternion.RotationX(time * 0.5f);

            //if(!ScreenShotAutomationEnabled)
            //    DrawCustomEffect();
        }

        //private void DrawCustomEffect()
        //{
        //    GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
        //    GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
        //    GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

        //    effectParameters.Set(MyCustomShaderKeys.ColorFactor2, (Vector4)Color.Red);
        //    effectParameters.Set(CustomShaderKeys.SwitchEffectLevel, switchEffectLevel);
        //    effectParameters.Set(TexturingKeys.Texture0, UVTexture);
        //    // TODO: Add switch Effect to test and capture frames
        //    //switchEffectLevel++;
        //    dynamicEffectCompiler.Update(effectInstance, null);

        //    GraphicsDevice.DrawQuad(effectInstance.Effect, effectParameters);
        //}

        public static void Main()
        {
            using (var game = new TestScene())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunCustomEffect()
        {
            RunGameTest(new TestScene());
        }
    }
}