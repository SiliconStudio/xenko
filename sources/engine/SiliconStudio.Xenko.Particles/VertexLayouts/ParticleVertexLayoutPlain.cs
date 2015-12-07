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
    public class ParticleVertexLayoutPlain : ParticleVertexLayout
    {
        public unsafe override void SetPosition(ref Vector3 position)
        {
            *((Vector3*)(vertexBuffer + OffsetPosition)) = position;
        }

        public unsafe override void SetColor(ref Color4 color)
        {
            *((uint*)(vertexBuffer + OffsetColor)) = (uint)color.ToRgba();
        }

        public unsafe override void SetPosition(IntPtr position)
        {
            *((Vector3*)(vertexBuffer + OffsetPosition)) = (*((Vector3*)position));
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

        public override void SetLifetimeForParticle(IntPtr lifetime)
        {
            var oldPtr = vertexBuffer;

            for (var i = 0; i < VerticesPerParticle; i++)
            {
                SetLifetime(lifetime);
                NextVertex();
            }

            vertexBuffer = oldPtr;
        }

        public override void SetLifetimeForParticle(float lifetime)
        {
            var oldPtr = vertexBuffer;

            for (var i = 0; i < VerticesPerParticle; i++)
            {
                SetLifetime(lifetime);
                NextVertex();
            }

            vertexBuffer = oldPtr;
        }

        public override void SetRandomSeedForParticle(IntPtr seed)
        {
            var oldPtr = vertexBuffer;

            for (var i = 0; i < VerticesPerParticle; i++)
            {
                SetRandomSeed(seed);
                NextVertex();
            }

            vertexBuffer = oldPtr;
        }

        public unsafe override void SetLifetime(float lifetime)
        {
            *((float*)(vertexBuffer + OffsetLifetime)) = lifetime;
        }

        public unsafe override void SetLifetime(IntPtr lifetime)
        {
            *((float*)(vertexBuffer + OffsetLifetime)) = (*((float*)lifetime));
        }

        public unsafe override void SetRandomSeed(UInt32 randSeed)
        {
            *((float*)(vertexBuffer + OffsetRandom)) = 0.5f + (float) randSeed;
        }

        public unsafe override void SetRandomSeed(IntPtr randSeed)
        {
            *((float*)(vertexBuffer + OffsetRandom)) = 0.5f + (float) (*((UInt32*)randSeed));
        }

    }
}
