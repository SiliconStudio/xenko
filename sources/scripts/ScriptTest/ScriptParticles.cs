// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

// Copyright (c) 2011 ReShader - Alexandre Mutel

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.Mathematics;
using Xenko.Framework.Shaders;
using Vector2 = SiliconStudio.Xenko.Games.Mathematics.Vector2;
using Vector3 = SiliconStudio.Xenko.Games.Mathematics.Vector3;

namespace ScriptTest
{
    public class ScriptParticles
    {
        // Generic particle structure 3 * float4 = 48 bytes 
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct ParticleData
        {
            // Position of this article in world space
            [FieldOffset(0)]
            public Vector3 Position;

            // Opacity of this particle
            [FieldOffset(12)]
            public float Opacity;

            // Velocity direction vector
            [FieldOffset(16)]
            public Vector3 Velocity;

            // Size of this particle in screen space
            [FieldOffset(28)]
            public float Time;

            // Custom factor (used for example to sample a texture array)
            [FieldOffset(32)]
            public Vector3 Factors;

            // Time since the particle's creation
            [FieldOffset(44)]
            public float Size;
        };

        public static async Task Run(EngineContext engineContext)
        {
            ParticlePlugin particlePlugin;
            if (!engineContext.DataContext.RenderPassPlugins.TryGetValueCast("ParticlePlugin", out particlePlugin))
                return;

            var count = particlePlugin.CapacityCount;

            var particlesBuffer = new ParticleData[count];
            var random = new Random();
            for (int i = 0; i < particlesBuffer.Length; i++)
            {
                particlesBuffer[i] = new ParticleData
                    {
                        Position = new Vector3(1000.0f - (float)random.NextDouble() * 6000, 1500.0f - (float)random.NextDouble() * 3000.0f, 0),
                        Velocity = new Vector3(0, 0, 2.0f + 10.0f * (float)random.NextDouble()),
                        Time = 5000.0f * (float)random.NextDouble(),
                        Size = 1.0f + (float)random.NextDouble() * 10.0f,
                        Factors = new Vector3(1.0f + ((i & 255) == 0 ? (25.0f + 50.0f * (float)random.NextDouble()) : -(float)random.NextDouble() * ((i & 3) == 0 ? 2.0f : 1.0f)), 0, 0),
                        Opacity = 1.0f,
                    };
                particlesBuffer[i].Position.Z = particlesBuffer[i].Velocity.Z * particlesBuffer[i].Time / 100.0f;
            }

            var particleUpdater = new ParticleEmitterComponent()
                {
                    Type = ParticleEmitterType.GpuStatic,
                    Count = count,
                    Shader = new ShaderClassSource("ParticleUpdaterTest1"),
                };
            particleUpdater.ParticleData = particlesBuffer;
            particleUpdater.ParticleElementSize = Utilities.SizeOf<ParticleData>();

            // Add this particle updater to the particle engine
            particlePlugin.Updaters.Add(particleUpdater);
        }
    }
}
