// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
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
using SiliconStudio.Xenko.Rendering.Shadows;

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
        private List<LightComponent> pointLights = new List<LightComponent>();

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
            ProfilerSystem.EnableProfiling(false, GameProfilingKeys.GameDrawFPS);
            ProfilerSystem.EnableProfiling(false, ProfilingKeys.Engine);

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
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3((float)random.NextDouble()*PlaneSize - HalfPlaneSize,
                    (float)random.NextDouble()*3.0f + 0.2f,
                    (float)random.NextDouble()*PlaneSize - HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                scene.Entities.Add(cube);
            }

            for (int i = 0; i < 32; i++)
            {
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3((float)random.NextDouble()*PlaneSize - HalfPlaneSize,
                    (float)random.NextDouble()*-3.0f + PlaneSize - 0.2f,
                    (float)random.NextDouble()*PlaneSize - HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                scene.Entities.Add(cube);
            }

            for (int i = 0; i < 16; i++)
            {
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3(((float)random.NextDouble() * 2.0f - 1.0f) * HalfPlaneSize,
                    (float)random.NextDouble()*PlaneSize,
                    ((float)random.NextDouble() * 2.0f - 1.0f) * HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                cube.Transform.Scale = Vector3.One*1.5f;
                scene.Entities.Add(cube);
            }

            //var cube1 = GenerateCube(2.0f);
            //cube1.Transform.Position = new Vector3(0.0f, HalfPlaneSize, 0.0f);
            //cube1.Transform.Scale = new Vector3(1.0f);
            //scene.Entities.Add(cube1);

            {
                var planeLeft = GeneratePlane();
                planeLeft.Transform.RotationEulerXYZ = new Vector3(0, 0, -MathUtil.PiOverTwo);
                planeLeft.Transform.Position = new Vector3(-HalfPlaneSize, HalfPlaneSize, 0.0f);
                scene.Entities.Add(planeLeft);

                var planeBack = GeneratePlane();
                planeBack.Transform.RotationEulerXYZ = new Vector3(-MathUtil.PiOverTwo, MathUtil.Pi, 0);
                planeBack.Transform.Position = new Vector3(0.0f, HalfPlaneSize, -HalfPlaneSize);
                scene.Entities.Add(planeBack);

                var planeBottom = GeneratePlane();
                scene.Entities.Add(planeBottom);
            }

            {
                var planeRight = GeneratePlane();
                planeRight.Transform.RotationEulerXYZ = new Vector3(0,0,MathUtil.PiOverTwo);
                planeRight.Transform.Position = new Vector3(HalfPlaneSize, HalfPlaneSize, 0.0f);
                scene.Entities.Add(planeRight);

                var planeFront = GeneratePlane();
                planeFront.Transform.RotationEulerXYZ = new Vector3(MathUtil.PiOverTwo, MathUtil.Pi, 0);
                planeFront.Transform.Position = new Vector3(0.0f, HalfPlaneSize, HalfPlaneSize);
                scene.Entities.Add(planeFront);

                var planeTop = GeneratePlane();
                planeTop.Transform.RotationEulerXYZ = new Vector3(MathUtil.Pi, 0, 0);
                planeTop.Transform.Position = new Vector3(0.0f, PlaneSize, 0.0f);
                scene.Entities.Add(planeTop);
            }

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
            for(int i = 0; i < 64; i++)
            {
                var lightType = new LightPoint();
                lightType.Shadow.Enabled = true;
                (lightType.Shadow as LightPointShadowMap).Type = LightPointShadowMapType.DualParaboloid;
                lightType.Shadow.Size = LightShadowMapSize.XSmall;
                //lightType.Shadow.Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter7x7 };
                Color4 color = new ColorHSV((float)random.NextDouble()*360.0f, 1.0f, 1.0f, 1.0f).ToColor();
                lightType.Color = new ColorRgbProvider(new Color3(color.R, color.G, color.B));
                lightType.Radius = PlaneSize;

                var lightComponent = new LightComponent { Type = lightType, Intensity = 15.0f };
                var lightSubEntity = new Entity { lightComponent };
                pointLights.Add(lightComponent);
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
            //    lightEntity.Transform.RotationEulerXYZ = new Vector3(-MathUtil.PiOverFour, MathUtil.PiOverFour, 0.0f);
            //    lightEntity.Transform.UpdateWorldMatrix();
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
        private float lightDistance = 4.0f;

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

            // Toggle shadow map type
            if (Input.IsKeyPressed(Keys.F4))
            {
                foreach (var lc in pointLights)
                {
                    var point = lc.Type as LightPoint;
                    var shadow = point.Shadow as LightPointShadowMap;
                    if(shadow.Type == LightPointShadowMapType.Cubemap)
                        shadow.Type = LightPointShadowMapType.DualParaboloid;
                    else
                        shadow.Type = LightPointShadowMapType.Cubemap;
                }
            }

            // Adjust bias 
            float adjustBias = 0.0f;
            if (Input.IsKeyDown(Keys.NumPad2))
                adjustBias = 0.01f;
            else if (Input.IsKeyDown(Keys.NumPad1))
                adjustBias = -0.01f;
            if(adjustBias != 0.0f)
            {
                adjustBias *= (float)UpdateTime.Elapsed.TotalSeconds;
                foreach (var lc in pointLights)
                {
                    var point = lc.Type as LightPoint;
                    var shadow = point.Shadow as LightPointShadowMap;
                    shadow.BiasParameters.DepthBias += adjustBias;
                    if (shadow.BiasParameters.DepthBias < 0.0f)
                        shadow.BiasParameters.DepthBias = 0.0f;
                }
            }

            // Adjust filter 
            int adjustFilter = 0;
            if (Input.IsKeyPressed(Keys.NumPad5))
                adjustFilter = 1;
            else if (Input.IsKeyPressed(Keys.NumPad4))
                adjustFilter = -1;
            if (adjustFilter != 0)
            {
                foreach (var lc in pointLights)
                {
                    var point = lc.Type as LightPoint;
                    var shadow = point.Shadow as LightPointShadowMap;
                    int current = 0;
                    var pcf = shadow.Filter as LightShadowMapFilterTypePcf;
                    if (pcf != null)
                        current = 1 + (int)pcf.FilterSize;

                    current = MathUtil.Clamp(current+adjustFilter, 0, 3);

                    if (current == 0)
                        shadow.Filter = null;
                    else
                    {
                        shadow.Filter = new LightShadowMapFilterTypePcf { FilterSize = (LightShadowMapFilterTypePcfSize)(current - 1) };
                    }
                }
            }

            // TODO INPUT MANAGER: Remove 120
            lightDistance += Input.MouseWheelDelta/120.0f*0.25f;

            // Rotate the light on the timer + offset
            for(int i = 0; i < pointLights.Count; i++)
            {
                float phase = (i*-0.2f) + (float)lightTimer.Elapsed.TotalSeconds*(1.0f-i*0.3f);
                float distMult = (float)Math.Cos(phase * 0.25f + lightRotationOffset) * 0.5f + 1.5f;
                float lightX = (float)Math.Cos(phase + lightRotationOffset)* lightDistance * distMult;
                float lightZ = (float)Math.Sin(phase + lightRotationOffset)* lightDistance * distMult;
                float lightY = (float)-Math.Sin(phase * 0.5f + lightRotationOffset) * lightDistance * distMult;
                pointLights[i].Entity.Transform.Position = new Vector3(lightX, lightY, lightZ);
                //lightEntity1.Transform.Position = new Vector3(lightX, HalfPlaneSize, lightZ);
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