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
    public class ScriptSingleSphere
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
                .Using(new GBufferShaderPlugin { RenderPassPlugin = (GBufferPlugin)engineContext.DataContext.RenderPassPlugins.TryGetValue("GBufferPlugin") })
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

            var meshData = MeshDataHelper.CreateSphere(200, 30, 30, Color.Gray);
            var sphereMesh = new EffectMesh(effect, meshData);
            sphereMesh.Parameters.Set(TransformationKeys.World,Matrix.Translation(new Vector3(0, 0, 50)));
            sphereMesh.Parameters.Set(MaterialKeys.SpecularPower, 0f);
            sphereMesh.Parameters.Set(MaterialKeys.SpecularColor, Color.White);
            effectMeshGroup.AddMesh(sphereMesh);

            //while (true)
            //{
            //    await Scheduler.Current.WaitFrame();


            //    for (int iy = 0; iy < sizeY; iy++)
            //    {
            //        for (int ix = 0; ix < sizeX; ix++)
            //        {
            //            var iFactor = (float)(iy * sizeY + ix) / (sizeX * sizeY);

            //            var sphere = spheres[iy, ix];
            //            var sphereMesh = sphere.Mesh;
            //            var specularColor = R8G8B8A8.SmoothStep(R8G8B8A8.GreenYellow, R8G8B8A8.Gray, iFactor);

            //            // Matrix.RotationX((float)Math.PI/2.0f) * M
            //            sphereMesh.Parameters.Set(
            //                TransformationKeys.World,
            //                Matrix.Translation(
            //                    new R32G32B32_Float(
            //                        (ix - sizeX / 2) * (size * 1.2f) * 2.0f,
            //                        (iy - sizeY / 2) * (size * 1.2f) * 2.0f,
            //                        (float)(2000 * (0.5 + 0.5 * Math.Sin(clock.ElapsedMilliseconds / 1000.0f * sphere.Speed * 0.5f + Math.PI * sphere.Phase))))));
            //            sphereMesh.Parameters.Set(MaterialKeys.SpecularPower, iFactor * 0.9f);
            //            sphereMesh.Parameters.Set(MaterialKeys.SpecularColor, specularColor);
            //        }
            //    }

            //    time = clock.ElapsedMilliseconds / 1000.0f;

            //    if (lightInfo.Length > 0)
            //    {
            //        int index = 0;
            //        foreach (var mesh in effectLight.Meshes)
            //        {
            //            mesh.Parameters.Set(LightKeys.LightPosition, new R32G32B32_Float(lightInfo[index].Radius * (float)Math.Cos(-time * 0.17f + lightInfo[index].Phase), lightInfo[index].Radius * (float)Math.Sin(-time * 0.05f + lightInfo[index].Phase), lightInfo[index].Z * (0.5f + 0.5f * (float)Math.Sin(-time * 0.1f + lightInfo[index].Phase * 2.0f))));
            //            index++;
            //        }
            //    }
            //}
        }
    }
}
