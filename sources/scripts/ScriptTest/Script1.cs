// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Configuration;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Games.IO;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Particles;
using SiliconStudio.Shaders;
using ScriptShader.Effects;

using ScriptTest2;
#if NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace ScriptTest
{
    [XenkoScript]
    public class Script1
    {
        [XenkoScript(ScriptFlags.None)]
        public static async Task Run3()
        {
            for (int i = 0; i < 100; i++)
            {
                await Scheduler.Current.NextFrame();
                if (i % 5 == 0)
                    System.Threading.Thread.Sleep(20);
            }
        }

        [XenkoScript(ScriptFlags.None)]
        public static async Task Run2(EngineContext engineContext)
        {
            for (int i = 0; i < 10; i++)
                Scheduler.Current.Add(Run3);
        }

        public static void CommonSetup(EngineContext engineContext)
        {
            VirtualFileSystem.MountFileSystem("/global_data", "..\\..\\deps\\data\\");
            VirtualFileSystem.MountFileSystem("/global_data2", "..\\..\\data\\");
            VirtualFileSystem.MountFileSystem("/shaders", "..\\..\\sources\\shaders\\");

            engineContext.EntityManager.Systems.Add(new MeshProcessor());
            engineContext.EntityManager.Systems.Add(new HierarchicalProcessor());
            engineContext.EntityManager.Systems.Add(new AnimationProcessor());
            engineContext.EntityManager.Systems.Add(new TransformationProcessor());
            engineContext.EntityManager.Systems.Add(new TransformationUpdateProcessor());
            engineContext.EntityManager.Systems.Add(new SkinningProcessor());
            engineContext.EntityManager.Systems.Add(new ModelConverterProcessor(engineContext));

            engineContext.AssetManager.RegisterSerializer(new GpuTextureSerializer(engineContext.RenderContext.GraphicsDevice));
            engineContext.AssetManager.RegisterSerializer(new GpuSamplerStateSerializer(engineContext.RenderContext.GraphicsDevice));
            engineContext.AssetManager.RegisterSerializer(new ImageSerializer());
        }

        public static void PipelineSetup(EngineContext engineContext, string effectFilename = null)
        {
            var config = AppConfig.GetConfiguration<Config>("Script1");
            var renderingSetup = RenderingSetup.Singleton;
            var optionalFeatures = config.PipelineFeatures.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            renderingSetup.Initialize(engineContext, effectFilename, optionalFeatures);
        }

        [XenkoScript(ScriptFlags.AssemblyStartup)]
        public static async Task Run(EngineContext engineContext)
        {
            var config = AppConfig.GetConfiguration<Config>("Script1");

            CommonSetup(engineContext);

            if (config.Synthetic)
            {
                await ScriptCube.Run(engineContext);
            }
            else
            {
                PipelineSetup(engineContext);

                // Check config file
                var configDebug = AppConfig.GetConfiguration<ScriptDebug.Config>("ScriptDebug");
                if (configDebug.DebugManager)
                    engineContext.Scheduler.Add(() => ScriptDebug.RunDebug(engineContext));

                engineContext.Scheduler.Add(async () =>
                    {
                        if (config.Scene == "cave")
                        {
                            VirtualFileSystem.MountFileSystem("/sync", ".");
                            await ScriptCave.Run(engineContext);
                        }
                        else if (config.Scene == "sync")
                        {
                            ScriptSceneSerialization.gitFolder = "..\\..\\gittest\\" + config.SyncFolder + "\\";
                            VirtualFileSystem.MountFileSystem("/sync", ScriptSceneSerialization.gitFolder);
                            await ScriptCube.GenerateSimpleCubeEffect(engineContext);
                        }
                        else if (config.Scene == "factory")
                        {
                            await SetupFactory(engineContext);
                            await LightScript.MoveLights(engineContext);
                        }
                        else if (config.Scene == "particles")
                        {
                            //ScriptParticleSmoke.Run(engineContext);
                        }
                        else if (config.Scene == "cputest")
                        {
                            await ScriptMulticore.Run(engineContext);
                        }
                        else if (config.Scene == "permutation")
                        {
                            await ScriptPermutation.Run(engineContext);
                        }
                    });
            }
        }

        [XenkoScript]
        public static async Task LoadDude(EngineContext engineContext)
        {
            var mainPlugin = engineContext.RenderContext.RenderPassPlugins.OfType<MainPlugin>().FirstOrDefault();
            EffectOld effect = engineContext.RenderContext.BuildEffect("SimpleSkinning")
                .Using(new BasicShaderPlugin("ShaderBase") { RenderPassPlugin = mainPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = mainPlugin })
                    .Using(new BasicShaderPlugin(new ShaderMixinSource() {
                            new ShaderClassSource("AlbedoDiffuseBase"),
                            new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorTexture", TexturingKeys.DiffuseTexture, "TEXCOORD")),
                            new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColor")), // TODO: Default values!
                    }) { RenderPassPlugin = mainPlugin })
                .Using(new BasicShaderPlugin("AlbedoFlatShading") { RenderPassPlugin = mainPlugin })
                ;

            var characterEntity = await AnimScript.LoadFBXModel(engineContext, "/global_data/fbx/test_mesh.hotei#");
            await AnimScript.AnimateFBXModel(engineContext, characterEntity);
        }

        [XenkoScript]
        public static async Task SetupFactory(EngineContext engineContext, string effectName = "Simple")
        {
            var renderingSetup = RenderingSetup.Singleton;

            renderingSetup.RegisterLighting(engineContext);

            // Setup lighting
            LightingPlugin lightingPlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("LightingPlugin", out lightingPlugin))
            {
                var shadowMapEntity = new Entity();
                shadowMapEntity.Set(TransformationComponent.Key, TransformationTRS.CreateComponent());
                shadowMapEntity.Set(LightComponent.Key, new LightComponent { Type = LightType.Directional, Intensity = 0.9f, Color = new Color3(1.0f, 1.0f, 1.0f), LightDirection = new Vector3(-1.0f, -1.0f, -1.0f), ShadowMap = true, DecayStart = 40000.0f });
                engineContext.EntityManager.AddEntity(shadowMapEntity);
            }

            // Load asset
            var entity = await engineContext.AssetManager.LoadAsync<Entity>("/global_data/factoryfbx.hotei#");

            // Flip it and scale it
            var transformationComponent = (TransformationTRS)entity.Transformation.Value;
            transformationComponent.Scaling *= 0.1f;

            //await engineContext.EntitySystem.Prepare(entity);
            await engineContext.EntityManager.AddEntityAsync(entity);
        }

        private static void SetupParticles(EngineContext engineContext)
        {
            // Create particle system
            var particleSystem = new SiliconStudio.Xenko.Particles.ParticleSystem();

            // Set particle default size to 10.0f
            particleSystem.GetOrCreateFieldWithDefault(ParticleFields.Size, 10.0f);

            // Add particle plugins
            particleSystem.Plugins.Add(new ResetAcceleration());
            particleSystem.Plugins.Add(new SimpleEmitter());
            particleSystem.Plugins.Add(new RemoveOldParticles(10.0f));
            particleSystem.Plugins.Add(new Gravity());
            particleSystem.Plugins.Add(new UpdateVelocity());

            // Create particle system mesh for rendering.
            var particleEffect = engineContext.RenderContext.Effects.First(x => x.Name == "DefaultParticle");
            var particleMesh = new EffectMesh(particleEffect);
            particleMesh.Parameters.Set(ParticleRendererPlugin.ParticleSystemKey, particleSystem);

            // Load particle texture
            var smokeVolTexture = (Texture2D)engineContext.AssetManager.Load<Texture>("/global_data/gdc_demo/fx/smokevol.dds");
            particleMesh.Parameters.Set(TexturingKeys.DiffuseTexture, smokeVolTexture);

            // Register it to rendering
            engineContext.RenderContext.GlobalMeshes.AddMesh(particleMesh);
        }

        [XenkoScript]
        public static async Task SetupPostEffects(EngineContext engineContext)
        {
            var config = AppConfig.GetConfiguration<Config>("Script1");

            var renderingSetup = RenderingSetup.Singleton;
            renderingSetup.Initialize(engineContext);

            bool bloom = config.Bloom;
            bool fxaa = config.FXAA;

            bool useHBAO = false;
            bool doBlur = true;
            bool mixAOWithColorImage = false;
            bool halfResAO = true;

            var effectMeshGroup = new RenderPassListEnumerator();
            engineContext.RenderContext.RenderPassEnumerators.Add(effectMeshGroup);

            PostEffectPlugin postEffectPlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("PostEffectPlugin", out postEffectPlugin)
                && (bloom || fxaa || useHBAO))
            {
                if (bloom)
                {
                    // Create various effects required by the bloom effect
                    EffectOld brightPassFilter = engineContext.RenderContext.BuildEffect("BrightPass")
                        .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectPlugin })
                        .Using(new BasicShaderPlugin("ShadingTexturing"))
                        .Using(new BasicShaderPlugin("PostEffectBrightFilter"));

                    EffectOld blurEffect = engineContext.RenderContext.BuildEffect("Blur")
                        .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectPlugin })
                        .Using(new BasicShaderPlugin("PostEffectBlur"));

                    EffectOld downsampleEffect = engineContext.RenderContext.BuildEffect("DownSample")
                        .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectPlugin })
                        .Using(new BasicShaderPlugin("ShadingTexturing"));

                    EffectOld mixEffect = engineContext.RenderContext.BuildEffect("Mix")
                        .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectPlugin })
                        .Using(new BasicShaderPlugin("ShadingTexturing"))
                        .Using(new BasicShaderPlugin("PosteffectTexturing2"));

                    EffectOld fxaaEffect = engineContext.RenderContext.BuildEffect("Fxaa")
                        .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectPlugin })
                        .Using(new BasicShaderPlugin("PostEffectFXAA.xksl"));


                    // Create post effect meshes: downsampling and blurs
                    int bloomLevels = 6;
                    var downsampleMeshes = new EffectMesh[bloomLevels];
                    var lastBlurs = new EffectMesh[bloomLevels];
                    for (int i = 0; i < bloomLevels; ++i)
                    {
                        downsampleMeshes[i] = new EffectMesh(i == 0 ? brightPassFilter : downsampleEffect, name: "Downsample " + i);
                        postEffectPlugin.AddEffectMesh(downsampleMeshes[i]);

                        // Blur effect
                        var blurQuadMesh = new EffectMesh[2];
                        for (int j = 0; j < 2; ++j)
                        {
                            blurQuadMesh[j] = new EffectMesh(blurEffect, name: string.Format("Blur level {0}:{1}", i, j));
                            blurQuadMesh[j].Parameters.Set(PostEffectBlurKeys.Coefficients, new[] { 0.30f, 0.20f, 0.20f, 0.15f, 0.15f });
                            var unit = j == 0 ? Vector2.UnitX : Vector2.UnitY;
                            blurQuadMesh[j].Parameters.Set(PostEffectBlurKeys.Offsets, new[] { Vector2.Zero, unit * -1.3862832f, unit * +1.3862832f, unit * -3.2534592f, unit * +3.2534592f });
                            postEffectPlugin.AddEffectMesh(blurQuadMesh[j]);
                        }
                        lastBlurs[i] = blurQuadMesh[1];
                        postEffectPlugin.AddLink(downsampleMeshes[i], RenderTargetKeys.RenderTarget, blurQuadMesh[0], TexturingKeys.Texture0, new TextureDescription { Width = 1024 >> (i + 1), Height = 768 >> (i + 1), Format = PixelFormat.R8G8B8A8_UNorm });
                        postEffectPlugin.AddLink(blurQuadMesh[0], RenderTargetKeys.RenderTarget, blurQuadMesh[1], TexturingKeys.Texture0);
                        if (i > 0)
                            postEffectPlugin.AddLink(downsampleMeshes[i - 1], RenderTargetKeys.RenderTarget, downsampleMeshes[i], TexturingKeys.Texture0);
                    }

                    // Create post effect meshes: mix
                    EffectMesh lastMix = null;
                    for (int i = 0; i < bloomLevels; ++i)
                    {
                        var mixMesh = new EffectMesh(mixEffect, name: "Mix " + (bloomLevels - 1 - i));
                        mixMesh.Parameters.Set(PostEffectKeys.MixCoefficients, (i < bloomLevels - 1) ? new[] { 0.10f, 0.90f } : new[] { 1.0f, 3.0f });
                        postEffectPlugin.AddEffectMesh(mixMesh);


                        if (i < bloomLevels - 1)
                            postEffectPlugin.AddLink(lastBlurs[bloomLevels - 2 - i], RenderTargetKeys.RenderTarget, mixMesh, TexturingKeys.Texture0);
                        postEffectPlugin.AddLink(lastMix ?? lastBlurs[bloomLevels - 1], RenderTargetKeys.RenderTarget, mixMesh, TexturingKeys.Texture2);

                        lastMix = mixMesh;
                    }

                    EffectMesh lastEffectMesh = lastMix;

                    //add fxaa?
                    if (fxaa)
                    {
                        var fxaaQuadMesh = new EffectMesh(fxaaEffect, name: "FXAA level");
                        postEffectPlugin.AddEffectMesh(fxaaQuadMesh);
                        postEffectPlugin.AddLink(lastMix, RenderTargetKeys.RenderTarget, fxaaQuadMesh, TexturingKeys.Texture0, new TextureDescription { Width = 1024, Height = 768, Format = PixelFormat.R8G8B8A8_UNorm });
                        lastEffectMesh = fxaaQuadMesh;
                    }

                    engineContext.RenderContext.GraphicsResizeContext.SetupResize((resizeContext) =>
                        {
                            var renderTarget = renderingSetup.MainTargetPlugin.RenderTarget;
                            //blurQuadMesh[0].Parameters.Set(TextureFeature.Texture0, renderTarget);
                            //blurQuadMesh[1].Parameters.Set(RenderTargetKeys.RenderTarget, renderTarget);
                            downsampleMeshes[0].Parameters.SetWithResize(resizeContext, TexturingKeys.Texture0, (Texture2D)renderTarget.Texture);
                            lastMix.Parameters.SetWithResize(resizeContext, TexturingKeys.Texture0, (Texture2D)renderTarget.Texture);
                            lastMix.Parameters.SetWithResize(resizeContext, RenderTargetKeys.RenderTarget, engineContext.RenderContext.RenderTarget);

                            lastEffectMesh.Parameters.SetWithResize(resizeContext, RenderTargetKeys.RenderTarget, engineContext.RenderContext.RenderTarget);
                        });
                }
                else if (fxaa)
                {
                    //fxaa effect setup (fxaa only, no bloom effect)
                    EffectOld fxaaEffect = engineContext.RenderContext.BuildEffect("Fxaa")
                        .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectPlugin })
                        .Using(new BasicShaderPlugin("..\\..\\sources\\shaders\\posteffect_fxaa.xksl"));

                    var fxaaQuadMesh = new EffectMesh(fxaaEffect, name: "FXAA level");

                    fxaaQuadMesh.Parameters.Set(TexturingKeys.Texture0, (Texture2D)renderingSetup.MainTargetPlugin.RenderTarget.Texture);
                    fxaaQuadMesh.Parameters.Set(RenderTargetKeys.RenderTarget, engineContext.RenderContext.RenderTarget);
                    //fxaaQuadMesh.Parameters.Set(PostEffectFXAAKeys.FxaaQualitySubpix, 0);

                    postEffectPlugin.AddEffectMesh(fxaaQuadMesh);

                    //TODO, application will crashes if we resize or move the window!!
                }

                foreach (var mesh in postEffectPlugin.Meshes)
                    effectMeshGroup.AddMesh(mesh);

                //engineContext.RenderContext.RootRenderPass.AddPass(postEffectPlugin.RenderPass);

                engineContext.RenderContext.GraphicsResizeContext.SetupResize((resizeContext) =>
                    {
                        // Link post effects (this will create intermediate surfaces)
                        postEffectPlugin.Resolve();
                    });
            }
        }

        public class Config
        {
            public Config()
            {
                PipelineFeatures = "Yebis HeatShimmering SkyBox";
                Tessellation = true;
                Scene = "cave";
                SyncFolder = "hotei_data1";
                Yebis = true;
                LightShafts = true;
            }

            [XmlAttribute("pipeline_features")]
            public string PipelineFeatures { get; set; }

            [XmlAttribute("tessellation")]
            public bool Tessellation { get; set; }

            [XmlAttribute("scene")]
            public string Scene { get; set; }

            [XmlAttribute("syncfolder")]
            public string SyncFolder { get; set; }

            [XmlAttribute("synthetic")]
            public bool Synthetic { get; set; }

            [XmlAttribute("particles")]
            public bool Particles { get; set; }

            [XmlAttribute("bloom")]
            public bool Bloom { get; set; }

            [XmlAttribute("ao")]
            public bool AO { get; set; }

            [XmlAttribute("yebis")]
            public bool Yebis { get; set; }

            [XmlAttribute("lightshafts")]
            public bool LightShafts { get; set; }

            [XmlAttribute("fxaa")]
            public bool FXAA { get; set; }
        }
    }

    [XenkoScript]
    public class Script2
    {
        [XenkoScript(ScriptFlags.None)]
        public static async Task Run(EngineContext engineContext)
        {
            await TaskEx.Delay(1000);
        }
    }

    [XenkoScript]
    public class Script3
    {
        [XenkoScript(ScriptFlags.None)]
        public static async Task Run(EngineContext engineContext)
        {
            await TaskEx.Delay(1000);
        }
    }
}
