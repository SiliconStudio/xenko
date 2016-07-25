// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// Sorts the particles by ascending order of their Depth (position on the camera's Z axis)
    /// </summary>
    public class ParticleSorterDepth : ParticleSorterCustom<Vector3>
    {
        public ParticleSorterDepth(ParticlePool pool) : base(pool, ParticleFields.Position) { }

        public override IParticleSortedList GetSortedList(Vector3 depth)
        {
            var sortField = ParticlePool.GetField(fieldDesc);

            if (!sortField.IsValid())
                return new ParticleSortedListCustom<Vector3>(ParticlePool, ArrayPool);

            return new ParticleSortedListCustom<Vector3>(ParticlePool, ArrayPool, fieldDesc, new DepthCalculator(depth));
        }

        struct DepthCalculator : ISortValueCalculator<Vector3>
        {
            private readonly Vector3 depthVector;

            public DepthCalculator(Vector3 depth)
            {
                depthVector = depth;
            }

            public float GetSortValue(Vector3 position) => Vector3.Dot(depthVector, position);
        }
    }

}
