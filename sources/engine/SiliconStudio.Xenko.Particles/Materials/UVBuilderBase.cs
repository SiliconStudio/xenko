using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("UVBuilderBase")]
    public abstract class UVBuilderBase
    {
        public abstract void BuildUVCoordinates(ParticleVertexBuffer vtxBuilder, ParticleSorter sorter);
    }
}
