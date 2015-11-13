// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    public delegate void CopyParticlePoolDelegate(IntPtr oldPool, int oldCapacity, int oldSize, IntPtr newPool, int newCapacity, int newSize);

    public class ParticlePool : IDisposable, IEnumerable
    {
        public enum ListPolicy
        {
            /// <summary>
            /// New particles are allocated from the next free index which loops when it reaches the end of the list.
            /// The pool doesn't care about dead particles - they don't move and get overwritten by new particles.
            /// </summary>
            Ring,

            /// <summary>
            /// New particles are allocated at the top of the stack. Dead particles are swapped out with the top particle.
            /// The stack stays small but order of the particles gets scrambled.
            /// </summary>
            Stack

            // OrderedStack,
            // DynamicStack            
        }

        private bool disposed;
        private readonly ListPolicy listPolicy;

        public IntPtr ParticleData { get; private set; } = IntPtr.Zero;

        public int ParticleCapacity { get; private set; }

        public void SetCapacity(int newCapacity)
        {
            if (newCapacity < 0)
                return;

            RelocatePool(ParticleSize, newCapacity, CapacityChangedRelocate);
        }

        public int ParticleSize { get; private set; }

        /// <summary>
        /// For ring implementations, the index just increases, looping when it reaches max count.
        /// For stack implementations, the index points to the top of the stack and can reach 0 when there are no living particles.
        /// </summary>
        private int nextFreeIndex;

        public ParticlePool(int size, int capacity, ListPolicy listPolicy = ListPolicy.Ring)
        {
            this.listPolicy = listPolicy;

            nextFreeIndex = 0;

            RelocatePool(size, capacity, (pool, oldCapacity, oldSize, newPool, newCapacity, newSize) => { });
        }

        private void RelocatePool(int newSize, int newCapacity, CopyParticlePoolDelegate poolCopy)
        {
            if (newCapacity == ParticleCapacity && newSize == ParticleSize)
                return;

            var newParticleData = IntPtr.Zero;

            var newMemoryBlockSize = newCapacity * newSize;

            if (newMemoryBlockSize > 0)
                newParticleData = Utilities.AllocateMemory(newMemoryBlockSize);

            if (ParticleData != IntPtr.Zero)
            {
                poolCopy(ParticleData, ParticleCapacity, ParticleSize, newParticleData, newCapacity, newSize);

                Utilities.FreeMemory(ParticleData);
            }

            ParticleData = newParticleData;
            ParticleCapacity = newCapacity;
            ParticleSize = newSize;
        }

        public void Reset()
        {
            fields.Clear();
#if PARTICLES_SOA
            fieldDescriptions.Clear();
#endif
            RelocatePool(0, ParticleCapacity, (pool, oldCapacity, oldSize, newPool, newCapacity, newSize) => { });
        }

        ~ParticlePool()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void DisposeParticleData()
        {
            if (ParticleData == IntPtr.Zero)
                return;

            Utilities.FreeMemory(ParticleData);
            ParticleData = IntPtr.Zero;
            ParticleCapacity = 0;
        }

        protected virtual void Dispose(bool managed)
        {
            if (disposed)
                return;

            DisposeParticleData();

            disposed = true;
        }

        private void CopyParticleData(int dst, int src)
        {
            var dstParticle = FromIndex(dst);
            var srcParticle = FromIndex(src);

#if PARTICLES_SOA
            foreach (var field in fields.Values)
            {
                var accessor = new ParticleFieldAccessor(field);
                Utilities.CopyMemory(dstParticle[accessor], srcParticle[accessor], field.Stride);
            }
#else
            Utilities.CopyMemory(dstParticle, srcParticle, ParticleSize);
#endif
        }

#if PARTICLES_SOA
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private Particle FromIndex(int idx)
        {
            return new Particle(idx);
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Particle FromIndex(int idx)
        {
            return new Particle(ParticleData + idx * ParticleSize);
        }
#endif

        /// <summary>
        /// Add a new particle to the pool. Doesn't worry about initialization.
        /// </summary>
        /// <returns></returns>
        public Particle AddParticle()
        {
            if (nextFreeIndex != ParticleCapacity)
                return FromIndex(nextFreeIndex++);

            if (listPolicy != ListPolicy.Ring)
                return Particle.Invalid();

            nextFreeIndex = 0;
            return FromIndex(nextFreeIndex++);
        }

        private void RemoveCurrent(ref Particle particle, ref int oldIndex, ref int indexMax)
        {
            // In case of a Ring list we don't bother to remove dead particles
            if (listPolicy == ListPolicy.Ring)
                return;
            
            // Next free index shouldn't be 0 because we are removing a particle
            Debug.Assert(nextFreeIndex > 0);

            // Update the top index since the list is shorter now
            indexMax = --nextFreeIndex;
            if (indexMax != oldIndex)
                CopyParticleData(oldIndex, indexMax);

            particle = FromIndex(indexMax);        

            // We need to position the cursor of the enumerator to the previous particle, so that enumeration works fine
            oldIndex--;            
        }

#region Fields
        private const int DefaultMaxFielsPerPool = 8;
        private readonly Dictionary<ParticleFieldDescription, ParticleField> fields = new Dictionary<ParticleFieldDescription, ParticleField>(DefaultMaxFielsPerPool);
#if PARTICLES_SOA
        private readonly List<ParticleFieldDescription> fieldDescriptions = new List<ParticleFieldDescription>(DefaultMaxFielsPerPool);
#endif
        internal ParticleField AddField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            ParticleField existingField;
            if (fields.TryGetValue(fieldDesc, out existingField))
                return existingField;

            var newFieldSize = ParticleUtilities.AlignedSize(Utilities.SizeOf<T>(), 4);

            var newParticleSize = ParticleSize + newFieldSize;

#if PARTICLES_SOA
            var newField = new ParticleField(newFieldSize, IntPtr.Zero);
            fieldDescriptions.Add(fieldDesc);
#else
            var newField = new ParticleField() { Offset = ParticleSize, Size = newFieldSize };
#endif
            fields.Add(fieldDesc, newField);

            RelocatePool(newParticleSize, ParticleCapacity, FieldAddedRelocate);

#if PARTICLES_SOA
            {
                var fieldOffset = 0;
                foreach (var desc in fieldDescriptions)
                {
                    var fieldSize = fields[desc].Stride;
                    fields[desc] = new ParticleField(fieldSize, ParticleData + fieldOffset * ParticleCapacity);
                    fieldOffset += fieldSize;
                }
            }
#endif
            return fields[fieldDesc];
        }

        private void FieldAddedRelocate(IntPtr oldPool, int oldCapacity, int oldSize, IntPtr newPool, int newCapacity, int newSize)
        {
            // Old particle capacity and new particle capacity should be the same when only the size changes.
            // If this is not the case, something went wrong. Reset the particle count and do not copy.
            // Also, since we are adding a field, the new particle size is expected to get bigger.
            if (oldCapacity != newCapacity || newCapacity <= 0 || newSize <= 0 || oldPool == IntPtr.Zero || newPool == IntPtr.Zero || oldSize >= newSize)
            {
                nextFreeIndex = 0;
                return;
            }

#if PARTICLES_SOA
            // Easy case - the new field is added to the end. Copy the existing memory block into the new one
            Utilities.CopyMemory(newPool, oldPool, oldSize * oldCapacity);
            Utilities.ClearMemory(newPool + oldSize * oldCapacity, 0, (newSize - oldSize) * oldCapacity);
#else
            // Clear the memory first instead of once per particle
            Utilities.ClearMemory(newPool, 0, newSize * newCapacity);

            // Complex case - needs to copy the head of each particle
            for (var i = 0; i < oldCapacity; i++)
            {
                Utilities.CopyMemory(newPool + i * newSize, oldPool + i * oldSize, oldSize);
            }
#endif
        }

        private void CapacityChangedRelocate(IntPtr oldPool, int oldCapacity, int oldSize, IntPtr newPool, int newCapacity, int newSize)
        {
            // Old particle size and new particle size should be the same when only the capacity changes.
            // If this is not the case, something went wrong. Reset the particle count and do not copy.
            if (oldSize != newSize || newCapacity <= 0 || newSize <= 0 || oldPool == IntPtr.Zero || newPool == IntPtr.Zero)
            {
                nextFreeIndex = 0;
                return;
            }

            if (nextFreeIndex > newCapacity)
                nextFreeIndex = newCapacity;

#if PARTICLES_SOA
            // Clear the memory first instead of once per particle
            Utilities.ClearMemory(newPool, 0, newSize * newCapacity);

            var oldOffset = 0;
            var newOffset = 0;

            foreach (var field in fields.Values)
            {                
                var copySize = Math.Min(oldCapacity, newCapacity) * field.Stride;
                Utilities.CopyMemory(newPool + newOffset, oldPool + oldOffset, field.Stride * copySize);

                oldOffset += (field.Stride * oldCapacity);
                newOffset += (field.Stride * newCapacity);
            }
#else
            if (newCapacity > oldCapacity)
            {
                Utilities.CopyMemory(newPool, oldPool, newSize * oldCapacity);
                Utilities.ClearMemory(newPool + newSize * oldCapacity, 0, newSize * (newCapacity - oldCapacity));
            }
            else
            {
                Utilities.CopyMemory(newPool, oldPool, newSize * newCapacity);
            }
#endif
        }

        // TODO internal RemoveField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        //  - will have to be handled by the emitter to ensure a field still in use is not removed

        /// <summary>
        /// Unsafe method for getting a <see cref="ParticleFieldAccessor"/>.
        /// If the field doesn't exist an invalid accessor is returned to the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldDesc"></param>
        /// <returns></returns>
        public ParticleFieldAccessor<T> GetField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            ParticleField field;
            if (fields.TryGetValue(fieldDesc, out field))
                return new ParticleFieldAccessor<T>(field);

            return ParticleFieldAccessor<T>.Invalid();
        }

        public bool TryGetField<T>(ParticleFieldDescription<T> fieldDesc, out ParticleFieldAccessor<T> accessor) where T : struct
        {
            ParticleField field;
            if (!fields.TryGetValue(fieldDesc, out field))
            {
                accessor = ParticleFieldAccessor<T>.Invalid();
                return false;
            }

            accessor = new ParticleFieldAccessor<T>(field);
            return true;
        }

        public bool FieldExists<T>(ParticleFieldDescription<T> fieldDesc, bool forceCreate = false) where T : struct
        {
            ParticleField field;
            if (fields.TryGetValue(fieldDesc, out field))
                return true;

            if (!forceCreate)
                return false;

            if (fields.Count >= DefaultMaxFielsPerPool)
                return false;

            AddField(fieldDesc);
            return true;
        }
