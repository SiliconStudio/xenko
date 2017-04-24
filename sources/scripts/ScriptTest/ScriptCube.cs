// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
#if XENKO_YEBIS
using Xenko.Effects.Yebis;
#endif
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.IO;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.Mathematics;
using Xenko.Framework.Shaders;

namespace ScriptTest
{
    [XenkoScript]
    public class ScriptCube
    {
        public void Dispose()
        {
            //depthTextureHolder.Release();
        }

        class Sphere
        {
            public EffectMesh Mesh;
            public float Phase;
            public float Speed;
        }

        struct LightInfo
        {
            public float Radius;
            public float Phase;
            public float Z;
        }

        [XenkoScript]
        public static async Task GenerateSimpleCubeEffect(EngineContext engineContext)
        {
            var renderingSetup = RenderingSetup.Singleton;
            renderingSetup.RegisterLighting(engineContext);

#if XENKO_YEBIS
            YebisPlugin yebisPlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("YebisPlugin", out yebisPlugin))
            {
                yebisPlugin.Glare.Enable = false;
                yebisPlugin.DepthOfField.Enable = false;
                yebisPlugin.ToneMap.AutoExposure.Enable = false;
                yebisPlugin.ToneMap.Exposure = 1.0f;
                yebisPlugin.ToneMap.Gamma = 2.2f;
            }
#endif
            var lightPrepassPlugin = (LightingPrepassPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("LightingPrepassPlugin");
            var gbufferPlugin = (GBufferPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("GBufferPlugin");

            EffectOld effect = engineContext.RenderContext.BuildEffect("SimpleCube")
                .Using(new BasicShaderPlugin("ShaderBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("AlbedoSpecularBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("AlbedoDiffuseBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("NormalVSGBuffer") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("SpecularPowerPerMesh") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("PositionVSGBuffer") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("BRDFDiffuseLambert") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("BRDFSpecularBlinnPhong") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin(new ShaderMixinSource() {
                    new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorStream"))}) { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin(new ShaderMixinSource() {
                    new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColorSynthetic"))}) { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new GBufferShaderPlugin { RenderPassPlugin = gbufferPlugin })
                .Using(new DeferredLightingShaderPlugin() { RenderPassPlugin = lightPrepassPlugin })
                .Using(new BasicShaderPlugin("LightDirectionalShading") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                ;

            EffectOld effect2 = engineContext.RenderContext.BuildEffect("SimpleSkinning")
                .Using(new BasicShaderPlugin("ShaderBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationSkinning") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("AlbedoSpecularBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("AlbedoDiffuseBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("NormalVSGBuffer") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("SpecularPowerPerMesh") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("PositionVSGBuffer") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("BRDFDiffuseLambert") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("BRDFSpecularBlinnPhong") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin(new ShaderMixinSource() {
                            new ShaderClassSource("AlbedoDiffuseBase"),
                            new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorTexture", TexturingKeys.DiffuseTexture.Name, "TEXCOORD")),
                            new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColor")),
                    }) { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new GBufferShaderPlugin { RenderPassPlugin = gbufferPlugin })
                .Using(new DeferredLightingShaderPlugin() { RenderPassPlugin = (LightingPrepassPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("LightingPrepassPlugin") })
                .Using(new BasicShaderPlugin("LightDirectionalShading") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                ;
        }

        [XenkoScript]
        public static async Task GenerateTestPrefabs(EngineContext engineContext)
        {
            var entityCube = new Entity("Cube");
            var meshComponent = new ModelComponent();
            meshComponent.SubMeshes.Add(new EffectMeshData { EffectData = new EffectData("SimpleCube"), MeshData = MeshDataHelper.CreateBox(100.0f, 100.0f, 100.0f, Color.Gray) });
            entityCube.Set(ModelComponent.Key, meshComponent);
            entityCube.Set(TransformationComponent.Key, new TransformationComponent());
            engineContext.AssetManager.Url.Set(entityCube, "/global_data/cube.hotei#/root");
            engineContext.AssetManager.Save(entityCube);

            var entitySphere = new Entity("Cube");
            meshComponent = new ModelComponent();
            meshComponent.SubMeshes.Add(new EffectMeshData { EffectData = new EffectData("SimpleCube"), MeshData = MeshDataHelper.CreateSphere(100.0f, 30, 30, Color.Gray) });
            entitySphere.Set(ModelComponent.Key, meshComponent);
            entitySphere.Set(TransformationComponent.Key, new TransformationComponent());
            engineContext.AssetManager.Url.Set(entitySphere, "/global_data/sphere.hotei#/root");
            engineContext.AssetManager.Save(entitySphere);
        }

        [XenkoScript]
        public static async Task Run(EngineContext engineContext)
        {
            var renderingSetup = RenderingSetup.Singleton;
            renderingSetup.Initialize(engineContext);
            renderingSetup.RegisterLighting(engineContext);

#if XENKO_YEBIS
            YebisPlugin yebisPlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("YebisPlugin", out yebisPlugin))
            {
                yebisPlugin.Glare.Enable = true;
                yebisPlugin.ToneMap.Exposure = 1.0f;
                yebisPlugin.ToneMap.Gamma = 2.2f;
            }
#endif

            var lightPrepassPlugin = (LightingPrepassPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("LightingPrepassPlugin");
            var gbufferPlugin = (GBufferPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("GBufferPlugin");

            EffectOld effect = engineContext.RenderContext.BuildEffect("SimpleCube")
                .Using(new BasicShaderPlugin("ShaderBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("AlbedoSpecularBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("AlbedoDiffuseBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("NormalVSGBuffer") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("SpecularPowerPerMesh") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("PositionVSGBuffer") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("BRDFDiffuseLambert") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("BRDFSpecularBlinnPhong") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin(new ShaderMixinSource() {
                    new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorStream"))}) { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin(new ShaderMixinSource() {
                    new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColorSynthetic"))}) { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new GBufferShaderPlugin { RenderPassPlugin = gbufferPlugin })
                .Using(new DeferredLightingShaderPlugin() { RenderPassPlugin = lightPrepassPlugin })
                .Using(new LightingShaderPlugin() { RenderPassPlugin = (LightingPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("LightingPlugin") })
                .Using(new BasicShaderPlugin("LightDirectionalShading") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                ;

            var shadowMap1 = new ShadowMap(new DirectionalLight() { LightColor = new Color3(1.0f, 1.0f, 1.0f), LightDirection = new Vector3(1.0f, 1.0f, 1.0f) });
            effect.Permutations.Set(ShadowMapPermutationArray.Key, new ShadowMapPermutationArray { ShadowMaps = { shadowMap1 } });

            var r = new Random(0);


            VirtualFileSystem.MountFileSystem("/global_data", "..\\..\\deps\\data\\");
            VirtualFileSystem.MountFileSystem("/global_data2", "..\\..\\data\\");

            SkyBoxPlugin skyBoxPlugin;
            if (engineContext.DataContext.RenderPassPlugins.TryGetValueCast("SkyBoxPlugin", out skyBoxPlugin))
            {
                var skyBoxTexture = (Texture2D)await engineContext.AssetManager.LoadAsync<Texture>("/global_data/gdc_demo/bg/GDC2012_map_sky.dds");
                skyBoxPlugin.Texture = skyBoxTexture;
            }

            var effectMeshGroup = new RenderPassListEnumerator();
            engineContext.RenderContext.RenderPassEnumerators.Add(effectMeshGroup);

            var groundMesh = new EffectMesh(effect, MeshDataHelper.CreateBox(10000, 10000, 1, Color.White));
            groundMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);
            effectMeshGroup.AddMesh(groundMesh);
            groundMesh.Parameters.Set(TransformationKeys.World, Matrix.Translation(new Vector3(0, 0, 0)));

            // Lights
            for (int i = 0; i < 1024; ++i)
            {

                Color3 color = (Color3)Color.White;

                switch (i % 4)
                {
                    case 0: color = (Color3)Color.DarkOrange; break;
                    case 1: color = (Color3)Color.DarkGoldenrod; break;
                    case 2: color = (Color3)Color.DarkSalmon; break;
                    case 3: color = (Color3)Color.DarkRed; break;
                }
                var effectMesh = new EffectMesh(lightPrepassPlugin.Lights);
                effectMesh.Parameters.Set(LightKeys.LightRadius, (float)r.NextDouble() * 200 + 200.0f);
                effectMesh.Parameters.Set(LightKeys.LightColor, color);
                effectMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);

                effectMeshGroup.AddMesh(effectMesh);
            }

            EffectOld effectLight = lightPrepassPlugin.Lights;

            var lightInfo = new LightInfo[effectLight != null ? effectLight.Meshes.Count : 0];
            for (int i = 0; i < lightInfo.Length; ++i)
            {
                lightInfo[i].Radius = (float)r.NextDouble() * 7000.0f + 500.0f;
                lightInfo[i].Phase = (float)(r.NextDouble() * Math.PI * 2.0);
                lightInfo[i].Z = (float)r.NextDouble() * 3000.0f; ;
            }
            float time = 0.0f;


            // Meshes (quad) that will later be generated by the engine (light pre pass, SSAO, etc...)
                // Lights
            //var effectMesh = new EffectMesh(setup.LightingPrepassPlugin.Lights);
            //effectMesh.Parameters.Set(LightKeys.LightRadius, 1000.0f);
            //effectMesh.Parameters.Set(LightKeys.LightColor, new R32G32B32_Float(1.0f, 1.0f, 1.0f));
            //effectMesh.Parameters.Set(LightKeys.LightPosition, new R32G32B32_Float(0, 0, 1200));

            //effectMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);
            //effectMeshGroup.AddMesh(effectMesh);

            //var boxMesh = new EffectMesh(effect, MeshDataHelper.CreateBox(300, R8G8B8A8.LightBlue));
            //boxMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);
            //boxMesh.Parameters.Set(TransformationKeys.World, Matrix.Translation(new R32G32B32_Float(0, 0, 200)));
            //effectMeshGroup.AddMesh(boxMesh);


            var clock = new Stopwatch();
            clock.Start();


            int sizeX = 10;
            int sizeY = 10;

            var spheres = new Sphere[sizeY,sizeX];

            Random random = new Random(0);

            int size = 200;
            var meshData = MeshDataHelper.CreateSphere(size, 30, 30, Color.Gray);

            for (int iy = 0; iy < sizeY; iy++)
            {
                for (int ix = 0; ix < sizeX; ix++)
                {
                    var sphere = new Sphere();

                    sphere.Mesh = new EffectMesh(effect, meshData);
                    sphere.Phase = (float)random.NextDouble();
                    sphere.Speed = (float)random.NextDouble();

                    spheres[iy, ix] = sphere;
                    effectMeshGroup.AddMesh(sphere.Mesh);
                }
            }
            

            while (true)
            {
                await Scheduler.Current.NextFrame();


                for (int iy = 0; iy < sizeY; iy++)
                {
                    for (int ix = 0; ix < sizeX; ix++)
                    {
                        var iFactor = (float)(iy * sizeY + ix) / (sizeX * sizeY);

                        var sphere = spheres[iy, ix];
                        var sphereMesh = sphere.Mesh;
                        var specularColor = Color.SmoothStep(Color.GreenYellow, Color.Gray, iFactor);

                        // Matrix.RotationX((float)Math.PI/2.0f) * M
                        sphereMesh.Parameters.Set(
                            TransformationKeys.World,
                            Matrix.Translation(
                                new Vector3(
                                    (ix - sizeX / 2) * (size * 1.2f) * 2.0f,
                                    (iy - sizeY / 2) * (size * 1.2f) * 2.0f,
                                    (float)(2000 * (0.5 + 0.5 * Math.Sin(clock.ElapsedMilliseconds / 1000.0f * sphere.Speed * 0.5f + Math.PI * sphere.Phase))))));
                        sphereMesh.Parameters.Set(MaterialKeys.SpecularPower, iFactor * 0.9f);
                        sphereMesh.Parameters.Set(MaterialKeys.SpecularColor, specularColor);
                    }
                }

                time = clock.ElapsedMilliseconds / 1000.0f;

                if (lightInfo.Length > 0)
                {
                    int index = 0;
                    foreach (var mesh in effectLight.Meshes)
                    {
                        mesh.Parameters.Set(LightKeys.LightPosition, new Vector3(lightInfo[index].Radius * (float)Math.Cos(-time * 0.17f + lightInfo[index].Phase), lightInfo[index].Radius * (float)Math.Sin(-time * 0.05f + lightInfo[index].Phase), lightInfo[index].Z * (0.5f + 0.5f * (float)Math.Sin(-time * 0.1f + lightInfo[index].Phase * 2.0f))));
                        index++;
                    }
                }
            }
        }
    }
}
