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
            const int numberOfTests = 30;

            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                TestParticles();
            }
            var timeTestParticles = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestParticles:0000.000} ms to run TestParticles");

            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                TestPool(listPolicy: ParticlePool.ListPolicy.Ring, fieldsPolicy: ParticlePool.FieldsPolicy.AoS);
            }
            var timeTestPool1 = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestPool1:0000.000} ms to run TestPool/Ring+AoS");

            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                TestPool(listPolicy: ParticlePool.ListPolicy.Stack, fieldsPolicy: ParticlePool.FieldsPolicy.AoS);
            }
            var timeTestPool2 = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestPool2:0000.000} ms to run TestPool/Stack+AoS");



            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                TestPool(listPolicy: ParticlePool.ListPolicy.Ring, fieldsPolicy: ParticlePool.FieldsPolicy.SoA);
            }
            var timeTestPool3 = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestPool3:0000.000} ms to run TestPool/Ring+SoA");

            watch.Restart();
            for (int i = 0; i < numberOfTests; i++)
            {
                TestPool(listPolicy: ParticlePool.ListPolicy.Stack, fieldsPolicy: ParticlePool.FieldsPolicy.SoA);
            }
            var timeTestPool4 = watch.Elapsed.TotalMilliseconds;
            System.Console.Out.WriteLine($"{timeTestPool4:0000.000} ms to run TestPool/Stack+SoA");

            System.Console.ReadLine();

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

        private static ParticleFieldAccessor<T> AddField<T>(IntPtr poolPtr, int size) where T : struct
        {
            return new ParticleFieldAccessor<T>(poolPtr, size);
        }

        private static void TestParticles()
        {
            var particleCount = 10000;
            var particleSize = 32;
            var poolPtr = Utilities.AllocateMemory(particleSize * particleCount);

            // Create several random fields and try to access them
            var positionField = AddField<Vector3>(poolPtr + 4 * 0, particleSize);
            var lifetimeField = AddField<float>  (poolPtr + 4 * 3, particleSize);
            var velocityField = AddField<Vector3>(poolPtr + 4 * 4, particleSize);
            var sizeField     = AddField<float>  (poolPtr + 4 * 7, particleSize);


            for (var i = 0; i < particleCount; i++)
            {
                var particle = new Particle(i);

                particle.Set(positionField, new Vector3(0, i, 0));
                particle.Set(lifetimeField, i);
                particle.Set(velocityField, new Vector3(0, i, 0));
                particle.Set(sizeField, i);
            }

            for (var i = 0; i < particleCount; i++)
            {
                var particle = new Particle(i);

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
        private static void TestPool(ParticlePool.ListPolicy listPolicy, ParticlePool.FieldsPolicy fieldsPolicy)
        {
            const int particleCount = 10000;
            var particlePool = new ParticlePool(0, particleCount, fieldsPolicy, listPolicy);

            const bool forceCreation = true;
            particlePool.FieldExists(ParticleFields.Position     , forceCreation);
            particlePool.FieldExists(ParticleFields.RemainingLife, forceCreation);
            particlePool.FieldExists(ParticleFields.Velocity     , forceCreation);
            particlePool.FieldExists(ParticleFields.Size         , forceCreation);

            var positionField = particlePool.GetField(ParticleFields.Position);
            var lifetimeField = particlePool.GetField(ParticleFields.RemainingLife);
            var velocityField = particlePool.GetField(ParticleFields.Velocity);
            var sizeField     = particlePool.GetField(ParticleFields.Size);

            for (int idx = 0; idx < particleCount; idx++)
            {
                particlePool.AddParticle();
            }

            var i = 0;
            var vecToSet = new Vector3(0, 0, 0);
            foreach (var particle in particlePool)
            {
                vecToSet.Y = i;
                particle.Set(positionField, vecToSet);
                particle.Set(lifetimeField, i);
                particle.Set(velocityField, vecToSet);
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
                vecToSet.Y = i;
                Assert(particle.Get(positionField).Equals(vecToSet + particle.Get(velocityField) * dt), "Particle's position is different!");
                Assert(Math.Abs(particle.Get(lifetimeField) - i - 1) <= MathUtil.ZeroTolerance, "Particle's lifetime is different!");
                i++;
            }

            var testVec = new Vector3(0, 1, 0);
            // Perf test - many mundane operations at once
            foreach (var particle in particlePool)
            {
                particle.Set(velocityField, particle.Get(velocityField) + testVec);
                particle.Set(velocityField, particle.Get(velocityField) + testVec);
                particle.Set(velocityField, particle.Get(velocityField) + testVec);
                particle.Set(velocityField, particle.Get(velocityField) + testVec);
                particle.Set(velocityField, particle.Get(velocityField) + testVec);
            }

            foreach (var particle in particlePool)
            {
                particle.Set(sizeField, particle.Get(sizeField) + 1);
                particle.Set(sizeField, particle.Get(sizeField) + 1);
                particle.Set(sizeField, particle.Get(sizeField) + 1);
                particle.Set(sizeField, particle.Get(sizeField) + 1);
                particle.Set(sizeField, particle.Get(sizeField) + 1);
            }

            foreach (var particle in particlePool)
            {
                particle.Set(positionField, particle.Get(positionField) + testVec);
                particle.Set(positionField, particle.Get(positionField) + testVec);
                particle.Set(positionField, particle.Get(positionField) + testVec);
                particle.Set(positionField, particle.Get(positionField) + testVec);
                particle.Set(positionField, particle.Get(positionField) + testVec);
            }

            foreach (var particle in particlePool)
            {
                particle.Set(lifetimeField, particle.Get(lifetimeField) + 1);
                particle.Set(lifetimeField, particle.Get(lifetimeField) + 1);
                particle.Set(lifetimeField, particle.Get(lifetimeField) + 1);
                particle.Set(lifetimeField, particle.Get(lifetimeField) + 1);
                particle.Set(lifetimeField, particle.Get(lifetimeField) + 1);
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

            if (listPolicy != ParticlePool.ListPolicy.Ring)
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
