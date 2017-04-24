// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Effects;
#if XENKO_YEBIS
using Xenko.Effects.Yebis;
#endif
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Configuration;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Prefabs;

using ScriptTest2;
#if NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace ScriptTest
{
    [XenkoScript]
    public class ScriptCave
    {
        private const float CaveSceneTotalTime = 79.0f;
        private const float FullExposureTime = CaveSceneTotalTime - 7.0f;
        private const float LogoTimeBegin = FullExposureTime + 2.5f;
        private const float LogoTimeFull = LogoTimeBegin + 0.8f;
        private const float LogoTimeURL = LogoTimeFull + 3.5f;
        private const float LogoTimeStartToEnd = LogoTimeURL + 0.8f;
        private const float LogoTimeEnd = LogoTimeStartToEnd + 1f;
        private const float CaveSceneEndBlack = LogoTimeEnd + 1.5f;
        private const float CaveSceneRestart = CaveSceneEndBlack + 0.1f;

        [XenkoScript]
        public static async Task Run(EngineContext engineContext)
        {
            var renderingSetup = RenderingSetup.Singleton;
            renderingSetup.RegisterLighting(engineContext);

            ParticlePlugin particlePlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("ParticlePlugin", out particlePlugin))
            {
                ScriptParticleSmoke.Run(engineContext);
            }


            var yebisPlugin = engineContext.RenderContext.RenderPassPlugins.OfType<YebisPlugin>().FirstOrDefault();
            if (yebisPlugin != null)
            {
                var yebisConfig = AppConfig.GetConfiguration<YebisConfig>("Yebis");

                // yebisPlugin.ToneMap.Type = ToneMapType.Linear;
                yebisPlugin.ToneMap.Gamma = yebisConfig.Gamma;
                yebisPlugin.ColorCorrection.Saturation = yebisConfig.Saturation;
                yebisPlugin.ColorCorrection.Contrast = yebisConfig.Contrast;
                yebisPlugin.ColorCorrection.Brightness = yebisConfig.Brightness;
                yebisPlugin.ColorCorrection.ColorTemperature = yebisConfig.ColorTemperature;
                yebisPlugin.Lens.Vignette.Enable = true;
                yebisPlugin.Lens.Vignette.PowerOfCosine = 5.0f;
                yebisPlugin.Lens.Distortion.Enable = true;
                yebisPlugin.Lens.Distortion.EdgeRoundness = 0.1f;
                yebisPlugin.Lens.Distortion.EdgeSmoothness = 1.0f;
            }
            
            // Run the script to animate the intro fade-in/fade-out
            engineContext.Scheduler.Add(async () => await AnimateIntroAndEndScene(engineContext));

            var cameraEntityRootPrefab = await engineContext.AssetManager.LoadAsync<Entity>("/global_data/gdc_demo/char/camera.hotei#");
            var lightCamEntityRootPrefab = await engineContext.AssetManager.LoadAsync<Entity>("/global_data/gdc_demo/char/light_cam.hotei#");

            var lightCamEntityRoot = Prefab.Inherit(lightCamEntityRootPrefab);
            var cameraEntityRoot = Prefab.Inherit(cameraEntityRootPrefab);

            engineContext.EntityManager.AddEntity(cameraEntityRoot);
            engineContext.EntityManager.AddEntity(lightCamEntityRoot);
            Scheduler.Current.Add(() => AnimScript.AnimateFBXModel(engineContext, cameraEntityRoot, CaveSceneTotalTime, CaveSceneRestart));
            Scheduler.Current.Add(() => AnimScript.AnimateFBXModel(engineContext, lightCamEntityRoot, CaveSceneTotalTime, CaveSceneRestart));

            foreach(var light in ParameterContainerExtensions.CollectEntityTree(lightCamEntityRoot))
            {
                var lightComp = light.Get(LightComponent.Key);
                if (lightComp != null)
                {
                    
                    if (!lightComp.ShadowMap && lightComp.Type == LightType.Directional)
                    {
                        lightComp.Intensity *= 0.1f;
                    }
                }
            }

            var config = AppConfig.GetConfiguration<Script1.Config>("Script1");

            var shadowMap1 = new Entity();
            var shadowMap2 = new Entity();
            shadowMap1.Set(TransformationComponent.Key, TransformationTRS.CreateComponent());
            shadowMap2.Set(TransformationComponent.Key, TransformationTRS.CreateComponent());
            shadowMap1.Set(LightComponent.Key, new LightComponent { Type = LightType.Directional, Intensity = 0.9f, Color = new Color3(0.9f, 0.9f, 1.0f), LightDirection = new Vector3(-0.2f, -0.1f, -1.0f), ShadowMap = true, DecayStart = 40000.0f });
            shadowMap2.Set(LightComponent.Key, new LightComponent { Type = LightType.Directional, Color = new Color3(1.0f, 1.0f, 1.0f), LightDirection = new Vector3(-0.5f, 0.1f, -1.0f), ShadowMap = true, DecayStart = 40000.0f });
            shadowMap1.Set(LightShaftsComponent.Key, new LightShaftsComponent { Color = new Color3(1.0f, 1.0f, 1.0f), LightShaftsBoundingBoxes =
                {
                    new EffectMeshData { MeshData = MeshDataHelper.CreateBox(1, 1, 1, Color.White, true), Parameters = new ParameterCollection { { TransformationKeys.World, Matrix.Scaling(3000, 3500, 3000) * Matrix.Translation(-2500, 0, 1500) } } }
                } });
            shadowMap2.Set(LightShaftsComponent.Key, new LightShaftsComponent { Color = new Color3(1.0f, 1.0f, 1.0f), LightShaftsBoundingBoxes =
                {
                    new EffectMeshData { MeshData = MeshDataHelper.CreateBox(1, 1, 1, Color.White, true), Parameters = new ParameterCollection { { TransformationKeys.World, Matrix.Scaling(3500, 3500, 3000) * Matrix.Translation(-3000, 0, 1500) } } }
                } });

            engineContext.EntityManager.AddEntity(shadowMap1);
            engineContext.EntityManager.AddEntity(shadowMap2);


            var dragon = await LoadDragon(engineContext);
            await LoadCave(engineContext, dragon);

            var dragonHead = engineContext.EntityManager.Entities.FirstOrDefault(x => x.Name == "English DragonHead");
            TransformationTRS headCameraTransfo = null;
            if (dragonHead != null)
            {
                var headCamera = new Entity("Head camera");
                headCamera.Set(CameraComponent.Key, new CameraComponent { AspectRatio = 16.0f / 9.0f, VerticalFieldOfView = (float)Math.PI * 0.3f, Target = dragonHead, AutoFocus = true, NearPlane = 10.0f });
                headCamera.Set(TransformationComponent.Key, new TransformationComponent(new TransformationTRS { Translation = new Vector3(100.0f, -100.0f, 300.0f) }));
                //engineContext.EntitySystem.Entities.Add(headCamera);
                dragonHead.Transformation.Children.Add(headCamera.Transformation);
            }

            engineContext.Scheduler.Add(() => AnimateLights(engineContext));

            // Performs several full GC after the scene has been loaded
            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                Thread.Sleep(1);
            }

            while (true)
            {
                await engineContext.Scheduler.NextFrame();

                if (headCameraTransfo != null)
                {
                    var time = (double)DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerSecond;
                    float rotationSpeed = 0.317f;
                    var position = new Vector2((float)Math.Cos(time * rotationSpeed), (float)Math.Sin(time * rotationSpeed)) * 330.0f * ((float)Math.Sin(time * 0.23f) * 0.4f + 0.9f);
                    headCameraTransfo.Translation = new Vector3(position.X, -150.0f + (float)Math.Cos(time * rotationSpeed) * 50.0f, position.Y);
                }

                if (engineContext.InputManager.IsKeyPressed(Keys.F1))
                {
                    bool isWireframeEnabled = renderingSetup.ToggleWireframe();
                    if (yebisPlugin != null)
                    {
                        yebisPlugin.DepthOfField.Enable = !isWireframeEnabled;
                        yebisPlugin.Lens.Vignette.Enable = !isWireframeEnabled;
                        yebisPlugin.Lens.Distortion.Enable = !isWireframeEnabled;
                    }
                }

                if (engineContext.InputManager.IsKeyPressed(Keys.D1))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(0);
                if (engineContext.InputManager.IsKeyPressed(Keys.D2))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(10);
                if (engineContext.InputManager.IsKeyPressed(Keys.D3))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(20);
                if (engineContext.InputManager.IsKeyPressed(Keys.D4))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(30);
                if (engineContext.InputManager.IsKeyPressed(Keys.D5))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(40);
                if (engineContext.InputManager.IsKeyPressed(Keys.D6))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(50);
                if (engineContext.InputManager.IsKeyPressed(Keys.D7))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(60);
                if (engineContext.InputManager.IsKeyPressed(Keys.D8))
                    engineContext.CurrentTime = TimeSpan.FromSeconds(70);

                if (engineContext.InputManager.IsKeyPressed(Keys.T))
                {
                    if (particlePlugin != null)
                    {
                        particlePlugin.EnableSorting = !particlePlugin.EnableSorting;
                    }
                }

            }
        }

        public static async Task AnimateIntroAndEndScene(EngineContext engineContext)
        {
            var yebisPlugin = engineContext.RenderContext.RenderPassPlugins.OfType<YebisPlugin>().FirstOrDefault();
            var slideShowPlugin = engineContext.RenderContext.RenderPassPlugins.OfType<SlideShowPlugin>().FirstOrDefault();

            // Return immediately if there is no Yebis
            if (yebisPlugin == null || slideShowPlugin == null)
            {
                return;
            }

            var xenkoLogo = (Texture2D)await engineContext.AssetManager.LoadAsync<Texture>("/global_data/gdc_demo/bg/LogoXenko.dds");
            var xenkoLogoURL = (Texture2D)await engineContext.AssetManager.LoadAsync<Texture>("/global_data/gdc_demo/bg/LogoXenkoURL.dds");

            slideShowPlugin.TextureFrom = xenkoLogo;
            slideShowPlugin.TextureTo = xenkoLogoURL;
            slideShowPlugin.RenderPass.Enabled = false;

            double lastTime = 0;
            float lastExposure = 0.0f;

            yebisPlugin.ToneMap.AutoExposure.Enable = true;

            var savedToneMap = yebisPlugin.ToneMap;
            var savedLens = yebisPlugin.Lens;
            var savedColorCorrection = yebisPlugin.ColorCorrection;

            while (true)
            {
                await engineContext.Scheduler.NextFrame();

                var animationTime = (float)engineContext.CurrentTime.TotalSeconds % CaveSceneRestart;

                var deltaTime = (float)(animationTime - lastTime);

                // If scene restart
                if (deltaTime < 0.0f)
                {
                    yebisPlugin.ToneMap = savedToneMap;
                    yebisPlugin.Lens = savedLens;
                    yebisPlugin.ColorCorrection = savedColorCorrection;

                    // Enable all
                    foreach (var pass in engineContext.RenderContext.RootRenderPass.Passes)
                    {
                        if (pass.Name == "WireframePass")
                            continue;
                        pass.Enabled = true;
                    }
                    // Disable SlideShow
                    slideShowPlugin.RenderPass.Enabled = false;
                    yebisPlugin.ToneMap.Exposure = 0.005f;
                    yebisPlugin.ToneMap.AutoExposure.Enable = true;
                    slideShowPlugin.TransitionFactor = 0.0f;
                }

                // If we reset, reupload the particle buffer
                if (animationTime < FullExposureTime)
                {
                    yebisPlugin.ToneMap.AutoExposure.Enable = true;
                }
                else if (animationTime < LogoTimeBegin)
                {
                    if (yebisPlugin.ToneMap.AutoExposure.Enable)
                    {
                        lastExposure = yebisPlugin.ToneMap.Exposure;
                        yebisPlugin.ToneMap.AutoExposure.Enable = false;
                    }

                    yebisPlugin.ToneMap.Exposure = lastExposure + (60.0f - lastExposure) *  (float)Math.Pow((animationTime - FullExposureTime) / (LogoTimeBegin - FullExposureTime), 2.0f);
                }
                else 
                {
                    if (!slideShowPlugin.RenderPass.Enabled)
                    {
                        // Enable only Yebis and SlideShow
                        foreach (var pass in engineContext.RenderContext.RootRenderPass.Passes)
                        {
                            pass.Enabled = false;
                        }
                        yebisPlugin.RenderPass.Enabled = true;
                        slideShowPlugin.RenderPass.Enabled = true;

                        yebisPlugin.ToneMap.Type = ToneMapType.Linear;
                        yebisPlugin.Lens.Vignette.Enable = false;
                        yebisPlugin.ToneMap.Gamma = 1.0f;
                        yebisPlugin.ColorCorrection.ColorTemperature = 6500;
                        yebisPlugin.ColorCorrection.Saturation = 1.0f;
                        yebisPlugin.ColorCorrection.Contrast = 1.0f;
                        yebisPlugin.ColorCorrection.Brightness = 1.0f;
                        lastExposure = yebisPlugin.ToneMap.Exposure;
                        slideShowPlugin.TransitionFactor = 0.0f;
                    }

                    if (animationTime < LogoTimeFull)
                    {
                        slideShowPlugin.ZoomFactor = 0.5f + (1.0f - 0.5f) * Quintic((animationTime - LogoTimeBegin) / (LogoTimeFull - LogoTimeBegin));
                        yebisPlugin.ToneMap.Exposure = lastExposure + (1.0f - lastExposure) * (float)Math.Pow((animationTime - LogoTimeBegin) / (LogoTimeFull - LogoTimeBegin), 0.1f);
                    }
                    else
                    {
                        slideShowPlugin.ZoomFactor = 1.0f;
                        if (animationTime < LogoTimeURL)
                        {
                            yebisPlugin.ToneMap.Exposure = 1.0f;
                        }
                        else if (animationTime < LogoTimeStartToEnd)
                        {
                            slideShowPlugin.TransitionFactor = Math.Min(1.0f, (animationTime - LogoTimeURL) / (LogoTimeStartToEnd - LogoTimeURL));
                        }
                        else if (animationTime < LogoTimeEnd)
                        {
                            slideShowPlugin.TransitionFactor = 1.0f;
                        }
                        else if (animationTime < CaveSceneEndBlack)
                        {
                            yebisPlugin.ToneMap.Exposure = Math.Max(0.0f, 1.0f - (animationTime - LogoTimeEnd) / (CaveSceneEndBlack - LogoTimeEnd));
                        }
                        else
                        {
                            yebisPlugin.ToneMap.Exposure = 0f;
                        }
                    }
                }
                //Console.WriteLine("Exposure: {0}", yebisPlugin.ToneMap.Exposure);

                lastTime = animationTime;
            }
        }

        public static async Task AnimateLights(EngineContext engineContext)
        {
            while (true)
            {
                await engineContext.Scheduler.NextFrame();

                if (engineContext.InputManager.IsKeyPressed(Keys.F3))
                {
                    var lights = engineContext.EntityManager.Entities.Components(LightComponent.Key);
                    await engineContext.Scheduler.WhenAll(lights.Select((lightComponent, index) =>
                        engineContext.Scheduler.Add(() => AnimateLight(engineContext, index, lightComponent))).ToArray());
                }
            }
        }

        private static float Quintic(float x)
        {
            return x * x * x * (x * (x * 6.0f - 15.0f) + 10.0f);
        }

        public static async Task AnimateLight(EngineContext engineContext, int index, LightComponent lightComponent)
        {
            // Wait different time for each light
            await TaskEx.Delay(index * 20);
            var startIntensity = lightComponent.Intensity;

            // Turn light off
            var startTime = DateTime.UtcNow;
            while (true)
            {
                await engineContext.Scheduler.NextFrame();
                var elapsedTime = (DateTime.UtcNow - startTime).Seconds * 0.2f;
                if (elapsedTime > 1.0f)
                    break;
                lightComponent.Intensity = startIntensity * (1.0f - elapsedTime);
            }

            // Turn light on
       
            lightComponent.Intensity = startIntensity;
        }

        //[XenkoScript]
        public static async Task<Entity> LoadDragon(EngineContext engineContext)
        {
            var renderingSetup = RenderingSetup.Singleton;
            renderingSetup.RegisterLighting(engineContext);
            
            var characterEntity = await AnimScript.LoadFBXModel(engineContext, "/global_data/gdc_demo/char/dragon_camera.hotei#");
            characterEntity.Name = "Dragon";
            Scheduler.Current.Add(() => AnimScript.AnimateFBXModel(engineContext, characterEntity, CaveSceneTotalTime, CaveSceneRestart));

            // Setup predefined specular intensities/power for dragon
            foreach (var entity in ParameterContainerExtensions.CollectEntityTree(characterEntity))
            {
                var meshComponent = entity.Get(ModelComponent.Key);
                if (meshComponent == null)
                    continue;

                foreach (var effectMesh in meshComponent.InstantiatedSubMeshes)
                {
                    effectMesh.Value.Parameters.Set(TessellationKeys.DesiredTriangleSize, 4.0f);

                    switch (effectMesh.Key.EffectData.Part)
                    {
                        case "skin":
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.4f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.4f);
                            break;
                        case "mouth":
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.3f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.3f);
                            break;
                        case "skin2":
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.5f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.5f);
                            break;
                        case "wing":
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.4f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.5f);
                            break;
                        case "tooth":
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.4f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.7f);
                            break;
                        case "eye":
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.7f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.7f);
                            break;
                        default:
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.3f);
                            effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 0.3f);
                            break;
                    }
                }
            }

            return characterEntity;
        }

        // [XenkoScript]
        public static async Task LoadCave(EngineContext engineContext, Entity animationEntity)
        {
            // Setup "fake" directional light for outdoor so that normal maps stand out.
            var outdoorLight = new DirectionalLight { LightDirection = new Vector3(-2.0f, -1.0f, -1.0f) };
            //outdoor.Permutations.Set(LightingPermutation.Key, new LightingPermutation { Lights = { outdoorLight } });
            //outdoor.Permutations.Set(LightingPermutation.Key, new LightingPermutation { Lights = { outdoorLight } });

            var effectMagma = engineContext.RenderContext.Effects.First(x => x.Name == "Magma");

            effectMagma.Parameters.AddSources(engineContext.DataContext.RenderPassPlugins.TryGetValue("NoisePlugin").Parameters);

            //var assetManager = new AssetManager(new ContentSerializerContextGenerator(engineContext.VirtualFileSystem, engineContext.PackageManager, ParameterContainerExtensions.DefaultSceneSerializer));
            var caveEntityPrefab1 = await engineContext.AssetManager.LoadAsync<Entity>("/global_data/gdc_demo/bg/bg1.hotei#");
            var caveEntityPrefab2 = await engineContext.AssetManager.LoadAsync<Entity>("/global_data/gdc_demo/bg/bg2.hotei#");

            var caveEntity1 = Prefab.Clone(caveEntityPrefab1);
            var caveEntity2 = Prefab.Clone(caveEntityPrefab2);

            SkyBoxPlugin skyBoxPlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("SkyBoxPlugin", out skyBoxPlugin))
            {
                var skyBoxTexture = (Texture2D)await engineContext.AssetManager.LoadAsync<Texture>("/global_data/gdc_demo/bg/GDC2012_map_sky.dds");
                skyBoxPlugin.Texture = skyBoxTexture;
            }

            var magmaTexturePaths = new[]
                {
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_04.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_05.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_06.dds",

                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_00.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_01.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_02.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_03.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_noise_00.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_noise_01.dds",
                    "/global_data/gdc_demo/bg/GDC2012_map_maguma_normals.dds",
                };
            var magmaTextures = new Texture2D[magmaTexturePaths.Length];
            for (int i = 0; i < magmaTextures.Length; i++)
            {
                magmaTextures[i] = (Texture2D)await engineContext.AssetManager.LoadAsync<Texture>(magmaTexturePaths[i]);
            }

            var lightVectors = new[]
                {
                    new Vector3(-1.0f, 0.3f, -1.0f),
                    new Vector3(1.0f, 0.0f, -1.0f),
                    new Vector3(1.0f, 0.0f, -1.0f),
                };


            var random = new Random(0);
            int planeIndex = 0;
            foreach (var entity in ParameterContainerExtensions.CollectEntityTree(caveEntity1))
            {
                var meshComponent = entity.Get(ModelComponent.Key);
                if (meshComponent == null)
                    continue;

                // Setup textures for magma
                if (entity.Name.StartsWith("maguma_"))
                {
                    meshComponent.MeshParameters.Set(LightKeys.LightDirection, lightVectors[planeIndex]);
                    meshComponent.MeshParameters.Set(ParameterKeys.IndexedKey(TexturingKeys.Texture0, 1), magmaTextures[planeIndex]);
                    planeIndex++;
                    for (int i = 3; i < magmaTextures.Length; i++)
                        meshComponent.MeshParameters.Set(ParameterKeys.IndexedKey(TexturingKeys.Texture0, i - 1), magmaTextures[i]);


                    foreach (var effectMesh in meshComponent.SubMeshes)
                    {
                        effectMesh.EffectData.Name = "Magma";
                    }

                    // Attach a bullet particle emitter to the magma
                    var emitter = new BulletParticleEmitterComponent()
                    {
                        //Count = 4096,
                        Count = 16384,
                        Description = new BulletEmitterDescription()
                        {
                            Target = new Vector3(-3016.261f, -70.52288f, 800.8788f),
                            BulletSize = 4.0f,
                            //MaxTimeTarget = 1000.0f + 5000.0f * (float)random.NextDouble(),
                            VelocityUp =  new Vector3(0, 0, 200),
                            MaxTimeUp = 5000.0f,
                            MaxTimeTarget = 20000.0f,
                            VelocityTarget = 200.0f,
                            Opacity = 1.0f,
                            //DistanceDragonRepulse = 1200.0f,
                            //DecayDragonRepulse = 70.0f,
                            DistanceDragonRepulse = 600.0f,
                            DecayDragonRepulse = 70.0f,
                            VelocityRepulse = 200.0f,
                        },
                        RootAnimation = animationEntity,
                    };
                    emitter.OnAddToSystem(engineContext.EntityManager, engineContext.RenderContext);
                    emitter.OnUpdateData();
                    entity.Set(ParticleEmitterComponent.Key, emitter);
                }

                foreach (var effectMesh in meshComponent.SubMeshes)
                {
                    effectMesh.Parameters.Set(ParameterKeys.IndexedKey(TexturingKeys.DiffuseTexture, 3), (Texture2D)await engineContext.AssetManager.LoadAsync<Texture>("/global_data/gdc_demo/bg/GDC2012_map_dis_ao.dds"));
                }
            }

            await engineContext.EntityManager.AddEntityAsync(caveEntity1);
            await engineContext.EntityManager.AddEntityAsync(caveEntity2);

            foreach (var entity in ParameterContainerExtensions.CollectEntityTree(caveEntity1).Concat(ParameterContainerExtensions.CollectEntityTree(caveEntity2)))
            {
                var meshComponent = entity.Get(ModelComponent.Key);
                if (meshComponent == null)
                    continue;

                foreach (var effectMesh in meshComponent.InstantiatedSubMeshes)
                {
                    effectMesh.Value.Parameters.Set(MaterialKeys.SpecularIntensity, 2.0f);
                    effectMesh.Value.Parameters.Set(MaterialKeys.SpecularPower, 0.1f);
                }
            }
        }
    }

    public class YebisConfig
    {
        public YebisConfig()
        {
            Gamma = 2.2f;
            Saturation = 0.7f;
            Contrast = 1.06f;
            Brightness = 1.2f;
            ColorTemperature = 5500.0f;
        }

        //yebisPlugin.ToneMap.Gamma = 2.2f;
        //yebisPlugin.ColorCorrection.Saturation = 0.7f;
        //yebisPlugin.ColorCorrection.Contrast = 1.06f;
        //yebisPlugin.ColorCorrection.Brightness = 1.2f;
        //yebisPlugin.ColorCorrection.ColorTemperature = 5500;

        [XmlAttribute("gamma")]
        public float Gamma { get; set; }

        [XmlAttribute("saturation")]
        public float Saturation { get; set; }

        [XmlAttribute("contrast")]
        public float Contrast { get; set; }

        [XmlAttribute("brightness")]
        public float Brightness { get; set; }

        [XmlAttribute("temperature")]
        public float ColorTemperature { get; set; }
    }
}
