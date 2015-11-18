using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("PArticleModuleBase")]
    public abstract class ParticleModuleBase
    {
        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);
    }
}
