// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
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
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Colors;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestShadows : GraphicTestGameBase
    {
        private const float PlaneSize = 20.0f;
        private const float HalfPlaneSize = PlaneSize*0.5f;
        private Material material;
        private Entity lightEntity;
        private Entity lightEntity1;
        private Entity cameraEntity;

        public TestShadows()
        {
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
        }

        Entity GenerateTeapot()
        {
            // Create a cube entity
            var cubeEntity = new Entity();
            var model = new Model();
            model.Materials.Add(material);
            cubeEntity.Add(new ModelComponent(model));

            var modelDescriptor = new ProceduralModelDescriptor(new TeapotProceduralModel() { Tessellation = 5 });
            modelDescriptor.GenerateModel(Services, model);

            return cubeEntity;
        }

        Entity GenerateCube(float size = 1.0f)
        {
            // Create a cube entity
            var cubeEntity = new Entity();
            var model = new Model();
            model.Materials.Add(material);
            cubeEntity.Add(new ModelComponent(model));

            var modelDescriptor = new ProceduralModelDescriptor(new CubeProceduralModel() { Size = new Vector3(size) });
            modelDescriptor.GenerateModel(Services, model);

            return cubeEntity;
        }

        Entity GenerateSphere(float radius = 0.5f)
        {
            // Create a cube entity
            var cubeEntity = new Entity();
            var model = new Model();
            model.Materials.Add(material);
            cubeEntity.Add(new ModelComponent(model));

            var modelDescriptor = new ProceduralModelDescriptor(new SphereProceduralModel() { Radius = radius });
            modelDescriptor.GenerateModel(Services, model);

            return cubeEntity;
        }

        Entity GeneratePlane()
        {
            // Create plane
            var model = new Model();
            model.Materials.Add(material);
            var modelDescriptor = new ProceduralModelDescriptor(new PlaneProceduralModel { Size = new Vector2(PlaneSize), Tessellation = new Int2(16) });
            modelDescriptor.GenerateModel(Services, model);
            var planeEntity = new Entity { new ModelComponent(model) };
            return planeEntity;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            Window.AllowUserResizing = true;

            // Instantiate a scene with a single entity and model component
            var scene = new Scene();


            // Create a procedural model with a diffuse material
            material = Material.New(GraphicsDevice, new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.White)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
            });

            Random random = new Random(11324);
            for (int i = 0; i < 16; i++)
            {
                // Add the cube to the scene
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3((float)random.NextDouble()*PlaneSize - HalfPlaneSize,
                    (float)random.NextDouble()*2.0f + 0.2f,
                    (float)random.NextDouble()*PlaneSize - HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                scene.Entities.Add(cube);
            }

            for (int i = 0; i < 32; i++)
            {
                // Add the cube to the scene
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3((float)random.NextDouble()*PlaneSize - HalfPlaneSize,
                    (float)random.NextDouble()*-2.0f + PlaneSize - 0.2f,
                    (float)random.NextDouble()*PlaneSize - HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                scene.Entities.Add(cube);
            }

            for (int i = 0; i < 16; i++)
            {
                // Add the cube to the scene
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3((float)random.NextDouble()*2.0f + -HalfPlaneSize + 0.2f,
                    (float)random.NextDouble()*PlaneSize,
                    (float)random.NextDouble()*PlaneSize - HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                cube.Transform.Scale = Vector3.One*1.5f;
                scene.Entities.Add(cube);
            }

            var cube1 = GenerateCube(2.0f);
            cube1.Transform.Position = new Vector3(0.0f, HalfPlaneSize, 0.0f);
            cube1.Transform.Scale = new Vector3(1.0f);
            scene.Entities.Add(cube1);

            //var planeRight = GeneratePlane();
            //planeRight.Transform.RotationEulerXYZ = new Vector3(0,0,MathUtil.PiOverTwo);
            //planeRight.Transform.Position = new Vector3(HalfPlaneSize, HalfPlaneSize, 0.0f);
            //scene.Entities.Add(planeRight);

            var planeLeft = GeneratePlane();
            planeLeft.Transform.RotationEulerXYZ = new Vector3(0, 0, -MathUtil.PiOverTwo);
            planeLeft.Transform.Position = new Vector3(-HalfPlaneSize, HalfPlaneSize, 0.0f);
            scene.Entities.Add(planeLeft);

            var planeBack = GeneratePlane();
            planeBack.Transform.RotationEulerXYZ = new Vector3(-MathUtil.PiOverTwo, 0, MathUtil.PiOverTwo);
            planeBack.Transform.Position = new Vector3(0.0f, HalfPlaneSize, HalfPlaneSize);
            scene.Entities.Add(planeBack);

            //var planeFront = GeneratePlane();
            //planeFront.Transform.RotationEulerXYZ = new Vector3(MathUtil.PiOverTwo, 0, 0);
            //planeFront.Transform.Position = new Vector3(0.0f, HalfPlaneSize, -HalfPlaneSize);
            //scene.Entities.Add(planeFront);

            var planeBottom = GeneratePlane();
            scene.Entities.Add(planeBottom);

            //var planeTop = GeneratePlane();
            //planeTop.Transform.RotationEulerXYZ = new Vector3(MathUtil.Pi, 0, 0);
            //planeTop.Transform.Position = new Vector3(0.0f, PlaneSize, 0.0f);
            //scene.Entities.Add(planeTop);

            // Create a camera entity and add it to the scene
            cameraEntity = new Entity { new CameraComponent() };
            cameraEntity.Transform.Position = new Vector3(0, 0.8f, HalfPlaneSize*0.75f);
            scene.Entities.Add(cameraEntity);

            // Create a light
            //{
            //    var lightType = new LightDirectional();
            //    lightType.Shadow.Enabled = true;
            //    lightType.Shadow.Size = LightShadowMapSize.Large;
            //    lightType.Color = new ColorRgbProvider(Color.PaleVioletRed);
            //
            //
            //    var lightSubEntity = new Entity { new LightComponent { Type = lightType, Intensity = 0.5f } };
            //    lightEntity = GenerateSphere();
            //    lightEntity.Transform.RotationEulerXYZ = new Vector3(-MathUtil.PiOverFour, MathUtil.PiOverTwo, 0);
            //    lightEntity.Transform.UpdateWorldMatrix();
            //    lightEntity.Transform.Position = -lightEntity.Transform.WorldMatrix.Forward*PlaneSize;
            //    lightEntity.AddChild(lightSubEntity);
            //    lightEntity.Get<ModelComponent>().IsShadowCaster = false;
            //    lightEntity.Get<ModelComponent>().IsShadowReceiver = false;
            //    scene.Entities.Add(lightEntity);
            //}

            // Create a light
            {
                var lightType = new LightPoint();
                lightType.Shadow.Enabled = true;
                lightType.Shadow.Size = LightShadowMapSize.Large;
                lightType.Shadow.Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter7x7 };
                lightType.Color = new ColorRgbProvider(Color.White);
                lightType.Radius = PlaneSize*2.0f;

                var lightSubEntity = new Entity { new LightComponent { Type = lightType, Intensity = 15.0f } };
                lightEntity1 = GenerateSphere();
                lightEntity1.AddChild(lightSubEntity);
                lightEntity1.Get<ModelComponent>().IsShadowCaster = false;
                lightEntity1.Get<ModelComponent>().IsShadowReceiver = false;
                lightEntity1.Transform.Position = new Vector3(0, HalfPlaneSize, 0);
                scene.Entities.Add(lightEntity1);
            }

            // Create a light
            //{
            //    var lightType = new LightDirectional();
            //    lightType.Shadow.Enabled = true;
            //    lightType.Color = new ColorRgbProvider(Color.White);
            //
            //    var lightEntity = new Entity { new LightComponent { Type = lightType, Intensity = 0.2f } };
            //    lightEntity.Transform.Position = new Vector3(0, 2, 0);
            //    lightEntity.Transform.RotationEulerXYZ = new Vector3(MathUtil.DegreesToRadians(-85.0f), MathUtil.DegreesToRadians(45), 0.0f);
            //    scene.Entities.Add(lightEntity);
            //}

            // Create a graphics compositor
            var compositor = new SceneGraphicsCompositorLayers();

            bool isLDR = false;
            if (isLDR)
            {
                compositor.Master.Renderers.Add(new ClearRenderFrameRenderer() { Color = Color4.Black });
                compositor.Master.Renderers.Add(new SceneCameraRenderer());
            }
            else
            {
                var layer = new SceneGraphicsLayer();
                var renderHDROutput = new LocalRenderFrameProvider { Descriptor = { Format = RenderFrameFormat.HDR, DepthFormat = RenderFrameDepthFormat.Shared } };
                layer.Output = renderHDROutput;
                layer.Renderers.Add(new ClearRenderFrameRenderer() { Color = Color4.Black });
                layer.Renderers.Add(new SceneCameraRenderer());
                compositor.Layers.Add(layer);
                PostProcessingEffects postEffects;
                compositor.Master.Renderers.Add(new SceneEffectRenderer()
                {
                    Effect = postEffects = new PostProcessingEffects()
                });

                postEffects.AmbientOcclusion.Enabled = false;
                postEffects.DepthOfField.Enabled = false;
            }

            FpsTestCamera camera = new FpsTestCamera();
            cameraEntity.Add(camera);

            compositor.Cameras.Add(cameraEntity.Get<CameraComponent>());

            // Use this graphics compositor
            scene.Settings.GraphicsCompositor = compositor;

            // Create a scene instance
            SceneSystem.SceneInstance = new SceneInstance(Services, scene);
        }

        Stopwatch lightTimer = new Stopwatch();
        private float lightRotationOffset = 0.0f;

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            // Manual light rotation
            if (Input.IsMouseButtonDown(MouseButton.Left))
                lightRotationOffset += Input.MouseDelta.X*1.5f;

            // Toggle automatic light rotation
            if (Input.IsKeyPressed(Keys.Space))
                if (lightTimer.IsRunning)
                    lightTimer.Stop();
                else
                    lightTimer.Start();

            // Rotate the light on the timer + offset
            {
                float lightMoveRadius = 4.0f;
                float lightX = (float)Math.Cos(lightTimer.Elapsed.TotalSeconds + lightRotationOffset)*lightMoveRadius;
                float lightZ = (float)Math.Sin(lightTimer.Elapsed.TotalSeconds + lightRotationOffset)*lightMoveRadius;
                lightEntity1.Transform.Position = new Vector3(lightX, HalfPlaneSize, lightZ);
            }
        }

        public static void Main()
        {
            using (var game = new TestShadows())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunShadows()
        {
            RunGameTest(new TestShadows());
        }
    }
}