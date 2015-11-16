using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Particles.Modules
{
    public abstract class ParticleModule
    {         
        /// <summary>
        /// Updates the module instance in case it has properties which change with time
        /// </summary>
        /// <param name="dt">Delta time, elapsed time since the last call, in seconds</param>
        /// <param name="parentSystem">The parent <see cref="ParticleSystem"/> hosting this module</param>
        public void Update(float dt, ParticleSystem parentSystem)
        {
            
        }

        public abstract void Apply(float dt, ParticlePool pool);
    }
}
