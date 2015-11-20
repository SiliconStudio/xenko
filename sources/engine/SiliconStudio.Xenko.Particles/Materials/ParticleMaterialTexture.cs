using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialTexture")]
    [Display("StaticTexture")]
    public class ParticleMaterialTexture : ParticleMaterialBase
    {
        [DataMember(100)]
        [Display("Texture")]
        public Texture Texture;


    }
}
