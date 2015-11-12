// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Particles
{
    public delegate void CopyParticlePoolDelegate(IntPtr oldPool, IntPtr newPool);

    public class ParticlePool : IDisposable, IEnumerable
    {
        public enum FieldsPolicy
        {
            /// <summary>
            /// The particle pool is initialized as an Array of Structs
            /// Each particle is a struct and they are organized as an array with length equal to the pool's capacity
            /// </summary>
            AoS,

            /// <summary>
            /// The particle pool is initialized as na Struct of Arrays
            /// Each particle field is a separate array with length equal to the pool's capacity
            /// The particle is not a contained struct anymore, but just an index to its data in the array.
            /// </summary>
            SoA
        }

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
            Stack,

            /// <summary>
            /// NOT IMPLEMENTED YET
            /// Same as stack but the order of the particles is kept the same.
            /// </summary>
            OrderedStack,

            /// <summary>
            /// NOT IMPLEMENTED YET
            /// Same as stack, but resized dynamically to fit more or less particles
            /// </summary>
            DynamicStack            
        }

        private bool disposed = false;
        private readonly FieldsPolicy fieldsPolicy;
        private readonly ListPolicy listPolicy;

        public IntPtr ParticleData { get; private set; } = IntPtr.Zero;

        public int ParticleCapacity { get; private set; } = 0;

        public int ParticleSize { get; private set; } = 0;

        /// <summary>
        /// For ring implementations, the index just increases, looping when it reaches max count.
        /// For stack implementations, the index points to the top of the stack and can reach 0 when there are no living particles.
        /// </summary>
        private int nextFreeIndex;

        public ParticlePool(int size, int capacity, FieldsPolicy fieldsPolicy = FieldsPolicy.AoS, ListPolicy listPolicy = ListPolicy.Ring)
        {
            if (listPolicy == ListPolicy.OrderedStack || listPolicy == ListPolicy.DynamicStack)
            {
                throw new NotImplementedException();
            }

            this.fieldsPolicy = fieldsPolicy;
            this.listPolicy = listPolicy;

            nextFreeIndex = 0;

            RelocatePool(size, capacity, (pool, newPool) => {});
        }

        public void RelocatePool(int newSize, int newCapacity, CopyParticlePoolDelegate poolCopy)
        {
            if (newCapacity == ParticleCapacity && newSize == ParticleSize)
                return;

            var newParticleData = IntPtr.Zero;

            var newMemoryBlockSize = newCapacity * newSize;

            if (newMemoryBlockSize > 0)
                newParticleData = Utilities.AllocateMemory(newMemoryBlockSize);

            if (ParticleData != IntPtr.Zero)
            {
                poolCopy(ParticleData, newParticleData);

                Utilities.FreeMemory(ParticleData);
            }

            ParticleData = newParticleData;
            ParticleCapacity = newCapacity;
            ParticleSize = newSize;
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
            // TODO Copy data - will depend on fields policy
            
        }

        private Particle FromIndex(int idx)
        {
            return new Particle(idx);
        }

        /// <summary>
        /// Add a new particle to the pool. Doesn't worry about initialization.
        /// </summary>
        /// <returns></returns>
        public Particle AddParticle()
        {
            if (nextFreeIndex == ParticleCapacity)
            {
                if (listPolicy == ListPolicy.Ring)
                {
                    nextFreeIndex = 0;
                }
                else if (listPolicy == ListPolicy.DynamicStack)
                {
                    throw new NotImplementedException();                  
                }
                else
                {
                    return Particle.Invalid();
                }
            }

            return FromIndex(nextFreeIndex++);
        }

        private void RemoveCurrent(ref Particle particle, ref int oldIndex, ref int indexMax)
        {
            if (listPolicy != ListPolicy.Ring)
            {
                // Next free index shouldn't be 0 because we are removing a particle
                Debug.Assert(nextFreeIndex > 0);

                // Update the top index since the list is shorter now
                indexMax = --nextFreeIndex;
                if (indexMax != oldIndex)
                    CopyParticleData(oldIndex, indexMax);

                particle = FromIndex(indexMax);        

                // Subract 1 from the old index to position the cursor of the enumerator to the previous particle
                oldIndex--;
            }
        }

        #region Fields
        private const int DefaultMaxFielsPerPool = 8;
        private readonly Dictionary<ParticleFieldDescription, ParticleField> fields = new Dictionary<ParticleFieldDescription, ParticleField>(DefaultMaxFielsPerPool);
        private readonly List<ParticleFieldDescription> fieldDescriptions = new List<ParticleFieldDescription>(DefaultMaxFielsPerPool);
         
        internal ParticleField AddField<T>(ParticleFieldDescription<T> fieldDesc) where T : struct
        {
            ParticleField existingField;
            if (fields.TryGetValue(fieldDesc, out existingField))
                return existingField;

            var newFieldSize = ParticleUtilities.AlignedSize(Utilities.SizeOf<T>(), 4);

            var newParticleSize = ParticleSize + newFieldSize;

            var newField = new ParticleField(newFieldSize, IntPtr.Zero, 0);
            fieldDescriptions.Add(fieldDesc);

            fields.Add(fieldDesc, newField);

            // TODO Proper particle relocation
            CopyParticlePoolDelegate emptyCopy = (pool, newPool) => { };
            RelocatePool(newParticleSize, ParticleCapacity, emptyCopy);

            if (fieldsPolicy == FieldsPolicy.SoA)
            {
                int fieldOffset = 0;
                foreach (var desc in fieldDescriptions)
                {
                    int fieldSize = fields[desc].FieldSize;
                    fields[desc] = new ParticleField(fieldSize, ParticleData + fieldOffset * ParticleCapacity, fieldSize);
                    fieldOffset += fieldSize;
                }
            }
            else
            {
                int fieldOffset = 0;
                foreach (var desc in fieldDescriptions)
                {
                    int fieldSize = fields[desc].FieldSize;
                    fields[desc] = new ParticleField(fieldSize, ParticleData + fieldOffset, ParticleSize);
                    fieldOffset += fieldSize;
                }
            }

            return fields[fieldDesc];
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

            return new ParticleFieldAccessor<T>(IntPtr.Zero, 0);
        }

        public bool TryGetField<T>(ParticleFieldDescription<T> fieldDesc, out ParticleFieldAccessor<T> accessor) where T : struct
        {
            ParticleField field;
            if (!fields.TryGetValue(fieldDesc, out field))
            {
                accessor = new ParticleFieldAccessor<T>(IntPtr.Zero, 0);
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
            return (IEnumerator)GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return (listPolicy == ListPolicy.Ring) ? 
                new Enumerator(this) :
                new Enumerator(this, 0, nextFreeIndex - 1);
        }

        public struct Enumerator : IEnumerator<Particle>
        {
            private int index;

            private readonly ParticlePool particlePool;
            private readonly int particleSize;
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
                particleSize = particlePool.ParticleSize;
                indexFrom = 0;
                indexTo = particlePool.ParticleCapacity - 1;
                index = indexFrom - 1;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="particlePool">Particle pool to iterate</param>
            /// <param name="idxFrom">First valid particle index</param>
            /// <param name="idxTo">Last valid particle index</param>
            public Enumerator(ParticlePool particlePool, int idxFrom, int idxTo)
            {
                this.particlePool = particlePool;
                particleSize = particlePool.ParticleSize;
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
            }
           
            /// <inheritdoc />
            public bool MoveNext()
            {
                ++index;
                var hasNext = (index <= indexTo && index >= indexFrom);
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

            /// <inheritdoc />
            object IEnumerator.Current => new Particle(index);

            /// <inheritdoc />
            public Particle Current => new Particle(index);
        }
        #endregion


    }
}
