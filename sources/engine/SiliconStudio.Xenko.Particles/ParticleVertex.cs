using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles
{
    // TODO This is just a copy from the old implementation. It will change when the particle shader is done.
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleVertex
    {
        public Vector3   Position;
        public float     Size;
        public Vector2   TexCoord;
        public uint      Color;
    }

    
}
