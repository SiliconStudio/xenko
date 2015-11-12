// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    public static class ParticleFields
    {
        /// <summary>
        /// Particle position in 3D space
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Position = new ParticleFieldDescription<Vector3>("Position", new Vector3(0, 0, 0));

        /// <summary>
        /// Particle velocity in 3D space
        /// </summary>
        public static readonly ParticleFieldDescription<Vector3> Velocity = new ParticleFieldDescription<Vector3>("Velocity", new Vector3(0, 0, 0));

        /// <summary>
        /// Particle uniform size. If particles are rendered as a 2D quads or 3D meshes, these extra dimensions can be set on the material side.
        /// </summary>
        public static readonly ParticleFieldDescription<float> Size = new ParticleFieldDescription<float>("Size", 1);

        /// <summary>
        /// Particle remaining lifetime. When it reaches 0, the particle dies.
        /// Remaining life is easier to work with because it is an absolute value. Total life needs to know what the maximum life is.
        /// </summary>
        public static readonly ParticleFieldDescription<float> RemainingLife = new ParticleFieldDescription<float>("RemainingLife", 1);

    }
}
