using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public static class VertexAttributes
    {
        public static AttributeDescription Position = new AttributeDescription("POSITION");

//        public static AttributeDescription TexCoord = new AttributeDescription("TEXCOORD");

        public static AttributeDescription Color = new AttributeDescription("COLOR");

        public static AttributeDescription Lifetime = new AttributeDescription("BATCH_LIFETIME");

        public static AttributeDescription RandomSeed = new AttributeDescription("BATCH_RANDOMSEED");
    }

}
