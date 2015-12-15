using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Particles.Sorters
{
    /// <summary>
    /// The default sorter doesn not sort the particles, but only passes them directly to the renderer
    /// </summary>
    public class ParticleSorterDefault : ParticleSorter
    {
        public ParticleSorterDefault(ParticlePool pool) : base(pool)
        {           
        }

        public override void Sort<T>(ParticleFieldDescription<T> fieldDesc, GetSortIndex<T> getIndex) 
        {
        }

        public override IEnumerator<Particle> GetEnumerator()
        {
            return ParticlePool.GetEnumerator();
        }
    }
}
