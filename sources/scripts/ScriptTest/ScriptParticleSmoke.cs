// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

// Copyright (c) 2011 ReShader - Alexandre Mutel

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.Mathematics;
using System.Linq;

using ScriptShader.Effects;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

//using Vector2 = Xenko.Framework.Mathematics.Vector2;
//using R32G32B32_Float = Xenko.Framework.Mathematics.R32G32B32_Float;

namespace ScriptTest
{
    public class ScriptParticleSmoke
    {
        /// <summary>
        /// Position for smoke emitters
        /// </summary>
        internal static readonly ParameterKey<SmokeEmitterDescription> SmokeEmitterKey = ParameterKeys.Value<SmokeEmitterDescription>();

        /// <summary>
        /// Position for bullet emitters
        /// </summary>
        internal static readonly ParameterKey<BulletEmitterDescription> BulletEmitterKey = ParameterKeys.Value<BulletEmitterDescription>();

        /// <summary>
        /// Position for smoke emitters
        /// </summary>
        internal static readonly ParameterKey<Buffer> VerticesEmitterKey = ParticleUpdaterBulletKeys.VerticesEmitter;

        /// <summary>
        /// First Texture0.
        /// </summary>
        internal static readonly ParameterKey<Texture> SmokeTexture = ParticleRenderSmokeKeys.SmokeTexture;

        /// <summary>
        /// First Texture0.
        /// </summary>
        internal static readonly ParameterKey<Texture> SmokeColor = ParticleRenderSmokeKeys.SmokeColor;

        // Generic particle structure
        [StructLayout(LayoutKind.Explicit, Size = 36, Pack = 4)]
        public struct ParticleData
        {
            // Position of this article in world space
            [FieldOffset(0)]
            public Vector3 Position;

            // Opacity of this particle
            [FieldOffset(12)]
            public float Time;

            // Velocity direction vector
            [FieldOffset(16)]
            public Half3 Velocity;

            // Size of this particle in screen space
            [FieldOffset(22)]
            public Half Opacity;

            // Custom factor (used for example to sample a texture array)
            [FieldOffset(24)]
            public Half4 Factors;

            // Time since the particle's creation
            [FieldOffset(32)]
            public Half Size;

            // Time since the particle's creation
            [FieldOffset(34)]
            public Half TimeStep;
        };

