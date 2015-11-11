// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    public delegate void CopyParticlePoolDelegate(IntPtr oldPool, IntPtr newPool);

    public class ParticlePool : IDisposable, IEnumerable
    {
        private bool disposed = false;

        public IntPtr ParticleData { get; private set; } = IntPtr.Zero;

        public int ParticleCapacity { get; private set; } = 0;

        public int ParticleSize { get; private set; } = 0;

        public ParticlePool(int size, int capacity)
        {
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            // TODO Differennt impl for Ring and List
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<Particle>
        {
            private IntPtr particlePtr;
            private int index;

            private readonly ParticlePool particlePool;
            private readonly int particleSize;
            private readonly int indexFrom;
            private readonly int indexTo;

            /// <summary>
            /// Creates an enumarator which iterates over all particles (living and dead) in the particle pool.
            /// </summary>
            /// <param name="particlePool">Particle pool to iterate</param>
            public Enumerator(ParticlePool particlePool)
            {
                this.particlePool = particlePool;
                particleSize = particlePool.ParticleSize;
                particlePtr = IntPtr.Zero;
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
                particlePtr = IntPtr.Zero;
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
                particlePtr = IntPtr.Zero;
            }
           
            /// <inheritdoc />
            public bool MoveNext()
            {
                ++index;
                var hasNext = (index <= indexTo && index >= indexFrom);
                particlePtr = (hasNext) ? particlePool.ParticleData + index * particleSize : IntPtr.Zero;
                return hasNext;
            }

            /// <inheritdoc />
            object IEnumerator.Current
            {
                get { return new Particle(particlePtr); }
            } 

            /// <inheritdoc />
            public Particle Current
            {
                get { return new Particle(particlePtr);}
            }
        }
    }
}
