using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;



namespace SiliconStudio.Xenko.Particles.Rendering
{
    using VerxtexParticleBasic = VertexPositionColorTextureSwizzle; // TODO Change when the shader is ready


    public class ParticleBatch : BatchBase<ParticleBatch.ParticleDrawInfo>
    {

        public ParticleBatch(GraphicsDevice device, int bufferElementCount = 1024, int batchCapacity = 64)
            : base(device, SpriteBatch.Bytecode, SpriteBatch.BytecodeSRgb, StaticQuadBufferInfo.CreateQuadBufferInfo("ParticleBatch.VertexIndexBuffer", false, bufferElementCount, batchCapacity), VerxtexParticleBasic.Layout)
        {
        }

        // TODO Implement
        protected unsafe override void UpdateBufferValuesFromElementInfo(ref ElementInfo elementInfo, IntPtr vertexPointer, IntPtr indexPointer, int vexterStartOffset)
        {
            var vertex = (VertexPositionColorTextureSwizzle*)vertexPointer;
            fixed (ParticleDrawInfo* drawInfo = &elementInfo.DrawInfo)
            {
                var currentPosition = drawInfo->LeftTopCornerWorld;

                var textureCoordX = new Vector2(drawInfo->Source.Left, drawInfo->Source.Right);
                var textureCoordY = new Vector2(drawInfo->Source.Top, drawInfo->Source.Bottom);

                // set the two first line of vertices
                for (int r = 0; r < 2; r++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        vertex->Color = drawInfo->Color;
                        vertex->Swizzle = (int)drawInfo->Swizzle;
                        vertex->TextureCoordinate.X = textureCoordX[c];
                        vertex->TextureCoordinate.Y = textureCoordY[r];

                        vertex->Position.X = currentPosition.X;
                        vertex->Position.Y = currentPosition.Y;
                        vertex->Position.Z = currentPosition.Z;
                        vertex->Position.W = currentPosition.W;

                        vertex++;

                        if (c == 0)
                            Vector4.Add(ref currentPosition, ref drawInfo->UnitXWorld, out currentPosition);
                        else
                            Vector4.Subtract(ref currentPosition, ref drawInfo->UnitXWorld, out currentPosition);
                    }

                    Vector4.Add(ref currentPosition, ref drawInfo->UnitYWorld, out currentPosition);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ParticleDrawInfo
        {
            public Vector4 LeftTopCornerWorld;
            public Vector4 UnitXWorld;
            public Vector4 UnitYWorld;
            public RectangleF Source;
            public Color4 Color;
            public SwizzleMode Swizzle;
        }
    }
}
