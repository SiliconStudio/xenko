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
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ParticleVertex // Identical to VertexPositionColorTextureSwizzle
    {
        public Vector3   Position;
        public Vector2   TexCoord;
        public uint      Color;
//        public float     Swizzle;


        /// <summary>
        /// Defines structure byte size.
        /// </summary>
        public static readonly int Size = 24;

        /// <summary>
        /// The vertex layout of this struct.
        /// </summary>
        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            VertexElement.Position<Vector3>(),
            VertexElement.TextureCoordinate<Vector2>(),
            VertexElement.Color<Color>()
//            new VertexElement("BATCH_SWIZZLE", PixelFormat.R32_Float)
            );
    }

    /*
    public struct ParticleVertex
    {
        public Vector3   Position;
        public float     Size;
        public Vector2   TexCoord;
        public uint      Color;
    }
    */

}
