// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    public struct  Particle
    {
        public readonly int Index;

        /// <summary>
        /// Creates a new <see cref="Particle"/>
        /// </summary>
        /// <param name="index"></param>
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
        #region Accessors


        /// <summary>
        /// Gets the particle's field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor accessor) where T : struct
        {
        //    return ParticleUtilities.ToStruct<T>(Pointer + accessor);
              return Utilities.Read<T>(accessor[Index]);
        }

        /// <summary>
        /// Gets the particle's field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor<T> accessor) where T : struct
        {
            return Utilities.Read<T>(accessor[Index]);
        }

        /// <summary>
        /// Sets the particle's field to a value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <param name="value">The value to set</param>
        public void Set<T>(ParticleFieldAccessor<T> accessor, ref T value) where T : struct
        {
            Utilities.Write(accessor[Index], ref value);
        }

        /// <summary>
        /// Sets the particle's field to a value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <param name="value">The value to set</param>
        public void Set<T>(ParticleFieldAccessor<T> accessor, T value) where T : struct
        {
            Utilities.Write(accessor[Index], ref value);
        }

        #endregion

        public IntPtr this[ParticleFieldAccessor accessor] => accessor[Index];

        /// <summary>
        /// Casts the index of the <see cref="Particle"/> as int
        /// </summary>
        /// <param name="particle"></param>
        public static implicit operator int(Particle particle) => particle.Index;

        /// <summary>
        /// Since particles are only indices, the comparison is only meaningful if it's done within the same particle pool
        /// </summary>
        /// <param name="particleLeft">Left side particle to compare</param>
        /// <param name="particleRight">Right side particle to compare</param>
        /// <returns></returns>
        public static bool operator ==(Particle particleLeft, Particle particleRight) => (particleLeft.Index == particleRight.Index);
        public static bool operator !=(Particle particleLeft, Particle particleRight) => (particleLeft.Index != particleRight.Index);
    }
}
