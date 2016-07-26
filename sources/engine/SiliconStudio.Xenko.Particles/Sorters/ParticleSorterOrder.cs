// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// Sorts the particles by ascending order of their Order attribute
    /// </summary>
    public class ParticleSorterOrder : ParticleSorterCustom<uint>
    {
        public ParticleSorterOrder(ParticlePool pool) : base(pool, ParticleFields.Order) { }

        public override IParticleSortedList GetSortedList(Vector3 depth)
        {
            var sortField = ParticlePool.GetField(fieldDesc);

            if (!sortField.IsValid())
                return new ParticleSortedListCustom<uint>(ParticlePool, ArrayPool);

            return new ParticleSortedListCustom<uint>(ParticlePool, ArrayPool, fieldDesc, new OrderCalculator());
        }

        struct OrderCalculator : ISortValueCalculator<uint>
        {
            public unsafe float GetSortValue(uint order) => *((float*)(&order)) * -1f;
        }
    }

}