#endregion

#region Enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> to the particles in this <see cref="ParticlePool"/>
        /// In case of <see cref="ListPolicy.Ring"/> dead particles are returned too, so the calling entity should handle such cases.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return (listPolicy == ListPolicy.Ring) ? 
                new Enumerator(this) :
                new Enumerator(this, 0, nextFreeIndex - 1);
        }

        public struct Enumerator : IEnumerator<Particle>
        {
#if PARTICLES_SOA
#else
            private IntPtr particlePtr;
            private readonly int particleSize;
#endif
            private int index;

            private readonly ParticlePool particlePool;
            private readonly int indexFrom;

            // indexTo can change if particles are removed during iteration
            private int indexTo;

            /// <summary>
            /// Creates an enumarator which iterates over all particles (living and dead) in the particle pool.
            /// </summary>
            /// <param name="particlePool">Particle pool to iterate</param>
            public Enumerator(ParticlePool particlePool)
            {
                this.particlePool = particlePool;
#if PARTICLES_SOA
#else
                particleSize = particlePool.ParticleSize;
                particlePtr = IntPtr.Zero;
#endif
                indexFrom = 0;
                indexTo = particlePool.ParticleCapacity - 1;
                index = indexFrom - 1;
            }

            /// <summary>
            /// <see cref="Enumerator"/> to the particles in this <see cref="ParticlePool"/>
            /// </summary>
            /// <param name="particlePool">Particle pool to iterate</param>
            /// <param name="idxFrom">First valid particle index</param>
            /// <param name="idxTo">Last valid particle index</param>
            public Enumerator(ParticlePool particlePool, int idxFrom, int idxTo)
            {
                this.particlePool = particlePool;
#if PARTICLES_SOA
#else
                particleSize = particlePool.ParticleSize;
                particlePtr = IntPtr.Zero;
#endif
                indexFrom = Math.Max(0, Math.Min(idxFrom, idxTo));
                indexTo = Math.Min(particlePool.ParticleCapacity - 1, Math.Max(idxFrom, idxTo));
                index = indexFrom - 1;
            }

            /// <inheritdoc />
            public void Dispose()
            {                
            }

            /// <inheritdoc />
            public void Reset()
            {
                index = indexFrom - 1;
#if PARTICLES_SOA
#else
                particlePtr = IntPtr.Zero;
#endif
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                ++index;
                var hasNext = (index <= indexTo && index >= indexFrom);
#if PARTICLES_SOA
#else
                particlePtr = (hasNext) ? particlePool.ParticleData + index * particleSize : IntPtr.Zero;
#endif
                return hasNext;
            }

            /// <summary>
            /// Removes the current particle. A reference to the particle is required so that the addressing can be updated and
            /// prevent illegal access.
            /// </summary>
            /// <param name="particle"></param>
            public void RemoveCurrent(ref Particle particle)
            {
                // Cannot remove particle which is not current
                if (particle != Current)
                    return;

                particlePool.RemoveCurrent(ref particle, ref index, ref indexTo);
            }

#if PARTICLES_SOA
            /// <inheritdoc />
            object IEnumerator.Current => new Particle(index);

            /// <inheritdoc />
            public Particle Current => new Particle(index);
#else
            /// <inheritdoc />
            object IEnumerator.Current => new Particle(particlePtr);

            /// <inheritdoc />
            public Particle Current => new Particle(particlePtr);

#endif

        }
#endregion


    }
}
