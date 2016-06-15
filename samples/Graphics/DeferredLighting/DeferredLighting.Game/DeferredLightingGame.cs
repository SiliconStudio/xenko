using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;
using SiliconStudio.Xenko.UI.Panels;

namespace DeferredLighting
{
    public class DeferredLightingGame : Game
    {
        private Entity characterEntity;

        private List<Entity> pointLightEntities;

        private LightComponent directionalLight;
        private LightComponent spotLight;
        private List<LightComponent> pointLights;
        private CameraComponent camera;
        private bool rotateLights;
        private Button buttonShadow;
        private Button buttonSpotShadow;
        private Button buttonLightRotate;
        private float rotationFactor;

        private readonly Vector3 cameraInitPos = new Vector3(750, 0, 60);
        private readonly Vector3 characterInitPos = new Vector3(0, 0, 20);

        public DeferredLightingGame()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
            GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.None;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;

            pointLightEntities = new List<Entity>();
            pointLights = new List<LightComponent>();
        }

        protected override async Task LoadContent()
        {
            CreatePipeline();

            await base.LoadContent();

            IsMouseVisible = true;
            rotateLights = true;

            // load the model
            characterEntity = Content.Load<Entity>("character_00");
            characterEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5 * Math.PI));
            characterEntity.Transformation.Translation = characterInitPos;
            Entities.Add(characterEntity);

            // create the stand
            var material = Content.Load<Material>("character_00_material_mc00");
            var standEntity = CreateStand(material);
            standEntity.Transformation.Translation = new Vector3(0, 0, -80);
            standEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5 * Math.PI));
            Entities.Add(standEntity);

            var standBorderEntity = CreateStandBorder(material);
            standBorderEntity.Transformation.Translation = new Vector3(0, 0, -80);
            standBorderEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5 * Math.PI));
            Entities.Add(standBorderEntity);

            // create the lights
            var directLightEntity = CreateDirectLight(new Vector3(-1, 1, -1), new Color3(1, 1, 1), 0.9f);
            directionalLight = directLightEntity.Get<LightComponent>();
            Entities.Add(directLightEntity);

            var spotLightEntity = CreateSpotLight(new Vector3(0, -500, 600), new Vector3(0, -200, 0), 15, 20, new Color3(1, 1, 1), 0.9f);
            Entities.Add(spotLightEntity);
            spotLight = spotLightEntity.Get<LightComponent>();

            var rand = new Random();
            for (var i = -800; i <= 800; i = i + 200)
            {
                for (var j = -800; j <= 800; j = j + 200)
                {
                    var position = new Vector3(i, j, (float)(rand.NextDouble()*150));
                    var color = new Color3((float)rand.NextDouble() + 0.3f, (float)rand.NextDouble() + 0.3f, (float)rand.NextDouble() + 0.3f);
                    var light = CreatePointLight(position, color);
                    pointLights.Add(light.Get<LightComponent>());
                    pointLightEntities.Add(light);
                    Entities.Add(light);
                }
            }

            // set the camera
            var targetEntity = new Entity(characterInitPos);
            var cameraEntity = CreateCamera(cameraInitPos, targetEntity, (float)GraphicsDevice.BackBuffer.Width / (float)GraphicsDevice.BackBuffer.Height);
            camera = cameraEntity.Get<CameraComponent>();
            Entities.Add(cameraEntity);
            RenderSystem.Pipeline.SetCamera(camera);

            // UI
            CreateUI();

            // Add a custom script
            Script.Add(UpdateScene);
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderPipelineLightingFactory.CreateDefaultDeferred(this, "DeferredLightingEffectMain", "DeferredLightingPrepassEffect", Color.DarkBlue, true, true, "XenkoBackground");
        }

        private void CreateUI()
        {
            VirtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 1);

            var font = Content.Load<SpriteFont>("Font");
            var canvas = new Canvas();
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                MinimumWidth = 200
            };

            var buttonLightDirect = CreateButton("direct", GetButtonTextOnOff("Direct light: ", directionalLight.Enabled), font, Thickness.UniformRectangle(5));
            buttonLightDirect.Click += ToggleLight;

            var buttonLightPoint = CreateButton("point", GetButtonTextOnOff("Point lights: ", pointLights[0].Enabled), font, Thickness.UniformRectangle(5));
            buttonLightPoint.Click += ToggleLight;

            var buttonLightSpot = CreateButton("spot", GetButtonTextOnOff("Spot light: ", spotLight.Enabled), font, Thickness.UniformRectangle(5));
            buttonLightSpot.Click += ToggleLight;

            buttonShadow = CreateButton("direct", GetButtonTextOnOff("Shadow: ", directionalLight.ShadowMap), font, new Thickness(20, 5, 5, 5));
            buttonShadow.Opacity = directionalLight.Enabled ? 1.0f : 0.3f;
            buttonShadow.CanBeHitByUser = directionalLight.Enabled;
            buttonShadow.Click += ToggleShadowMap;

            buttonSpotShadow = CreateButton("spot", GetButtonTextOnOff("Shadow: ", spotLight.ShadowMap), font, new Thickness(20, 5, 5, 5));
            buttonSpotShadow.Opacity = spotLight.Enabled ? 1.0f : 0.3f;
            buttonSpotShadow.CanBeHitByUser = spotLight.Enabled;
            buttonSpotShadow.Click += ToggleShadowMap;

            buttonLightRotate = CreateButton("rotate", GetButtonTextOnOff("Lights rotation: ", rotateLights), font, new Thickness(20, 5, 5, 5));
            var enabled = pointLights.Count > 0 && pointLights[0].Enabled;
            buttonLightRotate.Opacity = enabled ? 1.0f : 0.3f;
            buttonLightRotate.CanBeHitByUser = enabled;
            buttonLightRotate.Click += ToogleRotation;

            stackPanel.Children.Add(buttonLightDirect);
            stackPanel.Children.Add(buttonShadow);
            stackPanel.Children.Add(buttonLightPoint);
            stackPanel.Children.Add(buttonLightRotate);
            stackPanel.Children.Add(buttonLightSpot);
            stackPanel.Children.Add(buttonSpotShadow);
            canvas.Children.Add(stackPanel);
            UI.RootElement = canvas;
        }

        private void ToggleShadowMap(Object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            if (button.Name == "direct")
            {
                directionalLight.ShadowMap = !directionalLight.ShadowMap;
                ((TextBlock)button.Content).Text = GetButtonTextOnOff("Shadow: ", directionalLight.ShadowMap);
            }
            else if (button.Name == "spot")
            {
                spotLight.ShadowMap = !spotLight.ShadowMap;
                ((TextBlock)button.Content).Text = GetButtonTextOnOff("Shadow: ", spotLight.ShadowMap);
            }
        }

        private void ToggleLight(Object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            if (button.Name == "direct")
            {
                directionalLight.Enabled = !directionalLight.Enabled;
                ((TextBlock)button.Content).Text = GetButtonTextOnOff("Direct light: ", directionalLight.Enabled);
                buttonShadow.Opacity = directionalLight.Enabled ? 1.0f : 0.3f;
                buttonShadow.CanBeHitByUser = directionalLight.Enabled;
            }
            else if (button.Name == "spot")
            {
                spotLight.Enabled = !spotLight.Enabled;
                ((TextBlock)button.Content).Text = GetButtonTextOnOff("Spot light: ", spotLight.Enabled);
                buttonSpotShadow.Opacity = directionalLight.Enabled ? 1.0f : 0.3f;
                buttonSpotShadow.CanBeHitByUser = directionalLight.Enabled;
            }
            else if (button.Name == "point")
            {
                var enabled = false;
                foreach (var point in pointLights)
                {
                    point.Enabled = !point.Enabled;
                    enabled = enabled || point.Enabled;
                }

                ((TextBlock)button.Content).Text = GetButtonTextOnOff("Point lights: ", enabled);
                buttonLightRotate.Opacity = enabled ? 1.0f : 0.3f;
                buttonLightRotate.CanBeHitByUser = enabled;
            }
        }

        private void ToogleRotation(Object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            rotateLights = !rotateLights;
            ((TextBlock) button.Content).Text = GetButtonTextOnOff("Lights rotation: ", rotateLights);
        }

        private async Task UpdateScene()
        {
            var dragValue = 0f;
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                if (rotateLights)
                {
                    // rotate the lights
                    foreach (var light in pointLightEntities)
                        light.Transformation.Translation = Vector3.TransformCoordinate(light.Transformation.Translation, Matrix.RotationAxis(Vector3.UnitZ, (float) (-0.005*Math.PI)));
                }

                var characterAnimationPeriod = 2 * Math.PI * (UpdateTime.Total.TotalMilliseconds % 10000) / 10000;
                characterEntity.Transformation.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)(0.5 * Math.PI));
                characterEntity.Transformation.Rotation *= Quaternion.RotationAxis(Vector3.UnitZ, (float)characterAnimationPeriod);

                characterEntity.Transformation.Translation = characterInitPos + new Vector3(0, 0, 10 * (float)Math.Sin(3 * characterAnimationPeriod));

                // rotate camera
                dragValue = 0.95f * dragValue;
                if (Input.PointerEvents.Count > 0)
                {
                    dragValue = Input.PointerEvents.Sum(x => x.DeltaPosition.X);
                }
                rotationFactor -= dragValue;
                camera.Position = Vector3.Transform(cameraInitPos, Quaternion.RotationZ((float)(2 * Math.PI * rotationFactor)));
            }
        }

        #region Helper functions

        private static string GetButtonTextOnOff(string baseString, bool enabled)
        {
            return baseString + (enabled ? "ON" : "OFF");
        }

        private static Button CreateButton(string name, string text, SpriteFont font, Thickness thickness)
        {
            return new Button
            {
                Name = name,
                Margin = thickness,
                Content = new TextBlock { Text = text, Font = font, TextAlignment = TextAlignment.Center },
            };
        }

        private Entity CreateStand(Material material)
        {
            var mesh = new Mesh
            {
                Draw = GeometricPrimitive.Cylinder.New(GraphicsDevice, 10, 720, 64, 6).ToMeshDraw(),
                Material = material
            };
            mesh.Parameters.Set(LightingKeys.ReceiveShadows, true);

            return new Entity()
            {
                new ModelComponent
                {
                    Model = new Model()
                    {
                        mesh
                    },
                    Parameters =
                    {
                        {TexturingKeys.Texture0, Content.Load<Texture>("TrainingFloor")},
                        {TexturingKeys.Sampler0, GraphicsDevice.SamplerStates.AnisotropicWrap},
                        {MaterialKeys.SpecularColorValue, 0.1f*Color4.White}
                    }
                }
            };
        }

        private Entity CreateStandBorder(Material material)
        {
            var mesh = new Mesh
            {
                Draw = GeometricPrimitive.Torus.New(GraphicsDevice, 720, 10, 64).ToMeshDraw(),
                Material = material
            };
            mesh.Parameters.Set(LightingKeys.ReceiveShadows, true);

            return new Entity()
            {
                new ModelComponent
                {
                    Model = new Model()
                    {
                        mesh
                    },
                    Parameters =
                    {
                        {TexturingKeys.Texture0, Content.Load<Texture>("red")},
                        {TexturingKeys.Sampler0, GraphicsDevice.SamplerStates.AnisotropicWrap},
                        {MaterialKeys.SpecularColorValue, 0.3f*Color4.White}
                    }
                }
            };
        }

        private static Entity CreateCamera(Vector3 position, Entity target, float aspectRatio)
        {
            return new Entity()
            {
                new CameraComponent
                {
                    AspectRatio = aspectRatio,
                    FarPlane = 10000,
                    NearPlane = 10,
                    Target = target,
                    TargetUp = Vector3.UnitZ,
                    VerticalFieldOfView = (float) Math.PI*0.2f
                },
                new TransformationComponent {Translation = position}
            };
        }

        private static Entity CreateSpotLight(Vector3 position, Vector3 target, float beamAngle, float fieldAngle, Color3 color, float intensity)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Type = LightType.Spot,
                    Color = color,
                    Deferred = true,
                    Enabled = true,
                    Intensity = intensity,
                    DecayStart = 500,
                    Layers = RenderLayers.RenderLayerAll,
                    LightDirection = target - position,
                    SpotBeamAngle = beamAngle,
                    SpotFieldAngle = fieldAngle,
                    ShadowMap = false,
                    ShadowFarDistance = 10000,
                    ShadowNearDistance = 10,
                    ShadowMapFilterType = ShadowMapFilterType.Nearest,
                    ShadowMapCascadeCount = 1
                },
                new TransformationComponent {Translation = position}
            };
        }

        private static Entity CreatePointLight(Vector3 position, Color3 color)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Enabled = true,
                    Color = color,
                    Intensity = 0.5f,
                    Layers = RenderLayers.RenderLayerAll,
                    Deferred = true,
                    Type = LightType.Point,
                    DecayStart = 120.0f
                },
                new TransformationComponent {Translation = position}
            };
        }

        private static Entity CreateDirectLight(Vector3 direction, Color3 color, float intensity)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Type = LightType.Directional,
                    Color = color,
                    Deferred = true,
                    Enabled = true,
                    Intensity = intensity,
                    LightDirection = direction,
                    Layers = RenderLayers.RenderLayerAll,
                    ShadowMap = false,
                    ShadowFarDistance = 10000,
                    ShadowNearDistance = 10,
                    ShadowMapFilterType = ShadowMapFilterType.Nearest,
                    ShadowMapCascadeCount = 4
                }
            };
        }
        
        #endregion
    }
}
