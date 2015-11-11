// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    public struct  Particle
    {
        public readonly IntPtr Pointer;

        public Particle(IntPtr pointer)
        {
            Pointer = pointer;
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
              return Utilities.Read<T>(Pointer + accessor);
        }

        /// <summary>
        /// Gets the particle's field value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <returns>The field value.</returns>
        public T Get<T>(ParticleFieldAccessor<T> accessor) where T : struct
        {
        //    return ParticleUtilities.ToStruct<T>(Pointer + accessor);
            return Utilities.Read<T>(Pointer + accessor);
        }

        /// <summary>
        /// Sets the particle's field to a value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <param name="value">The value to set</param>
        public void Set<T>(ParticleFieldAccessor<T> accessor, ref T value) where T : struct
        {
        //    ParticleUtilities.ToPtr(value, Pointer + accessor);
            Utilities.Write(Pointer + accessor, ref value);
        }

        /// <summary>
        /// Sets the particle's field to a value.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="accessor">The field accessor</param>
        /// <param name="value">The value to set</param>
        public void Set<T>(ParticleFieldAccessor<T> accessor, T value) where T : struct
        {
        //    ParticleUtilities.ToPtr(value, Pointer + accessor);
            Utilities.Write(Pointer + accessor, ref value);
        }

        #endregion

        public IntPtr this[ParticleFieldAccessor accessor]
        {
            get { return Pointer + accessor; }
        }

        public static implicit operator IntPtr(Particle particle) => particle.Pointer;
    }
}
