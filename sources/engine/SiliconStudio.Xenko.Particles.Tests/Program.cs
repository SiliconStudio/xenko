// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();

            watch.Restart();
            TestParticles();
            var timeTestParticles = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestParticles:0000.000} ms to run TestParticles");

            watch.Restart();
            TestPool(poolListPolicy: ParticlePool.PoolListPolicy.Ring, poolFieldsPolicy: ParticlePool.PoolFieldsPolicy.AoS);
            var timeTestPool1 = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestPool1:0000.000} ms to run TestPool/Ring+AoS");

            watch.Restart();
            TestPool(poolListPolicy: ParticlePool.PoolListPolicy.Stack, poolFieldsPolicy: ParticlePool.PoolFieldsPolicy.AoS);
            var timeTestPool2 = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestPool2:0000.000} ms to run TestPool/Stack+AoS");

            // TODO AoS pool vs SoA pool testing

            // Later - test Emitter updates

            // Later - test adding/removing fields

            // Later - test adding/removing modules

            // Much later - test draw calls
        }

        private static void Assert(bool condition, string message, [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int callerLine = 0)
        {
            Debug.Assert(condition, $"Assert failed in {callingFilePath} at line[{callerLine}]: {message}");
        }

        private static ParticleFieldAccessor<T> AddField<T>(ref int totalOffset) where T : struct
        {
            var fieldAccessor = new ParticleFieldAccessor<T>(totalOffset);

            var fieldSize = Utilities.SizeOf<T>();

            totalOffset += fieldSize;

            return fieldAccessor;
        }

        private static void TestParticles()
        {
            // Create several random fields and try to access them
            var totalOffset = 0;
            var positionField = AddField<Vector3>(ref totalOffset);
            var lifetimeField = AddField<float>(ref totalOffset);
            var velocityField = AddField<Vector3>(ref totalOffset);
            var sizeField = AddField<float>(ref totalOffset);

            var particleCount = 10000;
            var poolPtr = Utilities.AllocateMemory(totalOffset * particleCount);

            for (var i = 0; i < particleCount; i++)
            {
                var particle = new Particle(poolPtr + i * totalOffset);

                particle.Set(positionField, new Vector3(0, i, 0));
                particle.Set(lifetimeField, i);
                particle.Set(velocityField, new Vector3(0, i, 0));
                particle.Set(sizeField, i);
            }

            for (var i = 0; i < particleCount; i++)
            {
                var particle = new Particle(poolPtr + i * totalOffset);

                Assert(particle.Get(positionField).Equals(new Vector3(0, i, 0)), $"Position.Y does not equal {i}");
                Assert(Math.Abs(particle.Get(lifetimeField) - i) <= MathUtil.ZeroTolerance, $"Remaining lifetime does not equal {i}");
                Assert(particle.Get(velocityField).Equals(new Vector3(0, i, 0)), $"Velocity.Y does not equal {i}");
                Assert(Math.Abs(particle.Get(sizeField) - i) <= MathUtil.ZeroTolerance, $"Size does not equal {i}");
            }

            Utilities.FreeMemory(poolPtr);
        }

        
        /// <summary>
        /// This test simulates how an Emitter would access and update the particles.
        /// Some numbers are fixed, like particle total count and field offsets.
        /// </summary>
        private static void TestPool(ParticlePool.PoolListPolicy poolListPolicy, ParticlePool.PoolFieldsPolicy poolFieldsPolicy)
        {
            // TODO Fields should be registered in the particle pool
            var totalOffset = 0;
            var positionField = AddField<Vector3>(ref totalOffset);
            var lifetimeField = AddField<float>  (ref totalOffset);
            var velocityField = AddField<Vector3>(ref totalOffset);
            var sizeField     = AddField<float>  (ref totalOffset);

            var particleCount = 10000;
            var particlePool = new ParticlePool(totalOffset, particleCount, poolFieldsPolicy, poolListPolicy);
            for (int idx = 0; idx < particleCount; idx++)
            {
                particlePool.AddParticle();
            }

            var i = 0;
            foreach (var particle in particlePool)
            {
                particle.Set(positionField, new Vector3(0, i, 0));
                particle.Set(lifetimeField, i);
                particle.Set(velocityField, new Vector3(0, i, 0));
                particle.Set(sizeField, i);
                i++;
            }

            i = 0;
            foreach (var particle in particlePool)
            {
                Assert(particle.Get(positionField).Equals(new Vector3(0, i, 0)), $"Position.Y does not equal {i}");
                Assert(Math.Abs(particle.Get(lifetimeField) - i) <= MathUtil.ZeroTolerance, $"Remaining life does not equal {i}");
                Assert(particle.Get(velocityField).Equals(new Vector3(0, i, 0)), $"Velocity.Y does not equal {i}");
                Assert(Math.Abs(particle.Get(sizeField) - i) <= MathUtil.ZeroTolerance, $"Size does not equal {i}");
                i++;
            }


            i = 0;
            var dt = 0.033f;
            foreach (var particle in particlePool)
            {
                particle.Set(positionField, particle.Get(positionField) + particle.Get(velocityField) * dt);
                particle.Set(lifetimeField, particle.Get(lifetimeField) + 1);
                i++;
            }

            i = 0;
            foreach (var particle in particlePool)
            {
                Assert(particle.Get(positionField).Equals(new Vector3(0, i, 0) + particle.Get(velocityField) * dt), "Particle's position is different!");
                Assert(Math.Abs(particle.Get(lifetimeField) - i - 1) <= MathUtil.ZeroTolerance, "Particle's lifetime is different!");
                i++;
            }

            i = 0;
            var poolEnumerator = particlePool.GetEnumerator();
            while (poolEnumerator.MoveNext())
            {
                var particle = poolEnumerator.Current;
                if (i%2 == 0)
                {
                    poolEnumerator.RemoveCurrent(ref particle);
                    particle.Set(sizeField, particleCount + 9000);                   
                }
                i++;
            }

            if (poolListPolicy != ParticlePool.PoolListPolicy.Ring)
            {
                i = 0;
                foreach (var particle in particlePool)
                {
                    Assert(particle.Get(sizeField) < particleCount + 9000, "This particle is supposed to be dead!");
                    i++;
                }

                Assert(i <= particleCount/2, "Number of living particles should be no more than half!");
            }
        }
    }
}
