using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Modules
{
    [DataContract("UpdaterBase")]
    public abstract class UpdaterBase : ParticleModuleBase
    {
//        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        public abstract void Update(float dt, ParticlePool pool);
        /*
        {
            // Example - nullify the position's Y coordinate
            if (!pool.FieldExists(ParticleFields.Position))
                return;

            var posField = pool.GetField(ParticleFields.Position);

            foreach (var particle in pool)
            {
                (*((Vector3*)particle[posField])).Y = 0;
            }
        }
        //*/
    }
}