        public static void Run(EngineContext engineContext)
        {
            ParticlePlugin particlePlugin;
            if (!engineContext.DataContext.RenderPassPlugins.TryGetValueCast("ParticlePlugin", out particlePlugin))
                return;

            //engineContext.RenderContext.UIControl.KeyUp += (sender, args) =>
            //    {
            //        if (args.KeyCode >= Keys.F1 && args.KeyCode <= Keys.F12)
            //        {

            //            var stream = new FileStream("picking.txt", FileMode.Append);
            //            var streamWriter = new StreamWriter(stream);
            //            streamWriter.WriteLine("---------------------------------------------");
            //            streamWriter.WriteLine("- {0}", args.KeyCode);
            //            streamWriter.WriteLine("---------------------------------------------");
            //            streamWriter.Flush();
            //            stream.Close();
            //        }
            //    };

            var particleSystem = engineContext.EntityManager.GetSystem<ParticleProcessor>();

            var emitterPositions = new[]
                {
                    new Vector3(-2047.287f, -613.5108f, -400.0f),						// 0
                    new Vector3(-1881.002f, -564.9566f, -400.0f),						// 1
                    new Vector3(-1627.844f, -449.1949f, -400.0f),						// 2
                    new Vector3(-1391.335f, -423.1865f, -400.0f),						// 3
                    new Vector3(-1314.865f, -482.0599f, -400.0f),						// 4
                    new Vector3(-1019.54f, -932.4803f,  -400.0f),						// 5
                    new Vector3(-957.3735f, -988.7004f, -400.0f),						// 6
                    new Vector3(-759.9126f, -1168.646f, -400.0f),						// 7
                    new Vector3(-529.1716f, -1083.003f, -400.0f),						// 8
                    new Vector3(-198.7756f, -1029.24f,  -400.0f),						// 9
                    new Vector3(309.9702f, -832.7861f,  -400.0f),						// 10
                    new Vector3(876.9819f, -667.9489f,  -400.0f),						// 11
                    new Vector3(1908.686f, -1085.583f,  -400.0f),						// 12
                    new Vector3(2308.45f, -995.1572f,   -400.0f),						// 13
                    new Vector3(2864.581f, -906.4545f,  -400.0f),						// 14
                    new Vector3(3770.119f, -832.0695f,  -400.0f),						// 15
                    new Vector3(4561.941f, -728.9376f,  -400.0f),						// 16
                    new Vector3(5429.49f, -722.3638f,   -400.0f),						// 17
                    new Vector3(6447.015f, -310.0454f,  -400.0f),						// 18
                    new Vector3(6420.864f, 532.3475f,   -400.0f),						// 19
                    new Vector3(6157.83f, 658.0294f,    -400.0f),						// 20
                    new Vector3(4732.579f, 955.4061f,   -400.0f),						// 21
                    new Vector3(1630.28f, 1551.338f,    -400.0f),						// 22
                    new Vector3(931.7393f, 1274.533f,   -400.0f),						// 23
                    new Vector3(1586.493f, 1505.558f,   -400.0f),						// 24
                    new Vector3(906.572f, 1268.478f,    -400.0f),						// 25
                    new Vector3(390.1973f, 1314.976f,   -400.0f),						// 26
                    new Vector3(-30.39231f, 1553.894f,  -400.0f),						// 27
                    new Vector3(-356.4023f, 1605.162f,  -400.0f),						// 28
                    new Vector3(-1055.699f, 971.7286f,  -400.0f),						// 29
                    new Vector3(-1218.041f, 727.1427f,  -400.0f),						// 30
                    new Vector3(-1377.148f, 606.9602f,  -400.0f),						// 31
                    new Vector3(-1676.512f, 640.7913f,  -400.0f),						// 32
                    new Vector3(-2089.593f, 833.8343f,  -400.0f),						// 33
                    new Vector3(-2290.1f, 992.6068f,    -400.0f),						// 34
                    new Vector3(-2196.059f, 764.4152f,  -400.0f),						// 35
                    new Vector3(-1448.233f, 391.5037f,  -400.0f),						// 36
                    new Vector3(-1337.535f, 223.827f,   -400.0f),						// 37
                    new Vector3(-1287.335f, -125.6966f, -400.0f),						// 38
                    new Vector3(-4226.484f, -1213.027f, -400.0f),						// 39 - Magma Left
                    new Vector3(-4593.09f, -1091.131f,  -400.0f),						// 40
                    new Vector3(-4803.661f, -958.4816f, -400.0f),						// 41
                    new Vector3(-5262.959f, -1025.99f,  -400.0f),						// 42
                    new Vector3(-5519.119f, -881.3628f, -400.0f),						// 43
                    new Vector3(-5543.972f, -547.7667f, -400.0f),						// 44
                    new Vector3(-5775.069f, -294.6195f, -400.0f),						// 45
                    new Vector3(-6333.859f, -423.4442f, -400.0f),						// 46
                    new Vector3(-6977.528f, 840.5598f,  -400.0f),						// 47
                    new Vector3(-6847.938f, 1640.414f,  -400.0f),						// 48
                    new Vector3(-7259.18f, 1724.889f,   -400.0f),						// 49
                    new Vector3(-7693.181f, 1660.773f,  -400.0f),						// 50
                    new Vector3(-8300.401f, 1609.711f,  -400.0f),						// 51
                    new Vector3(-8704.221f, 1241.705f,  -400.0f),						// 52
                    new Vector3(-9049.8f, 905.2922f,    -400.0f),						// 53
                    new Vector3(-8739.72f, 105.7951f,   -400.0f),						// 54
                    new Vector3(-8515.267f, -371.7517f, -400.0f),						// 55
                    new Vector3(-8110.098f, -316.8557f, -400.0f),						// 56
                    new Vector3(-7915.391f, -304.8632f, -400.0f),						// 57
                    new Vector3(-7191.82f, -353.2674f,  -400.0f),						// 58
                    new Vector3(-6270.604f, -2246.958f, -400.0f),						// 59 - Magma right
                    new Vector3(-6655.961f, -2615.954f, -400.0f),						// 60
                    new Vector3(-7056.6f, -2839.48f,    -400.0f),						// 61
                    new Vector3(-7632.455f, -3047.234f, -400.0f),						// 62
                    new Vector3(-8325.431f, -2937.415f, -400.0f),						// 63
                    new Vector3(-8273.172f, -3403.743f, -400.0f),						// 64
                    new Vector3(-8179.38f, -3616.764f,  -400.0f),						// 65
                    new Vector3(-7814.024f, -4484.587f, -400.0f),						// 66
                    new Vector3(-6525.229f, -4816.507f, -400.0f),						// 67
                    new Vector3(-5648.252f, -4344.051f, -400.0f),						// 68
                    new Vector3(-6140.713f, -3957.125f, -400.0f),						// 69
                    new Vector3(-7001.114f, -3650.077f, -400.0f),						// 70
                };


            var random = new Random(1);

            var emitters = new SmokeParticleEmitterComponent[emitterPositions.Length];
            for (int i = 0; i < emitters.Length; i++)
            {

                var verticalScatter = (float)(2.0 + 3.0 * random.NextDouble());
                var horizontalScatter = (float)(3.0 + 6.0 * random.NextDouble());

                var emitter = new SmokeParticleEmitterComponent()
                    {
                        Count = 256,
                        Description = new SmokeEmitterDescription()
                        {
                            Position = emitterPositions[i],
                            Scatter = new Vector3(horizontalScatter, horizontalScatter, verticalScatter),
                            Velocity = new Vector3(0, 0.0f, 0.5f + 4.0f * (float)random.NextDouble()),
                            MaxTime = 1000.0f + 4000.0f * (float)random.NextDouble(),
                            InitialSize = 50.0f + 30.0f * (float)random.NextDouble(),
                            DeltaSize = 30.0f + 20.0f * (float)random.NextDouble(),
                            Opacity = 0.7f,
                        }
                    };
                emitter.OnUpdateData();

                emitters[i] = emitter;
            }

            var smokeVolTexture = (Texture2D)engineContext.AssetManager.Load<Texture>("/global_data/gdc_demo/fx/smokevol.dds");
            var smokeGradTexture = (Texture2D)engineContext.AssetManager.Load<Texture>("/global_data/gdc_demo/fx/smokegrad.dds");
            particlePlugin.Parameters.Set(SmokeTexture, smokeVolTexture);
            particlePlugin.Parameters.Set(SmokeColor, smokeGradTexture);

            var particleEmitterRootEntity = new Entity("ParticleEmitters");
            particleEmitterRootEntity.Set(TransformationComponent.Key, new TransformationComponent());
            engineContext.EntityManager.AddEntity(particleEmitterRootEntity);

            for (int index = 0; index < emitters.Length; index++)
            {
                var emitter = emitters[index];
                var entity = new Entity(string.Format("ParticleEmitter-{0}", index));
                entity.Set(TransformationComponent.Key, new TransformationComponent(new TransformationTRS { Translation = emitter.Description.Position }));
                entity.Set(ParticleEmitterComponent.Key, emitter);

                particleEmitterRootEntity.Transformation.Children.Add(entity.Transformation);
            }
        }
    }
}
