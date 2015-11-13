// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    public struct  Particle
    {
#if PARTICLES_SOA
        public readonly int Index;

        public Particle(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Creates an invalid <see cref="Particle"/>. Accessing the invalid <see cref="Particle"/> is not resticted by the engine, so the user has to restrict it.
        /// </summary>
        /// <returns></returns>
        static public Particle Invalid()
        {
            return new Particle(int.MaxValue);
        }
#else
        public readonly IntPtr Pointer;

        public Particle(IntPtr pointer)
        {
            Pointer = pointer;
        }

        /// <summary>
        /// Creates an invalid <see cref="Particle"/>. Accessing the invalid <see cref="Particle"/> is not resticted by the engine, so the user has to restrict it.
        /// </summary>
        /// <returns></returns>
        static public Particle Invalid()
        {
            return new Particle(IntPtr.Zero);
        }
#endif

        #region Accessors


        /// <summary>
        /// Gets the particle's field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor accessor) where T : struct
        {
#if PARTICLES_SOA
            return Utilities.Read<T>(accessor[Index]);
#else
            return Utilities.Read<T>(Pointer + accessor);
#endif
        }

        /// <summary>
        /// Gets the particle's field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor<T> accessor) where T : struct
        {
#if PARTICLES_SOA
            return Utilities.Read<T>(accessor[Index]);
#else
            return Utilities.Read<T>(Pointer + accessor);
#endif
        }

        /// <summary>
        /// Sets the particle's field to a value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <param name="value">The value to set</param>
        public void Set<T>(ParticleFieldAccessor<T> accessor, ref T value) where T : struct
        {
#if PARTICLES_SOA
            Utilities.Write(accessor[Index], ref value);
#else
            Utilities.Write(Pointer + accessor, ref value);
#endif
        }

        /// <summary>
        /// Sets the particle's field to a value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <param name="value">The value to set</param>
        public void Set<T>(ParticleFieldAccessor<T> accessor, T value) where T : struct
        {
#if PARTICLES_SOA
            Utilities.Write(accessor[Index], ref value);
#else
            Utilities.Write(Pointer + accessor, ref value);
#endif
        }

        #endregion

#if PARTICLES_SOA
        public IntPtr this[ParticleFieldAccessor accessor] => accessor[Index];

        public static implicit operator int(Particle particle) => particle.Index;
#else
        public static implicit operator IntPtr(Particle particle) => particle.Pointer;

        public IntPtr this[ParticleFieldAccessor accessor] => Pointer + accessor;
#endif

#if PARTICLES_SOA
        /// <summary>
        /// Since particles are only indices, the comparison is only meaningful if it's done within the same particle pool
        /// </summary>
        /// <param name="particleLeft">Left side particle to compare</param>
        /// <param name="particleRight">Right side particle to compare</param>
        /// <returns></returns>
        public static bool operator ==(Particle particleLeft, Particle particleRight) => (particleLeft.Index == particleRight.Index);
        public static bool operator !=(Particle particleLeft, Particle particleRight) => (particleLeft.Index != particleRight.Index);
#else
        /// <summary>
        /// Checks if the two particles point to the same pointer.
        /// </summary>
        /// <param name="particleLeft">Left side particle to compare</param>
        /// <param name="particleRight">Right side particle to compare</param>
        /// <returns></returns>
        public static bool operator ==(Particle particleLeft, Particle particleRight) => (particleLeft.Pointer == particleRight.Pointer);
        public static bool operator !=(Particle particleLeft, Particle particleRight) => (particleLeft.Pointer != particleRight.Pointer);

#endif
    }
}
