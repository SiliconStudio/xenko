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
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.ProceduralModels;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestShadows : GraphicTestGameBase
    {
        private const float PlaneSize = 20.0f;
        private const float HalfPlaneSize = PlaneSize*0.5f;
        private const int InitialLightCount = 8;
        
        private Material material;
        private Entity lightEntity;
        private Entity lightEntity1;
        private Entity cameraEntity;
        private List<LightComponent> pointLights = new List<LightComponent>();
        private SpriteFont font;

        private Stopwatch lightTimer = new Stopwatch();
        private float lightRotationOffset = 0.0f;
        private float lightDistance = 5.0f;

        // Initial shadow map settings
        private LightPointShadowMapType shadowMapType = LightPointShadowMapType.CubeMap;
        private LightShadowMapSize shadowMapSize = LightShadowMapSize.Medium;
        private float shadowMapBias = 0.05f;
        private int shadowMapFilter = 0;
        
        public TestShadows()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_10_0 };
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
        }

        Entity GenerateTeapot()
        {
            // Create a cube entity
            var cubeEntity = new Entity();
            var model = new Model();
            model.Materials.Add(material);
            cubeEntity.Add(new ModelComponent(model));

            var modelDescriptor = new ProceduralModelDescriptor(new TeapotProceduralModel() { Tessellation = 3 });
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

            Input.ActivatedGestures.Add(new GestureConfigComposite
            {
                RequiredNumberOfFingers = 2,
            });

            ProfilerSystem.EnableProfiling(false, GameProfilingKeys.GameDrawFPS);
            ProfilerSystem.EnableProfiling(false, ProfilingKeys.Engine);

            Window.AllowUserResizing = true;

            var scene = new Scene();

            // Create diffuse material
            material = Material.New(GraphicsDevice, new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.White)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
            });


            Random random = new Random(11324);

            // Random teapots
            for (int i = 0; i < 32; i++)
            {
                var cube = GenerateTeapot();
                cube.Transform.Position = new Vector3((float)random.NextDouble()*PlaneSize - HalfPlaneSize,
                    (float)random.NextDouble()*PlaneSize,
                    (float)random.NextDouble()*PlaneSize - HalfPlaneSize);
                cube.Transform.Rotation = Quaternion.RotationYawPitchRoll((float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi,
                    (float)random.NextDouble()*MathUtil.TwoPi);
                scene.Entities.Add(cube);
            }

            {
                var planeLeft = GeneratePlane();
                planeLeft.Transform.RotationEulerXYZ = new Vector3(0, 0, -MathUtil.PiOverTwo);
                planeLeft.Transform.Position = new Vector3(-HalfPlaneSize, HalfPlaneSize, 0.0f);
                scene.Entities.Add(planeLeft);

                var planeBack = GeneratePlane();
                planeBack.Transform.RotationEulerXYZ = new Vector3(-MathUtil.PiOverTwo, MathUtil.Pi + 0.1f, 0);
                planeBack.Transform.Position = new Vector3(0.0f, HalfPlaneSize, -HalfPlaneSize);
                scene.Entities.Add(planeBack);

                var planeBottom = GeneratePlane();
                scene.Entities.Add(planeBottom);
            }

            {
                var planeRight = GeneratePlane();
                planeRight.Transform.RotationEulerXYZ = new Vector3(0, 0, MathUtil.PiOverTwo);
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
            cameraEntity.Transform.Position = new Vector3(PlaneSize*1.5f, HalfPlaneSize, PlaneSize*1.5f);
            cameraEntity.Transform.RotationEulerXYZ = new Vector3(0.0f, MathUtil.PiOverFour, 0.0f);
            cameraEntity.Add(new LightComponent
            {
                Type = new LightAmbient
                {
                    Color = new ColorRgbProvider(new Color((Color4.White*0.1f).ToRgba()))
                }
            });
            scene.Entities.Add(cameraEntity);
            
            FpsTestCamera camera = new FpsTestCamera();
            cameraEntity.Add(camera);

            // Load default graphics compositor
            SceneSystem.GraphicsCompositor = GraphicsCompositor.CreateDefault(true);// Content.Load<GraphicsCompositor>("GraphicsCompositor");
            SceneSystem.GraphicsCompositor.Cameras[0] = cameraEntity.Get<CameraComponent>();

            // Create a scene instance
            SceneSystem.SceneInstance = new SceneInstance(Services, scene);

            //font = Content.Load<SpriteFont>("Font");
            //BuildUI();

            // Create initial set of lights
            RegenerateLights(InitialLightCount);
        }

        private Button CustomButtom(string text, Action action)
        {
            var button = new Button
            {
                Content = new TextBlock { Font = font, Text = text, TextSize = 0.0f, VerticalAlignment = VerticalAlignment.Center },
                BackgroundColor = Color.Gray,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            button.Click += (sender, args) => action();
            return button;
        }

        private void BuildUI()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_IOS
            var width = 1000;
#else
            var width = 1920;
#endif
            var bufferRatio = GraphicsDevice.Presenter.BackBuffer.Width/(float)GraphicsDevice.Presenter.BackBuffer.Height;
            var ui = new UIComponent { Resolution = new Vector3(width, width/bufferRatio, 500) };
            SceneSystem.SceneInstance.RootScene.Entities.Add(new Entity { ui });

            ui.Page = new UIPage
            {
                RootElement = new StackPanel()
                {
                    BackgroundColor = Color.Black.WithAlpha(128),
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Children =
                    {
                        CustomButtom("Map Type", () => ToggleShadowMapType()),
                        new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Children =
                            {
                                CustomButtom("Bias-", () => AdjustBias(-0.1f)),
                                CustomButtom("Bias+", () => AdjustBias(0.1f)),
                            }
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Children =
                            {
                                CustomButtom("Filter-", () => AdjustFilter(-1)),
                                CustomButtom("Filter+", () => AdjustFilter(1)),
                            }
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Children =
                            {
                                CustomButtom("Lights-", () => AdjustLightCount(-1)),
                                CustomButtom("Lights+", () => AdjustLightCount(1)),
                            }
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Children =
                            {
                                CustomButtom("MapSize-", () => AdjustMapSize(-1)),
                                CustomButtom("MapSize+", () => AdjustMapSize(1)),
                            }
                        },
                        CustomButtom("Light Movement", () => ToggleLightMovement())
                    }
                }
            };
        }

        void ToggleShadowMapType()
        {
            shadowMapType = (LightPointShadowMapType)(((int)shadowMapType + 1)%2);

            foreach (var lc in pointLights)
            {
                var point = lc.Type as LightPoint;
                var shadow = point.Shadow as LightPointShadowMap;

                shadow.Type = shadowMapType;
            }
        }

        void AdjustBias(float adjustBias)
        {
            if (adjustBias != 0.0f)
            {
                adjustBias *= (float)UpdateTime.Elapsed.TotalSeconds;
                shadowMapBias += adjustBias;
                if (shadowMapBias < 0.0f)
                    shadowMapBias = 0.0f;

                foreach (var lc in pointLights)
                {
                    var point = lc.Type as LightPoint;
                    var shadow = point.Shadow as LightPointShadowMap;
                    shadow.BiasParameters.DepthBias = shadowMapBias;
                }
            }
        }

        void AdjustMapSize(int adjustSize)
        {
            if (adjustSize != 0)
            {
                shadowMapSize = (LightShadowMapSize)MathUtil.Clamp((int)shadowMapSize + adjustSize, 0, (int)LightShadowMapSize.XLarge);
                foreach (var lc in pointLights)
                {
                    var point = lc.Type as LightPoint;
                    point.Shadow.Size = shadowMapSize;
                }
            }
        }

        void AdjustLightCount(int adjustSize)
        {
            var targetCount = MathUtil.Clamp(pointLights.Count + adjustSize, 0, 64);
            RegenerateLights(targetCount);
        }

        void RegenerateLights(int amount)
        {
            var scene = SceneSystem.SceneInstance.RootScene;
            foreach (var l in pointLights)
            {
                scene.Entities.Remove(l.Entity);
            }
            pointLights.Clear();

            // Always use the same random number source
            Random random = new Random(1527918523);

            // Create lights
            for (int i = 0; i < amount; i++)
            {
                var lightType = new LightPoint();
                lightType.Shadow.Enabled = true;
                (lightType.Shadow as LightPointShadowMap).Type = shadowMapType;
                lightType.Shadow.BiasParameters.DepthBias = shadowMapBias;
                lightType.Shadow.Size = shadowMapSize;
                SetFilterType(lightType, shadowMapFilter);
                Color4 color = new ColorHSV((float)random.NextDouble()*360.0f, 1.0f, 1.0f, 1.0f).ToColor();
                lightType.Color = new ColorRgbProvider(new Color3(color.R, color.G, color.B));
                lightType.Radius = HalfPlaneSize;

                var lightComponent = new LightComponent { Type = lightType, Intensity = 60.0f/amount };
                pointLights.Add(lightComponent);
                lightEntity1 = GenerateSphere();
                lightEntity1.Add(lightComponent);
                lightEntity1.Get<ModelComponent>().IsShadowCaster = false;
                lightEntity1.Get<ModelComponent>().Materials[0] = Material.New(GraphicsDevice, new MaterialDescriptor
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.White)),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                        Emissive = new MaterialEmissiveMapFeature(new ComputeColor(color))
                    }
                });
                lightEntity1.Transform.Position = new Vector3(0, HalfPlaneSize, 0);
                scene.Entities.Add(lightEntity1);
            }
        }

        void AdjustFilter(int adjustFilter)
        {
            if (adjustFilter != 0)
            {
                shadowMapFilter = MathUtil.Clamp(shadowMapFilter + adjustFilter, 0, 3);
                foreach (var lc in pointLights)
                {
                    var point = lc.Type as LightPoint;
                    SetFilterType(point, shadowMapFilter);
                }
            }
        }

        void SetFilterType(LightPoint point, int type)
        {
            var shadow = point.Shadow as LightPointShadowMap;

            if (type == 0)
                shadow.Filter = null;
            else
            {
                shadow.Filter = new LightShadowMapFilterTypePcf { FilterSize = (LightShadowMapFilterTypePcfSize)(type - 1) };
            }
        }

        void ToggleLightMovement()
        {
            if (lightTimer.IsRunning)
                lightTimer.Stop();
            else
                lightTimer.Start();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Manual light rotation
            if (Input.IsMouseButtonDown(MouseButton.Left))
                lightRotationOffset += Input.MouseDelta.X*1.5f;

            // Toggle automatic light rotation
            if (Input.IsKeyPressed(Keys.Space))
                ToggleLightMovement();

            // Toggle shadow map type
            if (Input.IsKeyPressed(Keys.F4))
            {
                ToggleShadowMapType();
            }

            foreach (var g in Input.GestureEvents)
            {
                var c = g as GestureEventComposite;
                if (c != null)
                {
                    lightRotationOffset += c.DeltaTranslation.X*1.5f;
                    lightDistance += c.DeltaTranslation.Y*2.0f;
                }
            }

            // Adjust bias 
            float adjustBias = 0.0f;
            if (Input.IsKeyDown(Keys.NumPad2))
                adjustBias = 0.01f;
            else if (Input.IsKeyDown(Keys.NumPad1))
                adjustBias = -0.01f;
            AdjustBias(adjustBias);

            // Adjust filter 
            int adjustFilter = 0;
            if (Input.IsKeyPressed(Keys.NumPad5))
                adjustFilter = 1;
            else if (Input.IsKeyPressed(Keys.NumPad4))
                adjustFilter = -1;
            AdjustFilter(adjustFilter);

            // TODO INPUT MANAGER: Remove 120
            lightDistance += Input.MouseWheelDelta/120.0f*0.25f;

            // Rotate the light on the timer + offset
            for (int i = 0; i < pointLights.Count; i++)
            {
                float phase = (i*-0.2f) + (float)(lightTimer.Elapsed.TotalSeconds + lightRotationOffset)*(1.0f - i*0.3f);
                float distMult = (float)(Math.Cos(phase*0.25f)*0.5f + 1.0f)*0.2f + 0.8f;
                float lightX = (float)Math.Cos(phase)*lightDistance*distMult;
                float lightZ = (float)Math.Sin(phase)*lightDistance*distMult;
                float lightY = (float)-Math.Sin(phase*0.5f)*lightDistance*distMult + HalfPlaneSize;
                pointLights[i].Entity.Transform.Position = new Vector3(lightX, lightY, lightZ);
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