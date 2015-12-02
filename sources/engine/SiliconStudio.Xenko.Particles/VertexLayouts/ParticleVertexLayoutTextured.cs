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
    public class ParticleVertexLayoutTextured : ParticleVertexLayout
    {
        public unsafe override void SetPosition(ref Vector3 position)
        {
            // TODO Not hardcoded offset
            * ((Vector3*)(vertexBuffer + OffsetPosition)) = position;
        }

        public unsafe override void SetUvCoords(ref Vector2 uvCoords)
        {
            // TODO Not hardcoded offset
            *((Vector2*)(vertexBuffer + OffsetUv)) = uvCoords;
        }

        public unsafe override void SetColor(ref Color4 color)
        {
            // TODO Not hardcoded offset
            *((uint*)(vertexBuffer + OffsetColor)) = (uint)color.ToRgba();
        }

        public unsafe override void SetPosition(IntPtr position)
        {
            *((Vector3*)(vertexBuffer + OffsetPosition)) = (*((Vector3*)position));
        }

        public unsafe override void SetUvCoords(IntPtr uv)
        {
            *((Vector2*)(vertexBuffer + OffsetUv)) = (*((Vector2*)uv));
        }

        public unsafe override void SetColor(IntPtr color)
        {
            *((uint*)(vertexBuffer + OffsetColor)) = (uint)(*((Color4*)color)).ToRgba();
        }

        public override void SetColorForParticle(ref Color4 color)
        {
            var oldPtr = vertexBuffer;

            for (var i = 0; i < VerticesPerParticle; i++)
            {
                SetColor(ref color);
                NextVertex();
            }

            vertexBuffer = oldPtr;
        }

        public override void SetColorForParticle(IntPtr color)
        {
            var oldPtr = vertexBuffer;

            for (var i = 0; i < VerticesPerParticle; i++)
            {
                SetColor(color);
                NextVertex();
            }

            vertexBuffer = oldPtr;

        }
    }
}
