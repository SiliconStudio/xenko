// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// Sorts the particles by descending order of their remaining Life
    /// </summary>
    public class ParticleSorterAge : ParticleSorterCustom<float>, IParticleSorter
    {
        public ParticleSorterAge(ParticlePool pool) : base(pool, ParticleFields.Life) { }

        IParticleSortedList IParticleSorter.GetSortedList(Vector3 depth) => GetSortedList(depth);

        public ParticleSortedListCustom<float> GetSortedList(Vector3 depth)
        {
            var sortField = ParticlePool.GetField(fieldDesc);

            if (!sortField.IsValid())
                return new ParticleSortedListCustom<float>(ParticlePool, ArrayPool);

            return new ParticleSortedListCustom<float>(ParticlePool, ArrayPool, fieldDesc, new AgeCalculator());
        }

        struct AgeCalculator : ISortValueCalculator<float>
        {
            public float GetSortValue(float life) => -life;
        }
    }
}
