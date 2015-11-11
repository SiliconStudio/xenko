// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestParticles();

            TestPool();

            // TODO AoS pool vs SoA pool testing

            // Later - test Emitter updates

            // Later - test adding/removing fields

            // Later - test adding/removing modules

            // Much later - test draw calls
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

                Debug.Assert(particle.Get(positionField).Equals(new Vector3(0, i, 0)));
                Debug.Assert(Math.Abs(particle.Get(lifetimeField) - i) <= MathUtil.ZeroTolerance);
                Debug.Assert(particle.Get(velocityField).Equals(new Vector3(0, i, 0)));
                Debug.Assert(Math.Abs(particle.Get(sizeField) - i) <= MathUtil.ZeroTolerance);
            }

            Utilities.FreeMemory(poolPtr);
        }

        /// <summary>
        /// This test simulates how an Emitter would access and update the particles.
        /// Some numbers are fixed, like particle total count and field offsets.
        /// </summary>
        private static void TestPool()
        {
            // Create several random fields and try to access them
            var totalOffset = 0;
            var positionField = AddField<Vector3>(ref totalOffset);
            var lifetimeField = AddField<float>  (ref totalOffset);
            var velocityField = AddField<Vector3>(ref totalOffset);
            var sizeField     = AddField<float>  (ref totalOffset);

            var particleCount = 10000;
            var particlePool = new ParticlePool(totalOffset, particleCount);

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
                Debug.Assert(particle.Get(positionField).Equals(new Vector3(0, i, 0)));
                Debug.Assert(Math.Abs(particle.Get(lifetimeField) - i) <= MathUtil.ZeroTolerance);
                Debug.Assert(particle.Get(velocityField).Equals(new Vector3(0, i, 0)));
                Debug.Assert(Math.Abs(particle.Get(sizeField) - i) <= MathUtil.ZeroTolerance);
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
                Debug.Assert(particle.Get(positionField).Equals(new Vector3(0, i, 0) + particle.Get(velocityField) * dt));
                Debug.Assert(Math.Abs(particle.Get(lifetimeField) - i - 1) <= MathUtil.ZeroTolerance);
                i++;
            }
        }
    }
}
