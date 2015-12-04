using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public class ParticleVertexLayoutTextured : ParticleVertexLayoutPlain
    {
        public unsafe override void SetUvCoords(ref Vector2 uvCoords)
        {
            *((Vector2*)(vertexBuffer + OffsetUv)) = uvCoords;
        }

        public unsafe override void SetUvCoords(IntPtr uv)
        {
            *((Vector2*)(vertexBuffer + OffsetUv)) = (*((Vector2*)uv));
        }

    }
}
