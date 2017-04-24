// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Shaders;

#if NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace ScriptTest
{
    [XenkoScript]
    public class ScriptPermutation
    {
        [XenkoScript]
        public static async Task Run(EngineContext engineContext)
        {
            var renderingSetup = RenderingSetup.Singleton;
            //engineContext.RenderContext.Register(renderingSetup.LightingPlugin);
            //renderingSetup.RegisterLighting(engineContext);

            var shadowMapPlugin = new LightingPlugin { MainPlugin = renderingSetup.MainPlugin, RenderPass = engineContext.DataContext.RenderPasses.TryGetValue("ShadowMapPass") };
            shadowMapPlugin.RenderContext = engineContext.RenderContext;

            var shadowMap1 = new ShadowMap(new DirectionalLight()) { Level = CascadeShadowMapLevel.X1, ShadowMapSize = 1024, ShadowDistance = 2000.0f }
                .SetFilter(sm => new ShadowMapFilterDefault(sm));
            var shadowMap2 = new ShadowMap(new DirectionalLight()) { Level = CascadeShadowMapLevel.X2, ShadowMapSize = 1024, ShadowDistance = 2000.0f }
                .SetFilter(sm => new ShadowMapFilterVsm(sm));

            shadowMapPlugin.AddShadowMap(shadowMap1);
            shadowMapPlugin.AddShadowMap(shadowMap2);

            shadowMap1.DirectionalLight.LightDirection = new Vector3(-1.0f, -1.0f, -1.0f);
            shadowMap1.DirectionalLight.LightColor = new Color3(1.0f, 0.5f, 0.5f);
            shadowMap2.DirectionalLight.LightDirection = new Vector3(-1.0f, 1.0f, -1.0f);
            shadowMap2.DirectionalLight.LightColor = new Color3(0.5f, 0.5f, 1.0f);

            engineContext.RenderContext.Register(shadowMapPlugin);

            EffectOld effect = engineContext.RenderContext.BuildEffect("Permutation")
                .Using(new BasicShaderPlugin("ShaderBase") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new BasicShaderPlugin(new ShaderMixinSource()
                                    {
                                        "NormalVSStream",
                                        "PositionVSStream",
                                        new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorStream")),
                                        new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColor")), // TODO: Default values!
                                        "BRDFDiffuseLambert",
                                        "BRDFSpecularBlinnPhong",
                                        "ShadingBase",
                                    }) { RenderPassPlugin = renderingSetup.MainTargetPlugin })
                .Using(new LightingShaderPlugin() { RenderPassPlugin = shadowMapPlugin })
                ;

            var sphereMeshData = MeshDataHelper.CreateSphere(10.0f, 20, 20, Color.White);

            var effectMeshGroup = new RenderPassListEnumerator();
            engineContext.RenderContext.RenderPassEnumerators.Add(effectMeshGroup);

            int objectSqrtCount = 1;
            int meshCount = objectSqrtCount * objectSqrtCount * objectSqrtCount;

            var shadowMapPermutation1 = new ShadowMapPermutationArray { ShadowMaps = { shadowMap1 } };
            var shadowMapPermutation2 = new ShadowMapPermutationArray { ShadowMaps = { shadowMap2 } };
            var shadowMapPermutation3 = new ShadowMapPermutationArray { ShadowMaps = { shadowMap1, shadowMap2 } };

            effect.Permutations.Set(ShadowMapPermutationArray.Key, shadowMapPermutation1);

            var groups2 = new[] { shadowMapPermutation1, shadowMapPermutation2, shadowMapPermutation3 };
            int groupIndex2 = 0;

            var effectMeshes = new List<EffectMesh>();
            for (int j = 0; j < meshCount; ++j)
            {
                var effectMesh = new EffectMesh(effect, sphereMeshData);
                effectMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);

                effect.Permutations.Set(ShadowMapPermutationArray.Key, groups2[2]);

                var w2 = Matrix.Scaling(1.0f)
                            * Matrix.Translation(new Vector3(
                                (j % objectSqrtCount - objectSqrtCount / 2) * 30.0f - 30.0f,
                                (((j / objectSqrtCount) % objectSqrtCount) - objectSqrtCount / 2) * 30.0f - 30.0f,
                                (j / (objectSqrtCount * objectSqrtCount)) * 30.0f + 30.0f));

                effectMesh.Parameters.Set(TransformationKeys.World, w2);
                effectMeshes.Add(effectMesh);
            }

            var groundMesh = new EffectMesh(effect, MeshDataHelper.CreateBox(1000, 1000, 1, Color.White));
            groundMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);
            effectMeshGroup.AddMesh(groundMesh);
            groundMesh.Parameters.Set(TransformationKeys.World, Matrix.Translation(0.0f, 0.0f, 0.0f));

            var groups = new[] { new int[] { 0, 1 }, new int[] { 0 }, new int[] { 1 } };
            int groupIndex = 0;

            await TaskEx.Delay(1000);
            foreach (var effectMesh in effectMeshes)
                effectMeshGroup.AddMesh(effectMesh);

            while (true)
            {
                await engineContext.Scheduler.NextFrame();

                if (engineContext.InputManager.IsKeyPressed(Keys.F8))
                {
                    var permutation = new LightingPermutation { LightBindings = { new Light() } };
                    effect.Permutations.Set(LightingPermutation.Key, permutation);
                }
                if (engineContext.InputManager.IsKeyPressed(Keys.F9))
                {
                    effect.Permutations.Set(ShadowMapPermutationArray.Key, groups2[(groupIndex2++) % groups2.Length]);
                }
            }
        }
    }
}
