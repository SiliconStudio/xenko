// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// A particle system, containing particles and their updaters.
    /// </summary>
    public class ParticleSystem : IDisposable
    {
        private bool disposed = false;
        private const int particleDefaultCapacity = 256;
        private int particleCount;
        private int particleCapacity;
        private int particleSize;
        private IntPtr particleData;
        
        // We could also store it at beginning of particleData?
        private IntPtr particleDefaultValue;

        /// <value>
        /// The particle fields.
        /// </value>
        private Dictionary<ParticleFieldDescription, ParticleField> fields = new Dictionary<ParticleFieldDescription, ParticleField>(8);


        /// <summary>
        /// Gets the particle system plugins.
        /// </summary>
        /// <value>
        /// The particle system plugins.
        /// </value>
        public TrackingCollection<IParticlePlugin> Plugins { get; private set; }

        /// <summary>
        /// Gets the position field accessor.
        /// </summary>
        /// <value>
        /// The position field accessor.
        /// </value>
        public ParticleFieldAccessor<Vector3> Position { get; private set; }

        /// <summary>
        /// Gets the angle field accessor.
        /// </summary>
        /// <value>
        /// The angle field accessor.
        /// </value>
        public ParticleFieldAccessor<float> Angle { get; private set; }

        /// <summary>
        /// Gets the particle count.
        /// </summary>
        /// <value>
        /// The particle count.
        /// </value>
        public int ParticleCount
        {
            get { return particleCount; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystem" /> class.
        /// </summary>
        public ParticleSystem()
        {
            // Initialize collections
            Plugins = new TrackingCollection<IParticlePlugin>();

            // Add default position field
            Position = new ParticleFieldAccessor<Vector3>(AddField(ParticleFields.Position));
            Angle = new ParticleFieldAccessor<float>(AddField(ParticleFields.Angle));

            EnsureCapacity(particleDefaultCapacity);

            Plugins.CollectionChanged += Plugins_CollectionChanged;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ParticleSystem" /> class.
        /// </summary>
        ~ParticleSystem()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (particleData != IntPtr.Zero)
                {
                    Utilities.FreeMemory(particleData);
                    particleData = IntPtr.Zero;
                }
                if (particleDefaultValue != IntPtr.Zero)
                {
                    Utilities.FreeMemory(particleDefaultValue);
                    particleDefaultValue = IntPtr.Zero;
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Gets the <see cref="Particle"/> enumerator.
        /// </summary>
        /// <returns>A <see cref="Particle"/> enumerator.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private void Plugins_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var particlePluginListener = e.Item as IParticlePluginListener;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (particlePluginListener != null)
                        particlePluginListener.OnAddPlugin(this);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (particlePluginListener != null)
                        particlePluginListener.OnRemovePlugin(this);
                    break;
            }
        }

        /// <summary>
        /// Gets the field accessor specified by the given <see cref="ParticleFieldDescription{T}"/>.
        /// If the field doesn't exist in this <see cref="ParticleSystem"/>,
        /// a <see cref="ParticleFieldAccessor{T}"/> is returned with its <see cref="ParticleFieldAccessor{T}.IsValid()"/> returning false.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldDesc">The field description.</param>
        /// <returns>A valid field accessor for the requested field if the field exists; otherwise a non-valid one.</returns>
        public ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            ParticleField field;
            if (!fields.TryGetValue(fieldDesc, out field))
            {
                return new ParticleFieldAccessor<T>(-1);
            }
            return new ParticleFieldAccessor<T>(field);
        }

        /// <summary>
        /// Gets the field accessor for the given <see cref="ParticleFieldDescription{T}"/>.
        /// If it doesn't exist, a new field will be created in the <see cref="ParticleSystem"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldDesc">The field description.</param>
        /// <returns>A valid field accessor for the requested field.</returns>
        public ParticleFieldAccessor<T> GetOrCreateField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            ParticleField field;
            if (!fields.TryGetValue(fieldDesc, out field))
            {
                field = AddField(fieldDesc);
            }
            return new ParticleFieldAccessor<T>(field);
        }

        /// <summary>
        /// Gets the field accessor for the given <see cref="ParticleFieldDescription{T}"/>.
        /// If it doesn't exist, a new field will be created in the <see cref="ParticleSystem"/>.
        /// Weither the field exists or not, its default value will be changed to the supplied one.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldDesc">The field description.</param>
        /// <param name="defaultValue">The new field default value.</param>
        /// <returns>A valid field accessor for the requested field.</returns>
        public ParticleFieldAccessor<T> GetOrCreateFieldWithDefault<T>(ParticleFieldDescription<T> fieldDesc, T defaultValue) where T : struct
        {
            ParticleField field;
            if (!fields.TryGetValue(fieldDesc, out field))
            {
                field = AddField(fieldDesc);
            }
            SetDefaultValue(field, defaultValue);
            return new ParticleFieldAccessor<T>(field);
        }

        /// <summary>
        /// Sets the default value for the specified field.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="particleField">The field.</param>
        /// <param name="defaultValue">The new field default value.</param>
        internal void SetDefaultValue<T>(ParticleField particleField, T defaultValue) where T : struct
        {
            if (particleDefaultValue == IntPtr.Zero)
                throw new InvalidOperationException();

            // Write new default value for the field
            Utilities.Write(particleDefaultValue + particleField.Offset, ref defaultValue);
        }

        /// <summary>
        /// Adds a new field to the particle system.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="fieldDesc">The field description.</param>
        /// <returns>The field.</returns>
        /// <exception cref="System.ArgumentException">Particle field size must be a multiple of 4;size</exception>
        internal ParticleField AddField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            ParticleField existingField;
            if (fields.TryGetValue(fieldDesc, out existingField))
                return existingField;

            var size = Utilities.SizeOf<T>();

            // Only accept 4-byte multiples.
            if (size % 4 != 0)
                throw new ArgumentException("Particle field size must be a multiple of 4", "size");

            var newParticleSize = particleSize + size;

            // Update default value
            var newParticleDefaultValue = Utilities.AllocateMemory(newParticleSize);
            if (particleDefaultValue != IntPtr.Zero)
            {
                Utilities.CopyMemory(newParticleDefaultValue, particleDefaultValue, particleSize);
                Utilities.FreeMemory(particleDefaultValue);
            }

            particleDefaultValue = newParticleDefaultValue;

            // Write default value for new field
            var defaultValue = fieldDesc.DefaultValue;
            Utilities.Write(newParticleDefaultValue + particleSize, ref defaultValue);


            // Check if there is enough space to do the conversion in-place
            if (particleData != IntPtr.Zero && particleCapacity * particleSize < newParticleSize * particleCount)
            {
                // Copy in-place, particles from back to front (so that there is no memory overlap)
                // i.e.
                // AAABBBCCCDDD gets copied that way:
                // --------------D-
                // -------------DD-
                // ------------DDD-
                // ----------C-DDD-
                // ................
                // AAA-BBB-CCC-DDD-
                var particleSizeInDword = particleSize / 4;
                unsafe
                {
                    var particleEnd = (int*)(particleData + particleCount * particleSize);
                    var newParticleEnd = (int*)(particleData + particleCount * newParticleSize - size);
                    for (int i = particleCount - 1; i >= 0; --i)
                    {
                        // Copy one particle, from back to end (no overlap)
                        for (int j = 0; j < particleSizeInDword; ++j)
                        {
                            *--newParticleEnd = *--particleEnd;
                        }
                        // Leave space for the newly added field.
                        newParticleEnd -= size;

                        // Write default value for new field
                        Utilities.Write((IntPtr)newParticleEnd, ref defaultValue);
                    }
                }

                // Update capacity
                particleCapacity = particleCapacity * particleSize / newParticleSize;
            }
            else if (particleCapacity > 0)
            {
                var newParticleData = Utilities.AllocateMemory(particleCapacity * newParticleSize);

                // Copy previous data to the new buffer, interleaved with space for the new field
                if (particleData != IntPtr.Zero)
                {
                    var newParticleDataPtr = newParticleData;
                    var particleDataPtr = particleData;
                    for (int i = 0; i < particleCount; ++i)
                    {
                        // Copy particle values
                        Utilities.CopyMemory(newParticleDataPtr, particleDataPtr, particleSize);
                        // Write default value for new field
                        Utilities.Write(newParticleDataPtr + particleSize, ref defaultValue);
                        particleDataPtr += particleSize;
                        newParticleDataPtr += newParticleSize;
                    }
                    Utilities.FreeMemory(particleData);
                }

                particleData = newParticleData;
            }

            // Add the field
            var field = new ParticleField { Offset = particleSize, Size = size };
            fields.Add(fieldDesc, field);
            
            // Update particle size
            particleSize = newParticleSize;
            return field;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update(float dt)
        {
            foreach (var plugin in Plugins)
            {
                plugin.Update(this, dt);
            }
        }

        /// <summary>
        /// Adds the particle.
        /// </summary>
        /// <returns>The index of the newly added particle.</returns>
        public Particle AddParticle()
        {
            // Ensure there is enough space
            if (ParticleCount == particleCapacity)
                EnsureCapacity(ParticleCount + 1);

            // New particle will be stored at the end
            var particle = new Particle(particleData + particleSize * particleCount);

            // Initialize particle to default values
            Utilities.CopyMemory(particle.Pointer, particleDefaultValue, particleSize);

            particleCount++;
            return particle;
        }

        /// <summary>
        /// Removes the particle at the specified index.
        /// </summary>
        /// <param name="particleIndex">Index of the particle.</param>
        public void RemoveParticleAt(int particleIndex)
        {
            // If not removing last one, copy last one over removed one so that it can be done in O(1).
            var lastParticleIndex = ParticleCount - 1;
            if (particleIndex != lastParticleIndex)
            {
                Utilities.CopyMemory(particleData + particleIndex * particleSize, particleData + lastParticleIndex * particleSize, particleSize);
            }

            particleCount = ParticleCount - 1;
        }

        private void EnsureCapacity(int min)
        {
            if (ParticleCount < min)
            {
                int newCapacity = (particleCapacity == 0) ? particleDefaultCapacity : (particleCapacity * 2);
                if (newCapacity < min)
                    newCapacity = min;

                if (newCapacity != particleCapacity)
                {
                    // Allocate new buffer
                    var newParticleData = Utilities.AllocateMemory(newCapacity * particleSize);

                    if (particleData != IntPtr.Zero)
                    {
                        // Copy only real particles
                        Utilities.CopyMemory(newParticleData, particleData, particleSize * particleCount);

                        // Free previous buffer
                        Utilities.FreeMemory(particleData);
                    }

                    // Update with new array
                    particleData = newParticleData;
                    particleCapacity = newCapacity;
                }
            }
        }

        /// <summary>
        /// A <see cref="Particle"/> enumerator.
        /// </summary>
        public struct Enumerator : IEnumerator<Particle>
        {
            private ParticleSystem particleSystem;
            private int index;
            private int particleSize;
            private IntPtr particleData;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator" /> struct.
            /// </summary>
            /// <param name="particleSystem">The particle system.</param>
            public Enumerator(ParticleSystem particleSystem)
            {
                this.particleSystem = particleSystem;
                this.particleSize = particleSystem.particleSize;
                this.index = -1;
                this.particleData = IntPtr.Zero;
            }

            /// <summary>
            /// Gets the index of the current particle.
            /// </summary>
            /// <value>
            /// The index of the current particle.
            /// </value>
            public int Index
            {
                get { return index; }
                set
                {
                    // Updates index
                    index = value;

                    // Updates particle data pointer.
                    particleData = particleSystem.particleData + particleSize * index;
                }
            }

            /// <summary>
            /// Removes the current particle from the particle system.
            /// The iterator will be placed at the previous particle, so that
            /// next iteration with MoveNext() will point to the right particle.
            /// </summary>
            public void RemoveParticle()
            {
                particleSystem.RemoveParticleAt(Index--);
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                // Move to next (and update pointer)
                // Returns true if success (not out of bound)
                return ++Index < particleSystem.ParticleCount;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <inheritdoc/>
            public void Reset()
            {
                index = -1;
                particleData = IntPtr.Zero;
            }

            /// <inheritdoc/>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <inheritdoc/>
            public Particle Current
            {
                get { return new Particle(particleData); }
            }
        }
    }
}