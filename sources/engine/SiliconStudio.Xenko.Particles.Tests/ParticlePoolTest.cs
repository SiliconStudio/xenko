// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class ParticlePoolTest
    {       

        [TestCase(ParticlePool.ListPolicy.Stack)]
        [TestCase(ParticlePool.ListPolicy.Ring)]
        public unsafe void PoolCapacity(ParticlePool.ListPolicy policy)
        {
            const int maxParticles = 10;
            var pool = new ParticlePool(0, maxParticles, policy);

            const bool forceCreation = true;
            pool.FieldExists(ParticleFields.Position,       forceCreation);
            pool.FieldExists(ParticleFields.RemainingLife,  forceCreation);
            pool.FieldExists(ParticleFields.Velocity,       forceCreation);
            pool.FieldExists(ParticleFields.Size,           forceCreation);

            var testPos = new Vector3(1, 2, 3);
            var testVel = new Vector3(5, 6, 7);
            var testLife = 5f;
            var testSize = 4f;

            // Spawn all particles
            for (int i = 0; i < maxParticles; i++)
            {
                pool.AddParticle();
            }


            {
                // Field accessors break every time there is a change in the pool, so we need to exract them every time
                // We can extract them before the tight loop on all living particles
                var positionField   = pool.GetField(ParticleFields.Position);
                var lifetimeField   = pool.GetField(ParticleFields.RemainingLife);
                var velocityField   = pool.GetField(ParticleFields.Velocity);
                var sizeField       = pool.GetField(ParticleFields.Size);

                foreach (var particle in pool)
                {
                    *((Vector3*)particle[positionField]) = testPos;

                    *((float*)particle[lifetimeField]) = testLife;

                    *((Vector3*)particle[velocityField]) = testVel;

                    *((float*)particle[sizeField]) = testSize;
                }
            }

            // Double the pool capacity and assert that the first half of particles still have the same fields
            pool.SetCapacity(2 * maxParticles);
            {
                // Field accessors break every time there is a change in the pool
                var positionField = pool.GetField(ParticleFields.Position);
                var lifetimeField = pool.GetField(ParticleFields.RemainingLife);
                var velocityField = pool.GetField(ParticleFields.Velocity);
                var sizeField = pool.GetField(ParticleFields.Size);

                var sorter = new ParticleSorterLiving(pool);
                sorter.Sort();

                var i = 0;
                foreach (var particle in sorter)
                {
                    Assert.That(*((Vector3*)particle[positionField]), Is.EqualTo(testPos));

                    Assert.That(*((float*)particle[lifetimeField]), Is.EqualTo(testLife));

                    Assert.That(*((Vector3*)particle[velocityField]), Is.EqualTo(testVel));

                    Assert.That(*((float*)particle[sizeField]), Is.EqualTo(testSize));

                    i++;
                }

                // Assert that the number of living particles is still maxParticles, not maxParticles x2
                Assert.That(i, Is.EqualTo(maxParticles));
            }

            // Halve the pool capacity from its original size. Now all the particles should still have the same fields
            pool.SetCapacity(maxParticles / 2);
            {
                // Field accessors break every time there is a change in the pool
                var positionField = pool.GetField(ParticleFields.Position);
                var lifetimeField = pool.GetField(ParticleFields.RemainingLife);
                var velocityField = pool.GetField(ParticleFields.Velocity);
                var sizeField = pool.GetField(ParticleFields.Size);

                var sorter = new ParticleSorterLiving(pool);
                sorter.Sort();

                var i = 0;
                foreach (var particle in sorter)
                {
                    Assert.That(*((Vector3*)particle[positionField]), Is.EqualTo(testPos));

                    Assert.That(*((float*)particle[lifetimeField]), Is.EqualTo(testLife));

                    Assert.That(*((Vector3*)particle[velocityField]), Is.EqualTo(testVel));

                    Assert.That(*((float*)particle[sizeField]), Is.EqualTo(testSize));

                    i++;
                }

                // Assert that the number of living particles is still maxParticles /2, not maxParticles x2
                Assert.That(i, Is.EqualTo(maxParticles / 2));
            }

        }


    }
}
