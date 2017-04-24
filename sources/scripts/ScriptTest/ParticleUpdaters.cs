// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Particles;

namespace ScriptTest
{
    public class SimpleEmitter : IParticlePlugin
    {
        Random rand = new Random();

        public void Update(ParticleSystem particleSystem, float dt)
        {
            var velocityField = particleSystem.GetOrCreateField(ParticleFields.Velocity);

            // No more than 1000 particles
            if (particleSystem.ParticleCount < 1000)
            {
                var particle = particleSystem.AddParticle();

                // Default random velocity, going upward
                particle.Set(velocityField, new Vector3((float)rand.NextDouble() * 30.0f - 15.0f, (float)rand.NextDouble() * 30.0f - 15.0f, (float)rand.NextDouble() * 50.0f));
            }
        }
    }

    public class RemoveOldParticles : IParticlePlugin
    {
        /// <summary>Particle field storing the particle current lifetime (age).</summary>
        public static readonly ParticleFieldDescription<float> LifetimeField = new ParticleFieldDescription<float>("Lifetime");

        public float Lifetime { get; set; }

        public RemoveOldParticles(float lifetime)
        {
            Lifetime = lifetime;
        }

        public unsafe void Update(ParticleSystem particleSystem, float dt)
        {
            var lifetimeField = particleSystem.GetOrCreateField(LifetimeField);

            // Iterate over particles
            var particleEnumerator = particleSystem.GetEnumerator();
            while (particleEnumerator.MoveNext())
            {
                var particle = particleEnumerator.Current;
                var lifetime = (float*)particle[lifetimeField];

                // Remove particle through enumerator.
                // Enumerator will be valid again in next loop.
                if ((*lifetime += dt) > Lifetime)
                    particleEnumerator.RemoveParticle();
            }
        }
    }

    public class ResetAcceleration : IParticlePlugin
    {
        public unsafe void Update(ParticleSystem particleSystem, float dt)
        {
            var accelerationField = particleSystem.GetOrCreateField(ParticleFields.Acceleration);

            foreach (var particle in particleSystem)
            {
                var acceleration = (Vector3*)particle[accelerationField];
                *acceleration = new Vector3(0.0f);
            }
        }
    }

    public class Gravity : IParticlePlugin
    {
        public Vector3 GravityForce { get; set; }

        public Gravity()
        {
            GravityForce = new Vector3(0.0f, 0.0f, -9.81f);
        }

        public unsafe void Update(ParticleSystem particleSystem, float dt)
        {
            var accelerationField = particleSystem.GetOrCreateField(ParticleFields.Acceleration);

            foreach (var particle in particleSystem)
            {
                var acceleration = (Vector3*)particle[accelerationField];
                *acceleration += GravityForce;
            }
        }
    }

    public class UpdateVelocity : IParticlePlugin
    {
        public unsafe void Update(ParticleSystem particleSystem, float dt)
        {
            var accelerationField = particleSystem.GetOrCreateField(ParticleFields.Acceleration);
            var velocityField = particleSystem.GetOrCreateField(ParticleFields.Velocity);

            foreach (var particle in particleSystem)
            {
                var position = (Vector3*)particle[particleSystem.Position];
                var acceleration = (Vector3*)particle[accelerationField];
                var velocity = (Vector3*)particle[velocityField];
                *velocity += *acceleration * dt;
                *position += *velocity * dt;
            }
        }
    }
}
