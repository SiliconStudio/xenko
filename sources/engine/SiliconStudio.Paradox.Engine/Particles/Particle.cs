// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// A particle in the particle system.
    /// </summary>
    public struct Particle
    {
        public readonly IntPtr Pointer;

        public Particle(IntPtr pointer)
        {
            this.Pointer = pointer;
        }

        /// <summary>
        /// Gets the specified field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor fieldAccessor) where T : struct
        {
            return Utilities.Read<T>(Pointer + fieldAccessor);
        }

        /// <summary>
        /// Sets the specified field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <param name="value">The field value.</param>
        public void Set<T>(ParticleFieldAccessor fieldAccessor, ref T value) where T : struct
        {
            Utilities.Write<T>(Pointer + fieldAccessor, ref value);
        }

        /// <summary>
        /// Sets the specified field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <param name="value">The field value.</param>
        public void Set<T>(ParticleFieldAccessor fieldAccessor, T value) where T : struct
        {
            Utilities.Write<T>(Pointer + fieldAccessor, ref value);
        }

        /// <summary>
        /// Gets the specified field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor<T> fieldAccessor) where T : struct
        {
            return Utilities.Read<T>(Pointer + fieldAccessor);
        }

        /// <summary>
        /// Sets the specified field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <param name="value">The field value.</param>
        public void Set<T>(ParticleFieldAccessor<T> fieldAccessor, ref T value) where T : struct
        {
            Utilities.Write<T>(Pointer + fieldAccessor, ref value);
        }

        /// <summary>
        /// Sets the specified field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <param name="value">The field value.</param>
        public void Set<T>(ParticleFieldAccessor<T> fieldAccessor, T value) where T : struct
        {
            Utilities.Write<T>(Pointer + fieldAccessor, ref value);
        }

        /// <summary>
        /// Gets the pointer to the specifield field.
        /// </summary>
        /// <value>
        /// The field pointer, as an <see cref="IntPtr" />.
        /// </value>
        /// <param name="fieldAccessor">The field accessor.</param>
        /// <returns>The field pointer.</returns>
        public IntPtr this[ParticleFieldAccessor fieldAccessor]
        {
            get { return Pointer + fieldAccessor; }
        }
    }
}