// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
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

namespace ScriptTest
{
    [XenkoScript]
    public class ScriptMulticore
    {
        [XenkoScript]
        public static async Task Run(EngineContext engineContext)
        {
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

            EffectOld effect = engineContext.RenderContext.Effects.First(x => x.Name == "Multicore");
            //Effect effect = engineContext.RenderContext.BuildEffect("Multicore")
            //    .Using(new BasicShaderPlugin("ShaderBase") { RenderPassPlugin = renderingSetup.MainDepthReadOnlyPlugin })
            //    .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = renderingSetup.MainDepthReadOnlyPlugin })
            //    .Using(new BasicShaderPlugin(new ShaderMixinSource()
            //                        {
            //                            "NormalVSStream",
            //                            "PositionVSStream",
            //                            new ShaderComposition("albedoDiffuse", new ShaderClassSource("ComputeColorStream")),
            //                            new ShaderComposition("albedoSpecular", new ShaderClassSource("ComputeColor")), // TODO: Default values!
            //                            "BRDFDiffuseLambert",
            //                            "BRDFSpecularBlinnPhong",
            //                        }) { RenderPassPlugin = renderingSetup.MainDepthReadOnlyPlugin })
            //    .Using(new BasicShaderPlugin("AlbedoFlatShading") { RenderPassPlugin = renderingSetup.MainDepthReadOnlyPlugin })
            //    .Using(new LightingShaderPlugin { RenderPassPlugin = renderingSetup.LightingPlugin })
            //    //.Using(new BasicShaderPlugin("LightDirectionalShading") { RenderPassPlugin = renderingSetup.MainDepthReadOnlyPlugin })
            //    ;

            //effect.Permutations.Set(LightingPermutation.Key, new LightingPermutation { Lights = new Light[] { new DirectionalLight { LightColor = new Color3(1.0f), LightDirection = new R32G32B32_Float(-1.0f, -1.0f, 1.0f) } } });

            var rand = new Random();
            var cubeMeshData = Enumerable.Range(0, 10).Select(x => MeshDataHelper.CreateBox(10, 10, 10, new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f))).ToArray();

            var effectMeshGroup = new RenderPassListEnumerator();
            engineContext.RenderContext.RenderPassEnumerators.Add(effectMeshGroup);

            int objectSqrtCount = 31;
            int meshCount = objectSqrtCount * objectSqrtCount * objectSqrtCount;

            for (int j = 0; j < meshCount; ++j)
            {
                var effectMesh = new EffectMesh(effect, cubeMeshData[(j / 25) % 10]);
                effectMesh.KeepAliveBy(engineContext.SimpleComponentRegistry);
                effectMeshGroup.AddMesh(effectMesh);

                var w2 = Matrix.Scaling(1.0f)
                            * Matrix.Translation(new Vector3(
                                (j % objectSqrtCount - objectSqrtCount / 2) * 30.0f - 30.0f,
                                (((j / objectSqrtCount) % objectSqrtCount) - objectSqrtCount / 2) * 30.0f - 30.0f,
                                (j / (objectSqrtCount * objectSqrtCount) - objectSqrtCount / 2) * 30.0f - 30.0f));

                effectMesh.Parameters.Set(TransformationKeys.World, w2);
            }
        }
    }
}
