using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public abstract class ParticleVertexLayout
    {
        // This region will become abstract if the vertex buffers and binding is moved away from the ParticleBatch and into the ShapeBuilders directly
        #region Vertex Declaration

        /// <summary>
        /// This is the common vertex declaration the ParticleBatch uses.
        /// Later it might change to depend on the shape builder, in which case the vertex and index buffers and
        ///     bindings should be per shape as well.
        /// </summary>
        public static VertexDeclaration VertexDeclaration { get; }
            = new VertexDeclaration(
            VertexElement.Position<Vector3>(),
            VertexElement.TextureCoordinate<Vector2>(),
            VertexElement.Color<Color>(),
            new VertexElement("BATCH_LIFETIME",   PixelFormat.R32_Float),
            new VertexElement("BATCH_RANDOMSEED", PixelFormat.R32_Float)
            );

        protected const int OffsetPosition  = 0;
        protected const int OffsetUv        = 12 + OffsetPosition;
        protected const int OffsetColor     = 8  + OffsetUv;
        protected const int OffsetLifetime  = 4  + OffsetColor;
        protected const int OffsetRandom    = 4  + OffsetLifetime;
        protected const int OffsetMax       = 4  + OffsetRandom;

        public VertexDeclaration GetVertexDeclaration() => VertexDeclaration;
//        public abstract VertexDeclaration GetVertexDeclaration();

        public int Size { get; private set; } = OffsetMax; 
//        public abstract int Size { get; protected set; }

        public int VerticesPerParticle { get; set; } = 4;
//        public abstract int VerticesPerParticle { get; internal set; } // Will depend on the builder

        #endregion

        protected IntPtr vertexBuffer = IntPtr.Zero;
        protected IntPtr vertexBufferOrigin = IntPtr.Zero;

        public void StartBuffer(IntPtr vtxBuff)
        {
            vertexBufferOrigin = vertexBuffer = vtxBuff;
        }

        public void RestartBuffer()
        {
            vertexBuffer = vertexBufferOrigin;
        }

        public void EndBuffer()
        {
            vertexBuffer = IntPtr.Zero;
            vertexBufferOrigin = IntPtr.Zero;
        }

        public void NextVertex()
        {
            vertexBuffer += Size;
        }

        public void NextParticle()
        {
            vertexBuffer += Size * VerticesPerParticle;
        }

        public virtual void SetPosition(ref Vector3 position) { }

        public virtual void SetUvCoords(ref Vector2 uvCoords) { }

        public virtual void TransformUvCoords(ref Vector4 OffsetScale) { }

        public virtual void SetColor(ref Color4 color) { }

        public virtual void SetPosition(IntPtr ptr) { }

        public virtual void SetUvCoords(IntPtr ptr) { }

        public virtual void SetColor(IntPtr ptr) { }

        public virtual void SetLifetime(float lifetime) { }

        public virtual void SetLifetime(IntPtr lifetime) { }

        public virtual void SetRandomSeed(UInt32 randSeed) { }

        public virtual void SetRandomSeed(IntPtr randSeed) { }

        /// <summary>
        /// Sets the same position for all vertices created from the same particle.
        /// Assumes offset is at the first vertex of the particle.
        /// </summary>
        /// <param name="position">Position vector to assign</param>
        public virtual void SetPositionForParticle(ref Vector3 position) { }

        public virtual void SetUvCoordsForParticle(ref Vector2 uvCoords) { }

        public virtual void SetColorForParticle(ref Color4 color) { }

        public virtual void SetPositionForParticle(IntPtr ptr) { }

        public virtual void SetUvCoordsForParticle(IntPtr ptr) { }

        public virtual void SetColorForParticle(IntPtr ptr) { }

        public virtual void SetLifetimeForParticle(IntPtr ptr) { }

        public virtual void SetLifetimeForParticle(float lifetime) { }

        public virtual void SetRandomSeedForParticle(IntPtr ptr) { }

    }
}
